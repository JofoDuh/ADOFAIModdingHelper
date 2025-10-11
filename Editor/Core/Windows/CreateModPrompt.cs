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
        [SerializeField] private Texture2D dropdownSprite;

        private CreateModPromptData _localModInfo;
        private SerializedObject _serializedModInfo;

        // Main Information
        private TextField _idField;
        private TextField _nameField;
        private TextField _authorField;
        private TextField _versionField;

        // Mod ID
        private Toggle _modIDSameModName;

        // Scene Template
        private VisualElement _sceneTemplateContainer;
        private Toggle _sceneTemplateToggle;
        private Button _sceneTemplateDropdown;

        // Asset Folder
        private VisualElement _assetFoldersContainer;
        private Toggle _assetsFolderToggle;
        private Button _assetFoldersDropdown;

        // Others
        private Button _createButton;
        private Label _ModNameEmptyWarning;

        private bool ShowAssetFolderDropdown = true;
        private bool ShowSceneTemplateDropdown = true;
        private bool SameModIDasModName = true;

        private EditorCoroutine _warningCoroutine;
        private EditorCoroutine _hideAssetCoroutine;
        private EditorCoroutine _hideSceneCoroutine;

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

            // Scene Folder
            _sceneTemplateContainer = rootVisualElement.Q<VisualElement>("SceneTemplateContainer");
            _sceneTemplateToggle = rootVisualElement.Q<Toggle>("IncludeSceneFolderT");
            _sceneTemplateDropdown = rootVisualElement.Q<Button>("includeSceneFolderDropdown");

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

            _assetsFolderToggle.RegisterCallback<ChangeEvent<bool>>((x) => DropdownToggleCallBack(x, _assetFoldersContainer, _assetFoldersDropdown, ref _hideAssetCoroutine, ShowAssetFolderDropdown)); // figuring this out pmo
            _assetFoldersDropdown.clicked += () => DropdownButton(ref ShowAssetFolderDropdown, _assetFoldersContainer, _assetFoldersDropdown, ref _hideAssetCoroutine);

            _sceneTemplateToggle.RegisterCallback<ChangeEvent<bool>>((x) => DropdownToggleCallBack(x, _sceneTemplateContainer, _sceneTemplateDropdown, ref _hideSceneCoroutine, ShowSceneTemplateDropdown)); 
            _sceneTemplateDropdown.clicked += () => DropdownButton(ref ShowSceneTemplateDropdown, _sceneTemplateContainer, _sceneTemplateDropdown, ref _hideSceneCoroutine);

            _modIDSameModName.Unbind();
            _modIDSameModName.RegisterCallback<ChangeEvent<bool>>(ModIDSameAsModName);

            _createButton.clicked += OnCreateClicked;

            // Initial setup
            if (dropdownSprite != null)
            {
                Background background = Background.FromTexture2D(dropdownSprite);
                _assetFoldersDropdown.style.backgroundImage = background;
                _sceneTemplateDropdown.style.backgroundImage = background;
            }
            else
            {
                Debug.LogWarning("Dropdown sprite is not assigned in the inspector!");
            }

            DropdownToggleCallBack(ChangeEvent<bool>.GetPooled(false, _localModInfo.SceneFolder), _sceneTemplateContainer, _sceneTemplateDropdown, ref _hideSceneCoroutine, ShowSceneTemplateDropdown, true);
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

        // Dropdown Function
        private void DropdownButton(ref bool showdropdown, VisualElement container, Button dropdown, ref EditorCoroutine coroutine)
        {
            showdropdown = !showdropdown;
            ShowDropdown(showdropdown, container, dropdown, ref coroutine, showdropdown);
        }
        private void DropdownToggleCallBack(ChangeEvent<bool> evt, VisualElement container, Button dropdown, ref EditorCoroutine coroutine, bool showdropdown, bool instant = false)
        {
            dropdown.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            ShowDropdown(evt.newValue ? showdropdown : false, container, dropdown, ref coroutine, showdropdown, instant);
        }

        private void ShowDropdown(bool show, VisualElement container, Button dropdown, ref EditorCoroutine coroutine, bool Showdropdown, bool instant = false)
        {
            if (coroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(coroutine);
                coroutine = null;
            }

            dropdown.style.rotate = Showdropdown ? new Rotate(new Angle(0, AngleUnit.Degree)) : new Rotate(new Angle(-90, AngleUnit.Degree));

            float duration = UXMLUtils.GetUXMLAnimationProperty<TimeValue>(container.style, "bottom").value; // ts didnt work broken heart emoji
            if (duration <= 0f) duration = 0.3f; // this is the one that keeps it working cuh

            if (show)
            {
                container.style.opacity = 0f;
                container.style.bottom = 10f;
                container.style.display = DisplayStyle.Flex;

                container.style.opacity = 1f;
                container.style.bottom = 0f;
            }
            else
            {
                container.style.display = DisplayStyle.Flex;
                container.style.opacity = 1f;
                container.style.bottom = 0f;

                container.style.opacity = 0f;
                container.style.bottom = 10f;

                coroutine = EditorCoroutineUtility.StartCoroutine(HideAfter(instant ? 0 : duration, () => container.style.display = DisplayStyle.None), this);
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
        }

        [System.Serializable]
        public class CreateModPromptData : ScriptableObject
        {
            public string ModName;
            public string ModID;
            public string ModAuthor;
            public string ModVersion;

            public bool SceneFolder = false;
            public bool SceneTemplate = true;

            public bool AssetFolder = true;
            public bool Texture2dFolder = true;
            public bool AudioFolder = false;
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
