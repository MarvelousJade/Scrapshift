using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        Mesh mittenMesh;
        readonly MeshRenderer[] weaponHands=new MeshRenderer[24];
        readonly Vector2[] handGrips=new Vector2[24];
        void AddWeaponHand(int index,Equipment weapon)
        {
            weaponHands[index]=null;
            if(weapon.id=="fist"||weapon.id=="hand"||weapon.id=="power_fist"||weapon.id=="claw")return;
            if(mittenMesh==null)
            {
                var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
                void Oval(float radius,float z,Color color)
                {
                    int start=vertices.Count;vertices.Add(new Vector3(0,0,z));colors.Add(color);
                    for(int j=0;j<32;j++){float a=j*Mathf.PI*2/32;vertices.Add(new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.88f,z));colors.Add(color);}
                    for(int j=0;j<32;j++){triangles.Add(start);triangles.Add(start+j+1);triangles.Add(start+(j+1)%32+1);}
                }
                Oval(1,0,new Color(.035f,.03f,.035f));Oval(.73f,-.01f,new Color(1,.92f,.75f));
                mittenMesh=new Mesh{name="Simple cream grip mittens",vertices=vertices.ToArray(),colors=colors.ToArray(),triangles=triangles.ToArray()};mittenMesh.RecalculateBounds();
            }
            var hand=new GameObject("Cream grip mitten");hand.transform.SetParent(equipmentViews[index].transform,false);
            var bounds=weapon.image.bounds;
            handGrips[index]=new Vector2(bounds.center.x+bounds.size.x*(weapon.melee?-.34f:-.22f),bounds.center.y+bounds.size.y*(weapon.melee?0:-.25f));
            hand.transform.localPosition=new Vector3(handGrips[index].x,handGrips[index].y,-.02f);
            hand.transform.localScale=Vector3.one*(.065f/ActorPresentation.WeaponScale(weapon));
            hand.AddComponent<MeshFilter>().sharedMesh=mittenMesh;
            var view=hand.AddComponent<MeshRenderer>();view.sharedMaterial=catalog.spriteMaterial;view.sortingOrder=13;weaponHands[index]=view;
        }
        void PoseWeaponHand(int index,bool flipped,int weaponOrder)
        {
            var hand=weaponHands[index];if(hand==null)return;
            var p=handGrips[index];hand.transform.localPosition=new Vector3(p.x,flipped?-p.y:p.y,-.02f);
            hand.sortingOrder=weaponOrder+1;
        }
    }
}
