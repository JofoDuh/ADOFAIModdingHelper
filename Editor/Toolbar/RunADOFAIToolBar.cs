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
                    setting.CurrentConfig = setting.AllMods[^1];
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
                        if (setting.CurrentConfig == null)
                        {
                            GUILayout.Label("Add a mod twin", GUILayout.Width(ProjectUtilities.DynamicaWidth("Add a mod twin", 5f)));
                        }
                        if (setting.AllMods != null &&
                            setting.AllMods.Count > 0 &&
                            setting.CurrentConfig != null)
                        {
                            var list = new GUIContent[setting.AllMods.Count];
                            for (int i = 0; i < setting.AllMods.Count; i++)
                            {
                                var mod = setting.AllMods[i].modInfo;
                                list[i] = mod != null
                                    ? new GUIContent(mod.Id)
                                    : new GUIContent("<null>", "Add a Mod Info");
                            }

                            int selectedIndex = EditorGUILayout.Popup(
                                setting.AllMods.FindIndex(config => config == setting.CurrentConfig),
                                list,
                                GUILayout.Width(ProjectUtilities.DynamicaWidth(
                                    setting.CurrentConfig.modInfo == null ? "<null>" : setting.CurrentConfig.modInfo.Id, 5f))
                            );

                            if (selectedIndex >= 0 && selectedIndex < setting.AllMods.Count)
                            {
                                setting.CurrentConfig = setting.AllMods[selectedIndex];
                            }

                            if (setting.CurrentConfig != null || setting.CurrentConfig.modInfo)
                            {
                                if (GUILayout.Button(
                                    new GUIContent("Run", "Compile Everything and Run ADOFAI"),
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
                            if (setting.CurrentConfig != null) setting.CurrentConfig.RunApp();
                            else setting.RunAppWithoutConfig();
                        }

                        if (GUILayout.Button(new GUIContent(gearIcon, "Open ADOFAIRunner Settings"),
                            GUILayout.Width(20f), GUILayout.Height(20f)))
                        {
                            ModsConfigWindow.OpenBuild();
                        }
                        GUILayout.Space(5);
                        //setting.CurrentConfig.generateDebugSymbols = GUILayout.Toggle(setting.CurrentConfig.generateDebugSymbols, new GUIContent("PDB", "Check to include the PDB file when moving"));
                    }
                    finally { }
                }
            }
        }
    }
}