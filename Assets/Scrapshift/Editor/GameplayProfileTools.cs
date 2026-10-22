using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static class GameplayProfileTools
    {
        static string Arg(string name,string fallback=null){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,name);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        public static void BuildDevelopment()
        {
            string path=Arg("--profile-build","Builds/GameplayProfile20261017/Scrapshift.exe");Directory.CreateDirectory(Path.GetDirectoryName(path));
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ContentBuilder.ScenePath},locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            string report=Arg("--profile-build-report","artifacts/gameplay-profile-build.txt");Directory.CreateDirectory(Path.GetDirectoryName(report));
            File.WriteAllText(report,$"Result: {result.summary.result}\nErrors: {result.summary.totalErrors}\nWarnings: {result.summary.totalWarnings}\nBytes: {result.summary.totalSize}\nDevelopment only; no Deep Profiling.\n");
            if(result.summary.result!=BuildResult.Succeeded||result.summary.totalErrors!=0)throw new Exception("Development profiling build failed.");
        }
        [Serializable] sealed class MarkerCost
        {
            public string thread,name;public double totalMs,selfMs,calls,gcBytes;public int occurrences;
        }
        [Serializable] sealed class FrameCost{public int frame;public string thread;public double totalMs;}
        [Serializable] sealed class ExportReport
        {
            public string status,rawPath,scope="Non-Deep Unity CPU profile. Self-time rows can be aggregated; inclusive totals overlap and must not be summed across parent/child markers. Synthetic fixture cannot alone prove the cause of R21 lag.";
            public int firstFrame,lastFrame,frames,threadFrames;
            public MarkerCost[] topSelf,topTotal,waitsAndGc;
            public FrameCost[] frameCosts;
        }
        public static void ExportProfile()
        {
            string raw=Arg("--profile-raw"),output=Arg("--profile-export");
            if(string.IsNullOrEmpty(raw)||string.IsNullOrEmpty(output))throw new ArgumentException("Use --profile-raw <capture.raw> --profile-export <report.json>.");
            ProfilerDriver.enabled=false;ProfilerDriver.deepProfiling=false;
            if(!ProfilerDriver.LoadProfile(raw,false))throw new Exception("ProfilerDriver.LoadProfile failed: "+raw);
            var report=new ExportReport{rawPath=Path.GetFullPath(raw),firstFrame=ProfilerDriver.firstFrameIndex,lastFrame=ProfilerDriver.lastFrameIndex};
            if(report.firstFrame<0||report.lastFrame<report.firstFrame)throw new Exception("Profile contains no frames.");
            var costs=new Dictionary<string,MarkerCost>();var frames=new List<FrameCost>();var children=new List<int>();var pending=new Stack<int>();
            for(int frame=report.firstFrame;frame<=report.lastFrame;frame++)
            {
                bool any=false;
                for(int thread=0;thread<128;thread++)
                {
                    string threadName;
                    using(var rawView=ProfilerDriver.GetRawFrameDataView(frame,thread)){if(!rawView.valid)break;threadName=rawView.threadName;}
                    // Main/render and worker threads are retained to avoid silently attributing worker work to the main thread.
                    using(var view=ProfilerDriver.GetHierarchyFrameDataView(frame,thread,HierarchyFrameDataView.ViewModes.Default,HierarchyFrameDataView.columnDontSort,false))
                    {
                        if(!view.valid)continue;any=true;report.threadFrames++;int root=view.GetRootItemID();
                        frames.Add(new FrameCost{frame=frame,thread=threadName,totalMs=view.GetItemColumnDataAsDouble(root,HierarchyFrameDataView.columnTotalTime)});
                        pending.Clear();view.GetItemChildren(root,children);foreach(int item in children)pending.Push(item);
                        while(pending.Count>0)
                        {
                            int item=pending.Pop();string name=view.GetItemName(item),key=threadName+"\n"+name;
                            if(!costs.TryGetValue(key,out var cost)){cost=new MarkerCost{thread=threadName,name=name};costs.Add(key,cost);}
                            cost.totalMs+=view.GetItemColumnDataAsDouble(item,HierarchyFrameDataView.columnTotalTime);
                            cost.selfMs+=view.GetItemColumnDataAsDouble(item,HierarchyFrameDataView.columnSelfTime);
                            cost.calls+=view.GetItemColumnDataAsDouble(item,HierarchyFrameDataView.columnCalls);
                            cost.gcBytes+=view.GetItemColumnDataAsDouble(item,HierarchyFrameDataView.columnGcMemory);cost.occurrences++;
                            view.GetItemChildren(item,children);foreach(int child in children)pending.Push(child);
                        }
                    }
                }
                if(any)report.frames++;
            }
            report.topSelf=costs.Values.OrderByDescending(x=>x.selfMs).Take(120).ToArray();
            report.topTotal=costs.Values.OrderByDescending(x=>x.totalMs).Take(120).ToArray();
            report.waitsAndGc=costs.Values.Where(x=>x.name.IndexOf("wait",StringComparison.OrdinalIgnoreCase)>=0||x.name.IndexOf("GC",StringComparison.OrdinalIgnoreCase)>=0||x.gcBytes>0).OrderByDescending(x=>x.totalMs).Take(100).ToArray();
            report.frameCosts=frames.ToArray();report.status=report.frames>0?"Exported":"Empty";
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));File.WriteAllText(output,JsonUtility.ToJson(report,true));
            if(report.frames==0)throw new Exception("No readable profile frames.");
        }
    }
}
