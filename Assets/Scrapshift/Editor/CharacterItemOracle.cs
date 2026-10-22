using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Scrapshift.Editor
{
    // Batch-mode companion to scripts/reference_character_item_oracle.gd.
    public static class CharacterItemOracle
    {
        [Serializable] sealed class CaseSpec
        {
            public string id,character,starter,selectedItem;
            public int tier;
            public string[] items;
        }
        [Serializable] sealed class Result
        {
            public string id,error;
            public float[] stats;
            public int unityItemCount;
        }

        public static void Export()
        {
            try
            {
                var arguments=Environment.GetCommandLineArgs();
                string input=Argument(arguments,"--item-oracle-cases");
                string output=Argument(arguments,"--item-oracle-output");
                if(string.IsNullOrEmpty(input)||string.IsNullOrEmpty(output))
                    throw new ArgumentException("Supply --item-oracle-cases and --item-oracle-output absolute paths");
                var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
                if(catalog==null||catalog.data==null)throw new InvalidOperationException("Missing ReferenceCatalog");
                var characters=catalog.data.characters.ToDictionary(c=>c.id);
                var items=catalog.data.items.ToDictionary(i=>i.id);
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
                int count=0;
                using(var writer=new StreamWriter(output,false,new UTF8Encoding(false)))
                foreach(var line in File.ReadLines(input))
                {
                    if(string.IsNullOrWhiteSpace(line))continue;
                    var spec=JsonUtility.FromJson<CaseSpec>(line);
                    var result=new Result{id=spec.id,error="",stats=new float[0]};
                    try
                    {
                        if(!characters.TryGetValue(spec.character,out var character))
                            throw new InvalidOperationException("Missing imported character "+spec.character);
                        var run=new RunRules(1704);run.Configure(catalog);
                        // The supplied source initializes new players with
                        // DebugService.starting_gold = 30 before item links resolve.
                        run.Start(character,string.IsNullOrEmpty(spec.starter)?null:spec.starter,30,
                            string.IsNullOrEmpty(spec.selectedItem)?null:spec.selectedItem);
                        // No live Player scene in the source exporter: keep
                        // input-driven TempStats inactive on this side too.
                        typeof(RunRules).GetProperty("Phase",BindingFlags.Instance|BindingFlags.Public)
                            .GetSetMethod(true).Invoke(run,new object[]{RunPhase.Shop});
                        foreach(var id in spec.items)
                        {
                            if(!items.TryGetValue(id,out var item))throw new InvalidOperationException("Missing imported item "+id);
                            run.GrantItem(item);
                        }
                        result.stats=StatPresentation.Keys.Select(run.Stat).ToArray();
                        result.unityItemCount=run.Items.Count;
                    }
                    catch(Exception exception){result.error=exception.GetType().Name+": "+exception.Message;}
                    writer.WriteLine(JsonUtility.ToJson(result));
                    count++;
                    if(count%1000==0)Debug.Log("CHARACTER_ORACLE_UNITY "+count);
                }
                Debug.Log("CHARACTER_ORACLE_UNITY_DONE "+count);
                EditorApplication.Exit(0);
            }
            catch(Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        static string Argument(string[] arguments,string name)
        {
            for(int i=0;i<arguments.Length-1;i++)if(arguments[i]==name)return arguments[i+1];
            return null;
        }
    }
}
