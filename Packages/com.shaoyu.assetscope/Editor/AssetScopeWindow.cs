using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace AssetScope
{
    public sealed class AssetScopeWindow : EditorWindow
    {
        [SerializeField] string folder = "Assets";
        [SerializeField] string query = "";
        [SerializeField] bool prefabsOnly;
        [SerializeField] ScanOptions options = new ScanOptions();
        [SerializeField] UnityEngine.Object referenceTarget;
        [SerializeField] bool findingsOnly;
        [SerializeField] bool transitive;
        SearchPresets presets;
        ScanSession session;
        ListView results;
        VisualElement details;
        Label status;
        HelpBox navigationStatus;
        Button scanButton;
        Button cancelButton;
        Button exportButton;
        Button csvButton;
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
            minSize = new Vector2(820, 600);
            presets = new SearchPresets();
            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = root.style.paddingRight = 12;
            root.style.paddingTop = root.style.paddingBottom = 10;
            var title = new Label("ASSETSCOPE  /  SEARCH & VALIDATION");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(title);
            root.Add(new HelpBox("Inspect serialized references, missing scripts and dependency chains. Results cover the chosen folder; no safe-to-delete inference. Saved-scene inspection is opt-in below.", HelpBoxMessageType.Info));
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
            var settings = new Foldout { text = "Scan options & saved searches", value = false };
            AddOption(settings, "Inspect serialized properties", "inspect-properties", options.inspectProperties, v => options.inspectProperties = v);
            AddOption(settings, "Inspect saved scenes", "inspect-scenes", options.inspectSavedScenes, v => options.inspectSavedScenes = v);
            settings.Add(new HelpBox("Saved scenes open briefly as preview scenes and close in finally. Unsaved edits are excluded. Custom editor/ExecuteAlways callbacks can still run while Unity loads assets.", HelpBoxMessageType.Info));
            AddOption(settings, "Reuse unchanged analysis", "use-cache", options.useCache, v => options.useCache = v);
            var excludes = new TextField("Exclude folders (; separated)") { value = options.excludedFolders, name = "excluded-folders", isDelayed = true };
            excludes.RegisterValueChangedCallback(e => { options.excludedFolders = e.newValue; Invalidate(); RefreshResults(); });
            settings.Add(excludes);
            var presetName = new TextField("Save as") { name = "preset-name" };
            var saved = new DropdownField("Saved searches", presets.Names.ToList(), -1) { name = "saved-searches" };
            settings.Add(presetName); settings.Add(saved);
            settings.Add(new Button(() => {
                try { presets.Save(new SearchPreset { name = presetName.value, folder = folder, query = query, prefabsOnly = prefabsOnly, options = options.Copy() }); saved.choices = presets.Names.ToList(); saved.value = presetName.value.Trim(); }
                catch (Exception e) { EditorUtility.DisplayDialog("Save search failed", e.Message, "OK"); }
            }) { text = "Save current search" });
            settings.Add(new Button(() => {
                var preset = presets.Get(saved.value); if (preset == null) return;
                session?.Cancel(); session = null; folder = preset.folder; query = preset.query; prefabsOnly = preset.prefabsOnly; options = preset.options ?? new ScanOptions();
                referenceTarget = null; findingsOnly = false; CreateGUI();
            }) { text = "Load saved search" });
            if (!string.IsNullOrEmpty(presets.Warning)) settings.Add(new HelpBox(presets.Warning, HelpBoxMessageType.Warning));
            root.Add(settings);
            var actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Row;
            scanButton = new Button(StartScan) { text = "Scan folder", name = "scan-folder" };
            cancelButton = new Button(() => { session?.Cancel(); UpdateStatus(); }) { text = "Cancel", name = "cancel-scan" };
            exportButton = new Button(Export) { text = "Export JSON", name = "export-report" };
            csvButton = new Button(() => ExportReport(true)) { text = "Export CSV", name = "export-csv" };
            selectButton = new Button(() => SelectAsset(selectedPath))
            {
                text = "Select in Project", name = "select-in-project",
                tooltip = "Choose an asset in the results to select and highlight it in the Project window."
            };
            actions.Add(scanButton); actions.Add(cancelButton); actions.Add(exportButton); actions.Add(csvButton); actions.Add(selectButton); root.Add(actions);
            var onlyFindings = new Toggle("Only assets with findings") { value = findingsOnly, name = "findings-only" };
            onlyFindings.RegisterValueChangedCallback(e => { findingsOnly = e.newValue; RefreshResults(); }); root.Add(onlyFindings);
            var target = new ObjectField("References to target") { objectType = typeof(UnityEngine.Object), allowSceneObjects = false, value = referenceTarget, name = "reference-target" };
            target.tooltip = "Drop an asset or subasset here. Filters sources by exact serialized target identity after a scan. Clear to show all.";
            target.RegisterValueChangedCallback(e => { referenceTarget = e.newValue; RefreshResults(); }); root.Add(target);
            var recursive = new Toggle("Show transitive dependencies") { value = transitive, name = "transitive-dependencies" };
            recursive.RegisterValueChangedCallback(e => { transitive = e.newValue; ShowDetails(); }); root.Add(recursive);
            status = new Label("Ready. Choose a folder and scan.") { name = "scan-status" };
            status.style.marginTop = status.style.marginBottom = 8;
            root.Add(status);
            navigationStatus = new HelpBox("", HelpBoxMessageType.Info) { name = "navigation-status" };
            navigationStatus.style.display = DisplayStyle.None;
            root.Add(navigationStatus);
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
            var paths = AssetCatalog.Find(query + (prefabsOnly ? " t:Prefab" : ""), folder).Where(options.Includes).ToArray();
            if (findingsOnly)
            {
                var sources = new HashSet<string>(session != null && session.HasResults ? session.Findings.Select(f => f.assetPath) : Enumerable.Empty<string>());
                paths = paths.Where(sources.Contains).ToArray();
            }
            if (referenceTarget != null)
            {
                var sources = new HashSet<string>(MatchingUsages().Select(u => u.sourceAssetPath));
                paths = paths.Where(sources.Contains).ToArray();
            }
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
            session = ScanSession.Create(folder, options);
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
            if (session.State != ScanState.Running) { elapsed.Stop(); RefreshResults(); }
            UpdateStatus();
        }

        void Invalidate()
        {
            session?.Invalidate();
            elapsed.Stop();
            UpdateStatus(); RefreshResults();
        }

        void UpdateStatus()
        {
            if (status == null) return;
            bool running = session?.State == ScanState.Running;
            scanButton.SetEnabled(!running && AssetDatabase.IsValidFolder(folder) && !EditorApplication.isPlayingOrWillChangePlaymode);
            cancelButton.SetEnabled(running);
            exportButton.SetEnabled(session != null && !running);
            csvButton.SetEnabled(session != null && !running);
            status.text = session == null ? "Ready. Choose a folder and scan." :
                $"{session.State}: {session.Processed}/{session.Total} assets | {session.Findings.Count} findings | {session.Usages.Count} references | {session.CacheHits} cached | {session.ElapsedMilliseconds} ms";
            status.style.whiteSpace = WhiteSpace.Normal;
            if (session?.State == ScanState.Stale) status.text += " — project changed; rescan before using results.";
            if (session?.State == ScanState.Cancelled) status.text += " — partial results.";
            if (session?.State == ScanState.Failed) status.text += " — " + session.Error;
            if (session?.ErrorCount > 0) status.text += $" — {session.ErrorCount} assets failed inspection; results are incomplete.";
            if (!string.IsNullOrEmpty(session?.CacheWarning)) status.text += " — " + session.CacheWarning;
        }

        void ShowDetails()
        {
            if (details == null) return;
            selectButton?.SetEnabled(selectedPath != null && AssetDatabase.LoadMainAssetAtPath(selectedPath) != null);
            details.Clear();
            if (selectedPath == null)
            {
                var hint = new Label(results?.itemsSource?.Count == 0
                    ? "No matching assets. Change the search or folder. Findings/target filters require a fresh scan."
                    : "Click an asset in the left-hand list to inspect it, then use Select in Project above.");
                hint.style.whiteSpace = WhiteSpace.Normal;
                details.Add(hint);
                return;
            }
            var pathLabel = new Label(selectedPath); pathLabel.style.whiteSpace = WhiteSpace.Normal;
            details.Add(pathLabel);
            if (session == null || !session.HasResults)
            {
                details.Add(new HelpBox("Complete a fresh scan to inspect dependencies and findings.", HelpBoxMessageType.Info));
                return;
            }
            var analysis = session.Assets.FirstOrDefault(a => a.assetPath == selectedPath);
            details.Add(new Label(analysis?.inspection ?? "Asset was not inspected successfully."));
            AddLinks(transitive ? "Transitive dependencies (within indexed graph)" : "Direct dependencies", transitive ? session.Index.Transitive(selectedPath) : session.Index.Dependencies(selectedPath));
            AddLinks("Referenced by (within scan scope)", transitive ? session.Index.Transitive(selectedPath, true) : session.Index.Referencers(selectedPath));
            if (referenceTarget != null)
            {
                var chain = session.Index.ShortestChain(selectedPath, AssetDatabase.GetAssetPath(referenceTarget));
                details.Add(new HelpBox(chain.Length == 0 ? "No dependency chain in this scan's graph." : "Shortest asset-file chain:\n" + string.Join(" → ", chain), HelpBoxMessageType.Info));
            }
            var findings = session.Findings.Where(f => f.assetPath == selectedPath).ToArray();
            AddRows("Findings", findings.Select(f => $"{f.severity} · {f.code}\n{f.hierarchyPath} · {f.componentType} · {f.propertyPath}\n{f.message}").ToArray());
            var usages = MatchingUsages().Where(u => u.sourceAssetPath == selectedPath).ToArray();
            details.Add(new Label($"Serialized reference locations: {usages.Length} — click to open source"));
            if (usages.Length > 0)
            {
                var list = new ListView { name = "reference-locations", itemsSource = usages, fixedItemHeight = 68, selectionType = SelectionType.None,
                    makeItem = () => new ReferenceUsageRow(NavigateToSource),
                    bindItem = (element, index) => ((ReferenceUsageRow)element).Bind(usages[index]),
                    unbindItem = (element, index) => ((ReferenceUsageRow)element).Unbind() };
                list.style.height = Math.Min(usages.Length, 4) * 68; details.Add(list);
            }
        }

        void NavigateToSource(ReferenceUsage usage)
        {
            var outcome = session != null && session.HasResults ? SourceNavigator.Navigate(usage) :
                new NavigationResult(false, "The scan is no longer current. Run a fresh scan before navigating.");
            navigationStatus.text = outcome.Message;
            navigationStatus.messageType = outcome.Success ? HelpBoxMessageType.Info : HelpBoxMessageType.Warning;
            navigationStatus.style.display = DisplayStyle.Flex;
        }

        void AddLinks(string heading, string[] paths)
        {
            details.Add(new Label($"{heading}: {paths.Length}"));
            if (paths.Length == 0) return;
            var list = new ListView { itemsSource = paths, fixedItemHeight = 24, selectionType = SelectionType.Single, makeItem = () => new Label(), bindItem = (e, i) => { ((Label)e).text = paths[i]; e.tooltip = paths[i]; } };
            list.style.height = Math.Min(paths.Length, 5) * 24;
            list.selectionChanged += values => SelectAsset(values.OfType<string>().FirstOrDefault());
            details.Add(list);
        }

        void AddRows(string heading, string[] rows)
        {
            details.Add(new Label($"{heading}: {rows.Length}"));
            if (rows.Length == 0) return;
            var list = new ListView { itemsSource = rows, fixedItemHeight = 62, selectionType = SelectionType.None, makeItem = () => new Label(), bindItem = (e, i) => { ((Label)e).text = rows[i]; e.tooltip = rows[i]; } };
            list.style.height = Math.Min(rows.Length, 4) * 62; details.Add(list);
        }

        IEnumerable<ReferenceUsage> MatchingUsages()
        {
            if (session == null || !session.HasResults) return Enumerable.Empty<ReferenceUsage>();
            if (referenceTarget == null) return session.Usages;
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(referenceTarget, out string guid, out long id)) return Enumerable.Empty<ReferenceUsage>();
            return session.Usages.Where(u => u.targetGuid == guid && u.targetLocalId == id);
        }

        void AddOption(VisualElement parent, string label, string name, bool value, Action<bool> assign)
        {
            var toggle = new Toggle(label) { name = name, value = value };
            toggle.RegisterValueChangedCallback(e => { assign(e.newValue); Invalidate(); }); parent.Add(toggle);
        }

        static void SelectAsset(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) return;
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        void Export() => ExportReport(false);

        void ExportReport(bool csv)
        {
            var path = EditorUtility.SaveFilePanel("Export AssetScope report", "", "assetscope-report", csv ? "csv" : "json");
            if (string.IsNullOrEmpty(path)) return;
            try { var report = ScanReport.From(session); if (csv) ReportWriter.WriteAtomic(path, ReportWriter.FindingsCsv(report)); else ReportWriter.WriteJson(path, report); }
            catch (Exception e) { EditorUtility.DisplayDialog("Export failed", e.Message, "OK"); }
        }
    }
}
