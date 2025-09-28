using System.IO;
using UnityEditor;
using UnityEngine;

namespace ADOFAIRunner.Common
{
    public class Constants
    {
        public const int ADOFAIRunnerMenuPriority = 15;
        public const string ADOFAIRunnerMenuRoot = "Tools/ADOFAI Runner/";
        public const string settingsFolder = "Assets/ADOFAIRunnerSettings";
        public const string LogFolder = "Assets/ADOFAIRunnerSettings/Logs";
        public static readonly string ADOFAIRunnerRootPath;

        static Constants()
        {
            string[] guids = AssetDatabase.FindAssets("ADOFAIRunner t:AssemblyDefinitionAsset");
            if (guids.Length > 0)
            {
                string asmdefPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                ADOFAIRunnerRootPath = Path.GetDirectoryName(Path.GetDirectoryName(asmdefPath)).Replace("\\", "/");

            }
            else
            {
                Debug.LogWarning("ADOFAIRunner.asmdef not found! Paths will fallback.");
                ADOFAIRunnerRootPath = "Assets/Plugins/Jofo's APIs/ADOFAIRunner";
            }
        }
    }
}
