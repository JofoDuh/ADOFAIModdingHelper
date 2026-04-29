//using ADOFAIModdingHelper.ScriptableObjects; 
//using ADOFAIModdingHelper.Utilities;
//using System;
//using System.IO;
//using System.Threading.Tasks;
//using UnityEngine;
//using Newtonsoft.Json;

//namespace ADOFAIModdingHelper.Core
//{
//    public static class RunLogic
//    {
//        public enum BuildTarget { Auto, BepInEx, UMM }

//        /// <summary>
//        /// Main entry point for the build, deploy, and run process.
//        /// </summary>
//        /// <param name="settings">The settings ScriptableObject containing all required paths and configurations.</param>
//        public static async Task BuildAndRun(Setting settings, bool fastRun = false, BuildTarget build = BuildTarget.Auto, bool BootGame = true)
//        {
//            if (fastRun && !BootGame) return;
//            Logger.Clear();
//            string buildType = ProjectUtilities.GetCurrentBuild();
//            if (string.IsNullOrEmpty(buildType))
//            {
//                Debug.Log("Build Type is null. Aborting. Make sure symbol definition are properly set");
//                return;
//            }

//            if (!fastRun)
//            {
//                ProjectUtilities.OpenConsoleWindow();



//                ProjectUtilities.OpenConsoleWindow();
//            }

//            #region 7. Launch the game executable

//            if (BootGame)
//            {
//                string exePath = build switch
//                {
//                    BuildTarget.Auto => buildType == "BEPINEX" ? settings.BepInExExePath : settings.UnityModManagerExePath,
//                    BuildTarget.BepInEx => settings.BepInExExePath,
//                    BuildTarget.UMM => settings.UnityModManagerExePath,
//                    _ => throw new ArgumentOutOfRangeException(nameof(build))
//                };

//                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
//                {
//                    Debug.LogError($"Executable path is not set or is invalid. Cannot run the game.");
//                    return;
//                }

//                Debug.Log($"Launching executable: {exePath}");
//                LaunchExecutable(exePath);
//            }
//            #endregion
//        }

//        /// <summary>
//        /// Process the mod info if ADOFAI Modding Helper is present
//        /// </summary>
//        private static async Task ProcessInfo(ModInfo modInfo, string destinationFolder, BuildTarget buildTarget)
//        {
//            if (modInfo == null)
//            {
//                Debug.Log("ModInfo is null");
//                return;
//            }
//            switch (buildTarget)
//            {
//                case BuildTarget.Auto:
//                    string buildType = ProjectUtilities.GetCurrentBuild();
//                    if (buildType == "BEPINEX")
//                    {
//                        //goto case BuildTarget.BepInEx;
//                    }
//                    else if (buildType == "UNITYMODMANAGER")
//                    {
//                        //goto case BuildTarget.UMM;
//                    }
//                    Debug.Log("No Appropriate Build");
//                    return;
//                //case BuildTarget.UMM:
//                //    ModInfoUMM modInfoUMM = modInfo.modInfoUMM;
//                //    if (modInfoUMM == null)
//                //    {
//                //        Debug.Log("modInfoUMM is null");
//                //        return;
//                //    }

//                //    Directory.CreateDirectory(destinationFolder);
//                //    var settings = new JsonSerializerSettings
//                //    {
//                //        Formatting = Formatting.Indented,
//                //    };
//                //    var json = JsonConvert.SerializeObject(modInfoUMM, settings);
//                //    File.WriteAllText(Path.Combine(destinationFolder, "Info.json"), json);

//                //    break;
//                //case BuildTarget.BepInEx:
//                //    //ModInfoBIE modInfoBIE = modInfo.modInfoBIE;

//                //    Debug.Log("Renaming!");
//                //    string infoFilePath = modInfoBIE.BIPModInfoCSPath;
//                //    if (!File.Exists(infoFilePath))
//                //    {
//                //        Debug.LogError($".cs not found at {infoFilePath}");
//                //        return;
//                //    }

//                //    string[] lines = File.ReadAllLines(infoFilePath);
//                //    for (int i = 0; i < lines.Length; i++)
//                //    {
//                //        if (lines[i].Contains("PLUGIN_GUID"))
//                //            lines[i] = $"        public const string PLUGIN_GUID = \"{modInfoBIE.GUID}\";";
//                //        else if (lines[i].Contains("PLUGIN_NAME"))
//                //            lines[i] = $"        public const string PLUGIN_NAME = \"{modInfoBIE.PluginName}\";";
//                //        else if (lines[i].Contains("PLUGIN_VERSION"))
//                //            lines[i] = $"        public const string PLUGIN_VERSION = \"{modInfoBIE.PluginVersion}\";";
//                //    }
//                //    File.WriteAllLines(infoFilePath, lines);
//                //    Debug.Log($"Updated EditorCustomModulesInfo.cs with {modInfoBIE.PluginName}");
//                //    break;
//            }
//            await Task.CompletedTask;
//        }


//        /// <summary>
//        /// Launches an external executable.
//        /// </summary>
//        /// <param name="exePath">The full path to the executable file.</param>
//        private static void LaunchExecutable(string exePath)
//        {
//            var startInfo = new System.Diagnostics.ProcessStartInfo
//            {
//                FileName = exePath,
//                WorkingDirectory = Path.GetDirectoryName(exePath),
//                UseShellExecute = true
//            };

//            System.Diagnostics.Process.Start(startInfo);
//        }
//    }
//}
