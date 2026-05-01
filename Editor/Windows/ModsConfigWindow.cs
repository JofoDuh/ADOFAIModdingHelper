using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.ScriptableObjects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ADOFAIModdingHelper.Windows
{
    public class ModsConfigWindow : EditorWindow
    {
        // -----------------------------------------------------------------------
        // Serialized UXML Assets
        // -----------------------------------------------------------------------

        [SerializeField] private VisualTreeAsset MainPanel;
        [SerializeField] private VisualTreeAsset ModsPanel;
        [SerializeField] private VisualTreeAsset ListElement;
        [SerializeField] private VisualTreeAsset ButtonTemplate;

        // -----------------------------------------------------------------------
        // UI Element Name Constants
        // -----------------------------------------------------------------------

        private static class Names
        {
            // Root
            public const string ModList = "ModList";
            public const string ModConfigContainer = "ModConfigContainer";

            // Mod Info
            public const string ModEntry = "ModEntry";
            public const string NoModInfo = "NoModInfo";
            public const string ModInfoLabelButton = "ModEntryLabelButton";
            public const string ModInfoArrowButton = "ModEntryArrowButton";
            public const string ModInfoDataContainer = "ModInfoDataContainer";

            // Builder
            public const string AssetBundleList = "AssetBundleListView";
            public const string PrecompList = "PreCompListView";
            public const string Platforms = "Platforms";
            public const string AllPlatformToggle = "AllPlatformABToggle";
            public const string WindowsButton = "windowsButton";
            public const string MacOSButton = "macOSButton";
            public const string LinuxButton = "linuxButton";
            public const string DebugPresetButton = "debugBuildButton";
            public const string ReleasePresetButton = "relaseBuildButton";
            public const string ClearPresetButton = "clearBuildOptionButton";
            public const string BuildButton = "BuildButton";

            // Build Management
            public const string CachedBuildLabel = "CachedBuildLabel";
            public const string OpenBuildDirectory = "OpenBuildDirectory";
            public const string DeleteAllBuilds = "DeleteAllBuilds";

            // Shortcuts
            public const string SceneNav = "SceneNav";
            public const string AsmDefNav = "AsmDefNav";
            public const string RepoButton = "RepositoryButton";
            public const string IssuesButton = "IssuesButton";
            public const string PRButton = "PullRequestsButton";
            public const string GithubConfigLabelButton = "GithubConfigLabelButton";
            public const string GithubConfigArrowButton = "GithubConfigArrowButton";
            public const string GithubConfigContainer = "GithubConfigContainer";
            public const string ScenePathTextField = "ScenesDirectoryTextField";
            public const string ScenePathBrowsBtn = "SceneDirBrowseButton";
            public const string NoScenes = "NoScenes";
            public const string NoAsmdefs = "NoAsmdef";

            // Nav Buttons
            public const string NavBuild = "buildButton";
            public const string NavModInfo = "modInfoButton";
            public const string NavBuilder = "builderButton";
            public const string NavBuildManagement = "buildManagementButton";
            public const string NavShortcut = "shortcutButton";
            public const string BarSpacer = "BarSpacer";

            // Nav Panels
            public const string PanelModInfo = "ModInfoProperties";
            public const string PanelBuilder = "BuilderProperties";
            public const string PanelBuildManagement = "BuildManagementProperties";
            public const string PanelShortcut = "ShortcutProperties";
        }

        // -----------------------------------------------------------------------
        // Nav Mapping: button name, panel name
        // -----------------------------------------------------------------------

        private static readonly IReadOnlyDictionary<string, string> NavMap =
            new Dictionary<string, string>
            {
                { Names.NavModInfo,          Names.PanelModInfo },
                { Names.NavBuilder,          Names.PanelBuilder },
                { Names.NavBuildManagement,  Names.PanelBuildManagement },
                { Names.NavShortcut,         Names.PanelShortcut },
            };

        // -----------------------------------------------------------------------
        // State
        // -----------------------------------------------------------------------

        private List<ModToolsConfig> _configs;
        private ListView _modList;
        private VisualElement _modConfigContainer;
        private VisualElement _currentActivePanel;

        private static string _currentNav = Names.NavModInfo;
        private static bool _showModInfo = true;
        private static bool _showGithubConfig = true;

        // File caches — updated by watchers, avoids redundant directory scans
        private readonly HashSet<string> _scenes = new();
        private readonly HashSet<string> _asmDefs = new();
        private readonly Dictionary<string, string> _asmDefRelativePaths = new();

        // Watchers
        private FileSystemWatcher _buildDirectoryWatcher;
        private FileSystemWatcher _assemblyDefinitionsWatcher;
        private FileSystemWatcher _scenesWatcher;

        // -----------------------------------------------------------------------
        // Menu Items
        // -----------------------------------------------------------------------

        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Mods Configs", false,
            priority: Constants.ADOFAIModdingHelperMenuPriority - 2)]
        public static void OpenInfo()
        {
            _currentNav = Names.NavModInfo;
            ShowSetting();
        }

        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Builds Configs", false,
            priority: Constants.ADOFAIModdingHelperMenuPriority - 2)]
        public static void OpenBuild()
        {
            _currentNav = Names.NavBuilder;
            ShowSetting();
        }

        public static void ShowSetting()
        {
            GetWindow<SettingsWindow>("Settings");
            GetWindow<ModsConfigWindow>("Mods Configs", typeof(SettingsWindow));
        }

        // -----------------------------------------------------------------------
        // Lifecycle
        // -----------------------------------------------------------------------

        private void CreateGUI()
        {
            _configs = Setting.Config.AllMods;

            MainPanel.CloneTree(rootVisualElement);
            _modList = rootVisualElement.Q<ListView>(Names.ModList);
            _modConfigContainer = rootVisualElement.Q<VisualElement>(Names.ModConfigContainer);

            _modList.itemsSource = _configs;
            _modList.makeItem = () => ListElement.CloneTree().ElementAt(0);
            _modList.bindItem = (element, i) => element.Q<Label>().text = _configs[i].modInfo?.Id ?? "No Mod Info";
            _modList.selectionChanged += items =>
            {
                if (items.FirstOrDefault() is ModToolsConfig selected)
                    OnModSelected(selected);
            };

            _modList.SetSelection(Setting.Config.AllMods.FindIndex(c => c == Setting.Config.CurrentConfig));
        }

        private void OnDestroy()
        {
            _buildDirectoryWatcher = DisposeWatcher(_buildDirectoryWatcher);
            _assemblyDefinitionsWatcher = DisposeWatcher(_assemblyDefinitionsWatcher);
            _scenesWatcher = DisposeWatcher(_scenesWatcher);
        }

        // -----------------------------------------------------------------------
        // Selection Handler
        // -----------------------------------------------------------------------

        private void OnModSelected(ModToolsConfig config)
        {
            _modConfigContainer.Clear();

            var panel = ModsPanel.CloneTree();
            _currentActivePanel = panel;
            _modConfigContainer.Add(panel);

            SerializedObject soConfig = new SerializedObject(config);
            panel.Bind(soConfig);

            panel.TrackPropertyValue(soConfig.FindProperty("modInfo"), (prop) =>
            {
                _modList.RefreshItems();
                OnModSelected(config);
            });

            if (config.modInfo != null)
            {
                SerializedObject soModInfo = new SerializedObject(config.modInfo);
                panel.Bind(soModInfo);

                panel.Q<VisualElement>(Names.PanelModInfo).TrackPropertyValue(soModInfo.FindProperty("Id"), (prop) =>
                {
                    _modList.RefreshItems();
                });
            }
            SetupModInfo(config, panel);
            SetupBuilder(config, panel);
            SetupBuildManagement(config, panel);
            SetupShortcutsElements(config, panel);
            SetupShortcuts(config, panel);
            SetupNavButtons(panel);

            SetNavButton(panel, _currentNav);
            UpdateSeparateTabs(Setting.Config.SeperateBuildTabs);
            ShowContainer(config.modInfo != null && _showModInfo, ref _showModInfo, panel.Q<VisualElement>(Names.ModInfoDataContainer), panel.Q<Button>(Names.ModInfoArrowButton));
            ShowContainer(_showGithubConfig, ref _showGithubConfig, panel.Q<VisualElement>(Names.GithubConfigContainer), panel.Q<Button>(Names.GithubConfigArrowButton));
        }

        // -----------------------------------------------------------------------
        // Mod Info Section
        // -----------------------------------------------------------------------

        private void SetupModInfo(ModToolsConfig config, VisualElement panel)
        {
            panel.Q<VisualElement>(Names.ModEntry).style.display = config.modInfo != null ? DisplayStyle.Flex : DisplayStyle.None;
            panel.Q<VisualElement>(Names.NoModInfo).style.display = config.modInfo == null ? DisplayStyle.Flex : DisplayStyle.None;
            if (config.modInfo == null) { return; }
            panel.Q<Button>(Names.ModInfoLabelButton).clicked += () => ShowContainer(!_showModInfo, ref _showModInfo, panel.Q<VisualElement>(Names.ModInfoDataContainer), panel.Q<Button>(Names.ModInfoArrowButton));
            panel.Q<Button>(Names.ModInfoArrowButton).clicked += () => ShowContainer(!_showModInfo, ref _showModInfo, panel.Q<VisualElement>(Names.ModInfoDataContainer), panel.Q<Button>(Names.ModInfoArrowButton));
        }
        // -----------------------------------------------------------------------
        // Builder Section
        // -----------------------------------------------------------------------

        private void SetupBuilder(ModToolsConfig config, VisualElement panel)
        {
            var configSO = new SerializedObject(config);
            var platformContainer = panel.Q<VisualElement>(Names.Platforms);
            var allPlatformToggle = panel.Q<Toggle>(Names.AllPlatformToggle);

            var assetBundlesProp = configSO.FindProperty("AssetBundles");
            List<string> projectBundles = AssetDatabase.GetAllAssetBundleNames().ToList();

            SetupCustomList(panel.Q<ListView>(Names.AssetBundleList), assetBundlesProp, "BundleDropdown", projectBundles, "(No Bundles Found)");

            var precompProp = configSO.FindProperty("PrecompAssemblies");
            List<string> projectDlls = AssetDatabase.FindAssets("t:DefaultAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".dll"))
                .Select(Path.GetFileName)
                .ToList();

            SetupCustomList(panel.Q<ListView>(Names.PrecompList), precompProp, "DllDropdown", projectDlls, "(No DLLs Found)");

            void UpdatePlatformVisibility(bool buildAll) =>
                platformContainer.style.display = buildAll ? DisplayStyle.None : DisplayStyle.Flex;

            UpdatePlatformVisibility(config.buildEveryPlatform);
            allPlatformToggle.RegisterValueChangedCallback(evt => UpdatePlatformVisibility(evt.newValue));

            SetupPlatformToggleButton(panel, Names.WindowsButton, BuildTarget.StandaloneWindows64, config);
            SetupPlatformToggleButton(panel, Names.MacOSButton, BuildTarget.StandaloneOSX, config);
            SetupPlatformToggleButton(panel, Names.LinuxButton, BuildTarget.StandaloneLinux64, config);

            void ApplyPreset(string preset, bool buildAll)
            {
                config.ApplyPreset(preset);
                configSO.Update();
                UpdatePlatformVisibility(buildAll);
                allPlatformToggle.value = buildAll;
            }

            panel.Q<Button>(Names.DebugPresetButton).clicked += () => ApplyPreset("Debug", false);
            panel.Q<Button>(Names.ReleasePresetButton).clicked += () => ApplyPreset("Release", true);
            panel.Q<Button>(Names.ClearPresetButton).clicked += () => ApplyPreset("Clear", false);

            panel.Q<Button>(Names.BuildButton).clicked += () =>
            {
                string dest = config.copyToDirectory
                    ? Path.Combine(Path.GetDirectoryName(Setting.Config.ADOFAIPath)!, "Mods", config.modInfo?.Id ?? $"{config.name}_{config.GetInstanceID()}")
                    : null;
                config.BuildMod(dest);
            };
        }

        private static void SetupPlatformToggleButton(VisualElement panel, string buttonName,
            BuildTarget target, ModToolsConfig config)
        {
            var btn = panel.Q<Button>(buttonName);

            void Refresh()
            {
                bool active = config.BuildPlatforms.Contains(target);
                btn.style.backgroundColor = active
                    ? new StyleColor(new Color(0, 0.52f, 0.92f))
                    : new StyleColor(StyleKeyword.Null);
                btn.style.unityFontStyleAndWeight = active ? FontStyle.Bold : FontStyle.Normal;
            }

            Refresh();
            btn.clicked += () =>
            {
                var platforms = config.BuildPlatforms;
                if (!platforms.Add(target))
                    platforms.Remove(target);

                config.BuildPlatforms = platforms;
                EditorUtility.SetDirty(config);
                Refresh();
            };
        }

        private void SetupCustomList(ListView listView, SerializedProperty prop, string dropdownName, List<string> choices, string emptyText = "No Items Found")
        {
            listView.BindProperty(prop);

            var originalChoices = new List<string>(choices);
            bool hasChoices = originalChoices.Count > 0;

            List<string> BuildChoicesFor(int index)
            {
                if (!hasChoices)
                    return new List<string> { emptyText };

                var currentValue = prop.GetArrayElementAtIndex(index).stringValue;

                var usedElsewhere = new HashSet<string>();
                for (int j = 0; j < prop.arraySize; j++)
                {
                    if (j == index) continue;
                    var v = prop.GetArrayElementAtIndex(j).stringValue;
                    if (!string.IsNullOrEmpty(v)) usedElsewhere.Add(v);
                }

                var available = new List<string> { "None" };
                foreach (var choice in originalChoices)
                {
                    if (!usedElsewhere.Contains(choice) || choice == currentValue)
                        available.Add(choice);
                }
                return available;
            }

            listView.makeItem = () =>
            {
                var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                var label = new Label("Element")
                {
                    name = "IndexLabel",
                    style = { minWidth = 95, unityTextAlign = TextAnchor.MiddleLeft }
                };

                var dropdown = new DropdownField
                {
                    name = dropdownName,
                    style = { flexGrow = 1, paddingBottom = 2, paddingTop = 2, minHeight = 24 }
                };

                var textElement = dropdown.Q<TextElement>();
                if (textElement != null)
                {
                    textElement.style.textOverflow = TextOverflow.Ellipsis;
                    textElement.style.overflow = Overflow.Hidden;
                    textElement.style.whiteSpace = WhiteSpace.NoWrap;
                }

                dropdown.RegisterValueChangedCallback(evt =>
                {
                    if (container.userData is not int idx) return;
                    if (idx >= prop.arraySize) return;

                    var property = prop.GetArrayElementAtIndex(idx);

                    if (string.IsNullOrEmpty(evt.newValue) || evt.newValue == "None" || evt.newValue == emptyText)
                    {
                        property.stringValue = string.Empty;
                        dropdown.SetValueWithoutNotify("None");
                    }
                    else
                    {
                        property.stringValue = evt.newValue;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                    listView.schedule.Execute(listView.Rebuild);
                });

                container.Add(label);
                container.Add(dropdown);
                return container;
            };

            listView.bindItem = (element, i) =>
            {
                element.userData = i;

                element.Q<Label>("IndexLabel").text = $"Element {i}";

                var dropdown = element.Q<DropdownField>(dropdownName);
                var property = prop.GetArrayElementAtIndex(i);

                dropdown.choices = BuildChoicesFor(i);

                var display = string.IsNullOrEmpty(property.stringValue) ? "None" : property.stringValue;
                dropdown.SetValueWithoutNotify(display);
            };

            listView.unbindItem = (element, _) =>
            {
                element.userData = null;
            };

            listView.itemsAdded += indices =>
            {
                foreach (var index in indices)
                    prop.GetArrayElementAtIndex(index).stringValue = string.Empty;

                prop.serializedObject.ApplyModifiedProperties();
                listView.schedule.Execute(listView.Rebuild);
            };
        }

        // -----------------------------------------------------------------------
        // Build Management Section
        // -----------------------------------------------------------------------

        private void SetupBuildManagement(ModToolsConfig config, VisualElement panel)
        {
            string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", config.modInfo?.Id ?? $"{config.name}_{config.GetInstanceID()}");
            var cachedBuildLabel = panel.Q<Label>(Names.CachedBuildLabel);

            panel.Q<Button>(Names.OpenBuildDirectory).clicked += () =>
            {
                if (!Directory.Exists(buildPath)) Directory.CreateDirectory(buildPath);
                EditorUtility.RevealInFinder(buildPath);
            };

            panel.Q<Button>(Names.DeleteAllBuilds).clicked += () =>
            {
                config.DeleteBuilds(config.deleteBuildsExceptLastN);
                UpdateBuildStats(config, cachedBuildLabel);
            };

            UpdateBuildStats(config, cachedBuildLabel);
            ChangeWatchedFolder(buildPath, config, cachedBuildLabel);
        }

        // -----------------------------------------------------------------------
        // Shortcuts Section
        // -----------------------------------------------------------------------

        private void SetupShortcuts(ModToolsConfig config, VisualElement panel)
        {
            var scenesContainer = panel.Q<VisualElement>(Names.SceneNav);
            var asmDefContainer = panel.Q<VisualElement>(Names.AsmDefNav);

            _scenesWatcher = DisposeWatcher(_scenesWatcher);
            _scenesWatcher = CreateFileWatcher(config.ScenesPath, "*.unity", () =>
            {
                EditorApplication.delayCall += () =>
                {
                    _scenes.Clear();
                    ReloadSceneCache(config.ScenesPath);
                    RefreshButtonList(scenesContainer, ButtonTemplate, _scenes, SetupSceneButton);
                    CheckIfButtonListEmptyAndShowEmptyText(scenesContainer, panel.Q<VisualElement>(Names.NoScenes));
                };
            });

            _assemblyDefinitionsWatcher = DisposeWatcher(_assemblyDefinitionsWatcher);
            _assemblyDefinitionsWatcher = CreateFileWatcher(Application.dataPath, "*.asmdef", () =>
            {
                EditorApplication.delayCall += () =>
                {
                    _asmDefs.Clear();
                    _asmDefRelativePaths.Clear();
                    ReloadAsmDefCache();
                    RefreshButtonList(asmDefContainer, ButtonTemplate, _asmDefs, SetupAsmDefButton);
                    CheckIfButtonListEmptyAndShowEmptyText(asmDefContainer, panel.Q<VisualElement>(Names.NoAsmdefs));
                };
            });

            _scenes.Clear();
            _asmDefs.Clear();
            _asmDefRelativePaths.Clear();

            ReloadSceneCache(config.ScenesPath);
            ReloadAsmDefCache();
            RefreshButtonList(scenesContainer, ButtonTemplate, _scenes, SetupSceneButton);
            RefreshButtonList(asmDefContainer, ButtonTemplate, _asmDefs, SetupAsmDefButton);
            CheckIfButtonListEmptyAndShowEmptyText(scenesContainer, panel.Q<VisualElement>(Names.NoScenes));
            CheckIfButtonListEmptyAndShowEmptyText(asmDefContainer, panel.Q<VisualElement>(Names.NoAsmdefs));
        }

        private void SetupShortcutsElements(ModToolsConfig config, VisualElement panel)
        {
            panel.Q<Button>(Names.GithubConfigLabelButton).clicked += () => ShowContainer(!_showGithubConfig, ref _showGithubConfig, panel.Q<VisualElement>(Names.GithubConfigContainer), panel.Q<Button>(Names.GithubConfigArrowButton));
            panel.Q<Button>(Names.GithubConfigArrowButton).clicked += () => ShowContainer(!_showGithubConfig, ref _showGithubConfig, panel.Q<VisualElement>(Names.GithubConfigContainer), panel.Q<Button>(Names.GithubConfigArrowButton));

            panel.Q<Button>(Names.RepoButton).clicked += () => Application.OpenURL(config.RepositoryLink);
            panel.Q<Button>(Names.IssuesButton).clicked += () => Application.OpenURL($"{config.RepositoryLink}/issues");
            panel.Q<Button>(Names.PRButton).clicked += () => Application.OpenURL($"{config.RepositoryLink}/pulls");

            var ScenePathTextfield = panel.Q<TextField>(Names.ScenePathTextField);
            var ScenePathBrowseButton = panel.Q<Button>(Names.ScenePathBrowsBtn);

            var scenesContainer = panel.Q<VisualElement>(Names.SceneNav);
            ScenePathTextfield.RegisterCallback<BlurEvent>(evt =>
            {
                _scenesWatcher = DisposeWatcher(_scenesWatcher);
                _scenesWatcher = CreateFileWatcher(config.ScenesPath, "*.unity", () =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        _scenes.Clear();
                        ReloadSceneCache(config.ScenesPath);
                        RefreshButtonList(scenesContainer, ButtonTemplate, _scenes, SetupSceneButton);
                        CheckIfButtonListEmptyAndShowEmptyText(scenesContainer, panel.Q<VisualElement>(Names.NoScenes));
                    };
                });
                _scenes.Clear();
                ReloadSceneCache(config.ScenesPath);
                RefreshButtonList(scenesContainer, ButtonTemplate, _scenes, SetupSceneButton);
                CheckIfButtonListEmptyAndShowEmptyText(scenesContainer, panel.Q<VisualElement>(Names.NoScenes));
            });

            ScenePathBrowseButton.clicked += () =>
            {

                string path = EditorUtility.OpenFolderPanel("Select Scenes Directory for Current Mod", Path.GetDirectoryName(AssetDatabase.GetAssetPath(config)), "Scenes");
                if (!string.IsNullOrEmpty(path))
                {
                    ScenePathTextfield.value = path;
                    ScenePathTextfield.Blur();
                }
            };
        }
        // -----------------------------------------------------------------------
        // File Cache Helpers
        // -----------------------------------------------------------------------

        private void ReloadSceneCache(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            foreach (var file in Directory.GetFiles(path, "*.unity", SearchOption.AllDirectories))
                _scenes.Add(Path.GetFullPath(file));
        }

        private void ReloadAsmDefCache()
        {
            foreach (var file in Directory.GetFiles(Application.dataPath, "*.asmdef", SearchOption.AllDirectories))
            {
                string full = Path.GetFullPath(file);
                if (_asmDefs.Add(full))
                    _asmDefRelativePaths[full] = ToRelativePath(full);
            }
        }

        private static string ToRelativePath(string fullPath)
        {
            string dataPath = Path.GetFullPath(Application.dataPath);
            return fullPath.StartsWith(dataPath)
                ? "Assets" + fullPath[dataPath.Length..].Replace('\\', '/')
                : null;
        }

        // -----------------------------------------------------------------------
        // Button List Builder
        // -----------------------------------------------------------------------

        private static void CheckIfButtonListEmptyAndShowEmptyText(VisualElement container, VisualElement text)
        {
            text.style.display = container.childCount != 0 ? DisplayStyle.None : DisplayStyle.Flex;
        }
        private static void RefreshButtonList(VisualElement container, VisualTreeAsset template,
            HashSet<string> files, System.Action<Button, string> setup)
        {
            foreach (var child in container.Children().Where(c => c.name != "TemplateNamePlaceholder").ToList())
                child.RemoveFromHierarchy();

            foreach (var file in files)
            {
                var btn = (Button)template.CloneTree().ElementAt(0);
                btn.text = Path.GetFileNameWithoutExtension(file);
                setup(btn, file);
                container.Add(btn);
            }
        }

        private static void SetupSceneButton(Button btn, string file)
        {
            btn.clicked += () =>
            {
                if (!TryPingAsset(file)) return;
                SwitchToScene(Path.GetFullPath(file));
            };
        }

        private static void SetupAsmDefButton(Button btn, string file)
        {
            btn.clicked += () => TryPingAsset(file);
        }

        private static bool TryPingAsset(string file)
        {
            string relativePath = ToRelativePath(Path.GetFullPath(file));
            if (relativePath == null)
            {
                Debug.LogWarning($"File is outside the Assets folder and cannot be loaded: {file}");
                return false;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            if (asset == null)
            {
                Debug.LogWarning($"Could not load asset at: {relativePath}");
                return false;
            }

            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
            return true;
        }

        // -----------------------------------------------------------------------
        // Navigation
        // -----------------------------------------------------------------------

        private void SetupNavButtons(VisualElement panel)
        {
            panel.Q<Button>(Names.NavBuild).clicked += () => SetNavButton(panel, Names.NavBuild);
            panel.Q<Button>(Names.NavModInfo).clicked += () => SetNavButton(panel, Names.NavModInfo);
            panel.Q<Button>(Names.NavBuilder).clicked += () => SetNavButton(panel, Names.NavBuilder);
            panel.Q<Button>(Names.NavBuildManagement).clicked += () => SetNavButton(panel, Names.NavBuildManagement);
            panel.Q<Button>(Names.NavShortcut).clicked += () => SetNavButton(panel, Names.NavShortcut);
        }

        private void SetNavButton(VisualElement container, string button)
        {
            foreach (var (navBtn, navPanel) in NavMap)
            {
                container.Q<Button>(navBtn).SetEnabled(navBtn != button);
                container.Q<VisualElement>(navPanel).style.display =
                    navBtn == button ? DisplayStyle.Flex : DisplayStyle.None;
            }

            bool isCombinedBuild = button == Names.NavBuild;
            container.Q<Button>(Names.NavBuild).SetEnabled(!isCombinedBuild);
            container.Query<VisualElement>(Names.BarSpacer).ForEach(e =>
                e.style.display = isCombinedBuild ? DisplayStyle.Flex : DisplayStyle.None);

            if (isCombinedBuild)
            {
                container.Q<VisualElement>(Names.PanelModInfo).style.display = DisplayStyle.Flex;
                container.Q<VisualElement>(Names.PanelBuilder).style.display = DisplayStyle.Flex;
                container.Q<VisualElement>(Names.PanelBuildManagement).style.display = DisplayStyle.Flex;
            }

            _currentNav = button;
        }

        public void UpdateSeparateTabs(bool separate)
        {
            if (_currentActivePanel == null) return;

            _currentActivePanel.Q<Button>(Names.NavBuild).style.display =
                separate ? DisplayStyle.None : DisplayStyle.Flex;
            _currentActivePanel.Q<Button>(Names.NavModInfo).style.display =
                separate ? DisplayStyle.Flex : DisplayStyle.None;
            _currentActivePanel.Q<Button>(Names.NavBuilder).style.display =
                separate ? DisplayStyle.Flex : DisplayStyle.None;
            _currentActivePanel.Q<Button>(Names.NavBuildManagement).style.display =
                separate ? DisplayStyle.Flex : DisplayStyle.None;

            // tf is ts this abomination :broken_heart:
            string nav = separate ? 
                _currentNav == Names.NavBuild ? Names.NavBuilder : _currentNav 
                : 
                _currentNav == Names.NavShortcut ? Names.NavShortcut : Names.NavBuild;

            /* if (seperated tab) {
             *     if (current nav is nav build) default to Builder
             *     else stay current nav
             *     }
             * else {
             *     if (current nav is nav short cut) then stay shortcut
             *     else go to Build tab
             *     }
             *     
             */

            SetNavButton(_currentActivePanel, nav);
        }

        // -----------------------------------------------------------------------
        // File System Watchers
        // -----------------------------------------------------------------------

        private static FileSystemWatcher CreateFileWatcher(string path, string filter, System.Action onChange)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
                             | NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = filter,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };

            FileSystemEventHandler handler = (_, _) => onChange();
            watcher.Created += handler;
            watcher.Deleted += handler;
            watcher.Changed += handler;
            watcher.Renamed += (_, _) => onChange();
            return watcher;
        }

        public void ChangeWatchedFolder(string path, ModToolsConfig config, Label label)
        {
            _buildDirectoryWatcher = DisposeWatcher(_buildDirectoryWatcher);
            _buildDirectoryWatcher = CreateFileWatcher(path, "*",
                () => EditorApplication.delayCall += () => UpdateBuildStats(config, label));
        }

        private static FileSystemWatcher DisposeWatcher(FileSystemWatcher watcher)
        {
            if (watcher == null) return null;
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            return null;
        }

        // -----------------------------------------------------------------------
        // Build Stats
        // -----------------------------------------------------------------------

        private static void UpdateBuildStats(ModToolsConfig config, Label label)
        {
            if (config == null || label == null) return;

            string path = Path.Combine(Directory.GetCurrentDirectory(), "Builds", config.modInfo?.Id ?? $"{config.name}_{config.GetInstanceID()}");
            if (!Directory.Exists(path))
            {
                label.text = "Cached Builds: 0 (0 B)";
                return;
            }

            try
            {
                var di = new DirectoryInfo(path);
                long totalBytes = di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
                label.text = $"Cached Builds: {di.GetDirectories().Length} ({FormatBytes(totalBytes)})";
            }
            catch (IOException) { }
        }

        private static string FormatBytes(long bytes) => bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:0.##} KB",
            < 1024 * 1024 * 1024L => $"{bytes / (1024.0 * 1024.0):0.##} MB",
            _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):0.##} GB",
        };

        // -----------------------------------------------------------------------
        // Other Shi
        // -----------------------------------------------------------------------

        private static void SwitchToScene(string path)
        {
            if (Application.isPlaying)
            {
                string name = path.Contains('/') ? path.Split('/').Last() : path;
                SceneManager.LoadScene(name);
            }
            else if (!SceneManager.GetActiveScene().name.Contains(path))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(path);
            }
        }
        private void ShowContainer(bool show, ref bool valueHolder, VisualElement Container, Button Arrow)
        {
            valueHolder = show;
            Arrow.style.rotate = show ? new Rotate(new Angle(0, AngleUnit.Degree)) : new Rotate(new Angle(-90, AngleUnit.Degree));
            Container.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}