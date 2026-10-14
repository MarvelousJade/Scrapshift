using System;
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
                new[]{"glasses","leather_vest","bat","injection","defective_steroids"}),
            new ShowcaseLoadout("Auto Turret Engineer",2,4,
                new[]{"screwdriver","wrench","rail_gun","laser_gun","chain_gun","rocket_launcher"},
                new[]{"helmet","glasses","leather_vest","injection","turret","turret_flame","turret_laser","turret_rocket","turret_healing","garden"}),
            new ShowcaseLoadout("Druid Companion",1,2,
                new[]{"pruner","scythe","wand","icicle","torch","slingshot"},
                new[]{"fertilizer","goat_skull","bat","leather_vest","blazemander","bonk_dog","doc_moth","catling_gun","ratzilla","lootworm"})
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

    public sealed partial class ArenaGame
    {
        int loadoutShowcase=-1;
        bool showcaseCleanFrame;

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
            if(index==2)
                for(int i=0;i<starterTurrets.Count;i++)
                    starterTurrets[i].pos=playerPos+new Vector2((i%3-1)*2.5f,2.3f-i/3*2.2f);
            if(index==3)
                for(int i=0;i<pets.Count;i++)
                {pets[i].pos=playerPos+new Vector2((i%3-1)*1.4f,-.7f-i/3*1.1f);pets[i].destination=playerPos;}
            Rules.PetCount=pets.Count;
            player.SetActive(true);invulnerable=10;
            gameCamera.orthographicSize=4.5f;
            music.StartOnce();
            Debug.Log($"Showcase: {setup.name}, real wave {Rules.Wave}, {Rules.Inventory.Count} weapons, {pets.Count} pets, {starterTurrets.Count} structures, {ShowcaseLoadouts.DamageMultiplier}x damage.");
        }

    }
}
