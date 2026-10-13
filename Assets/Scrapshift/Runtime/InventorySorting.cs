using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        int itemSort;bool reverseItems,itemSortOpen;Rect itemSortAnchor;
        static readonly string[] itemSortLabels={"Obtaining","Name","Tier","Category"};
        ItemEntry[] SortedItems()
        {
            var source=Rules.Items.GroupBy(i=>i.id).Select(g=>g.First());
            if(itemSort==1)source=source.OrderBy(i=>inspectionText.ItemName(i));
            else if(itemSort==2)source=source.OrderByDescending(i=>i.tier).ThenBy(i=>inspectionText.ItemName(i));
            else if(itemSort==3)source=source.OrderBy(i=>(i.tags??new string[0]).FirstOrDefault()??"").ThenBy(i=>inspectionText.ItemName(i));
            return (reverseItems?source.Reverse():source).ToArray();
        }
        void DrawItemSorting(Rect r)
        {
            itemSortAnchor=new Rect(r.x,r.y,r.width-42,r.height);
            if(Button(itemSortAnchor,"Sorted by "+itemSortLabels[itemSort]+" ▾"))itemSortOpen=!itemSortOpen;
            if(Button(new Rect(r.xMax-38,r.y,38,r.height),reverseItems?"↑":"↓"))reverseItems=!reverseItems;
        }
        void DrawItemSortMenu()
        {
            if(!itemSortOpen)return;
            var r=new Rect(itemSortAnchor.x,itemSortAnchor.y-itemSortLabels.Length*38-6,itemSortAnchor.width,itemSortLabels.Length*38);
            RoundedBox(r,Color.black,6);
            for(int i=0;i<itemSortLabels.Length;i++)if(MenuButton(new Rect(r.x+3,r.y+i*38+2,r.width-6,34),itemSortLabels[i],itemSort==i)){itemSort=i;itemScroll=Vector2.zero;itemSortOpen=false;}
            if(Event.current.type==EventType.MouseDown&&!r.Contains(Event.current.mousePosition)&&!itemSortAnchor.Contains(Event.current.mousePosition))itemSortOpen=false;
        }
    }
}
