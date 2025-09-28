using ADOFAIRunner.Common;
using ADOFAIRunner.DefineSymbols.Core;
using System.IO;
using UnityEditor;
using UnityEngine;
using ADOFAIRunner.DefineSymbols.Toolbar;
using ADOFAIRunner.Toolbar;

namespace ADOFAIRunner.Core
{
    [InitializeOnLoad]
    public static class Main
    {
        public static Setting setting;
        static Main()
        {
            EditorApplication.delayCall += SetupEnvironment;
        }
        private static void SetupEnvironment()
        {
            Debug.Log("Setting up ADOFAI Runner environment...");

            if (!Directory.Exists(Constants.settingsFolder))
            {
                Directory.CreateDirectory(Constants.settingsFolder);
                AssetDatabase.Refresh();
            }

            string assetPath = Constants.settingsFolder + "/ADOFAIRunnerSettings.asset";
            setting = AssetDatabase.LoadAssetAtPath<Setting>(assetPath);

            if (setting == null)
            {
                Debug.Log("setting is Null!");
                setting = ScriptableObject.CreateInstance<Setting>();
                setting.Initialize(); 
                AssetDatabase.CreateAsset(setting, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created new settings asset at {assetPath}");
            }
            setting.AvailableBuildOptionsSelectedIndex = DefineSymbolToggler.GetBuildFromDefines();
            EditorUtility.SetDirty(setting);

            Logger.Init();
            RunADOFAIToolbar.Init();
            RunADOFAISymbolToolbar.Init();

            AssetDatabase.Refresh();
        }
    }
}
