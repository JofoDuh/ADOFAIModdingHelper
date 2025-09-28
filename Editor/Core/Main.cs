using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.Core
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
            Debug.Log("Setting up ADOFAI Modding Helper environment...");

            AssetDatabase.Refresh();
        }
    }
}
