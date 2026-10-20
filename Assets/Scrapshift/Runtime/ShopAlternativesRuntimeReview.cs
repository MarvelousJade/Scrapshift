using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartShopAlternativesReview()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--shop-alternatives-review");
            if(i>=0&&i+1<args.Length)new GameObject("Shop alternatives review").AddComponent<ShopAlternativesReview>().folder=args[i+1];
        }
        sealed class ShopAlternativesReview:MonoBehaviour
        {
            public string folder;
            ArenaGame game;Gamepad pad;float started;
            [Serializable] sealed class Check {public string name,evidence;public bool passed;public int before,after,expected;}
            [Serializable] sealed class ControlMapping {public string name,logical,registered;public int focus;}
            [Serializable] sealed class Report
            {
                public string status,error;
                public string scope="Isolated imported-content fixture. Virtual A/Y events traverse actual Update/OnGUI; focus positions, starting inventory/materials/offers and wave deadlines are staged. Entry effects use the production shop transition. Not natural acquisition, physical-controller verification, or a full campaign.";
                public List<Check> checks=new List<Check>();public List<string> captures=new List<string>();public List<ControlMapping> mappings=new List<ControlMapping>();
            }
            readonly Report report=new Report();
            IEnumerator Start()
            {
                started=Time.realtimeSinceStartup;var stack=new Stack<IEnumerator>();stack.Push(Run());string error=null;
                while(stack.Count>0)
                {
                    object next=null;bool moved=false;
                    try
                    {
                        Require(Time.realtimeSinceStartup-started<70,"Shop alternatives exceeded 70 seconds.");
                        moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;else stack.Pop();
                    }
                    catch(Exception ex){error=ex.ToString();break;}
                    if(!moved)continue;if(next is IEnumerator child){stack.Push(child);continue;}yield return next;
                }
                report.error=error;report.status=error==null&&report.checks.Count>=18&&report.checks.All(c=>c.passed)?"Passed":"Failed";
                Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"shop-alternatives.json"),JsonUtility.ToJson(report,true));
                if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);Application.Quit(report.status=="Passed"?0:1);
            }
            IEnumerator Run()
            {
                folder=Path.GetFullPath(folder);Directory.CreateDirectory(folder);yield return null;yield return null;
                game=FindAnyObjectByType<ArenaGame>();Require(game!=null&&GameSettings.IsFixture,"Use --controller-check <input-folder> for isolated settings/profile.");
                Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                foreach(var device in Gamepad.all)InputSystem.DisableDevice(device);pad=InputSystem.AddDevice<Gamepad>();pad.MakeCurrent();
                GameSettings.Set("pauseFocus",false);GameSettings.Set("holdReroll",false);GameSettings.Set("mouseMove",false);GameSettings.Set("gamepadMove",false);game.sound.SetMuted(true);
                StartRun("well_rounded");game.Rules.GrantItem(Item("coupon"));game.Rules.GrantItem(Item("recycling_machine"));game.Rules.GrantItem(Item("dangerous_bunny"));Shop();
                var bag=Item("bag");game.Rules.Shop[0]=new ShopOffer{item=bag,rolledWave=1};yield return Render();
                int money=game.Rules.Scrap,price=game.Rules.Price(game.Rules.Shop[0]);
                int sourcePrice=(int)Mathf.Max(1,(bag.value+1+bag.value*.1f)*.95f);
                CheckThat("coupon-source-price",price==sourcePrice,"item_service.gd:get_value; imported Coupon -5%",price,price,sourcePrice);
                yield return Activate(PriceRect(0));
                CheckThat("purchase-ui",game.Rules.Items.Contains(bag)&&game.Rules.Shop[0]==null&&game.Rules.Scrap==money-price,"Virtual A on rendered price button",money,game.Rules.Scrap,money-price);
                game.Rules.Shop[0]=new ShopOffer{item=Item("coffee"),rolledWave=1};yield return Render();var locked=game.Rules.Shop[0];price=game.Rules.Price(locked);
                yield return Activate(LockRect(0));CheckThat("lock-ui",locked.locked,"Virtual A on Lock");
                money=game.Rules.Scrap;yield return Press(GamepadButton.North);
                CheckThat("free-reroll-ui",game.Rules.ShopRerollCount==1&&game.Rules.Scrap==money,"Virtual Y; imported Dangerous Bunny",money,game.Rules.Scrap,money);
                CheckThat("locked-offer-retention",ReferenceEquals(locked,game.Rules.Shop[0])&&game.Rules.Price(locked)==price,"Same locked offer and retained price after reroll");
                money=game.Rules.Scrap;price=game.Rules.RerollPrice;yield return Press(GamepadButton.North);
                CheckThat("paid-reroll-ui",price==1&&game.Rules.Scrap==money-1&&game.Rules.ShopRerollCount==2,"First paid wave-one reroll after free reroll",money,game.Rules.Scrap,money-1);
                yield return Activate(LockRect(0));CheckThat("unlock-ui",!locked.locked,"Virtual A on Locked");
                yield return Capture("coupon-free-paid-lock.png");

                var weapon=game.catalog.Weapon("stick",1);game.Rules.Inventory.Clear();game.Rules.Inventory.Add(weapon);game.Rules.Inventory.Add(weapon);game.SyncEquipment();yield return Render();
                yield return ActivateWeapon(0);CheckThat("inspect-ui",game.selectedWeapon==0,"Virtual A on first registered inventory weapon");
                yield return Render();
                CheckThat("weapon-popup-exclusive-navigation",game.menuNavigationButtons.Count==3,"Modal popup registers only Combine, Recycle, Cancel; no background shop/stat/inventory controls");
                var inventory=game.Rules.Inventory.ToArray();var offers=game.Rules.Shop.ToArray();money=game.Rules.Scrap;yield return ActivatePopup(2);
                CheckThat("cancel-ui",game.selectedWeapon<0&&inventory.SequenceEqual(game.Rules.Inventory)&&offers.SequenceEqual(game.Rules.Shop)&&game.Rules.Scrap==money,"Virtual A on Cancel; inventory, exact offer objects and currency unchanged");
                yield return ActivateWeapon(0);yield return Capture("weapon-combine-actions.png");yield return ActivatePopup(0);
                CheckThat("combine-ui",game.Rules.Inventory.Count==1&&game.Rules.Inventory[0].id=="stick"&&game.Rules.Inventory[0].tier==2&&game.selectedWeapon<0,"Virtual A on Combine; two tier-I copies -> one tier-II");
                yield return ActivateWeapon(0);money=game.Rules.Scrap;var upgraded=game.Rules.Inventory[0];
                // Source recycling: integer discounted shop value, then clamp(.25+.35), truncate.
                int shopValue=(int)((upgraded.value+1+upgraded.value*.1f)*.95f),recycle=Math.Max(1,(int)(shopValue*.60f));
                yield return ActivatePopup(0);
                CheckThat("recycle-ui",game.Rules.Inventory.Count==0&&game.Rules.Scrap==money+recycle,"Virtual A on Recycle; Coupon + Recycling Machine source formula",money,game.Rules.Scrap,money+recycle);
                yield return Capture("recycled-inventory.png");

                StartRun("well_rounded");game.Rules.CollectConsumable(ConsumableKind.Crate);game.Rules.CollectConsumable(ConsumableKind.Crate);game.Rules.Tick(game.Rules.Remaining);yield return Render();
                Require(game.Rules.Phase==RunPhase.Loot,"Two staged crates must open Item found.");var reward=game.Rules.CrateItem;int copies=game.Rules.Items.Count(i=>i.id==reward.id);
                yield return Activate(new Rect(335,586,440,56));
                CheckThat("crate-take-ui",game.Rules.PendingCrates==1&&game.Rules.Items.Count(i=>i.id==reward.id)==copies+1,"Virtual A on Item found Take");
                money=game.Rules.Scrap;recycle=game.Rules.CrateRecycleValue;yield return Capture("crate-disposal-actions.png");yield return Activate(new Rect(335,654,440,56));
                CheckThat("crate-disposal-ui",game.Rules.PendingCrates==0&&game.Rules.Scrap==money+recycle,"Virtual A on Item found Recycle",money,game.Rules.Scrap,money+recycle);

                StartRun("arms_dealer");Shop();yield return Render();
                CheckThat("arms-dealer-entry",game.Rules.Inventory.Count==0&&game.Rules.Shop.Count(o=>o?.weapon!=null)>=1,"Production entry destroys starting weapons; imported minimum weapon offers");
                yield return Capture("arms-dealer-shop.png");
                StartRun("fisherman");Shop();yield return Render();
                CheckThat("fisherman-guaranteed-bait",game.Rules.Shop.Any(o=>o?.item?.id=="bait"),"Production RollShop with imported guaranteed bait/specific price");
                yield return Capture("fisherman-guaranteed-bait.png");

                StartRun("well_rounded");game.Rules.Inventory.Clear();game.Rules.Inventory.Add(game.catalog.Weapon("stick",1));game.Rules.Inventory.Add(game.catalog.Weapon("pistol",1));game.Rules.GrantItem(Item("anvil"));Shop();
                CheckThat("anvil-random-eligible-target",game.Rules.Inventory.Count(w=>w.tier==2)==1&&game.Rules.Inventory.Count(w=>w.tier==1)==1,"Production entry chooses one of two eligible imported weapons");
                StartRun("well_rounded");game.Rules.Inventory.Clear();game.Rules.Inventory.Add(game.catalog.Weapon("stick",4));game.Rules.GrantItem(Item("anvil"));float armor=game.Rules.Stat("armor");Shop();
                CheckThat("anvil-no-target-fallback",game.Rules.Stat("armor")==armor+2&&game.Rules.Inventory.Single().tier==4,"Production entry: no eligible weapon -> +2 Armor");
                StartRun("well_rounded");game.Rules.GrantItem(Item("hourglass"));Shop();yield return Render();
                yield return Activate(new Rect(game.StatsLeft,686,336,56));
                CheckThat("hourglass-go-ui",game.Rules.Phase==RunPhase.Playing&&game.Rules.Wave==1&&!game.Rules.Items.Any(i=>i.id=="hourglass")&&game.Rules.Items.Any(i=>i.id=="broken_hourglass"),"Virtual A Go; repeat wave and replace real Hourglass");
                yield return Capture("hourglass-repeated-wave.png");
                StartRun("well_rounded");game.Rules.GrantItem(Item("candy_bag"));
                CheckThat("candy-imported-chance",game.Rules.Stat("extra_elite_next_wave_chance")==10,"Imported Candy Bag has 10% chance; branch below is explicitly forced");
                game.Rules.GainWeaponStat("extra_elite_next_wave_chance",90);Shop();yield return Render();
                yield return Activate(new Rect(game.StatsLeft,686,336,56));
                CheckThat("candy-forced-go-ui",game.Rules.Phase==RunPhase.Playing&&game.Rules.ExtraElite,"Virtual A Go; fixture raises chance to 100% to exercise successful branch, not probability distribution");
                yield return new WaitForSecondsRealtime(1.5f);
                CheckThat("candy-encounter-producer",game.enemies.Any(e=>e.type.role=="Elite"),"Actual StartWave/AddWaveEncounters/director/birth queue creates an Elite at second one");
                yield return Capture("candy-extra-elite.png");
            }
            ItemEntry Item(string id)=>game.catalog.data.items.Single(i=>i.id==id);
            void StartRun(string character)
            {
                game.playerAnimation.lookIndex=Array.FindIndex(game.catalog.data.characters,c=>c.id==character);Require(game.playerAnimation.lookIndex>=0,"Missing character "+character);
                game.startingWeapon=0;game.StartSelectedRun();game.ClearCombat();game.Rules.Start(game.SelectedCharacter,game.SelectedWeapon?.id,2000);game.arrivalAge=ArrivalMotion.Duration;game.invulnerable=1000;game.selectedWeapon=-1;
                game.director=new SpawnDirector(new WaveEntry{groups=Array.Empty<SpawnGroup>()},771);game.births.Clear();
            }
            void Shop(){game.Rules.Tick(game.Rules.Remaining);while(game.Rules.Phase==RunPhase.Upgrade)game.Rules.Choose(0);Require(game.Rules.Phase==RunPhase.Shop,"Shop staging failed.");game.SyncEquipment();}
            Rect PriceRect(int i)=>new Rect(game.OfferX(i)+(game.OfferWidth-154)/2,451,154,55);
            Rect LockRect(int i)=>new Rect(game.OfferX(i)+12,528,game.OfferWidth-24,43);
            Vector2 ScreenPoint(Vector2 logical)
            {
                float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);var anchor=game.controllerOfferRects[0];
                return anchor.min-new Vector2(game.OfferX(0),118)*scale+logical*scale;
            }
            IEnumerator ActivateWeapon(int weaponIndex)
            {
                yield return Render();Require(weaponIndex>=0&&weaponIndex<game.Rules.Inventory.Count,"Inventory index out of range.");
                // Scroll-view GUIToScreenPoint is not interchangeable with the root
                // canvas transform. Use actual repaint registration order: ShopUI
                // registers inventory tiles immediately before its Go control.
                int go=game.menuNavigationButtons.FindIndex(r=>r.Contains(ScreenPoint(new Rect(game.StatsLeft,686,336,56).center)));
                int index=go-game.Rules.Inventory.Count+weaponIndex;
                Require(go>=game.Rules.Inventory.Count&&index>=0,"Rendered inventory/Go navigation controls missing.");
                report.mappings.Add(new ControlMapping{name="weapon-"+weaponIndex,focus=index,logical=new Rect(game.StatsLeft-290+weaponIndex%3*84,672+weaponIndex/3*86,82,80).ToString(),registered=game.menuNavigationButtons[index].ToString()});
                game.menuNavigationActive=true;game.menuFocus=index;yield return Press(GamepadButton.South);
            }
            IEnumerator Activate(Rect logical)
            {
                yield return Render();var point=ScreenPoint(logical.center);int index=game.menuNavigationButtons.FindIndex(r=>r.Contains(point));
                Require(index>=0,"Rendered enabled control missing at "+logical+"; phase="+game.Rules.Phase+" selected="+game.selectedWeapon);
                game.menuNavigationActive=true;game.menuFocus=index;yield return Press(GamepadButton.South);
            }
            IEnumerator ActivatePopup(int index)
            {
                yield return Render();Require(game.selectedWeapon>=0,"Weapon popup must be open.");
                var weapon=game.Rules.Inventory[game.selectedWeapon];bool combine=game.Rules.CanCombine(game.selectedWeapon);
                Require(game.menuNavigationButtons.Count==(combine?3:2),"Only popup controls may register while weapon actions are open.");
                float visible=Mathf.Min(350,game.RichHeight(game.inspectionText.WeaponDescription(game.Rules,weapon),310));
                float height=128+visible+(combine?126:84),y=Mathf.Max(24,670-height)+122+visible;
                yield return Activate(new Rect(game.FullCanvas.xMax-689,y+index*42,320,34));
            }
            IEnumerator Press(GamepadButton button)
            {InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(button));yield return new WaitForSecondsRealtime(.08f);InputSystem.QueueStateEvent(pad,new GamepadState());yield return new WaitForSecondsRealtime(.14f);}
            IEnumerator Render(){yield return null;yield return new WaitForSecondsRealtime(.08f);}
            IEnumerator Capture(string name)
            {ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));yield return null;yield return new WaitForSecondsRealtime(.15f);Require(File.Exists(Path.Combine(folder,name)),"Missing capture "+name);report.captures.Add(name);}
            void CheckThat(string name,bool passed,string evidence,int before=0,int after=0,int expected=0)
            {report.checks.Add(new Check{name=name,passed=passed,evidence=evidence,before=before,after=after,expected=expected});Require(passed,name);}
            static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        }
    }
}
