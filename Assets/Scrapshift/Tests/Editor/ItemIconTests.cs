using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Scrapshift.Tests
{
    public sealed class ItemIconTests
    {
        const string CatalogPath="Assets/Scrapshift/Content/Definitions/ReferenceCatalog.asset";
        [Test] public void AllItemsHaveIndividualNonPlaceholderSprites()
        {
            var catalog=Object.Instantiate(AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(CatalogPath));
            try
            {
                SupportArt.Apply(catalog);
                Assert.AreEqual(209,catalog.data.items.Length);
                foreach(var item in catalog.data.items){Assert.IsNotNull(item.image,item.id);Assert.IsFalse(item.image.name.StartsWith("stats_"),item.id);}
                Assert.AreEqual(209,catalog.data.items.Select(i=>i.image.texture.name+":"+i.image.rect).Distinct().Count());
            }
            finally{Object.DestroyImmediate(catalog);}
        }
        [Test] public void PiercingStabilizerUsesItsOwnArtworkAndEffectIcon()
        {
            var c=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(CatalogPath);var item=c.data.items.Single(i=>i.id=="pumpkin");
            Assert.AreEqual("Item_pumpkin",item.image.name);Assert.AreSame(item.image,StatPresentation.Icon(c,"piercing_damage"));
            StringAssert.Contains("[[piercing_damage]]",new InspectionText(c.uiText).ItemDescription(item));
        }
    }
}
