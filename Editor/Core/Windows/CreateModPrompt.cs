using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ADOFAIModdingHelper.Core.Windows
{
    public class CreateModPrompt : EditorWindow
    {
        [SerializeField] private VisualTreeAsset mainPanel;

        private CreateModPromptData _localModInfo;
        private SerializedObject _serializedModInfo;

        // Main Information
        private TextField _idField;
        private TextField _nameField;
        private TextField _authorField;
        private TextField _versionField;

        // Mod ID
        private Toggle _modIDSameModName;

        // Asset Folder
        private VisualElement _assetFoldersContainer;
        private Toggle _assetsFolderToggle;
        private Button _assetFoldersDropdown;

        // Others
        private Button _createButton;
        private Label _ModNameEmptyWarning;

        private bool ShowAssetFolderDropdown = true;
        private bool SameModIDasModName = true;
        private EditorCoroutine _warningCoroutine;
        private EditorCoroutine _hideCoroutine;


        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Create Mod Template (Ctrl+Shift+M)", false, priority: Constants.ADOFAIModdingHelperMenuPriority - 1)]
        [MenuItem("Assets/" + Constants.ADOFAIModdingHelperRoot + "Create Mod Template (Ctrl+Shift+M)", false, priority: Constants.ADOFAIModdingHelperMenuPriority - 1)]
        public static void ShowWindow()
        {
            var wnd = GetWindow<CreateModPrompt>();
            wnd.titleContent = new GUIContent("Create Mod");
            wnd.minSize = new Vector2(650, 300);
        }

        [Shortcut("ADOFAIModdingHelper/Create Mod Template", KeyCode.M, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        private static void ShortcutHandler()
        {
            ShowWindow();
        }

        private void CreateGUI()
        {
            _localModInfo = ScriptableObject.CreateInstance<CreateModPromptData>();
            _serializedModInfo = new SerializedObject(_localModInfo);

            if (mainPanel != null)
            {
                mainPanel.CloneTree(rootVisualElement);
            }
            else
            {
                Debug.LogError("Assign the UXML in the inspector!");
                return;
            }

            // Main Information
            _idField = rootVisualElement.Q<TextField>("modIDTF");
            _nameField = rootVisualElement.Q<TextField>("modNameTF");
            _authorField = rootVisualElement.Q<TextField>("modAuthorTF");
            _versionField = rootVisualElement.Q<TextField>("modVersionTF");

            // Mod ID
            _modIDSameModName = rootVisualElement.Q<Toggle>("ModIDSameT");

            // Asset Folder
            _assetFoldersContainer = rootVisualElement.Q<VisualElement>("AssetFoldersContainer");
            _assetsFolderToggle = rootVisualElement.Q<Toggle>("IncludeAssetFolderT");
            _assetFoldersDropdown = rootVisualElement.Q<Button>("includeAssetFolderDropdown");

            // Others
            _createButton = rootVisualElement.Q<Button>("cmp-create");
            _ModNameEmptyWarning = rootVisualElement.Q<Label>("ModNameEmptyWarning");

            // Binding
            rootVisualElement.Bind(_serializedModInfo);

            // Custom Bindings
            _nameField.RegisterCallback<ChangeEvent<string>>(NameField);

            _assetsFolderToggle.RegisterCallback<ChangeEvent<bool>>(AssetsFolderToggleCallBack); // figuring this out pmo
            _assetFoldersDropdown.clicked += AssetsFolderDropdownButton;

            _modIDSameModName.Unbind();
            _modIDSameModName.RegisterCallback<ChangeEvent<bool>>(ModIDSameAsModName);

            _createButton.clicked += OnCreateClicked;

            // Initial setup
            _assetsFolderToggle.value = ShowAssetFolderDropdown;
            _modIDSameModName.value = SameModIDasModName;
            _localModInfo.ModVersion = "1.0.0";
        }

        // Update Mod ID
        private void NameField(ChangeEvent<string> evt)
        {
            if (SameModIDasModName) _idField.value = _nameField.value;
        }

        // Mod ID
        private void ModIDSameAsModName(ChangeEvent<bool> evt)
        {
            SameModIDasModName = evt.newValue;
            _idField.textEdition.isReadOnly = SameModIDasModName;
            if (evt.newValue) _idField.value = _nameField.value;
            _idField.SetEnabled(!SameModIDasModName);
        }

        // Asset Folder
        private void AssetsFolderDropdownButton()
        {
            ShowAssetFolderDropdown = !ShowAssetFolderDropdown;
            ShowAssetFolders(ShowAssetFolderDropdown);
        }
        private void AssetsFolderToggleCallBack(ChangeEvent<bool> evt)
        {
            _assetFoldersDropdown.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            ShowAssetFolders(evt.newValue ? ShowAssetFolderDropdown : false);
        }

        private void ShowAssetFolders(bool show)
        {
            if (_hideCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }

            _assetFoldersDropdown.style.rotate = ShowAssetFolderDropdown ? new Rotate(new Angle(0, AngleUnit.Degree)) : new Rotate(new Angle(-90, AngleUnit.Degree));

            float duration = UXMLUtils.GetUXMLAnimationProperty<TimeValue>(_assetFoldersContainer.style, "bottom").value; // ts didnt work broken heart emoji
            if (duration <= 0f) duration = 0.3f; // this is the one that keeps it working cuh

            if (show)
            {
                _assetFoldersContainer.style.opacity = 0f;
                _assetFoldersContainer.style.bottom = 10f;
                _assetFoldersContainer.style.display = DisplayStyle.Flex;

                _assetFoldersContainer.style.opacity = 1f;
                _assetFoldersContainer.style.bottom = 0f;
            }
            else
            {
                _assetFoldersContainer.style.display = DisplayStyle.Flex;
                _assetFoldersContainer.style.opacity = 1f;
                _assetFoldersContainer.style.bottom = 0f;

                _assetFoldersContainer.style.opacity = 0f;
                _assetFoldersContainer.style.bottom = 10f;

                _hideCoroutine = EditorCoroutineUtility.StartCoroutine(HideAfter(duration, () => _assetFoldersContainer.style.display = DisplayStyle.None), this);
            }
        }

        // Create Button
        private void OnCreateClicked()
        {
            bool ToReturn = false;
            if (string.IsNullOrEmpty(_localModInfo.ModName)) { ShowWarning(_ModNameEmptyWarning, "Mod name cannot be empty!!"); ToReturn = true; }
            else if (string.IsNullOrEmpty(_localModInfo.ModID)) { ShowWarning(_ModNameEmptyWarning, "Mod ID cannot be empty!!"); ToReturn = true; }
            else if (string.IsNullOrEmpty(_localModInfo.ModVersion)) { ShowWarning(_ModNameEmptyWarning, "Mod version cannot be empty!!"); ToReturn = true; }
            else if (!_localModInfo.BepInExCompatibility && !_localModInfo.UMMCompatibility) { ShowWarning(_ModNameEmptyWarning, "Choose a mod loader!!"); ToReturn = true; }
            if (ToReturn) return;

            _serializedModInfo.ApplyModifiedProperties();
            CreateModTemplate.CreateModAction(_localModInfo);

            Close();
        }

        // Helper
        private void ShowWarning(Label label, string text)
        {
            if (_warningCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_warningCoroutine);
                _warningCoroutine = null;
            }

            label.text = text;
            label.style.display = DisplayStyle.Flex;

            _warningCoroutine = EditorCoroutineUtility.StartCoroutine(HideAfter(5f, () => { label.style.display = DisplayStyle.None; }), this);
        }

        private IEnumerator HideAfter(float seconds, Action action)
        {
            double startTime = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - startTime < seconds)
            {
                yield return null;
            }

            action.Invoke();
            //Debug.Log("oabnsgoihasg");
        }

        [System.Serializable]
        public class CreateModPromptData : ScriptableObject
        {
            public string ModName;
            public string ModID;
            public string ModAuthor;
            public string ModVersion;

            public bool SceneFolder = false;

            public bool AssetFolder = true;
            public bool Texture2dFolder = true;
            public bool AudiosFolder = false;
            public bool PrefabsFolder = true;
            public bool ScriptsFolder = true;
            public bool FontsFolder = false;
            public bool MaterialsFolder = false;
            public bool ShadersFolder = false;
            public List<string> AdditionalAssetFolders = new List<string>();

            public bool BepInExCompatibility = true;
            public bool UMMCompatibility = true;
        }
    }
}
