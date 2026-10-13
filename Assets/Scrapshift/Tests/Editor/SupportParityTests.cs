using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class SupportParityTests
    {
        [Test] public void EveryAchievementHasExplicitArtIndependentOfItsReward()
        {
            var definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text);
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            foreach(var challenge in definitions.challenges)
            {
                var key=AchievementArt.Key(challenge);
                if(key.StartsWith("character:"))Assert.IsTrue(catalog.data.characters.Any(c=>"character:"+c.id==key),challenge.id);
                else{Assert.IsTrue(AchievementArt.HasSymbol(key),challenge.id);Assert.Greater(AchievementArt.Icon(key).rect.width,20,challenge.id);}
            }
            Assert.AreEqual("gatherer",AchievementArt.Key(definitions.challenges.Single(d=>d.id=="chal_gatherer_2")));
            Assert.AreEqual("character:well_rounded",AchievementArt.Key(definitions.challenges.Single(d=>d.id=="chal_well_rounded")));
            Assert.AreEqual(AchievementArt.Symbols.Length,AchievementArt.Symbols.Select(AchievementArt.Icon).Distinct().Count());
        }

        [Test] public void SerializedCatalogUsesMatchingSupportIconsAndPreservesOtherItems()
        {
            var source=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var catalog=Object.Instantiate(source);
            try
            {
                var before=catalog.data.items.ToDictionary(i=>i.id,i=>i.image);
                SupportArt.Apply(catalog);
                foreach(var item in catalog.data.items)
                {
                    var support=(item.conditionalEffects??System.Array.Empty<ConditionalEffect>()).LastOrDefault(e=>e.customKey=="pet"||e.customKey=="structure");
                    if(item.id=="jellyshield")Assert.AreSame(SupportArt.Pet(item.id),item.image,item.id);
                    else if(support?.customKey=="pet")Assert.AreEqual("Support_"+support.pet.kind,item.image.name,item.id);
                    else if(support?.customKey=="structure")Assert.AreEqual("Support_"+support.structure.kind,item.image.name,item.id);
                    else Assert.AreSame(before[item.id],item.image,item.id+" must retain authored art");
                }
                Assert.AreEqual("Support_ratzilla",catalog.data.items.Single(i=>i.id=="ratzilla").image.name);
            }
            finally{Object.DestroyImmediate(catalog);}
        }

        [Test] public void TurretStaysUprightAndRecoilsAsOneAssembly()
        {
            var root=new GameObject("test turret");root.AddComponent<SpriteRenderer>().sprite=SupportArt.Structure("turret");
            try
            {
                var rig=SupportRig.Create(root,"turret",true);var at=root.transform.position;var scale=root.transform.localScale;
                rig.Aim(Vector2.up,.1f,.1f,false);
                Assert.AreEqual(0,rig.AimingHead.eulerAngles.z);
                Assert.AreEqual(new Vector3(.8f,1.2f,1),rig.AnimatedAssembly.localScale);
                rig.Aim(Vector2.left,.1f,.3f,false);Assert.AreEqual(0,rig.AimingHead.eulerAngles.z);
                Assert.AreEqual(at,root.transform.position);Assert.AreEqual(scale,root.transform.localScale);
                rig.Aim(Vector2.up,.1f,.3f,true);Assert.AreEqual(Vector3.one,rig.AimingHead.localScale);
            }
            finally{Object.DestroyImmediate(root);}
        }
        [TestCase("doc_moth")]
        public void PetLimbsAnimateAndReducedMotionRestoresRestPose(string kind)
        {
            var root=new GameObject("test pet");root.AddComponent<SpriteRenderer>().sprite=SupportArt.Pet(kind);
            try
            {
                var rig=SupportRig.Create(root,kind,false);rig.PetPose(0,true,false,false,false);var first=rig.FirstLimb.localRotation;
                rig.PetPose(.25f,true,false,false,false);Assert.Greater(Quaternion.Angle(first,rig.FirstLimb.localRotation),15);
                rig.PetPose(.25f,true,false,true,false);Assert.Less(Quaternion.Angle(first,rig.FirstLimb.localRotation),.01f);
                Assert.IsFalse(root.GetComponent<SpriteRenderer>().enabled);
            }
            finally{Object.DestroyImmediate(root);}
        }
        [TestCase("bonk_dog")][TestCase("blazemander")][TestCase("ratzilla")][TestCase("scapegoat")]
        [TestCase("bot_o_mine")][TestCase("lootworm")][TestCase("jellyshield")]
        public void CompletePetBodiesKeepTheirAuthoredLimbsAndAnimate(string kind)
        {
            var root=new GameObject(kind);root.AddComponent<SpriteRenderer>().sprite=SupportArt.Pet(kind);
            try
            {
                var rig=SupportRig.Create(root,kind,false);Assert.IsNotNull(rig);Assert.IsNull(rig.FirstLimb);
                Assert.AreSame(SupportArt.Pet(kind),rig.Body.GetComponent<SpriteRenderer>().sprite);
                rig.PetPose(.5f,true,false,false,false);Assert.Greater(Quaternion.Angle(Quaternion.identity,rig.Body.localRotation),1);
                rig.PetPose(.5f,true,true,true,false);Assert.AreEqual(Quaternion.identity,rig.Body.localRotation);
                Assert.AreEqual(Vector3.zero,rig.AnimatedAssembly.localPosition);
            }
            finally{Object.DestroyImmediate(root);}
        }
        [Test] public void MothHoverMovesWingsAndBodyTogether()
        {
            var root=new GameObject("moth");root.AddComponent<SpriteRenderer>().sprite=SupportArt.Pet("doc_moth");
            try
            {
                var rig=SupportRig.Create(root,"doc_moth",false);var bodyAt=rig.Body.localPosition;var wingAt=rig.FirstLimb.localPosition;
                rig.PetPose(1,true,false,false,false);
                Assert.AreEqual(bodyAt,rig.Body.localPosition);Assert.AreEqual(wingAt,rig.FirstLimb.localPosition);
                Assert.Greater(rig.AnimatedAssembly.localPosition.y,0);
            }
            finally{Object.DestroyImmediate(root);}
        }
        [Test] public void CatUsesConnectedBodySwayWithoutDetachedLegs()
        {
            var root=new GameObject("cat");root.AddComponent<SpriteRenderer>().sprite=SupportArt.Pet("catling_gun");
            try
            {
                var rig=SupportRig.Create(root,"catling_gun",false);Assert.IsNull(rig.FirstLimb);
                var body=rig.AnimatedAssembly.Find("Pet body");var at=body.localPosition;
                rig.PetPose(.5f,true,false,false,false);Assert.Greater(Quaternion.Angle(Quaternion.identity,body.localRotation),1);
                Assert.AreEqual(at,body.localPosition);
                rig.PetPose(.5f,true,false,true,false);Assert.AreEqual(Quaternion.identity,body.localRotation);
            }
            finally{Object.DestroyImmediate(root);}
        }
        [Test] public void CatGunsAimIndependentlyEvenWhenBodyMirrors()
        {
            var root=new GameObject("cat guns");root.AddComponent<SpriteRenderer>().sprite=SupportArt.Pet("catling_gun");
            try
            {
                var rig=SupportRig.Create(root,"catling_gun",false);rig.ConfigureCatGuns(SupportRig.GatlingGun());
                foreach(bool flip in new[]{false,true})
                {
                    rig.PetPose(0,false,flip,false,false);rig.AimGun(0,Vector2.up);rig.AimGun(1,Vector2.left);
                    var a=rig.Body.Find("Independent gun 0");var b=rig.Body.Find("Independent gun 1");
                    Assert.Greater(Vector2.Dot(a.TransformVector(Vector3.right).normalized,Vector2.up),.999f);
                    Assert.Greater(Vector2.Dot(b.TransformVector(Vector3.right).normalized,Vector2.left),.999f);
                    Assert.AreEqual(Quaternion.identity,rig.AnimatedAssembly.Find("Pet body").localRotation);
                }
                Assert.AreEqual("CatBodyV2",rig.AnimatedAssembly.Find("Pet body").GetComponent<SpriteRenderer>().sprite.texture.name);
            }
            finally{Object.DestroyImmediate(root);}
        }
        [Test] public void CatBackMountsFollowSwayAndVisibleGunsTurnThroughAllDirections()
        {
            var root=new GameObject("back mounts");root.AddComponent<SpriteRenderer>().sprite=SupportArt.Pet("catling_gun");
            try
            {
                var rig=SupportRig.Create(root,"catling_gun",false);rig.ConfigureCatGuns(SupportRig.GatlingGun());
                root.transform.localScale=new Vector3(1.12f,.88f,1);
                foreach(bool flip in new[]{false,true})foreach(var direction in new[]{Vector2.right,Vector2.up,Vector2.left,Vector2.down})
                {
                    rig.PetPose(.5f,true,flip,false,false);
                    for(int i=0;i<2;i++)
                    {
                        var pivot=rig.Body.Find("Independent gun "+i);Assert.Greater(pivot.localPosition.y,0);
                        var mount=pivot.position;rig.AimGun(i,direction);rig.GunKick(i);rig.TickGunRecoil(.01f,false);
                        Assert.AreEqual(mount,pivot.position);
                        Assert.Greater(Vector2.Dot(pivot.TransformVector(Vector3.right).normalized,direction),.999f);
                        var view=pivot.GetComponentInChildren<SpriteRenderer>();
                        bool forward=rig.Body.InverseTransformVector(direction).x>=0;
                        Assert.AreEqual((i==0)==forward?9:6,view.sortingOrder);
                    }
                }
            }
            finally{Object.DestroyImmediate(root);}
        }
        [Test] public void CatStepsAlternateWithoutMovingHipAnchors()
        {
            var far=CatLegGait.StepOffset(.2f,.05f,.25f,true);
            var near=CatLegGait.StepOffset(.6f,.05f,.25f,true);
            Assert.Less(far.x,0);Assert.Greater(near.x,0);
            Assert.AreEqual(Vector2.zero,CatLegGait.StepOffset(.6f,.3f,.25f,true));
            Assert.AreEqual(Vector2.zero,CatLegGait.StepOffset(.6f,.05f,.25f,false));
            Assert.Less(CatLegGait.StepOffset(.6f,.05f,.75f,true).x,0);
        }
        [Test] public void ReferenceTargetReservationAndJumpRules()
        {
            Assert.IsFalse(SupportBehavior.MayReserve("ratzilla",true,false,false));
            Assert.IsFalse(SupportBehavior.MayReserve("bonk_dog",false,false,true));
            Assert.IsTrue(SupportBehavior.MayReserve("bonk_dog",true,false,true));
            Assert.IsTrue(SupportBehavior.MayReserve("blazemander",true,true,true));
            Assert.IsFalse(SupportBehavior.MayReserve("blazemander",true,false,true));
            Assert.AreEqual(.5f,SupportBehavior.JumpProgress(.5f));
            Assert.Less(SupportBehavior.JumpProgress(.25f),.05f);
            Assert.Greater(SupportBehavior.JumpProgress(.75f),.95f);
            Assert.AreEqual(.25f,SupportBehavior.CatInterval(.5f,true));
        }
        [Test] public void TurretKeyframesReturnToRestAndPetsUseDifferentMovementStrengths()
        {
            Assert.AreEqual(new Vector2(1.1f,.9f),SupportMotion.TurretScale(0));
            Assert.AreEqual(new Vector2(.8f,1.2f),SupportMotion.TurretScale(.1f));
            Assert.AreEqual(new Vector2(1.2f,.8f),SupportMotion.TurretScale(.2f));
            Assert.AreEqual(Vector2.one,SupportMotion.TurretScale(.3f));
            Assert.AreEqual(0,SupportMotion.TurretLift(.3f));
            Assert.AreEqual(new Vector2(1.2f,.8f),SupportMotion.PetScale("ratzilla",.5f,true,0));
            Assert.AreEqual(new Vector2(1.15f,.85f),SupportMotion.PetScale("scapegoat",.5f,true,0));
            Assert.Less(SupportMotion.PetScale("ratzilla",.5f,false,0).x,SupportMotion.PetScale("ratzilla",.5f,true,0).x);
        }
        [Test] public void ManualPressModeWorksWithoutAlwaysOnAndReleasesToAuto()
        {
            Assert.IsTrue(CombatInput.ManualAim(false,true,true));
            Assert.IsFalse(CombatInput.ManualAim(true,true,false));
            Assert.IsTrue(CombatInput.ManualAim(true,false,false));
            Assert.IsFalse(CombatInput.ManualAim(false,false,true));
        }
        [Test] public void AllSupportActorsHaveSeparateNonemptyTransparentSprites()
        {
            var sprites=new[]{"turret","flame","rocket","laser","healing","landmine","garden","tyler","wandering_bot"}.Select(SupportArt.Structure)
                .Concat(new[]{"bonk_dog","catling_gun","bot_o_mine","blazemander","ratzilla","doc_moth","lootworm","scapegoat","jellyshield"}.Select(SupportArt.Pet)).ToArray();
            Assert.AreEqual(18,sprites.Distinct().Count());
            foreach(var sprite in sprites){Assert.Greater(sprite.rect.width,40);Assert.Greater(sprite.rect.height,40);Assert.AreEqual("SupportAtlas",sprite.texture.name);}
        }
        [Test] public void WaveSixPricesAndRerollModifierMatchReference()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var r=new RunRules(42);r.Configure(catalog);r.Start();r.Collect(1000);
            for(int wave=1;wave<=6;wave++)
            {
                r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);
                if(wave<6)r.NextWave();
            }
            Assert.AreEqual(6,r.Wave);Assert.AreEqual(38,r.Price(20));
            foreach(int cost in new[]{6,8,10}){Assert.AreEqual(cost,r.RerollPrice);int before=r.Scrap;r.Reroll();Assert.AreEqual(before-cost,r.Scrap);}
            r.GainWeaponStat("reroll_price",50);Assert.AreEqual(18,r.RerollPrice);
            r.GainWeaponStat("reroll_price",-250);Assert.AreEqual(2,r.RerollPrice);
        }
        [Test] public void UniqueWeaponShopRejectsDowngradesAndFinalTierDuplicates()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var rules=new RunRules(42);rules.Configure(catalog);rules.Start(catalog.data.characters.First(c=>c.id=="well_rounded"),"knife");
            rules.GainWeaponStat("no_duplicate_weapons",1);rules.Inventory.Clear();rules.Inventory.Add(catalog.Weapon("knife",3));
            Assert.IsFalse(rules.CanOfferWeapon(catalog.Weapon("knife",2)));
            Assert.IsTrue(rules.CanOfferWeapon(catalog.Weapon("knife",3)));
            Assert.IsTrue(rules.CanOfferWeapon(catalog.Weapon("knife",4)));
            rules.Inventory.Clear();rules.Inventory.Add(catalog.Weapon("knife",4));
            Assert.IsFalse(rules.CanOfferWeapon(catalog.Weapon("knife",4)));
        }
    }
}
