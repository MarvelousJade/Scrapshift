using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static class HeadlessDynamicItemsOracle
    {
        [Serializable] sealed class Operation { public string kind,key; public int value; }
        [Serializable] sealed class CaseSpec { public string id,character,starter; public string[] items; public Operation[] operations; }
        // JsonUtility does not serialize jagged arrays; flatten the snapshots.
        [Serializable] sealed class Result { public string id,error; public float[] stats; public int snapshots; }

        public static void Export()
        {
            try
            {
                var args=Environment.GetCommandLineArgs();
                string input=Argument(args,"--item-oracle-cases"),output=Argument(args,"--item-oracle-output");
                if(string.IsNullOrEmpty(input)||string.IsNullOrEmpty(output))throw new ArgumentException("Input/output paths required");
                var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
                int count=0;
                using(var writer=new StreamWriter(output,false))
                foreach(var line in File.ReadLines(input))
                {
                    if(string.IsNullOrWhiteSpace(line))continue;
                    var spec=JsonUtility.FromJson<CaseSpec>(line);var result=new Result{id=spec.id,error=""};
                    try
                    {
                        var run=new RunRules(2010);run.Configure(catalog);
                        run.Start(catalog.data.characters.Single(c=>c.id==spec.character),spec.starter,30);
                        foreach(var id in spec.items)run.GrantItem(catalog.data.items.Single(item=>item.id==id));
                        var values=new List<float>();
                        foreach(var operation in spec.operations)
                        {
                            switch(operation.kind)
                            {
                                case "tick":run.Tick(operation.value);break;
                                case "move":run.SetMoving(operation.value>0);break;
                                case "fruit":
                                    // Fixture sets pre-contact HP directly to isolate full-HP
                                    // eligibility/caps from damage and consumable healing.
                                    float maximum=run.MaxHealth;_ = run.Health;
                                    typeof(RunRules).GetProperty("Health").GetSetMethod(true).Invoke(run,new object[]{maximum-operation.value});
                                    run.CollectConsumable(ConsumableKind.Fruit);break;
                                case "health":
                                    float max=run.MaxHealth;_ = run.Health;
                                    float hp=operation.value==0?max:operation.value==1?Mathf.Floor(max/2):operation.value==2?Mathf.Ceil(max/2):Mathf.Floor(max/2)-1;
                                    typeof(RunRules).GetProperty("Health").GetSetMethod(true).Invoke(run,new object[]{hp});break;
                                case "intermission":
                                    run.Tick(run.Remaining);
                                    if(run.Phase!=RunPhase.Shop)throw new InvalidOperationException("Doctor lifecycle case must reach ordinary shop without rewards");break;
                                case "remove":
                                    if(!run.RemoveItem(catalog.data.items.Single(item=>item.id==operation.key)))throw new InvalidOperationException("Missing removal item");break;
                                case "add":run.GrantItem(catalog.data.items.Single(item=>item.id==operation.key));break;
                                case "next_wave":run.NextWave();break;
                                case "end_lost":
                                    run.Hurt(10000);
                                    if(run.Phase!=RunPhase.Lost)throw new InvalidOperationException("Staged lethal damage did not cause loss");break;
                                case "end_won":
                                    run.SetShowcaseWave(20);run.Tick(run.Remaining);
                                    if(run.Phase!=RunPhase.Won)throw new InvalidOperationException("Staged final wave did not finish");break;
                                default:throw new ArgumentException(operation.kind);
                            }
                            values.AddRange(StatPresentation.Keys.Select(run.Stat));result.snapshots++;
                        }
                        result.stats=values.ToArray();
                    }
                    catch(Exception ex){result.error=ex.ToString();}
                    writer.WriteLine(JsonUtility.ToJson(result));count++;
                }
                Debug.Log("HEADLESS_DYNAMIC_ITEMS_UNITY_DONE "+count);EditorApplication.Exit(0);
            }
            catch(Exception ex){Debug.LogException(ex);EditorApplication.Exit(1);}
        }
        static string Argument(string[] args,string key){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:null;}
    }
}
