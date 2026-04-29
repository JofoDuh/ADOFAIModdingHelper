//using System.Linq;
//using UnityEditor;
//using UnityEngine;
//using ADOFAIModdingHelper.Common;

//namespace ADOFAIModdingHelper.Core
//{   
//    public static class DefineSymbolToggler
//    {
//        static Setting setting = Main.setting;
//        internal const string UMM_SYMBOL = "UNITYMODMANAGER";
//        internal const string BEPINEX_SYMBOL = "BEPINEX";

//        /// <summary>
//        /// Sets the build target define. Only one of UMM or BepInEx will exist after this.
//        /// </summary>
//        /// <param name="target">0 = UMM, 1 = BepInEx</param>
//        public static void SetBuild(int target)
//        {
//            bool isUMM = target == 0;

//            var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
//            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

//            var symbolList = symbols.Split(';').ToList();
//            symbolList.RemoveAll(s => s == UMM_SYMBOL || s == BEPINEX_SYMBOL);

//            symbolList.Add(isUMM ? UMM_SYMBOL : BEPINEX_SYMBOL);
//            symbols = string.Join(";", symbolList);

//            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, symbols);
//            Debug.Log("Switched scripting symbol to: " + symbols);
//            EditorUtility.SetDirty(Main.setting);
//        }

//        [MenuItem(Constants.ADOFAIModdingHelperRoot + "Switch Build/" + "BepInEx", false, priority: Constants.ADOFAIModdingHelperMenuPriority)]
//        public static void BepInEx()
//        {
//            if (setting.AvailableBuildOptionsSelectedIndex == 1) return;
//            setting.AvailableBuildOptionsSelectedIndex = 1;
//        }
//        [MenuItem(Constants.ADOFAIModdingHelperRoot + "Switch Build/" + "Unity Mod Manager", false, priority: Constants.ADOFAIModdingHelperMenuPriority)]
//        public static void UnityModManager()
//        {
//            if (setting.AvailableBuildOptionsSelectedIndex == 0) return;
//            setting.AvailableBuildOptionsSelectedIndex = 0;
//        }
//        public static int GetBuildFromDefines()
//        {
//            var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
//            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

//            if (symbols.Contains(UMM_SYMBOL))
//                return 0;
//            if (symbols.Contains(BEPINEX_SYMBOL))
//                return 1;

//            return 0;
//        }
//    }
//}