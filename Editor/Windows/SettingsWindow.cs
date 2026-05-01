using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.ScriptableObjects;

namespace ADOFAIModdingHelper.Windows
{
    public class SettingsWindow : EditorWindow
    {
        // -----------------------------------------------------------------------
        // Serialized UXML Assets
        // -----------------------------------------------------------------------

        [SerializeField] private VisualTreeAsset MainPanel;
        [SerializeField] private VisualTreeAsset SettingsPanel;
        [SerializeField] private VisualTreeAsset ListElement;

        // -----------------------------------------------------------------------
        // UI Element Name Constants
        // -----------------------------------------------------------------------

        private static class Names
        {
            // Root
            public const string SettingsList = "settings-list";
            public const string SPContainer = "SPContainer";

            // Settings Panel — ADOFAI Path
            public const string PathTextField = "ADOFAIPathTextField";
            public const string PathBrowseButton = "ADOFAIPathBrowseButton";

            // Settings Panel — Import
            public const string ImportGameButton = "ImportGameButton";

            // Settings Panel — Tabs toggle
            public const string SeperateTabsToggle = "SeperateTabsToggle";
        }

        // -----------------------------------------------------------------------
        // State
        // -----------------------------------------------------------------------

        private readonly string[] _settingsOptions = { "Settings" };

        private ListView _settingsList;
        private VisualElement _spContainer;

        // -----------------------------------------------------------------------
        // Menu Item
        // -----------------------------------------------------------------------

        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Settings", false,
            priority: Constants.ADOFAIModdingHelperMenuPriority - 2)]
        public static void ShowSetting()
        {
            var settingsWindow = GetWindow<SettingsWindow>("Settings");
            GetWindow<ModsConfigWindow>("Mods Configs", typeof(SettingsWindow));
            settingsWindow.Focus();
        }

        // -----------------------------------------------------------------------
        // Lifecycle
        // -----------------------------------------------------------------------

        private void CreateGUI()
        {
            MainPanel.CloneTree(rootVisualElement);

            _settingsList = rootVisualElement.Q<ListView>(Names.SettingsList);
            _spContainer = rootVisualElement.Q<VisualElement>(Names.SPContainer);

            _settingsList.itemsSource = _settingsOptions;
            _settingsList.makeItem = () => ListElement.CloneTree().ElementAt(0);
            _settingsList.bindItem = (element, i) => element.Q<Label>().text = _settingsOptions[i];

            _settingsList.selectionChanged += items =>
            {
                _spContainer.Clear();
                if (items.FirstOrDefault() is not string selectedItem) return;
                OnSettingSelected(selectedItem);
            };

            _settingsList.SetSelection(0);
        }

        // -----------------------------------------------------------------------
        // Panel Loader
        // -----------------------------------------------------------------------

        private void OnSettingSelected(string item)
        {
            switch (item)
            {
                case "Settings":
                    SetupSettings();
                    break;
            }
        }

        // -----------------------------------------------------------------------
        // Settings Panel
        // -----------------------------------------------------------------------

        private void SetupSettings()
        {
            var panel = SettingsPanel.CloneTree();
            _spContainer.Add(panel);

            panel.Bind(new SerializedObject(Setting.Config));
            panel.style.flexGrow = 1;

            SetupPathBrowse(panel);
            SetupImportGame(panel);
            SetupSeparateTabsToggle(panel);
        }

        private void SetupPathBrowse(VisualElement panel)
        {
            var pathField = panel.Q<TextField>(Names.PathTextField);
            var browseButton = panel.Q<Button>(Names.PathBrowseButton);

            browseButton.clicked += () =>
            {
                // Logic from from https://github.com/ADOFAI-gg/ADOFAI-Modding-Toolkit
                string initialDirectory;
                string extension;

#if UNITY_EDITOR_WIN
                initialDirectory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\A Dance of Fire and Ice";
                extension = "exe";
#elif UNITY_EDITOR_OSX
                initialDirectory = "~/Library/Application Support/Steam/steamapps/common/ADanceOfFireAndIce";
                extension = "app";
#elif UNITY_EDITOR_LINUX
                initialDirectory = "~/.local/share/Steam/steamapps/common/A Dance of Fire and Ice";
                extension = string.Empty;
#else
                initialDirectory = Application.dataPath;
                extension = string.Empty;
#endif
                string path = EditorUtility.OpenFilePanel("Select ADOFAI Path", initialDirectory, extension);
                if (!string.IsNullOrEmpty(path))
                {
                    pathField.value = path;
                    pathField.Blur();
                }
            };
        }

        private static void SetupImportGame(VisualElement panel)
        {
            var importButton = panel.Q<Button>(Names.ImportGameButton);

            importButton.clicked += () =>
            {
                // Logic from from https://github.com/ADOFAI-gg/ADOFAI-Modding-Toolkit
                bool continueImporting = EditorUtility.DisplayDialog(
                    "Are you sure?",
                    "Importing the game assembly may take a while, and you will likely be asked to restart the Unity Editor.",
                    "Yes, continue",
                    "No");

                if (!continueImporting) return;

                var config = Setting.Config;

                if (string.IsNullOrEmpty(config.ADOFAIPath))
                {
                    EditorUtility.DisplayDialog("Error", "Please set the ADOFAI Executable Path first.", "OK");
                    return;
                }

                config.Importer.SetGamePath(config.ADOFAIPath);
                config.Importer.Import();
            };
        }

        private static void SetupSeparateTabsToggle(VisualElement panel)
        {
            var toggle = panel.Q<Toggle>(Names.SeperateTabsToggle);

            toggle.RegisterValueChangedCallback(evt =>
            {
                if (!HasOpenInstances<ModsConfigWindow>()) return;

                var modsConfigWindow = GetWindow<ModsConfigWindow>("Mods Configs", focus: false);
                modsConfigWindow?.UpdateSeparateTabs(evt.newValue);
            });
        }
    }
}