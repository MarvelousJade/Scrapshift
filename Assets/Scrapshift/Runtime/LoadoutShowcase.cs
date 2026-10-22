using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed class ShowcaseLoadout
    {
        public readonly string name;
        public readonly string[] weapons, items;
        public readonly int look, map;
        public ShowcaseLoadout(string name, int look, int map, string[] weapons, string[] items)
        { this.name=name;this.look=look;this.map=map;this.weapons=weapons;this.items=items; }
    }

    public static class ShowcaseLoadouts
    {
        public const int Wave=19;
        public const float DamageMultiplier=2f;
        public static readonly ShowcaseLoadout[] All =
        {
            new ShowcaseLoadout("Martial Monk",1,1,
                new[]{"fist","fighting_stick","flaming_knuckles","power_fist","lightning_shiv","thunder_sword"},
                new[]{"blindfold","leather_vest","head_injury","blood_leech"}),
            new ShowcaseLoadout("Cyberpunk Ranger",2,5,
                new[]{"laser_gun","gatling_laser","rail_gun","minigun","rocket_launcher","sniper_gun"},
                new[]{"glasses","leather_vest","bat","injection","bandana","bandana","ricochet","ricochet","pumpkin","coffee","coffee","coffee","small_magazine","small_magazine","banner"}),
            new ShowcaseLoadout("Auto Turret Engineer",2,4,
                new[]{"screwdriver","wrench","rail_gun","laser_gun","chain_gun","rocket_launcher"},
                new[]{"helmet","glasses","leather_vest","injection","turret","turret","turret_flame","turret_laser","turret_laser","turret_rocket","turret_rocket","turret_healing","garden","tyler","clockwork_wasp","clockwork_wasp","clockwork_wasp","improved_tools","coffee","coffee","diploma","pile_of_books","bandana","ricochet","pumpkin","catling_gun","bot_o_mine","bonk_dog"}),
            new ShowcaseLoadout("Druid Companion",1,2,
                new[]{"pruner","scythe","wand","icicle","torch","slingshot"},
                new[]{"fertilizer","goat_skull","bat","leather_vest","blazemander","blazemander","bonk_dog","bonk_dog","doc_moth","catling_gun","catling_gun","ratzilla","lootworm","bot_o_mine","jellyshield","scapegoat","clockwork_wasp","clockwork_wasp","clockwork_wasp","diploma","pile_of_books"})
        };

        public static int Index(string name)
        {
            if(string.IsNullOrEmpty(name))return -1;
            if(name.Equals("monk",StringComparison.OrdinalIgnoreCase))return 0;
            if(name.Equals("cyber",StringComparison.OrdinalIgnoreCase)||name.Equals("cyberpunk",StringComparison.OrdinalIgnoreCase))return 1;
            if(name.Equals("turret",StringComparison.OrdinalIgnoreCase)||name.Equals("engineer",StringComparison.OrdinalIgnoreCase))return 2;
            if(name.Equals("druid",StringComparison.OrdinalIgnoreCase)||name.Equals("pet",StringComparison.OrdinalIgnoreCase))return 3;
            return -1;
        }
    }

    public static class ShowcaseSupportTiming
    {
        // StructureTimer defaults to one second; EntitySpawner releases queued
        // helpers once every three 60 Hz physics frames.
        public const float First=1f+3f/60f;
        public const float Interval=3f/60f;
        public static int ReleasedBy(float elapsed)=>elapsed+0.000001f<First?0:1+Mathf.FloorToInt((elapsed-First+0.000001f)/Interval);
    }

    public sealed partial class ArenaGame
    {
        int loadoutShowcase=-1;
        bool showcaseCleanFrame;
        readonly Queue<StructureSpec> queuedShowcaseStructures=new Queue<StructureSpec>();
        readonly Queue<PetSpec> queuedShowcasePets=new Queue<PetSpec>();
        float showcaseSupportElapsed;
        int showcaseSupportReleased;

        void ClearShowcaseSupportQueue()
        {queuedShowcaseStructures.Clear();queuedShowcasePets.Clear();showcaseSupportElapsed=0;showcaseSupportReleased=0;}
        void QueueShowcaseStructure(StructureSpec spec)=>queuedShowcaseStructures.Enqueue(spec);
        void QueueShowcasePet(PetSpec spec)=>queuedShowcasePets.Enqueue(spec);
        void TickShowcaseSupportQueue(float dt)
        {
            if(queuedShowcaseStructures.Count==0&&queuedShowcasePets.Count==0)return;
            showcaseSupportElapsed+=dt;
            int due=ShowcaseSupportTiming.ReleasedBy(showcaseSupportElapsed);
            while(showcaseSupportReleased<due&&(queuedShowcaseStructures.Count>0||queuedShowcasePets.Count>0))
            {
                // The source releases one structure and one pet every three physics frames.
                if(queuedShowcaseStructures.Count>0)
                {var spec=queuedShowcaseStructures.Dequeue();DeployStructure(spec,StructureImage(spec.kind));}
                if(queuedShowcasePets.Count>0)SpawnPet(queuedShowcasePets.Dequeue());
                Rules.PetCount=pets.Count;
                showcaseSupportReleased++;
                if(queuedShowcaseStructures.Count==0&&queuedShowcasePets.Count==0)
                    Debug.Log($"Showcase helpers deployed: {starterTurrets.Count} structures, {pets.Count} pets after {showcaseSupportElapsed:0.00}s.");
            }
        }

        void BeginLoadoutShowcase(int index)
        {
            if(index<0||index>=ShowcaseLoadouts.All.Length)return;
            var setup=ShowcaseLoadouts.All[index];
            supportShowcase=false;pauseOptions=pauseCodex=false;
            Rules.Progress=null;
            Begin();
            ClearCombat();
            loadoutShowcase=index;showcaseCleanFrame=false;
            Rules.Inventory.Clear();
            foreach(var id in setup.weapons)
            {
                var weapon=catalog.data.equipment.Where(w=>w.id==id).OrderByDescending(w=>w.tier).FirstOrDefault();
                if(weapon==null)throw new InvalidOperationException("Missing showcase weapon: "+id);
                Rules.Inventory.Add(weapon);
            }
            foreach(var id in setup.items)
            {
                var item=Array.Find(catalog.data.items,i=>i.id==id);
                if(item==null)throw new InvalidOperationException("Missing showcase item: "+id);
                Rules.GrantItem(item);
            }
            Rules.SetShowcaseWave(ShowcaseLoadouts.Wave);
            Rules.Heal(Rules.MaxHealth);
            ChangeLook(setup.look-playerAnimation.lookIndex);
            ApplyMapTheme(setup.map);
            StartWave();
            player.SetActive(true);invulnerable=10;
            gameCamera.orthographicSize=4.5f;
            music.StartOnce();
            Debug.Log($"Showcase: {setup.name}, real wave {Rules.Wave}, {Rules.Inventory.Count} weapons, {queuedShowcasePets.Count} queued pets, {queuedShowcaseStructures.Count} queued structures, {ShowcaseLoadouts.DamageMultiplier}x damage.");
        }

    }
}
