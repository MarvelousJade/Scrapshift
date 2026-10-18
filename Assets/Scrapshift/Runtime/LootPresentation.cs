using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        int cratesSpawned;
        bool collectingWaveRemainder;
        Texture2D cyberCrateTexture;
        Sprite cyberCrateSprite;
        Texture2D CrateTexture => cyberCrateTexture != null ? cyberCrateTexture : (cyberCrateTexture=Resources.Load<Texture2D>("CyberCrate"));
        void RouteInstantMaterialCollector(Pickup pickup)
        {
            if(pickup.fruit||!pickup.motion.magnetized||pickup.petCollector!=null||pickup.enemyCollector!=null)return;
            // entity_spawner.lootworms[player] stores the last spawned worm.
            // Instant Gold attraction prefers that companion over the player.
            var worm=pets.LastOrDefault(p=>p.spec?.kind=="lootworm"&&!p.dead&&p.obj!=null);
            if(worm==null)return;
            pickup.petCollector=worm;pickup.motion.magnetized=false;
        }
        void DrawCrateIcon(Rect r)
        {
            GUI.DrawTexture(r,CrateTexture,ScaleMode.ScaleToFit,true);
        }
        GameObject CrateObject(Vector2 position, bool legendary)
        {
            if(cyberCrateSprite==null)
                cyberCrateSprite=Sprite.Create(CrateTexture,new Rect(0,0,CrateTexture.width,CrateTexture.height),new Vector2(.5f,.5f),100);
            var obj=new GameObject(legendary?"Legendary cyber crate":"Cyber crate");
            obj.transform.position=new Vector3(position.x,position.y,-1.5f);
            var art=new GameObject("Crate artwork");art.transform.SetParent(obj.transform,false);
            var renderer=art.AddComponent<SpriteRenderer>();renderer.sprite=cyberCrateSprite;renderer.sortingOrder=4;
            art.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(cyberCrateSprite,.64f);
            if(legendary)renderer.color=new Color(1,.65f,.4f);
            return obj;
        }

        void DropConsumable(Vector2 position, ConsumableKind kind,float spread=0)
        {
            if(kind==ConsumableKind.None)return;
            bool crate=kind!=ConsumableKind.Fruit;
            if(crate)cratesSpawned++;
            var obj=crate?CrateObject(position,kind==ConsumableKind.LegendaryCrate):SpriteObject("Repair capsule",catalog.repairPickup,position,3);
            if(!crate)obj.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(catalog.repairPickup,.48f);
            else obj.transform.localScale=Vector3.one*(.85f/.64f);
            var motion=ScrapMotion.CreateDrop(position,PickupPhysicsRate,spread);
            motion.verticalSpeed=3;motion.height=.06f;motion.angle=crate?0:UnityEngine.Random.Range(0,360);motion.spin=crate?0:90;
            pickups.Add(new Pickup {obj=obj,fruit=true,consumable=kind,motion=motion});
        }

        void CollectPickup(Pickup pickup)
        {
            int levelBefore=Rules.Level;float healthBefore=Rules.Health;
            if(pickup.fruit){Rules.CollectConsumable(pickup.consumable,waveSweeping||collectingWaveRemainder);if(!waveSweeping&&!collectingWaveRemainder)QueueImplantExplosions("explode_on_consumable",pickup.motion.position);}
            else Rules.Collect(pickup.value);
            float healed=Rules.Health-healthBefore;
            if(healed>0)FloatText(playerPos,$"+{healed:0} HP",mint,true);
            if(pickup.consumable==ConsumableKind.Crate || pickup.consumable==ConsumableKind.LegendaryCrate)
            { FloatText(playerPos,"ITEM CRATE!",amber,true);sound.Play(SoundCue.Install); }
            else sound.Play(pickup.fruit?SoundCue.Consume:SoundCue.Pickup,pickup.motion.position);
            PickupContactEffect();
            if(Rules.Level>levelBefore){sound.Play(SoundCue.LevelUp);FloatText(playerPos,"LEVEL UP!",mint,true);}
        }

        void FinishWavePickups()
        {
            Rules.RecordWaveEnd(Rules.LivingTrees,pickups.Where(p=>!p.fruit).Sum(p=>p.value));
            // Source cleanup collects harmless consumables and banks unclaimed materials.
            // Do this while Playing, before RunRules selects the first reward phase.
            collectingWaveRemainder=true;
            try
            {
                foreach(var pickup in pickups)
                    if(pickup.fruit)CollectPickup(pickup);else Rules.BankMaterials(pickup.value);
            }
            finally{collectingWaveRemainder=false;}
            foreach(var pickup in pickups)Destroy(pickup.obj);
            pickups.Clear();
        }

        void ResolveCrate(bool take)
        {
            if(!Rules.ResolveCrate(take))return;
            sound.Play(take?SoundCue.Install:SoundCue.Pickup);
        }

        void LootUI()
        {
            Overlay();DrawHud();DrawStatsPanel();
            OutlinedText(24,100,1062,64,"Item found!",new GUIStyle(number){alignment=TextAnchor.MiddleCenter});
            var item=Rules.CrateItem;if(item==null)return;
            var r=new Rect(335,208,440,360);RoundedBox(r,Color.black,10);
            DrawIcon(item.image,new Rect(r.x+18,r.y+20,78,78));
            Text(r.x+112,r.y+20,305,60,inspectionText.ItemName(item),new GUIStyle(inspectionTitle){normal={textColor=TierColor(item.tier)}});
            Text(r.x+112,r.y+85,305,28,"Item",inspectionBody);
            RichText(inspectionText.ItemDescription(item),new Rect(r.x+25,r.y+138,390,200),18);
            if(MenuButton(new Rect(335,586,440,56),"Take",defaultFocus:true))ResolveCrate(true);
            if(Button(new Rect(335,654,440,56),$"Recycle (+{Rules.CrateRecycleValue})"))ResolveCrate(false);
            DrawSetTooltip();
        }
    }
}
