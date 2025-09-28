using ADOFAIRunner.Common;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ADOFAIRunner.Core.Windows
{
    public class SettingsWindow : EditorWindow
    {

        [SerializeField] private VisualTreeAsset MainPanel;

        private string[] settingsOptions =
        {
            "General",
            "Unity Mod Manager",
            "BepInEx"
        };
        ListView _settingslist;
        VisualElement _settingPanelContainer;
        Setting _adofaiSetting;

        [MenuItem(Constants.ADOFAIRunnerMenuRoot + "Settings", false, priority: Constants.ADOFAIRunnerMenuPriority)]
        public static void ShowSetting()
        {
            GetWindow<SettingsWindow>("Settings");
        }

        private void CreateGUI()
        {
            Debug.Log(Constants.ADOFAIRunnerRootPath);
            MainPanel.CloneTree(rootVisualElement);
            _settingslist = rootVisualElement.Q<ListView>("settings-list");
            _settingPanelContainer = rootVisualElement.Q<VisualElement>("SPContainer");

            _adofaiSetting = Main.setting;

            _settingslist.itemsSource = settingsOptions;
            _settingslist.makeItem = () => new Label();
            _settingslist.bindItem = (element, i) =>
            {
                (element as Label).text = settingsOptions[i];
            };

            _settingslist.selectionChanged += (selectedItems) =>
            {
                _settingPanelContainer.Clear();

                if (selectedItems.FirstOrDefault() is not string selectedItem) return;


                switch (selectedItem)
                {
                    case "General":
                        var generalPanelTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{Constants.ADOFAIRunnerRootPath}/UXML/SettingPanels/General/ARSPGeneral.uxml");
                        var generalPanelRoot = generalPanelTemplate.CloneTree();
                        _settingPanelContainer.Add(generalPanelRoot);
                        generalPanelRoot.Bind(new SerializedObject(_adofaiSetting));
                        generalPanelRoot.style.flexGrow = 1;

                        var generalFolderField = generalPanelRoot.Q<TextField>("THUNDERKITEPTF");

                        var generalBrowseFolderButton = generalPanelRoot.Q<UnityEngine.UIElements.Button>("thunderkit-export-browse");

                        generalBrowseFolderButton.clicked += () =>
                        {
                            string path = EditorUtility.OpenFolderPanel("Select Thunderkit Export Folder", "", "");
                            if (!string.IsNullOrEmpty(path))
                            {
                                generalFolderField.value = path;
                                generalFolderField.Blur();
                            }
                        };

                        break;
                    case "Unity Mod Manager":
                        var ummPanelTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{Constants.ADOFAIRunnerRootPath}/UXML/SettingPanels/UnityModManager/ARSPUMM.uxml");
                        var ummPanelRoot = ummPanelTemplate.CloneTree();
                        _settingPanelContainer.Add(ummPanelRoot);                
                        ummPanelRoot.Bind(new SerializedObject(_adofaiSetting));
                        ummPanelRoot.style.flexGrow = 1;

                        var ummFolderField = ummPanelRoot.Q<TextField>("UMMMODPATHTF");
                        var ummExeField = ummPanelRoot.Q<TextField>("UMMEXETF");

                        var ummBrowseFolderButton = ummPanelRoot.Q<UnityEngine.UIElements.Button>("umm-mod-browse");
                        var ummBrowseExeButton = ummPanelRoot.Q<UnityEngine.UIElements.Button>("umm-exe-browse");

                        ummBrowseFolderButton.clicked += () =>
                        {
                            string path = EditorUtility.OpenFolderPanel("Select UMM Mod Folder", "", "");
                            if (!string.IsNullOrEmpty(path))
                            {
                                ummFolderField.value = path;  
                                ummFolderField.Blur();        
                            }
                        };

                        ummBrowseExeButton.clicked += () =>
                        {
                            string path = EditorUtility.OpenFilePanel("Select A Dance of Fire and Ice.exe", "", "exe");
                            if (!string.IsNullOrEmpty(path))
                            {
                                ummExeField.value = path;
                                ummExeField.Blur();
                            }
                        };
                        break;
                    case "BepInEx":
                        var bipPanelTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{Constants.ADOFAIRunnerRootPath}/UXML/SettingPanels/BepInEx/ARSPBepInEx.uxml");
                        var bipPanelRoot = bipPanelTemplate.CloneTree();
                        _settingPanelContainer.Add(bipPanelRoot);
                        bipPanelRoot.Bind(new SerializedObject(_adofaiSetting));
                        bipPanelRoot.style.flexGrow = 1;

                        var bipFolderField = bipPanelRoot.Q<TextField>("BIPMODPATHTF");
                        var bipExeField = bipPanelRoot.Q<TextField>("BIPEXETF");

                        var bipBrowseFolderButton = bipPanelRoot.Q<UnityEngine.UIElements.Button>("bip-mod-browse");
                        var bipBrowseExeButton = bipPanelRoot.Q<UnityEngine.UIElements.Button>("bip-exe-browse");

                        bipBrowseFolderButton.clicked += () =>
                        {
                            string path = EditorUtility.OpenFolderPanel("Select BepInEx Mod Folder", "", "");
                            if (!string.IsNullOrEmpty(path))
                            {
                                bipFolderField.value = path;
                                bipFolderField.Blur();
                            }
                        };

                        bipBrowseExeButton.clicked += () =>
                        {
                            string path = EditorUtility.OpenFilePanel("Select A Dance of Fire and Ice.exe", "", "exe");
                            if (!string.IsNullOrEmpty(path))
                            {
                                bipExeField.value = path;
                                bipExeField.Blur();
                            }
                        };
                        break;
                }
            };

            _settingslist.SetSelection(0);
        }
    }
}