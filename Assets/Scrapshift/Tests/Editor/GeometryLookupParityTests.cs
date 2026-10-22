using System;
using NUnit.Framework;
using UnityEngine;

namespace Scrapshift.Tests
{
    public sealed class GeometryLookupParityTests
    {
        [Serializable] sealed class Entry { public string id; public float[] radii,hurt,contact,body,origin; public int bodyLayer; }
        [Serializable] sealed class Table { public Entry[] entries; }
        [Test] public void EveryAuthoredCollisionShapeAndOffsetMatchesTheResource()
        {
            var rows=JsonUtility.FromJson<Table>(Resources.Load<TextAsset>("enemy-collisions").text).entries;
            foreach(var row in rows)
            {
                var first=Array.Find(rows,e=>e.id==row.id);
                var shape=CombatGeometry.Enemy(row.id);
                Assert.AreEqual(first.radii[0],shape.body,row.id);
                Assert.AreEqual(first.radii[1],shape.hurt,row.id);
                Assert.AreEqual(first.radii[2],shape.contact,row.id);
                Assert.AreEqual(new Vector2(first.body[0],first.body[1]),CombatGeometry.BodyOffset(row.id),row.id);
                Assert.AreEqual(new Vector2(first.origin[0],first.origin[1]),CombatGeometry.OriginOffset(row.id),row.id);
                Assert.AreEqual(new Vector2(first.hurt[0],first.hurt[1]),CombatGeometry.HurtOffset(row.id),row.id);
                Assert.AreEqual(new Vector2(first.contact[0],first.contact[1]),CombatGeometry.ContactOffset(row.id),row.id);
                Assert.AreEqual(first.bodyLayer,CombatGeometry.BodyLayer(row.id),row.id);
            }
        }
        [TestCase(null)] [TestCase("unknown_enemy")] public void MissingProfileRetainsFallbacks(string id)
        {
            Assert.AreEqual(0,CombatGeometry.BodyLayer(id));
            Assert.AreEqual(Vector2.down*.15f,CombatGeometry.HurtOffset(id));
            Assert.Throws<ArgumentException>(()=>CombatGeometry.BodyOffset(id));
            Assert.Throws<ArgumentException>(()=>CombatGeometry.OriginOffset(id));
        }
    }
}
