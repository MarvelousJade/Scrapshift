using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Scrapshift
{
    // IMGUI has no inline sprite tag. Lay out words, colors and icons together so wrapping
    // uses the same measurements in cards and tooltips, without fragile space padding.
    public sealed partial class ArenaGame
    {
        float RichText(string text,Rect area,int fontSize=19,bool draw=true)
        {
            fontSize=Mathf.RoundToInt(fontSize*GameSettings.Value("fontScale"));
            var style=new GUIStyle(inspectionBody){fontSize=fontSize,richText=false,wordWrap=false,alignment=TextAnchor.MiddleLeft,padding=new RectOffset()};
            var colors=new Stack<Color>();colors.Push(Color.white);var sizes=new Stack<int>();sizes.Push(fontSize);
            float x=0,y=0,line=fontSize*1.22f;
            foreach(Match match in Regex.Matches(text??"",@"<[^>]+>|\[\[([^\]]+)\]\]|\r?\n|[^\S\r\n]+|[^\s<\[]+|[\[<]"))
            {
                string token=match.Value;
                if(token.StartsWith("<color=")){if(ColorUtility.TryParseHtmlString(token.Substring(7,token.Length-8),out var c))colors.Push(GameSettings.TextColor(c));continue;}
                if(token=="</color>"){if(colors.Count>1)colors.Pop();continue;}
                if(token.StartsWith("<size=")){if(int.TryParse(token.Substring(6,token.Length-7),out int s))sizes.Push(s);continue;}
                if(token=="</size>"){if(sizes.Count>1)sizes.Pop();continue;}
                if(token.StartsWith("<"))continue;
                style.fontSize=sizes.Peek();style.normal.textColor=colors.Peek();
                if(token.Contains("\n")){x=0;y+=line;line=fontSize*1.22f;continue;}
                if(token.StartsWith("[[")&&!GameSettings.Flag("effectIcons"))token=ItemEntry.StatName(token.Substring(2,token.Length-4));
                bool icon=token.StartsWith("[["),bullet=token=="•";float iconSize=style.fontSize*(x==0?1.55f:1.1f);
                float w=icon?iconSize+5:bullet?style.fontSize*.65f:style.CalcSize(new GUIContent(token)).x;
                if(x+w>area.width&&x>0){x=0;y+=line;line=fontSize*1.22f;}
                if(x==0&&string.IsNullOrWhiteSpace(token))continue;
                line=Mathf.Max(line,icon?iconSize+3:style.fontSize*1.22f);
                if(draw)
                {
                    if(icon){string key=token.Substring(2,token.Length-4);var sprite=StatPresentation.Icon(catalog,key);if(key=="levels")DrawHeroFace(new Rect(area.x+x,area.y+y+(line-iconSize)/2,iconSize,iconSize));else if(sprite!=null)DrawIcon(sprite,new Rect(area.x+x,area.y+y+(line-iconSize)/2,iconSize,iconSize));else GUI.Label(new Rect(area.x+x,area.y+y,w,line),"•",style);}
                    else if(bullet){float diameter=style.fontSize*.32f;RoundedBox(new Rect(area.x+x+2,area.y+y+(line-diameter)/2,diameter,diameter),colors.Peek(),diameter/2);}
                    else GUI.Label(new Rect(area.x+x,area.y+y,w+2,line),token,style);
                }
                x+=w;
            }
            return y+line;
        }
        float RichHeight(string text,float width,int fontSize=19)=>RichText(text,new Rect(0,0,width,0),fontSize,false);
        static string ColoredValue(string key,float value)=>"<color="+(SpecialItemEffects.IsPenalty(key,value)?"#FF3333":"#00FF00")+">"+value.ToString("+0.##;-0.##;0")+StatPresentation.Unit(key)+"</color>";
        string StatLine(string key,float value)=>"[["+key+"]] "+ColoredValue(key,value)+" "+inspectionText.StatName(key).TrimStart('%',' ');
    }
}
