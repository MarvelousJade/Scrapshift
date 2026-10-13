using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class SupportParityTests
    {
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

        [Test] public void ArticulatedTurretTurnsWithoutMovingItsBase()
        {
            var root=new GameObject("test turret");root.AddComponent<SpriteRenderer>().sprite=SupportArt.Structure("turret");
            try
            {
                var rig=SupportRig.Create(root,"turret",true);var at=root.transform.position;var scale=root.transform.localScale;
                rig.Aim(Vector2.up,.1f,.1f,false);
                Assert.That(rig.AimingHead.eulerAngles.z,Is.InRange(40,60));
                Assert.AreEqual(at,root.transform.position);Assert.AreEqual(scale,root.transform.localScale);
                rig.Aim(Vector2.up,.1f,.3f,true);Assert.AreEqual(Vector3.one,rig.AimingHead.localScale);
            }
            finally{Object.DestroyImmediate(root);}
        }
        [TestCase("bonk_dog")][TestCase("catling_gun")][TestCase("blazemander")][TestCase("ratzilla")][TestCase("scapegoat")][TestCase("doc_moth")]
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
