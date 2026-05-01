using ADOFAIModdingHelper.ScriptableObjects;
using ADOFAIModdingHelper.Utilities;
using ADOFAIModdingHelper.Windows;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using static UnityEditor.EditorGUILayout;

namespace ADOFAIModdingHelper.Toolbar
{
    public static class RunADOFAIToolbar
    {
        private static Texture2D gearIcon = EditorGUIUtility.IconContent("SettingsIcon").image as Texture2D;

        [InitializeOnLoadMethod]
        public static void Init()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            var config = ModToolsConfig.Config;
            var setting = Setting.Config;

            GUILayout.Space(20);
            using (new VerticalScope())
            {
                GUILayout.Space(2);
                using (new HorizontalScope())
                {
                    try
                    {
                        GUILayout.Label(ModInfo.Info.Id,
                                GUILayout.Width(ProjectUtilities.DynamicaWidth(ModInfo.Info.Id, 5f)));

                        if (GUILayout.Button(
                            new GUIContent("Run", "Compile Everything and Run ADOFAI"),
                            GUILayout.Width(35f)))
                        {
                            string dest = config.copyToDirectory
                                ? Path.Combine(Path.GetDirectoryName(setting.ADOFAIPath)!, "Mods", ModInfo.Info.Id)
                                : null;
                            config.BuildMod(dest);
                        }

                        if (GUILayout.Button(
                            new GUIContent("FRun", "Quick Run ADOFAI without compiling"),
                            GUILayout.Width(43f)))
                        {
                            config.RunApp();
                        }

                        if (GUILayout.Button(new GUIContent(gearIcon, "Open Mod Config"),
                            GUILayout.Width(20f), GUILayout.Height(20f)))
                        {
                            ModConfigWindow.OpenBuild();
                        }

                        GUILayout.Space(5);
                    }
                    finally { }
                }
            }
        }
    }
}
