using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ADOFAIModdingHelper.Common;

namespace ADOFAIModdingHelper.Core.Windows
{
    public class CreateModPrompt : EditorWindow
    {
        [SerializeField] private VisualTreeAsset mainPanel;

        private CreateModPromptData _localModInfo;
        private SerializedObject _serializedModInfo;

        private TextField _nameField;
        private TextField _authorField;
        private TextField _versionField;
        private Button _createButton;

        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Create Mod Template", false, priority: Constants.ADOFAIModdingHelperMenuPriority)]
        [MenuItem("Assets/Create/" + Constants.ADOFAIModdingHelperRoot + "Mod Template", false, 0)]
        public static void ShowWindow()
        {
            var wnd = GetWindow<CreateModPrompt>();
            wnd.titleContent = new GUIContent("Create Mod");
            wnd.maxSize = new Vector2(400, 300);
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

            _nameField = rootVisualElement.Q<TextField>("modNameTF");
            _authorField = rootVisualElement.Q<TextField>("modAuthorTF");
            _versionField = rootVisualElement.Q<TextField>("modVersionTF");
            _createButton = rootVisualElement.Q<Button>("cmp-create");

            rootVisualElement.Bind(_serializedModInfo);

            _createButton.clicked += OnCreateClicked;
        }

        private void OnCreateClicked()
        {
            // Ensure serializedObject applies pending changes
            _serializedModInfo.ApplyModifiedProperties();

            CreateModTemplate.CreateModAction(_localModInfo);

            Close();
        }

        [System.Serializable]
        public class CreateModPromptData : ScriptableObject
        {
            public string ModName;
            public string ModAuthor;
            public string ModVersion;

            public bool SceneFolder = false;

            public bool AssetFolder = true;
            public bool Texture2dFolder = true;
            public bool AudiosFolder = true;
            public bool PrefabFolder = true;
            public bool ScriptsFolder = true;
            public bool FontsFolder = true;
            public bool MaterialsFolder = true;
            public bool ShadersFolder = true;

            public bool BepInExCompatibility = true;
            public bool UMMCompatibility = true;
        }
    }
}
