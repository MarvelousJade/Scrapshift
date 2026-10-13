using UnityEngine;
namespace Scrapshift
{
    public static class MapThemes
    {
        public static readonly string[] Names={"Salvage Yard","Rust Flats","Overgrown Relay","Cryo Depot","Ash Foundry","Neon Drain"};
        static readonly string[] Floors={"#766955","#795B48","#566250","#687880","#544D49","#60566E"};
        static readonly string[] Accents={"#626C46","#9B7753","#718252","#91A4AE","#886346","#737C83"};
        public static Color Floor(int index){ColorUtility.TryParseHtmlString(Floors[Mathf.Clamp(index,0,5)],out var c);return c;}
        public static Color Accent(int index){ColorUtility.TryParseHtmlString(Accents[Mathf.Clamp(index,0,5)],out var c);return c;}
    }
    public sealed partial class ArenaGame
    {
        int selectedMap,mapChoice;
        bool mapPicker;
        string MapChoiceName=>mapChoice<0?"Random":MapThemes.Names[mapChoice];
        void ChooseMap(int choice)
        {
            mapChoice=Mathf.Clamp(choice,-1,MapThemes.Names.Length-1);
            GameSettings.Set("mapTheme",mapChoice);
            ApplyMapTheme(mapChoice<0?new System.Random().Next(MapThemes.Names.Length):mapChoice);
        }
        void DrawMapOption(ref float y)
        {
            RoundedBox(new Rect(0,y,940,65),new Color(0,0,0,.82f),8);
            Text(15,y+6,430,45,"Background",settingsLabel);
            if(SettingsButton(new Rect(495,y+7,428,51),MapChoiceName+"  \u25BE"))mapPicker=!mapPicker;
            y+=74;
            if(!mapPicker)return;
            for(int choice=-1;choice<MapThemes.Names.Length;choice++)
            {
                if(SettingsButton(new Rect(495,y,428,51),choice<0?"Random":MapThemes.Names[choice],choice==mapChoice)){ChooseMap(choice);mapPicker=false;}
                y+=56;
            }
        }
        void ApplyMapTheme(int index)
        {
            selectedMap=Mathf.Clamp(index,0,MapThemes.Names.Length-1);
            var yard=GameObject.Find("Deck art");if(yard==null)return;
            var floor=yard.transform.Find("Soft yard floor");
            if(floor!=null)
            {
                var block=new MaterialPropertyBlock();block.SetColor("_Color",MapThemes.Floor(selectedMap));
                floor.GetComponent<Renderer>().SetPropertyBlock(block);
            }
            var old=yard.transform.Find("Quiet grass and pebble clusters");
            if(old!=null){old.gameObject.SetActive(false);old.name="Retired ground details";Destroy(old.GetComponent<MeshFilter>().sharedMesh);Destroy(old.gameObject);}
            AddYardDetails(yard.transform);
            if(menuGround!=null){menuGround.Release();Destroy(menuGround);menuGround=null;}
        }
    }
}
