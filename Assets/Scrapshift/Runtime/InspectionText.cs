using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    [Serializable] public sealed class UiTextEntry { public string key,value; }
    [Serializable] public sealed class UiTextData { public UiTextEntry[] entries; }
    public sealed class InspectionText
    {
        readonly Dictionary<string,string> strings;
        public InspectionText(TextAsset asset)=>strings=JsonUtility.FromJson<UiTextData>(asset.text).entries.ToDictionary(e=>e.key,e=>e.value);
        public string T(string key,string fallback=null)=>strings.TryGetValue(key,out var value)?value:fallback??key;
        public string Format(string key,params object[] args)=>string.Format(CultureInfo.InvariantCulture,T(key),args);
        static string N(float value)=>value.ToString("0.##",CultureInfo.InvariantCulture);
        public string StatName(string key)=>T("STAT_"+(key.StartsWith("stat_")?key.Substring(5):key).ToUpperInvariant(),ItemEntry.StatName(key));
        public string StatEffect(string key,float value)
        {
            if(!key.StartsWith("stat_")&&SpecialItemEffects.IsSecondary(key))return SpecialItemEffects.Description(key,value);
            string name=StatName(key);bool percent=name.StartsWith("%");
            return (value>0?"+":"")+N(value)+(percent?"%":"")+" "+name.TrimStart('%',' ');
        }
        // Derive fallback from the stable ID: title already contains a tier after editor migration.
        public string WeaponName(Equipment w)=>T("WEAPON_"+(w.id=="cactus_mace"?"CACTI_CLUB":w.id.ToUpperInvariant()),CultureInfo.InvariantCulture.TextInfo.ToTitleCase(w.id.Replace('_',' ')))+(w.tier>1?" "+new[]{"","","II","III","IV"}[w.tier]:"");
        public string ItemName(ItemEntry i)=>CyberneticTheme.ItemName(i.id,T("ITEM_"+i.id.ToUpperInvariant(),i.title));
        public string StatDescription(RunRules rules,string stat)
        {
            float value=stat=="max_hp"?rules.MaxHealth:rules.Stat(stat);
            string key="INFO_"+(value<0?"NEG_":"POS_")+"STAT_"+stat.ToUpperInvariant();
            string number=N(Mathf.Abs(value));
            if(stat=="armor")
            {float coefficient=value>=0?1/(1+value/15):2-1/(1-value/15);return Format(key,N(Mathf.Round(Mathf.Abs(1-coefficient)*100))+"%");}
            if(stat=="hp_regeneration")return Format(key,value>0?"1":"0",value>0?RunRules.RegenerationInterval(value).ToString("0.##",CultureInfo.InvariantCulture):"5",value>0?(1/RunRules.RegenerationInterval(value)).ToString("0.##",CultureInfo.InvariantCulture):"0");
            if(stat=="harvesting")return Format(key+(value>=0?"_LIMITED":""),number,N(5+rules.Stat("harvesting_growth"))+"%","20","10%");
            if(stat=="dodge")return Format(key,number+"%",N(rules.DodgeLimit*100)+"%");
            if(stat=="lifesteal")return Format(key,number+"%","10");
            if(StatPresentation.Unit(stat)=="%"||stat=="luck")number+="%";
            return Format(key,number);
        }
        public string ItemDescription(ItemEntry item)=>string.Join("\n",item.effects.Select(e=>ItemStatRow(e.key,e.value))
            .Concat((item.conditionalEffects??Array.Empty<ConditionalEffect>()).Where(e=>!(e.customKey=="pet"&&e.value==0)).Select(e=>e.HasCustomDescription?"• "+e.Description:e.customKey=="stat_cap"?"• "+(e.key=="hp_cap"?"Max HP":"Speed")+" is capped at its value when you obtain this implant":ItemStatRow(e.key,e.value)+e.Suffix)));
        string ItemStatRow(string key,float value)
        {
            string stat=key.Replace("stat_","");string icon=StatPresentation.Keys.Contains(stat)?"[["+stat+"]] ":"• ";
            string text=StatEffect(key,value),color=SpecialItemEffects.IsPenalty(key,value)?"#FF3333":"#00FF00";
            return icon+System.Text.RegularExpressions.Regex.Replace(text,@"(?<![\w])[+-]?\d+(?:\.\d+)?%?",m=>"<color="+color+">"+m.Value+"</color>");
        }
        public string Sets(Equipment weapon)=>string.Join(", ",WeaponSets.All.Where(s=>weapon.HasSet(s.Id)).Select(s=>s.Name));
        static string Compared(float current,float original,string shown)=>"<color="+(Mathf.Approximately(current,original)?"#FFFFFF":current>original?"#76FF40":"#FF4444")+">"+shown+"</color>";
        public string WeaponDescription(RunRules rules,Equipment w)
        {
            float damage=rules.WeaponDamage(w),range=rules.WeaponRange(w)*100;
            float cd=rules.WeaponCooldown(w)+rules.WeaponAnimationDuration(w);
            float baseCd=w.interval+WeaponMotion.ReferenceDuration(w,0,w.range);
            string scaled=w.scaling==null?"":string.Join(" + ",w.scaling.Select(s=>"+"+N(s.value*100)+"%[["+s.key+"]]"));
            string dmg=Compared(damage,w.damage,N(damage))+(w.pellets>1?"x"+w.pellets:"")+(Mathf.Approximately(damage,w.damage)?"":" ["+N(w.damage)+"]")+(scaled.Length>0?" ("+scaled+")":"");
            var lines=new List<string>{Format("DAMAGE_FORMATTED",T("STAT_DAMAGE"),dmg),
                Format("CRITICAL_FORMATTED",T("CRITICAL"),"x"+N(w.critMultiplier),Compared(Mathf.Clamp01(rules.CritChance+w.crit),w.crit,N(Mathf.Clamp01(rules.CritChance+w.crit)*100))),
                Format("STAT_FORMATTED",T("COOLDOWN"),Compared(-cd,-baseCd,cd.ToString(cd<.2f?"0.000":"0.00",CultureInfo.InvariantCulture)+"s"))};
            if(w.knockback!=0)lines.Add(Format("STAT_FORMATTED",T("KNOCKBACK"),N(w.knockback*10)));
            lines.Add(Format("RANGE_FORMATTED",StatName("range"),Compared(range,w.range*100,N(range))+(Mathf.Approximately(range,w.range*100)?"":" ["+N(w.range*100)+"]"),T(w.melee?"MELEE":"RANGED")));
            if(w.bounce>0)lines.Add(Format("PIERCING_FORMATTED",T("BOUNCE"),w.bounce,N(w.bounceLoss*100)));
            if(w.piercing>0)lines.Add(Format("PIERCING_FORMATTED",T("PIERCING"),w.piercing,N(w.pierceLoss*100)));
            if(w.lifesteal>0)lines.Add(Format("STAT_FORMATTED",T("LIFESTEAL"),N(w.lifesteal*100)+"%"));
            if(w.reloadEvery>0)lines.Add(Format("ADDITIONAL_COOLDOWN_EVERY_X_SHOTS",N(rules.WeaponCooldown(w)*w.reloadMultiplier+rules.WeaponAnimationDuration(w))+"s",w.reloadEvery));
            if(w.traits!=null)foreach(var t in w.traits)
            {
                string effect=null;
                if(!string.IsNullOrEmpty(t.customKey)){lines.Add(StatEffect(t.key,t.value)+t.Conditional.Suffix);continue;}
                switch(t.key)
                {
                    case "effect_burning":effect=Format(t.key.ToUpperInvariant(),N(t.burnDamage),N(t.burnTicks),N(t.burnScale*100)+"%[[elemental_damage]]");break;
                    case "effect_slow_in_zone":effect=T(t.key.ToUpperInvariant());break;
                    case "effect_explode":case "effect_explode_melee":case "effect_explode_custom":effect=N(t.chance*100)+"% chance to explode on hit";break;
                    case "effect_projectiles_on_hit":case "effect_lightning_on_hit":effect=Format(t.key.ToUpperInvariant(),N(t.value),N(rules.WeaponDamage(t.projectile.AsEquipment())),N(t.projectile.bounce),string.Join(" + ",t.projectile.scaling.Select(s=>N(s.value*100)+"%[["+s.key+"]]")));break;
                    case "effect_weapon_stack":effect=Format(t.key.ToUpperInvariant(),"+"+N(t.value),T("STAT_DAMAGE"),WeaponName(w),N(t.value*Mathf.Max(0,rules.Inventory.Count(a=>a.id==w.id)-1)));break;
                    case "effect_gain_stat_every_killed_enemies":effect=Format(t.key.ToUpperInvariant(),"+"+N(t.amount),StatName(t.stat),N(t.value));break;
                    case "turret":effect=Format("EFFECT_TURRET",N(t.projectile.damage),t.projectile.scaling.Length>0?string.Join(" + ",t.projectile.scaling.Select(a=>N(a.value*100)+"% "+StatName(a.key))):"0% Engineering");break;
                    case "gold_on_crit_kill":effect=Format("EFFECT_GOLD_ON_CRIT_KILL",N(t.value)+"%");break;
                    case "bounce_on_crit":effect=Format("EFFECT_BOUNCE_ON_CRIT",N(t.value));break;
                    case "pierce_on_crit":effect=Format("EFFECT_PIERCE_ON_CRIT_WEAPON",N(t.value));break;
                    case "lose_hp_per_second":effect=$"Lose {N(t.value)} HP every second";break;
                    case "effect_no_hit_boost":effect=$"+{N(t.value)} base damage every {N(t.interval)} seconds without taking damage. Resets on damage and at wave end.";break;
                    case "one_shot_on_hit":effect=$"{N(t.value)}% chance to instantly kill on hit";break;
                    case "effect_weapon_slow_on_hit":effect=$"Slows enemies by {N(t.value)}% per Engineering on hit";break;
                    case "effect_slow_projectiles_on_hit":effect=$"Spawns {N(t.value)} slowing lightning projectiles on hit ({N(rules.WeaponDamage(t.projectile.AsEquipment()))} damage)";break;
                    case "burning_spread":effect=$"Burning spreads to {N(t.value)} additional enemies";break;
                    case "garden":effect=$"Spawns a garden that produces fruit about every {N(t.projectile.interval)} seconds";break;
                    case "landmine":effect=$"Spawns a landmine every {N(t.spawnInterval)} seconds ({N(SpecialWeaponRules.StructureDamage(rules,t.projectile.AsEquipment()))} damage)";break;
                    default:if(t.key.StartsWith("stat_"))effect=t.per>0?Format("EFFECT_GAIN_STAT_FOR_EVERY_PERCENT_PLAYER_MISSING_HEALTH","+"+N(t.value),StatName(t.key.Substring(5)),N(t.per)+"%","",N(Mathf.Floor((1-rules.Health/rules.MaxHealth)*100/t.per)*t.value)):"+"+N(t.value)+" "+StatName(t.key.Substring(5));break;
                }
                if(effect!=null)lines.Add("• "+effect);
            }
            return string.Join("\n",lines.Select((line,index)=>index<7&&line.Contains(":")?"<color=#E5DDAA>"+line.Substring(0,line.IndexOf(':')+1)+"</color>"+line.Substring(line.IndexOf(':')+1):line));
        }
    }
}
