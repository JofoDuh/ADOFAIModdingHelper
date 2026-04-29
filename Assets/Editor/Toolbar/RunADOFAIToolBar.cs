using ADOFAIModdingHelper.ScriptableObjects;
using ADOFAIModdingHelper.Utilities;
using ADOFAIModdingHelper.Windows;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using static UnityEditor.EditorGUILayout;

namespace ADOFAIModdingHelper.Toolbar
{
    public static class RunADOFAIToolbar
    {
        public static Setting setting = Setting.Config;
        private static Texture2D gearIcon = EditorGUIUtility.IconContent("SettingsIcon").image as Texture2D;
        private static bool runButtonEnabled = true;
        //static bool FastRunOption = false;

        [InitializeOnLoadMethod]
        public static void Init()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            if (setting.AllMods != null && setting.AllMods.Count > 0)
            {
                if (setting.CurrentConfig == null || !setting.AllMods.Contains(setting.CurrentConfig))
                {
                    setting.CurrentConfig = setting.AllMods[setting.AllMods.Count - 1];
                }
            }
            else
            {
                setting.CurrentConfig = null;
            }
            GUILayout.Space(20);
            using (new VerticalScope())
            {
                GUILayout.Space(2);
                using (new HorizontalScope())
                {
                    try
                    {
                        if (setting.AllMods != null &&
                            setting.AllMods.Count > 0 &&
                            setting.CurrentConfig != null)
                        {
                            var list = new GUIContent[setting.AllMods.Count];
                            for (int i = 0; i < setting.AllMods.Count; i++)
                            {
                                var mod = setting.AllMods[i].modInfo;
                                list[i] = mod != null
                                    ? new GUIContent(mod.Id, "Add more mods in the ADOFAIRunner settings!")
                                    : new GUIContent("<null>", "This slot is empty");
                            }

                            int selectedIndex = EditorGUILayout.Popup(
                                setting.AllMods.FindIndex(config => config == setting.CurrentConfig),
                                list,
                                GUILayout.Width(ProjectUtilities.DynamicaWidth(
                                    setting.CurrentConfig == null ? "<null>" : setting.CurrentConfig.modInfo.Id, 5f))
                            );

                            if (selectedIndex >= 0 && selectedIndex < setting.AllMods.Count)
                            {
                                setting.CurrentConfig = setting.AllMods[selectedIndex];
                            }

                            if (setting.CurrentConfig != null)
                            {
                                GUI.enabled = runButtonEnabled;
                                if (GUILayout.Button(
                                    new GUIContent("Run", "Run ADOFAI after importing ThunderKit's compiled things\nHold control when clicking to not run ADOFAI after building"),
                                    GUILayout.Width(35f)))
                                {
                                    bool ctrlHeld = Event.current != null && Event.current.control;
                                    Debug.Log($"Running {setting.CurrentConfig}");
                                    setting.CurrentConfig.BuildMod(setting.CurrentConfig.copyToDirectory ? Path.Combine(Path.GetDirectoryName(Setting.Config.ADOFAIPath)!, "Mods", setting.CurrentConfig.modInfo.Id) : null);
                                }
                            }
                        }
                        if (GUILayout.Button(
                            new GUIContent("FRun", "Quick Run ADOFAI without compiling or anything"),
                            GUILayout.Width(43f)))
                        {
                            //FastRunOption = !FastRunOption;
                            setting.CurrentConfig.RunApp();
                        }
                        //if (FastRunOption)
                        //{
                        //    if (GUILayout.Button(
                        //        new GUIContent("UMM", "Quick Run ADOFAI without compiling or anything"),
                        //        GUILayout.Width(45f)))
                        //    {
                        //        FastRunOption = false;
                        //        setting.CurrentConfig.RunApp();
                        //    }
                        //    if (GUILayout.Button(
                        //        new GUIContent("BepInEx", "Quick Run ADOFAI without compiling or anything"),
                        //        GUILayout.Width(55f)))
                        //    {
                        //        FastRunOption = false;
                        //        setting.CurrentConfig.RunApp();
                        //    }
                        //}
                        GUI.enabled = true;

                        if (GUILayout.Button(new GUIContent(gearIcon, "Open ADOFAIRunner Settings"),
                            GUILayout.Width(20f), GUILayout.Height(20f)))
                        {
                            ModsConfigWindow.OpenBuild();
                        }
                        GUILayout.Space(5);
                        setting.CurrentConfig.generateDebugSymbols = GUILayout.Toggle(setting.CurrentConfig.generateDebugSymbols, new GUIContent("PDB", "Check to include the PDB file when moving"));
                    }
                    finally { }
                }
            }
        }
    }
}