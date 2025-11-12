using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Handbook.ContentProvider;
using Handbook.Editor.Processing;
using Handbook.Editor.Scanning;
using Handbook.Models;
using Handbook.Parser;
using Handbook.Parser.Validation;
using Handbook.Routing;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Handbook.Editor
{
    public class HandbookManifestGeneratorWindow : EditorWindow
    {
        [SerializeField] private string _rootFolder;
        [SerializeField] private string _language = "ru";
        [SerializeField] private string _mediaBasePath = "media";
        [SerializeField] private string _version = "1.0.0";
        [SerializeField] private int _defaultPageIndex = -1;
        [SerializeField] private string _defaultPageId = "";
        [SerializeField] private string[] _pageOptions = Array.Empty<string>();
        [SerializeField] private HandbookRuntimeConfig _runtimeConfig;
        [SerializeField] private bool _preserveTitles = true;
        [SerializeField] private bool _preserveTags = true;
        [SerializeField] private bool _preserveSummary = true;
        [SerializeField] private bool _preserveHidden = true;
        [SerializeField] private int _maxIssuesToPrint = 100;
        
        [MenuItem("Tools/Handbook/Manifest Generator")]
        public static void Open()
        {
            var w = GetWindow<HandbookManifestGeneratorWindow>("Handbook Manifest");
            w.minSize = new Vector2(480, 360);
            w.InitDefaults();
            w.Show();
        }

        private void InitDefaults()
        {
            if (string.IsNullOrWhiteSpace(_rootFolder))
            {
                var sa = Application.streamingAssetsPath;
                _rootFolder = Path.Combine(sa, "Handbook").Replace('\\', '/');
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Config", EditorStyles.boldLabel);
            _runtimeConfig = (HandbookRuntimeConfig)EditorGUILayout.ObjectField("Config Asset", _runtimeConfig, typeof(HandbookRuntimeConfig), false);
            
            EditorGUILayout.BeginHorizontal();
            _rootFolder = EditorGUILayout.TextField("Root Folder", _rootFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                var picked = EditorUtility.OpenFolderPanel("Select Handbook Root", _rootFolder, "");
                if (!string.IsNullOrEmpty(picked))
                    _rootFolder = picked.Replace('\\', '/');
            }
            EditorGUILayout.EndHorizontal();

            _language = EditorGUILayout.TextField("Language", _language);
            _mediaBasePath = EditorGUILayout.TextField("Media Base Path", _mediaBasePath);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manifest", EditorStyles.boldLabel);
            _version = EditorGUILayout.TextField("Version", _version);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Default Page", GUILayout.Width(100));

                if (_pageOptions == null || _pageOptions.Length == 0)
                {
                    EditorGUILayout.LabelField("(no pages loaded)");
                    if (GUILayout.Button("Load Pages", GUILayout.Width(100)))
                        RebuildPageOptions();
                }
                else
                {
                    var newIndex = EditorGUILayout.Popup(_defaultPageIndex < 0 ? 0 : _defaultPageIndex, _pageOptions);
                    if (newIndex != _defaultPageIndex)
                    {
                        _defaultPageIndex = newIndex;
                        _defaultPageId = _pageOptions[_defaultPageIndex];
                    }

                    if (GUILayout.Button("Reload", GUILayout.Width(80)))
                        RebuildPageOptions();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preserve Fields (from existing manifest)", EditorStyles.boldLabel);
            _preserveTitles = EditorGUILayout.Toggle("Preserve Titles", _preserveTitles);
            _preserveTags = EditorGUILayout.Toggle("Preserve Tags", _preserveTags);
            _preserveSummary = EditorGUILayout.Toggle("Preserve Summary", _preserveSummary);
            _preserveHidden = EditorGUILayout.Toggle("Preserve Hidden", _preserveHidden);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan & Preview"))
                    ScanAndPreview();

                if (GUILayout.Button("Generate & Save"))
                {
                    GenerateAndSave();
                    SaveRuntimeConfig();
                }

                if (GUILayout.Button("Validate"))
                    ValidateCurrentManifest();
            }
        }

        private async void ValidateCurrentManifest()
        {
            if (!ValidateRoot()) return;

            try
            {
                var provider = new FileSystemHandbookContentProvider(_rootFolder);
                var router = new HandbookLinkRouter();
                var parser = new HandbookMarkdownParser(router);

                var repo = new HandbookRepository();
                await repo.InitializeAsync(provider, parser, CancellationToken.None);

                var validator = new HandbookValidator();
                var report = await validator.ValidateAllAsync(repo, CancellationToken.None);

                int errors = 0, warnings = 0, infos = 0;
                foreach (var issue in report.Issues)
                {
                    if (issue.Severity == HandbookValidationSeverity.Error) errors++;
                    else if (issue.Severity == HandbookValidationSeverity.Warning) warnings++;
                    else infos++;
                }

                Debug.Log($"[HB-Gen][Validate] Issues: Errors={errors}, Warnings={warnings}, Infos={infos}");

                int printed = 0;
                foreach (var i in report.Issues)
                {
                    if (printed >= _maxIssuesToPrint) break;
                    Debug.Log($"[HB-Gen][{i.Severity}] {i.Code}: {i.Message} (page='{i.PageId}', anchor='{i.Anchor}', url='{i.LinkUrl}', step='{i.StepId}')");
                    printed++;
                }

                if (errors == 0)
                    Debug.Log("[HB-Gen][Validate] OK");
                else
                    Debug.LogWarning("[HB-Gen][Validate] Completed with errors. See log above.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[HB-Gen][Validate] Failed: {ex}");
            }
        }
        
        private void SaveRuntimeConfig()
        {
            if (_runtimeConfig == null)
                return;

            _runtimeConfig.RootFolder = _rootFolder;
            _runtimeConfig.Language = _language;
            _runtimeConfig.MediaBasePath = _mediaBasePath;
            _runtimeConfig.Version = _version;

            EditorUtility.SetDirty(_runtimeConfig);
            AssetDatabase.SaveAssets();
        }

        private void OnEnable()
        {
            InitDefaults();
            RebuildPageOptions();
        }

        private void RebuildPageOptions()
        {
            try
            {
                var manifestPath = Path.Combine(_rootFolder, "manifest.json");
                if (!File.Exists(manifestPath))
                {
                    _pageOptions = Array.Empty<string>();
                    _defaultPageIndex = -1;
                    return;
                }

                var json = File.ReadAllText(manifestPath);
                var existing = JsonConvert.DeserializeObject<HandbookManifest>(json);
                if (existing == null || existing.sections == null)
                {
                    _pageOptions = Array.Empty<string>();
                    _defaultPageIndex = -1;
                    return;
                }

                var pages = new List<HandbookPageRef>();
                CollectAllPages(existing.sections, pages, includeHidden: true);

                // Берём id страниц (slug)
                var ids = new List<string>(pages.Count);
                for (int i = 0; i < pages.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(pages[i].id))
                        ids.Add(pages[i].id);
                }

                // Упорядочим по алфавиту (можно убрать, если нужен «как в манифесте»)
                ids.Sort(StringComparer.OrdinalIgnoreCase);

                _pageOptions = ids.ToArray();

                if (!string.IsNullOrWhiteSpace(_defaultPageId))
                {
                    _defaultPageIndex = Array.IndexOf(_pageOptions, _defaultPageId);
                }

                if (_defaultPageIndex < 0 && _pageOptions.Length > 0)
                {
                    _defaultPageIndex = 0;
                    _defaultPageId = _pageOptions[0];
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HB-Gen] Failed to rebuild page options: {ex}");
                _pageOptions = Array.Empty<string>();
                _defaultPageIndex = -1;
            }
        }
        
        private void ScanAndPreview()
        {
            if (!ValidateRoot()) return;

            var scanRoot = Path.Combine(_rootFolder, "pages", _language).Replace('\\', '/');
            var scanner = new DirectoryScanner();
            var rootSections = scanner.ScanSections(scanRoot, allowRootPagesNode: true);
            
            var filler = new MetadataFiller();
            filler.FillForTree(rootSections, _language);

            var manifestPath = Path.Combine(_rootFolder, "manifest.json");
            HandbookManifest existing = null;
            if (File.Exists(manifestPath))
                existing = Newtonsoft.Json.JsonConvert.DeserializeObject<HandbookManifest>(File.ReadAllText(manifestPath));

            var merger = new ManifestMerger(_preserveTitles, _preserveTags, _preserveSummary, _preserveHidden);
            var manifest = merger.BuildManifest(rootSections, existing, _version, _language, _mediaBasePath, _defaultPageId);

            // Быстрое превью структуры в Console
            Debug.Log($"[HB-Gen] Preview Manifest: version='{manifest.version}', language='{manifest.language}', sections={manifest.sections.Count}");
            for (int i = 0; i < manifest.sections.Count; i++)
                LogSection(manifest.sections[i], 0);
        }

        private void GenerateAndSave()
        {
            if (!ValidateRoot()) return;

            try
            {
                var scanRoot = Path.Combine(_rootFolder, "pages", _language).Replace('\\', '/');

                var scanner = new DirectoryScanner();
                var rootSections = scanner.ScanSections(scanRoot);

                var filler = new MetadataFiller();
                filler.FillForTree(rootSections, _language);

                var manifestPath = Path.Combine(_rootFolder, "manifest.json");
                HandbookManifest existing = null;
                if (File.Exists(manifestPath))
                    existing = Newtonsoft.Json.JsonConvert.DeserializeObject<HandbookManifest>(File.ReadAllText(manifestPath));

                var merger = new ManifestMerger(_preserveTitles, _preserveTags, _preserveSummary, _preserveHidden);
                var manifest = merger.BuildManifest(rootSections, existing, _version, _language, _mediaBasePath, _defaultPageId);

                // Сохраняем
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(manifest, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(manifestPath, json);
                AssetDatabase.Refresh();

                Debug.Log($"[HB-Gen] Manifest saved: {manifestPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HB-Gen] Generate failed: {ex}");
            }
        }

        private bool ValidateRoot()
        {
            if (string.IsNullOrWhiteSpace(_rootFolder))
            {
                Debug.LogError("[HB-Gen] Root folder is empty.");
                return false;
            }

            if (!Directory.Exists(_rootFolder))
            {
                Debug.LogError($"[HB-Gen] Root folder does not exist: {_rootFolder}");
                return false;
            }

            var pagesDir = Path.Combine(_rootFolder, "pages", _language);
            if (!Directory.Exists(pagesDir))
            {
                Debug.LogError($"[HB-Gen] Pages directory does not exist: {pagesDir}");
                return false;
            }

            return true;
        }

        private void LogSection(HandbookSection s, int level)
        {
            var indent = new string(' ', level * 2);
            Debug.Log($"{indent}- Section '{s.title}' (id='{s.id}'), pages={(s.pages != null ? s.pages.Count : 0)}");

            if (s.pages != null)
            {
                for (int i = 0; i < s.pages.Count; i++)
                {
                    var p = s.pages[i];
                    Debug.Log($"{indent}  • Page '{p.title}' (id='{p.id}', updatedAt='{p.updatedAt}', hash='{p.hash}')");
                }
            }

            if (s.children != null)
            {
                for (int i = 0; i < s.children.Count; i++)
                    LogSection(s.children[i], level + 1);
            }
        }

        private void CollectAllPages(List<HandbookSection> sections, List<HandbookPageRef> sink, bool includeHidden)
        {
            if (sections == null) return;

            for (int i = 0; i < sections.Count; i++)
            {
                var s = sections[i];

                if (s.pages != null)
                {
                    for (int j = 0; j < s.pages.Count; j++)
                    {
                        var p = s.pages[j];
                        if (!p.hidden || includeHidden)
                            sink.Add(p);
                    }
                }

                if (s.children != null)
                    CollectAllPages(s.children, sink, includeHidden);
            }
        }
    }
}