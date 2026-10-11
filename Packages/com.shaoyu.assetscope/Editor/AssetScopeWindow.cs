using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AssetScope
{
    public sealed class AssetScopeWindow : EditorWindow
    {
        [SerializeField] string folder = "Assets";
        [SerializeField] string query = "";
        [SerializeField] bool prefabsOnly;
        ScanSession session;
        ListView results;
        VisualElement details;
        Label status;
        Button scanButton;
        Button cancelButton;
        Button exportButton;
        Button selectButton;
        string selectedPath;
        readonly Stopwatch elapsed = new Stopwatch();

        [MenuItem("Window/AssetScope/Asset Search & Validation")]
        public static void Open() => GetWindow<AssetScopeWindow>("AssetScope");

        void OnEnable()
        {
            EditorApplication.update += Tick;
            EditorApplication.projectChanged += Invalidate;
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }

        void OnDisable()
        {
            session?.Cancel();
            EditorApplication.update -= Tick;
            EditorApplication.projectChanged -= Invalidate;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
        }

        void PlayModeChanged(PlayModeStateChange change) => Invalidate();

        public void CreateGUI()
        {
            minSize = new Vector2(760, 480);
            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = root.style.paddingRight = 12;
            root.style.paddingTop = root.style.paddingBottom = 10;
            var title = new Label("ASSETSCOPE  /  SEARCH & VALIDATION");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(title);
            root.Add(new HelpBox("Read-only. Missing scripts: prefabs and inactive children. Dependencies: selected folder's assets. Scenes are not opened; a clean scan does not mean an asset is unused.", HelpBoxMessageType.Info));
            var scope = new TextField("Folder") { value = folder, name = "scope-folder", isDelayed = true };
            scope.RegisterValueChangedCallback(e => { folder = e.newValue.Replace('\\', '/').TrimEnd('/'); Invalidate(); RefreshResults(); });
            root.Add(scope);
            var search = new TextField("Search") { value = query, name = "asset-query" };
            search.tooltip = "Unity AssetDatabase query: name words, t:Prefab, t:Material, l:label";
            search.RegisterValueChangedCallback(e => { query = e.newValue; RefreshResults(); });
            root.Add(search);
            var toggle = new Toggle("Prefabs only") { value = prefabsOnly };
            toggle.RegisterValueChangedCallback(e => { prefabsOnly = e.newValue; RefreshResults(); });
            root.Add(toggle);
            var actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Row;
            scanButton = new Button(StartScan) { text = "Scan folder", name = "scan-folder" };
            cancelButton = new Button(() => { session?.Cancel(); UpdateStatus(); }) { text = "Cancel", name = "cancel-scan" };
            exportButton = new Button(Export) { text = "Export JSON", name = "export-report" };
            selectButton = new Button(() => SelectAsset(selectedPath))
            {
                text = "Select in Project", name = "select-in-project",
                tooltip = "Choose an asset in the results to select and highlight it in the Project window."
            };
            actions.Add(scanButton); actions.Add(cancelButton); actions.Add(exportButton); actions.Add(selectButton); root.Add(actions);
            status = new Label("Ready. Choose a folder and scan.") { name = "scan-status" };
            status.style.marginTop = status.style.marginBottom = 8;
            root.Add(status);
            var panes = new TwoPaneSplitView(0, 340, TwoPaneSplitViewOrientation.Horizontal);
            panes.style.flexGrow = 1;
            results = new ListView
            {
                fixedItemHeight = 28, selectionType = SelectionType.Single,
                makeItem = () => new Label(),
                bindItem = (element, index) => {
                    var path = (string)results.itemsSource[index];
                    ((Label)element).text = path;
                    element.tooltip = path;
                },
                name = "asset-results"
            };
            results.selectionChanged += items => {
                selectedPath = items.OfType<string>().FirstOrDefault(); ShowDetails();
            };
            panes.Add(results);
            details = new ScrollView { name = "asset-details" };
            details.style.paddingLeft = 12;
            panes.Add(details); root.Add(panes);
            RefreshResults(); UpdateStatus(); ShowDetails();
        }

        void RefreshResults()
        {
            if (results == null) return;
            var previousPath = selectedPath;
            var paths = AssetCatalog.Find(query + (prefabsOnly ? " t:Prefab" : ""), folder);
            // List indices change during filtering. Clear first, then restore by asset identity.
            results.ClearSelection();
            results.itemsSource = paths;
            results.Rebuild();
            var index = Array.IndexOf(paths, previousPath);
            if (index < 0 && paths.Length == 1) index = 0;
            if (index >= 0) results.SetSelection(index);
            selectedPath = results.selectedItem as string;
            ShowDetails();
        }

        void StartScan()
        {
            if (!AssetDatabase.IsValidFolder(folder) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            session?.Cancel();
            session = ScanSession.Create(folder);
            elapsed.Restart();
            UpdateStatus(); ShowDetails();
        }

        void Tick()
        {
            if (session == null || session.State != ScanState.Running) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            var frame = Stopwatch.StartNew();
            // Unity APIs stay on the editor thread. Yield between assets after ~8ms.
            do { session.Step(); } while (session.State == ScanState.Running && frame.ElapsedMilliseconds < 8);
            if (session.State != ScanState.Running) { elapsed.Stop(); ShowDetails(); }
            UpdateStatus();
        }

        void Invalidate()
        {
            session?.Invalidate();
            elapsed.Stop();
            UpdateStatus(); ShowDetails();
        }

        void UpdateStatus()
        {
            if (status == null) return;
            bool running = session?.State == ScanState.Running;
            scanButton.SetEnabled(!running && AssetDatabase.IsValidFolder(folder) && !EditorApplication.isPlayingOrWillChangePlaymode);
            cancelButton.SetEnabled(running);
            exportButton.SetEnabled(session != null && !running);
            status.text = session == null ? "Ready. Choose a folder and scan." :
                $"{session.State}: {session.Processed}/{session.Total} assets | {session.Findings.Count} findings | {elapsed.ElapsedMilliseconds} ms";
            if (session?.State == ScanState.Stale) status.text += " — project changed; rescan before using results.";
            if (session?.State == ScanState.Cancelled) status.text += " — partial results.";
            if (session?.State == ScanState.Failed) status.text += " — " + session.Error;
        }

        void ShowDetails()
        {
            if (details == null) return;
            selectButton?.SetEnabled(selectedPath != null && AssetDatabase.LoadMainAssetAtPath(selectedPath) != null);
            details.Clear();
            if (selectedPath == null)
            {
                var hint = new Label(results?.itemsSource?.Count == 0
                    ? "No matching assets. Change the search or folder."
                    : "Click an asset in the left-hand list to inspect it, then use Select in Project above.");
                hint.style.whiteSpace = WhiteSpace.Normal;
                details.Add(hint);
                return;
            }
            var pathLabel = new Label(selectedPath); pathLabel.style.whiteSpace = WhiteSpace.Normal;
            details.Add(pathLabel);
            if (session == null || session.State != ScanState.Complete)
            {
                details.Add(new HelpBox("Complete a fresh scan to inspect dependencies and findings.", HelpBoxMessageType.Info));
                return;
            }
            AddLinks("Direct dependencies", session.Index.Dependencies(selectedPath));
            AddLinks("Referenced by (within scan scope)", session.Index.Referencers(selectedPath));
            var findings = session.Findings.Where(f => f.assetPath == selectedPath).ToArray();
            details.Add(new Label($"Missing-script findings: {findings.Length}"));
            foreach (var finding in findings)
                details.Add(new HelpBox(finding.hierarchyPath + "\n" + finding.message, HelpBoxMessageType.Warning));
        }

        void AddLinks(string heading, string[] paths)
        {
            details.Add(new Label($"{heading}: {paths.Length}"));
            foreach (var path in paths) details.Add(new Button(() => SelectAsset(path)) { text = path, tooltip = path });
        }

        static void SelectAsset(string path)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) return;
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        void Export()
        {
            var path = EditorUtility.SaveFilePanel("Export AssetScope report", "", "assetscope-report", "json");
            if (string.IsNullOrEmpty(path)) return;
            try { File.WriteAllText(path, JsonUtility.ToJson(ScanReport.From(session), true)); }
            catch (Exception e) { EditorUtility.DisplayDialog("Export failed", e.Message, "OK"); }
        }
    }
}
