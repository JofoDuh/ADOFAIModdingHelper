using ADOFAIModdingHelper.Core.ScriptableObjects;
using ADOFAIRunner.Common;
using ADOFAIRunner.Core.DataStructures;
using ADOFAIRunner.DefineSymbols.Core;
using System;
using System.Collections.Generic;
using System.IO;
using ThunderKit.Core.Actions;
using UnityEditor;
using UnityEngine;

namespace ADOFAIRunner.Core
{
    public static class CreateADOFAIRunnerSetting
    {
        [MenuItem("Assets/" + "ADOFAI Modding Helper/" + "ADOFAI Runner/" + "Create ADOFAI Runner Setting", false, priority: Constants.ADOFAIRunnerMenuPriority)]

        // Original code from ThunderKit -> SelectNewAsset<T> Method
        public static void CreateADOFAIRunnerSettingAction()
        {
            foreach (var file in Directory.GetFiles(Constants.settingsFolder, "*.asset"))
            {
                var assetFile = AssetDatabase.LoadAssetAtPath<Setting>(file);
                if (assetFile != null)
                {
                    Debug.Log("ADOFAIRunnerSetting already exists at: " + file);
                    return;
                }
            }
            Setting asset = ScriptableObject.CreateInstance<Setting>();

            string path = Constants.settingsFolder;
            var name = "ADOFAIRunnerSettings";
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/{name}.asset");
            Action<int, string, string> action =
                (int instanceId, string pathname, string resourceFile) =>
                {
                    AssetDatabase.CreateAsset(asset, pathname);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Selection.activeObject = asset;
                };

            var endAction = ScriptableObject.CreateInstance<SelfDestructingActionAsset>();
            endAction.action = action;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(asset.GetInstanceID(), endAction, assetPathAndName, null, null);
        }
    }

    public class Setting : ScriptableObject
    {
        public string BepInExModFolderPath;
        public string BepInExExePath;

        public string UnityModManagerExePath;
        public string UMMModFolderPath;

        public string ThunderkitOutputPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "ThunderKit");

        public List<ModBuild> AvailableMods = new List<ModBuild>();
        public int AvailableModsSelectedIndex;

        public bool IncludePDBFile = true;
        public string[] AvailableBuildOptions = new string[] { "Unity Mod Manager", "BepInEx" };
        [SerializeField] int _AvailableBuildOptionsSelectedIndex;
        public  int AvailableBuildOptionsSelectedIndex
        {
            get
            {
                return _AvailableBuildOptionsSelectedIndex;
            }
            set
            {
                if (value == _AvailableBuildOptionsSelectedIndex) return;
                _AvailableBuildOptionsSelectedIndex = value;
                DefineSymbolToggler.SetBuild(value);
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void Initialize()
        {
            DefineSymbols.Core.DefineSymbolToggler.SetBuild(0);
            string rootPath = Directory.GetParent(Application.dataPath).FullName;
            string gitignorePath = Path.Combine(rootPath, ".gitignore");

            if (!File.Exists(gitignorePath))
            {
                Debug.LogWarning($".gitignore not found at {gitignorePath}");
                return;
            }

            string header = "# Jofo Setting";
            string rule = "/[Aa]ssets/ADOFAIRunnerSettings";

            string all = File.ReadAllText(gitignorePath);

            bool endsWithNewline = all.EndsWith("\n") || all.EndsWith("\r");

            if (!all.Contains(rule))
            {
                using (StreamWriter sw = File.AppendText(gitignorePath))
                {
                    if (!endsWithNewline) sw.WriteLine(); 
                    sw.WriteLine("");
                    sw.WriteLine(header);
                    sw.Write(rule);
                }

                Debug.Log("Added ADOFAIRunnerSettings ignore rule to .gitignore");
            }
            else
            {
                Debug.Log("Rule already exists in .gitignore");
            }
        }
    }
}