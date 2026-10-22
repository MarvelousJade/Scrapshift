using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class HeadlessShopAcceptanceTests
    {
        [TestCaseSource(typeof(HeadlessShopAcceptance),nameof(HeadlessShopAcceptance.Cases))]
        public void SourceDerivedShopActionBoundaries(string name)
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            Assert.IsNotEmpty(HeadlessShopAcceptance.Execute(catalog,name));
        }
    }
}
