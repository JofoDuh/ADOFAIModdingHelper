using ADOFAIRunner.Core;
using ADOFAIRunner.Core.Windows;
using ADOFAIRunner.Utilities;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using static UnityEditor.EditorGUILayout;

namespace ADOFAIRunner.Toolbar
{
    public static class RunADOFAIToolbar
    {
        public static Setting setting = Main.setting;
        private static Texture2D gearIcon = EditorGUIUtility.IconContent("SettingsIcon").image as Texture2D;
        private static bool runButtonEnabled = true;
        static bool FastRunOption = false;

        public static void Init()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            if (setting.AvailableMods != null && setting.AvailableMods.Count > 0)
            {
                if (setting.AvailableModsSelectedIndex < 0 || setting.AvailableModsSelectedIndex >= setting.AvailableMods.Count)
                {
                    setting.AvailableModsSelectedIndex = setting.AvailableMods.Count - 1;
                }
            }
            else
            {
                setting.AvailableModsSelectedIndex = -1;
            }

            using (new VerticalScope())
            {
                GUILayout.Space(2);
                using (new HorizontalScope())
                {
                    try
                    {
                        if (setting.AvailableMods != null &&
                            setting.AvailableMods.Count > 0 &&
                            setting.AvailableModsSelectedIndex >= 0 &&
                            setting.AvailableModsSelectedIndex < setting.AvailableMods.Count)
                        {
                            var list = new GUIContent[setting.AvailableMods.Count];
                            for (int i = 0; i < setting.AvailableMods.Count; i++)
                            {
                                var mod = setting.AvailableMods[i].Pipeline;
                                list[i] = mod != null
                                    ? new GUIContent(mod.name, "Add more mods in the ADOFAIRunner settings!")
                                    : new GUIContent("<null>", "This slot is empty");
                            }

                            setting.AvailableModsSelectedIndex = EditorGUILayout.Popup(
                                setting.AvailableModsSelectedIndex,
                                list,
                                GUILayout.Width(ProjectUtilities.DynamicaWidth(
                                    setting.AvailableMods[setting.AvailableModsSelectedIndex].Pipeline == null ? "<null>" :
                                    setting.AvailableMods[setting.AvailableModsSelectedIndex].Pipeline.name, 5f))
                            );

                            if (setting.AvailableMods[setting.AvailableModsSelectedIndex].Pipeline != null)
                            {
                                GUI.enabled = runButtonEnabled;
                                if (GUILayout.Button(
                                    new GUIContent("Run", "Run ADOFAI after importing ThunderKit's compiled things"),
                                    GUILayout.Width(35f)))
                                {
                                    Debug.Log($"Running {setting.AvailableMods[setting.AvailableModsSelectedIndex].Pipeline}");
                                    OnRunButtonClicked();
                                }
                            }
                        }
                        if (GUILayout.Button(
                            new GUIContent("FRun", "Quick Run ADOFAI without compiling or anything"),
                            GUILayout.Width(43f)))
                        {
                            FastRunOption = !FastRunOption;
                        }
                        if (FastRunOption)
                        {
                            if (GUILayout.Button(
                                new GUIContent("UMM", "Quick Run ADOFAI without compiling or anything"),
                                GUILayout.Width(45f)))
                            {
                                FastRunOption = false;
                                OnRunButtonClicked(true, RunLogic.BuildTarget.UMM);
                            }
                            if (GUILayout.Button(
                                new GUIContent("BepInEx", "Quick Run ADOFAI without compiling or anything"),
                                GUILayout.Width(55f)))
                            {
                                FastRunOption = false;
                                OnRunButtonClicked(true, RunLogic.BuildTarget.BepInEx);
                            }
                        }
                        GUI.enabled = true; 

                        if (GUILayout.Button(new GUIContent(gearIcon, "Open ADOFAIRunner Settings"), 
                            GUILayout.Width(20f), GUILayout.Height(20f)))
                        {
                            SettingsWindow.ShowSetting();
                        }
                        //GUILayout.Space(5);
                        setting.IncludePDBFile = GUILayout.Toggle(setting.IncludePDBFile, new GUIContent("PDB", "Check to include the PDB file when moving"));
                    }
                    finally { }
                }
            }
        }
        private static async void OnRunButtonClicked(bool fastRun = false, RunLogic.BuildTarget build = RunLogic.BuildTarget.Auto)
        {
            runButtonEnabled = false;
            try
            {
                await RunLogic.BuildAndRun(Main.setting, fastRun, build);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"An error occurred during the build and run process: {e.Message}");
            }
            runButtonEnabled = true;
        }
    }
}