using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public static class AchievementArt
    {
        public static readonly string[] Symbols={"difficulty","survivor","gatherer","rookie","dying","agriculture","hallucination","fast","hoarder","turrets","lumberjack","medicine","perfect_vision","fireworks","recycling","slow","hungry","advanced_technology","giant_slayer","robust","baited","forest","bourgeoisie","student","reckless","scavenger","fast_learner","experimentation","magic_and_machinery","blood_drinker","evil_hat","hourglass","candy_bag","fruit_basket","will_o_the_wisp","ghost_outfit","vorpal_sword","paws_n_claws"};
        static readonly HashSet<string> Pets=new HashSet<string>{"bonk_dog","catling_gun","bot_o_mine","blazemander","doc_moth","lootworm","jellyshield"};
        static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        static WeaponIconMasks masks;
        public static WeaponIconMask Mask(string name)
        {
            if(masks==null)masks=JsonUtility.FromJson<WeaponIconMasks>(Resources.Load<TextAsset>("achievement-icon-masks").text);
            return Array.Find(masks.masks,m=>m.name==name);
        }
        public static string Key(UnlockDefinition challenge)
        {
            if(challenge.id.StartsWith("unlock_difficulty_"))return "difficulty";
            string id=challenge.id.Substring(5);
            if(challenge.description=="CHAL_CHARACTER_DESC")return "character:"+id;
            foreach(var family in new[]{"difficulty","survivor","gatherer"})if(id.StartsWith(family+"_"))return family;
            return id;
        }
        public static bool HasSymbol(string key)=>Array.IndexOf(Symbols,key)>=0||Pets.Contains(key);
        public static Sprite Icon(string key)
        {
            if(Pets.Contains(key))return SupportArt.Pet(key);
            if(sprites.TryGetValue(key,out var existing))return existing;
            int index=Array.IndexOf(Symbols,key);
            if(index<0)throw new ArgumentException("Unmapped achievement icon: "+key);
            bool first=index<24;
            var texture=Resources.Load<Texture2D>(first?"AchievementIconsA":"AchievementIconsB");
            if(texture==null)throw new InvalidOperationException("Missing achievement icon atlas");
            var mask=Mask("Achievement_"+key);
            if(mask==null)throw new InvalidOperationException("Missing achievement bounds: "+key);
            var sprite=Sprite.Create(texture,mask.source.Rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            sprite.name="Achievement_"+key;sprites[key]=sprite;return sprite;
        }
    }
    public sealed partial class ArenaGame
    {
        void DrawAchievementIcon(UnlockDefinition challenge,Rect r)
        {
            string key=AchievementArt.Key(challenge);
            if(key.StartsWith("character:"))
            {
                var character=Array.Find(catalog.data.characters,c=>"character:"+c.id==key);
                if(character==null)throw new InvalidOperationException("Missing achievement character: "+key);
                DrawCodexFace(character,r);
            }
            else DrawIcon(AchievementArt.Icon(key),r);
        }
    }
}

