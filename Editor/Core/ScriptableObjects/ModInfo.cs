using ADOFAIModdingHelper.Common;
using System;
using System.IO;
using ThunderKit.Core.Actions;
using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.Core.ScriptableObjects
{
    public static class CreateModInfo
    {
        [MenuItem("Assets/" + Constants.ADOFAIModdingHelperRoot + "Create Mod Info", false, priority: Constants.ADOFAIModdingHelperMenuPriority)]

        // Original code from ThunderKit -> SelectNewAsset<T> Method
        public static void CreateModInfoAction()
        {
            ModInfo asset = ScriptableObject.CreateInstance<ModInfo>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            var name = typeof(ModInfo).Name;
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

    public class ModInfo : ScriptableObject
    {
        public ModInfoUMM modInfoUMM;
        public ModInfoBIE modInfoBIE;
    }

    [Serializable]
    public class ModInfoUMM
    {
        public string Id;
        public string DisplayName;
        public string Author;
        public string Version;
        public string ManagerVersion;
        public string GameVersion;
        public string[] Requirements;
        public string[] LoadAfter;
        public string AssemblyName;
        public string EntryMethod;
        public string HomePage;
        public string Repository;
        public string ContentType;
    }

    [Serializable]
    public class ModInfoBIE
    {
        public string GUID;
        public string PluginName;
        public string PluginVersion;
        public string BIPModInfoCSPath;
    }
}