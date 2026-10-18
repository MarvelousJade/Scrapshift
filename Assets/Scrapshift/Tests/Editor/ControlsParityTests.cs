using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ControlsParityTests
    {
        [Test] public void DigitalMovementCombinesActionStrengthsBeforeAnalogSelection()
        {
            Assert.AreEqual(Vector2.zero,CombatInput.PlayerMovement(Vector2.zero,Vector2.right,Vector2.left,Vector2.zero));
            Assert.AreEqual(Vector2.up,CombatInput.PlayerMovement(Vector2.zero,Vector2.one,Vector2.left,Vector2.zero));
            Assert.AreEqual(Vector2.zero,CombatInput.PlayerMovement(Vector2.right,Vector2.right,Vector2.right,Vector2.zero),"Both keyboard directions remain pressed even with a D-pad event on one of them.");
            Assert.AreEqual(Vector2.left,CombatInput.PlayerMovement(Vector2.zero,Vector2.right,Vector2.left,Vector2.left),"After digital cancellation, analog movement wins.");
            Assert.That(CombatInput.ButtonMovement(Vector2.zero,Vector2.one,Vector2.one).magnitude,Is.EqualTo(1).Within(.00001f));
        }
        [Test] public void MouseOwnershipPreventsHeldStickFromEnablingPressOnlyManualAim()
        {
            Assert.IsFalse(CombatInput.ManualAim(false,true,false,Vector2.right,false));
            Assert.IsTrue(CombatInput.ManualAim(false,true,false,Vector2.right,true));
            Assert.IsTrue(CombatInput.ManualAim(false,true,true,Vector2.right,false),"Mouse press remains eligible after ownership changes.");
            Assert.IsTrue(CombatInput.ManualAim(true,false,false,Vector2.right,false),"Always-manual mouse aiming remains enabled.");
        }
        [Test] public void ControllerMenuDirectionPrioritizesHorizontalAxisAboveThreshold()
        {
            Assert.AreEqual(Vector2.right,CombatInput.MenuDirection(new Vector2(.51f,1)));
            Assert.AreEqual(Vector2.left,CombatInput.MenuDirection(new Vector2(-.51f,-1)));
            Assert.AreEqual(Vector2.up,CombatInput.MenuDirection(new Vector2(.5f,1)));
            Assert.AreEqual(Vector2.zero,CombatInput.MenuDirection(new Vector2(.5f,.5f)));
        }
        [TestCase("GamepadHorizontal",0,false)]
        [TestCase("GamepadVertical",1,true)]
        [TestCase("GamepadAimHorizontal",3,false)]
        [TestCase("GamepadAimVertical",4,true)]
        [TestCase("GamepadDpadHorizontal",5,false)]
        [TestCase("GamepadDpadVertical",6,false)]
        public void NamedGamepadAxesExistInUnitySerializedSettings(string name,int hardwareAxis,bool invert)
        {
            // Regression: text appended after m_UsePhysicalKeys looked like axes,
            // but Unity omitted those entries from its actual m_Axes array.
            var settings=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
            Assert.That(settings,Is.Not.Empty,"Unity must deserialize the real InputManager settings asset.");
            using(var serialized=new SerializedObject(settings[0]))
            {
                serialized.Update();var axes=serialized.FindProperty("m_Axes");
                Assert.IsNotNull(axes);Assert.IsTrue(axes.isArray);
                SerializedProperty found=null;int matches=0;var names=new System.Collections.Generic.List<string>();
                for(int i=0;i<axes.arraySize;i++)
                {
                    var axis=axes.GetArrayElementAtIndex(i);
                    names.Add(axis.FindPropertyRelative("m_Name").stringValue);
                    if(axis.FindPropertyRelative("m_Name").stringValue!=name)continue;
                    found=axis.Copy();matches++;
                }
                Assert.AreEqual(1,matches,name+" must occur once in Unity's deserialized m_Axes, rather than only in YAML text. Loaded "+axes.arraySize+": "+string.Join(", ",names));
                Assert.AreEqual(2,found.FindPropertyRelative("type").intValue,"Use joystick motion rather than keyboard or mouse input.");
                Assert.AreEqual(hardwareAxis,found.FindPropertyRelative("axis").intValue);
                Assert.AreEqual(invert,found.FindPropertyRelative("invert").boolValue);
                Assert.AreEqual(0,found.FindPropertyRelative("joyNum").intValue,"Accept the connected controller.");
                Assert.AreEqual(0,found.FindPropertyRelative("dead").floatValue,"CombatInput applies the reference radial deadzone.");
                Assert.AreEqual(1,found.FindPropertyRelative("sensitivity").floatValue);
            }
        }
        [Test] public void ControllerRerollHoldCancelsOnReleaseAndFiresOnlyOncePerPress()
        {
            var hold=new CombatInput.RerollHold();
            Assert.IsFalse(hold.Tick(true,true,true,true,.2f));
            Assert.That(hold.Progress,Is.EqualTo(.4f).Within(.0001f));
            Assert.IsFalse(hold.Tick(false,false,true,true,0));
            Assert.IsFalse(hold.Tick(true,true,true,true,.3f));
            Assert.IsTrue(hold.Tick(false,true,true,true,.2f));
            Assert.IsFalse(hold.Tick(false,true,true,true,2));
            hold.Tick(false,false,true,true,0);
            Assert.IsTrue(hold.Tick(true,true,true,false,0),"The functional hold setting permits immediate reroll.");
        }
        [Test] public void HeldInfoCannotActivateAnActionThatWasHiddenAtTheInitialPress()
        {
            var hold=new CombatInput.RerollHold();
            Assert.IsFalse(hold.Tick(true,true,false,true,.5f));
            Assert.IsFalse(hold.Tick(false,true,true,true,1),"Holding Y across a reward/shop or modal transition cannot arm a newly visible reroll.");
            Assert.IsFalse(hold.Tick(false,true,true,false,1));
            Assert.IsTrue(hold.Tick(true,true,true,false,0));
        }
        [Test] public void ShopSelectLocksFocusedOfferAndStealsThroughActualRulesWhenEligible()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var run=new RunRules(31);run.Configure(catalog);run.Start();run.Tick(run.Remaining);
            while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            var offer=run.Shop[0];Assert.IsNotNull(offer);
            Assert.AreEqual(1,CombatInput.SelectShopOffer(run,0));Assert.IsTrue(offer.locked);
            Assert.AreEqual(1,CombatInput.SelectShopOffer(run,0));Assert.IsFalse(offer.locked);
            Assert.AreEqual(0,CombatInput.SelectShopOffer(run,-1));
            run.Pause();Assert.AreEqual(0,CombatInput.SelectShopOffer(run,0));Assert.IsFalse(offer.locked);run.Pause();
            run.NextWave();
            run.GrantItem(new ItemEntry{id="control-thief",title="control-thief",tier=1,value=1,maxCount=-1,effects=new[]{new StatEffect{key="item_steals",value=1}},conditionalEffects=System.Array.Empty<ConditionalEffect>()});
            run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            var target=new ItemEntry{id="control-target",title="control-target",tier=1,value=10,maxCount=-1,effects=System.Array.Empty<StatEffect>(),conditionalEffects=System.Array.Empty<ConditionalEffect>()};
            run.Shop[0]=new ShopOffer{item=target,rolledWave=run.Wave};int money=run.Scrap;
            run.GrantItem(new ItemEntry{id="steal-lock-block",title="steal-lock-block",effects=new[]{new StatEffect{key="disable_item_locking",value=1}},conditionalEffects=System.Array.Empty<ConditionalEffect>()});
            Assert.IsFalse(run.CanLock(0));Assert.IsTrue(run.CanSteal(0),"The lock restriction must not disable theft.");
            Assert.AreEqual(2,CombatInput.SelectShopOffer(run,0));
            Assert.Contains(target,run.Items);Assert.AreEqual(money,run.Scrap);Assert.AreEqual(0,run.StealsRemaining);Assert.IsNull(run.Shop[0]);
        }
        [Test] public void FailedTheftAtWeaponCapacityStillTogglesFocusedOfferLock()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var run=new RunRules(31);run.Configure(catalog);run.Start();
            run.GrantItem(new ItemEntry{id="control-thief",title="control-thief",tier=1,value=1,maxCount=-1,effects=new[]{new StatEffect{key="item_steals",value=1}},conditionalEffects=System.Array.Empty<ConditionalEffect>()});
            run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            var weapon=System.Array.Find(catalog.data.equipment,w=>w.tier==4);
            Assert.IsNotNull(weapon);run.Inventory.Clear();for(int i=0;i<run.WeaponSlots;i++)run.Inventory.Add(weapon);
            var offer=new ShopOffer{weapon=weapon,rolledWave=1};run.Shop[0]=offer;
            Assert.IsFalse(run.CanSteal(0));Assert.AreEqual(1,run.StealsRemaining);
            Assert.AreEqual(1,CombatInput.SelectShopOffer(run,0));
            Assert.IsTrue(offer.locked);Assert.AreEqual(1,run.StealsRemaining);Assert.AreSame(offer,run.Shop[0]);
        }
        [TestCase("hourglass")]
        [TestCase("broken_hourglass")]
        public void NonLockableReferenceItemsRejectBothDirectAndControllerLock(string id)
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var run=new RunRules(31);run.Configure(catalog);run.Start();run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            var item=System.Array.Find(catalog.data.items,i=>i.id==id);Assert.IsNotNull(item);
            var offer=new ShopOffer{item=item,rolledWave=1};run.Shop[0]=offer;
            Assert.IsFalse(run.CanLock(0));run.ToggleLock(0);Assert.IsFalse(offer.locked);
            Assert.AreEqual(0,CombatInput.SelectShopOffer(run,0));Assert.IsFalse(offer.locked);
        }
        [Test] public void DisableLockFlagUsesPositiveEffectSumAndCannotBeBypassedByController()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var run=new RunRules(31);run.Configure(catalog);run.Start();run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            var offer=run.Shop[0];Assert.IsTrue(run.CanLock(0));
            var block=new ItemEntry{id="lock-block",title="lock-block",effects=new[]{new StatEffect{key="disable_item_locking",value=1}},conditionalEffects=System.Array.Empty<ConditionalEffect>()};
            run.GrantItem(block);Assert.IsFalse(run.CanLock(0));run.ToggleLock(0);
            Assert.AreEqual(0,CombatInput.SelectShopOffer(run,0));Assert.IsFalse(offer.locked);
            run.GrantItem(new ItemEntry{id="lock-offset",title="lock-offset",effects=new[]{new StatEffect{key="disable_item_locking",value=-1}},conditionalEffects=System.Array.Empty<ConditionalEffect>()});
            Assert.IsTrue(run.CanLock(0),"Reference get_player_effect_bool is true only for a positive sum.");
            Assert.AreEqual(1,CombatInput.SelectShopOffer(run,0));Assert.IsTrue(offer.locked);
            run.Pause();Assert.IsFalse(run.CanLock(0));run.ToggleLock(0);Assert.IsTrue(offer.locked);
        }
        [Test] public void FocusedShopChildControlsResolveTheirOfferAndIgnoreUnrelatedControls()
        {
            var cards=new[]{new Rect(24,118,250,453),new Rect(282,118,250,453)};
            Assert.AreEqual(0,CombatInput.OfferAtFocus(cards,new Rect(72,451,154,55)));
            Assert.AreEqual(1,CombatInput.OfferAtFocus(cards,new Rect(294,528,220,43)));
            Assert.AreEqual(-1,CombatInput.OfferAtFocus(cards,new Rect(24,670,80,80)));
            Assert.AreEqual(-1,CombatInput.OfferAtFocus(cards,new Rect(24,27,294,56)));
        }
        [Test] public void MouseOnlyMovementFollowsWithoutPressAndPressStops()
        {
            // Reference player_movement_behavior.gd: MIN_MOVE_DIST=20, left button stops.
            Assert.AreEqual(new Vector2(2,1),CombatInput.MouseMovement(new Vector2(2,1),false));
            Assert.AreEqual(Vector2.zero,CombatInput.MouseMovement(new Vector2(2,1),true));
            Assert.AreEqual(Vector2.zero,CombatInput.MouseMovement(new Vector2(.199f,.199f),false));
            Assert.AreEqual(new Vector2(.2f,0),CombatInput.MouseMovement(new Vector2(.2f,0),false));
            Assert.AreEqual(new Vector2(0,-.2f),CombatInput.MouseMovement(new Vector2(0,-.2f),false));
        }
        [Test] public void MovementUsesFullSpeedAfterDeadzoneEvenNearCursorOrPartialStick()
        {
            // Reference unit.gd:get_velocity normalizes the movement vector.
            Assert.That(CombatInput.Velocity(new Vector2(.2f,0),4),Is.EqualTo(new Vector2(4,0)));
            Assert.That(CombatInput.Velocity(new Vector2(1,1),4).magnitude,Is.EqualTo(4).Within(.00001f));
            Assert.AreEqual(Vector2.zero,CombatInput.Velocity(Vector2.zero,4));
            Assert.AreEqual(Vector2.zero,CombatInput.Stick(new Vector2(.5f,0)));
            Assert.That(CombatInput.Stick(new Vector2(.75f,0)).x,Is.EqualTo(.5f).Within(.00001f));
            Assert.AreEqual(Vector2.left,CombatInput.Movement(Vector2.left,new Vector2(.5f,0)));
        }
        [Test] public void PressOnlyAimRespondsToMouseOrRightStickWithoutDisablingAutomaticFire()
        {
            // Utils.is_manual_aim: press-only overrides always; stick threshold is squared .05.
            Assert.IsFalse(CombatInput.ManualAim(true,true,false,Vector2.zero));
            Assert.IsTrue(CombatInput.ManualAim(false,true,true,Vector2.zero));
            Assert.IsFalse(CombatInput.ManualAim(false,true,false,new Vector2(.2f,0)));
            Assert.IsTrue(CombatInput.ManualAim(false,true,false,new Vector2(.23f,0)));
            Assert.IsTrue(CombatInput.ManualAim(true,false,false,Vector2.zero));
        }
        [Test] public void MenuNavigationSelectsNearestControlInRequestedDirection()
        {
            var buttons=new[]{new Rect(0,0,80,40),new Rect(100,0,80,40),new Rect(0,80,80,40),new Rect(100,80,80,40)};
            Assert.AreEqual(1,CombatInput.Navigate(buttons,0,Vector2.right));
            Assert.AreEqual(2,CombatInput.Navigate(buttons,0,Vector2.up));
            Assert.AreEqual(0,CombatInput.Navigate(buttons,0,Vector2.left));
            Assert.AreEqual(0,CombatInput.Navigate(buttons,-1,Vector2.down));
            Assert.AreEqual(-1,CombatInput.Navigate(new Rect[0],0,Vector2.right));
        }
        [TestCase(false)] [TestCase(true)]
        public void PauseFreezesAndRestoresShopAndUpgrade(bool upgrade)
        {
            var run=new RunRules(12);run.Start();if(upgrade)run.Collect(16);run.Tick(run.Remaining);
            var phase=run.Phase;Assert.AreEqual(upgrade?RunPhase.Upgrade:RunPhase.Shop,phase);
            float remaining=run.Remaining;int money=run.Scrap;var offers=run.Offers;
            run.Pause();Assert.AreEqual(RunPhase.Paused,run.Phase);run.Tick(100);run.Pause();
            Assert.AreEqual(phase,run.Phase);Assert.AreEqual(remaining,run.Remaining);Assert.AreEqual(money,run.Scrap);Assert.AreSame(offers,run.Offers);
        }
        [Test] public void OptionsNavigationReachesEachRowBeforeBackFromEveryActiveTab()
        {
            // Options controls sit in a scroll view. Register the complete setting row,
            // otherwise the distant right-side On/Off button loses to Back below it.
            var controls=new Rect[12];
            for(int i=0;i<4;i++)controls[i]=new Rect(160+i*282,55,272,65);
            for(int i=0;i<7;i++)controls[i+4]=new Rect(235,155+i*62,940,53);
            controls[11]=new Rect(470,770,500,60);
            for(int tab=0;tab<4;tab++)Assert.AreEqual(4,CombatInput.Navigate(controls,tab,Vector2.up),"Down from active tab "+tab);
            for(int row=4;row<10;row++)Assert.AreEqual(row+1,CombatInput.Navigate(controls,row,Vector2.up));
            Assert.AreEqual(11,CombatInput.Navigate(controls,10,Vector2.up));
            Assert.AreEqual(10,CombatInput.Navigate(controls,11,Vector2.down));
        }
        [Test] public void PausedCrateCannotBeConsumedAndResumesTheSameRewardBeforeUpgrade()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var run=new RunRules(12);run.Configure(catalog);run.Start();run.CollectConsumable(ConsumableKind.Crate);run.Collect(16);run.Tick(run.Remaining);
            Assert.AreEqual(RunPhase.Loot,run.Phase);var reward=run.CrateItem;int levels=run.PendingLevels;
            run.Pause();run.Tick(100);Assert.IsFalse(run.ResolveCrate(true));run.Choose(0);
            Assert.AreSame(reward,run.CrateItem);Assert.AreEqual(1,run.PendingCrates);Assert.AreEqual(levels,run.PendingLevels);
            run.Pause();Assert.AreEqual(RunPhase.Loot,run.Phase);Assert.IsTrue(run.ResolveCrate(true));
            Assert.AreEqual(RunPhase.Upgrade,run.Phase);Assert.Contains(reward,run.Items);
        }
        [Test] public void ManualRangedAimTracksDuringAttackButMeleeLocksUntilAnimationEnds()
        {
            // weapon.gd defaults true; melee_weapon.gd overrides with !is_shooting.
            Assert.IsTrue(CombatInput.ShouldRotateManual(false,true));
            Assert.IsTrue(CombatInput.ShouldRotateManual(false,false));
            Assert.IsFalse(CombatInput.ShouldRotateManual(true,true));
            Assert.IsTrue(CombatInput.ShouldRotateManual(true,false));
        }
        [Test] public void GamepadAimHistoryChangesOnlyDuringManualGamepadAimAndSurvivesStickRelease()
        {
            // main.gd updates gamepad_attack_vector only in manual mode and for nonzero rjoy.
            var previous=Vector2.up;
            Assert.AreEqual(previous,CombatInput.RememberAim(previous,Vector2.left,false,true));
            Assert.AreEqual(previous,CombatInput.RememberAim(previous,Vector2.left,true,false));
            Assert.AreEqual(previous,CombatInput.RememberAim(previous,Vector2.zero,true,true));
            Assert.AreEqual(Vector2.left,CombatInput.RememberAim(previous,new Vector2(-.4f,0),true,true));
        }
        [Test] public void MouseCanTakeAimFromSteadyHeldStickUntilANewValidControllerEvent()
        {
            var held=new Vector2(.8f,0);
            Assert.IsFalse(CombatInput.GamepadMotionEvent(held,held));
            bool usingGamepad=CombatInput.DeviceAfterEvents(true,false,true);
            Assert.IsFalse(usingGamepad);
            Assert.IsFalse(CombatInput.DeviceAfterEvents(usingGamepad,CombatInput.GamepadMotionEvent(held,held),false));
            Assert.IsTrue(CombatInput.DeviceAfterEvents(usingGamepad,CombatInput.GamepadMotionEvent(held,new Vector2(.9f,0)),false));
            Assert.IsFalse(CombatInput.GamepadMotionEvent(held,Vector2.zero));
            Assert.IsFalse(CombatInput.GamepadMotionEvent(Vector2.zero,new Vector2(.4f,.4f)),"Reference valid-joy-motion threshold applies per axis, not radial length.");
        }
        [Test] public void FocusedSelectionPreviewIgnoresStationaryMouseOverADifferentCard()
        {
            Assert.IsTrue(CombatInput.PreviewFocus(true,true,false));
            Assert.IsFalse(CombatInput.PreviewFocus(true,false,true));
            Assert.IsTrue(CombatInput.PreviewFocus(false,false,true));
            Assert.IsFalse(CombatInput.PreviewFocus(false,true,false));
        }
        [Test] public void CharacterGridNavigationCanVisitALockedSlotBetweenUnlockedCards()
        {
            // Source BaseSelection retains locked displayed elements; focus displays
            // their requirement without selecting them. Keep their grid positions reachable.
            var cards=new[]{new Rect(57,603,70,80),new Rect(135,603,70,80),new Rect(213,603,70,80),new Rect(291,603,70,80)};
            int unlocked=1,locked=2,nextUnlocked=3;
            Assert.AreEqual(locked,CombatInput.Navigate(cards,unlocked,Vector2.right));
            Assert.AreEqual(nextUnlocked,CombatInput.Navigate(cards,locked,Vector2.right));
            Assert.AreEqual(unlocked,CombatInput.Navigate(cards,locked,Vector2.left));
        }
    }
}
