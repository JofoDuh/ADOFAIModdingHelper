using System.IO;
using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.Common
{
    public class Constants
    {
        public const int ADOFAIModdingHelperMenuPriority = 16;
        public const string ADOFAIModdingHelperRoot = "ADOFAI Modding Helper/";
        public const string ADOFAIModdingHelperMenuRoot = "Tools/ADOFAI Modding Helper/";
        public const string settingsFolder = "Assets/AMHSettings";
        public const string LogFolder = "Assets/AMHSettings/Logs";
        public static readonly string ADOFAIModdingHelperRootPath;
        public static readonly string AMHScenePath;
        public static readonly string ThunderKitDefaultPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "ThunderKit");

        public static readonly string TKDefaultLibrariesPath = Path.Combine(ThunderKitDefaultPath, "Libraries");
        public static readonly string TKDefaultAssetBundleStagingPath = Path.Combine(ThunderKitDefaultPath, "AssetBundleStaging");
        static Constants()
        {
            string[] guids = AssetDatabase.FindAssets("ADOFAIModdingHelper t:AssemblyDefinitionAsset");
            if (guids.Length > 0)
            {
                string asmdefPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                ADOFAIModdingHelperRootPath = Path.GetDirectoryName(Path.GetDirectoryName(asmdefPath)).Replace("\\", "/");
            }
            else
            {
                Debug.LogWarning("ADOFAIModdingHelper.asmdef not found! Paths will fallback.");
                ADOFAIModdingHelperRootPath = "Assets/Plugins/Jofo's APIs/ADOFAIModdingHelper";
            }
            AMHScenePath = Path.Combine(ADOFAIModdingHelperRootPath, "Assets", "Scene");
        }
    }
}
