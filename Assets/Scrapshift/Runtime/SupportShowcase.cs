using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        // Explicit local preview only; command-line fixtures use isolated progression.
        bool supportShowcase;
        float showcaseSpawnClock;
        void BeginSupportShowcase()
        {
            loadoutShowcase=-1;showcaseCleanFrame=false;
            supportShowcase=true;pauseOptions=pauseCodex=false;
            Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();SyncEquipment();
            reducedMotion=false;player.SetActive(true);showcaseSpawnClock=0;invulnerable=10;
            foreach(var item in catalog.data.items)
                if(item.conditionalEffects!=null&&item.conditionalEffects.Any(e=>(e.customKey=="pet"&&e.pet!=null)||(e.customKey=="structure"&&e.structure!=null)))Rules.Items.Add(item);
            ResetStructures();
            if(!pets.Any(p=>p.spec.kind=="jellyshield"))SpawnPet(new PetSpec{kind="jellyshield",health=1,zone=1});
            for(int i=0;i<starterTurrets.Count;i++)
                starterTurrets[i].pos=new Vector2((i%4-1.5f)*2.4f,2.5f-i/4*2.5f);
            for(int i=0;i<pets.Count;i++)
            {pets[i].pos=new Vector2((i%5-2)*1.4f,-2-i/5);pets[i].destination=playerPos;}
            Rules.PetCount=pets.Count;
            Spawn(catalog.Enemy("tree"),new Vector2(-5,-2));
            for(int i=0;i<10;i++)Drop(new Vector2(-5+i*.5f,-3),1,false);
            Debug.Log($"Support showcase ready: {pets.Count} pets, {starterTurrets.Count} structures, {mineSpawners.Count} mine spawners. WASD/arrows move; Esc pause; F5 reset. No wave end or player damage.");
        }
        void TickSupportShowcase(float dt)
        {
            invulnerable=10;showcaseSpawnClock-=dt;
            if(showcaseSpawnClock>0||enemies.Count>=14)return;
            showcaseSpawnClock=.8f;
            float angle=elapsed*.65f;
            Spawn(catalog.Enemy("baby_alien"),new Vector2(Mathf.Cos(angle)*6,Mathf.Sin(angle)*4));
        }
        void DrawSupportShowcase()
        {
            if(!supportShowcase||Rules.Phase!=RunPhase.Playing)return;
            RoundedBox(new Rect(260,750,920,45),new Color(0,0,0,.8f),8);
            Text(270,750,900,45,"WASD: Move · Esc: Pause · F5: Reset · Invulnerable",center);
        }
    }
}
