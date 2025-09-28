using ADOFAIRunner.Common;
using ADOFAIRunner.Core;
using ADOFAIRunner.DefineSymbols.Core;
using UnityEditor;
using UnityEngine;

namespace ADOFAIRunner.Utilities
{
    public static class ProjectUtilities
    {
        [MenuItem(Constants.ADOFAIRunnerMenuRoot + "Open Console %#c", false, priority: Constants.ADOFAIRunnerMenuPriority)]
        public static void OpenConsoleWindow()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var consoleWindowType = assembly.GetType("UnityEditor.ConsoleWindow");

            if (consoleWindowType != null)
            {
                EditorWindow.GetWindow(consoleWindowType);
            }
            else
            {
                Debug.LogError("Could not find UnityEditor.ConsoleWindow type.");
            }
        }

        public static float DynamicaWidth(string text, float extraAmount)
        {
            GUIStyle popupStyle = EditorStyles.popup;
            Vector2 size = popupStyle.CalcSize(new GUIContent(text));
            return size.x + extraAmount;
        }
        public static void CreateFolderInAssets(string folderName)
        {
            string path = "Assets/" + folderName;

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets", folderName);
                AssetDatabase.Refresh();
                Debug.Log($"Created folder at: {path}");
            }
            else
            {
                Debug.Log($"Folder already exists: {path}");
            }
        }

        public static string GetCurrentBuild()
        {
            string UMM_SYMBOL = "UNITYMODMANAGER";
            string BEPINEX_SYMBOL = "BEPINEX";

            var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            bool hasUMM = symbols.Contains(UMM_SYMBOL);
            bool hasBepInEx = symbols.Contains(BEPINEX_SYMBOL);

            if (!hasUMM && !hasBepInEx) DefineSymbolToggler.SetBuild(Main.setting.AvailableBuildOptionsSelectedIndex);
            if ((hasUMM && !hasBepInEx) || 
                (Main.setting.AvailableBuildOptions[Main.setting.AvailableBuildOptionsSelectedIndex] == "Unity Mod Manager"))
            {
                return UMM_SYMBOL;
            }
            else if ((hasBepInEx && !hasUMM) ||
                (Main.setting.AvailableBuildOptions[Main.setting.AvailableBuildOptionsSelectedIndex] == "BepInEx"))
            {
                return BEPINEX_SYMBOL;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}