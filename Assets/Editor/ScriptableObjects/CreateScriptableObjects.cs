using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Utilities;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace ADOFAIModdingHelper.ScriptableObjects
{
    public static class CreateScriptableObjects
    {
        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Create Mod Info", false, priority: Constants.ADOFAIModdingHelperMenuPriority)]
        public static void CreateModInfo()
        {
            CreateShitsAction<ModInfo>();
        }

        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Create Mod Config", false, priority: Constants.ADOFAIModdingHelperMenuPriority)]
        public static void CreateModConfig()
        {
            CreateShitsAction<ModToolsConfig>();
        }

        // Original code from ThunderKit -> SelectNewAsset<T> Method
        public static void CreateShitsAction<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = ProjectWindowPathHelper.GetCurrentProjectWindowPath();
                if (path == "") path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            var name = typeof(T).Name;
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
    public class SelfDestructingActionAsset : EndNameEditAction
    {
        public Action<int, string, string> action;

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            action(instanceId, pathName, resourceFile);
            CleanUp();
        }
    }
}