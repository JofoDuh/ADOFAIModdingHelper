using ADOFAIModdingHelper.Core.ScriptableObjects; 
using ADOFAIRunner.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Manifests.Datums;
using UnityEngine;

namespace ADOFAIRunner.Core
{
    public static class RunLogic
    {
        public enum BuildTarget { Auto, BepInEx, UMM }

        /// <summary>
        /// Main entry point for the build, deploy, and run process.
        /// </summary>
        /// <param name="settings">The settings ScriptableObject containing all required paths and configurations.</param>
        public static async Task BuildAndRun(Setting settings, bool fastRun = false, BuildTarget build = BuildTarget.Auto)
        {
            Logger.Clear();
            string buildType = ProjectUtilities.GetCurrentBuild();
            if (string.IsNullOrEmpty(buildType))
            {
                Debug.Log("Build Type is null. Aborting. Make sure symbol definition are properly set");
                return;
            }

            if (!fastRun)
            {
                ProjectUtilities.OpenConsoleWindow();

                #region 1. Get the selected pipeline from settings
                var selectedPipeline = settings.AvailableMods[settings.AvailableModsSelectedIndex].Pipeline;
                if (selectedPipeline == null)
                {
                    Debug.LogError("No pipeline selected in ADOFAIRunner settings. Aborting.");
                    return;
                }
                #endregion

                #region 2. Determine source and destination paths
                var manifest = selectedPipeline.manifest;
                if (manifest == null)
                {
                    Debug.LogError($"Pipeline '{selectedPipeline.name}' does not have an assigned manifest. Aborting.");
                    return;
                }

                string modName = manifest.Identity.Name;
                string baseModPath = build switch
                {
                    BuildTarget.Auto => buildType == "BEPINEX" ? settings.BepInExModFolderPath : settings.UMMModFolderPath,
                    BuildTarget.BepInEx => settings.BepInExModFolderPath,
                    BuildTarget.UMM => settings.UMMModFolderPath,
                    _ => throw new ArgumentOutOfRangeException(nameof(build))
                };

                if (string.IsNullOrEmpty(baseModPath))
                {
                    Debug.LogError($"{buildType} mod folder path is not set in settings. Aborting.");
                    return;
                }

                string modDestinationFolder = Path.Combine(baseModPath, modName);
                string assetsDestinationFolder = Path.Combine(modDestinationFolder, "assets");
                #endregion

                #region 3. Process mod info
                await ProcessInfo(settings.AvailableMods[settings.AvailableModsSelectedIndex].ModInfo, modDestinationFolder, build);
                #endregion

                #region 4. Execute the ThunderKit pipeline
                Debug.Log($"Executing pipeline: {selectedPipeline.name}...");
                await selectedPipeline.Execute();
                Debug.Log("Pipeline execution finished.");
                #endregion

                ProjectUtilities.OpenConsoleWindow();

                #region 5. Process and move assembly files
                await ProcessAssemblies(manifest, settings.ThunderkitOutputPath, modDestinationFolder, settings.IncludePDBFile);
                #endregion

                #region 6. Process and move asset bundles
                await ProcessAssetBundles(manifest, settings.ThunderkitOutputPath, assetsDestinationFolder); 
                #endregion
            }

            #region 7. Launch the game executable
            string exePath = build switch
            {
                BuildTarget.Auto => buildType == "BEPINEX" ? settings.BepInExExePath : settings.UnityModManagerExePath,
                BuildTarget.BepInEx => settings.BepInExExePath,
                BuildTarget.UMM => settings.UnityModManagerExePath,
                _ => throw new ArgumentOutOfRangeException(nameof(build))
            };

            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                Debug.LogError($"Executable path is not set or is invalid. Cannot run the game.");
                return;
            }

            Debug.Log($"Launching executable: {exePath}");
            LaunchExecutable(exePath); 
            #endregion
        }

        /// <summary>
        /// Process the mod info if ADOFAI Modding Helper is present
        /// </summary>
        private static async Task ProcessInfo(ModInfo modInfo, string destinationFolder, BuildTarget buildTarget)
        {
            if (modInfo == null)
            {
                Debug.Log("ModInfo is null");
                return;
            }
            switch (buildTarget)
            {
                case BuildTarget.Auto:
                    string buildType = ProjectUtilities.GetCurrentBuild();
                    if (buildType == "BEPINEX")
                    {
                        goto case BuildTarget.BepInEx;
                    }
                    else if (buildType == "UNITYMODMANAGER")
                    {
                        goto case BuildTarget.UMM;
                    }
                    Debug.Log("No Appropriate Build");
                    return;
                case BuildTarget.UMM:
                    ModInfoUMM modInfoUMM = modInfo.modInfoUMM;
                    if (modInfoUMM == null)
                    {
                        Debug.Log("modInfoUMM is null");
                        return;
                    }

                    Directory.CreateDirectory(destinationFolder);
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                    };
                    var json = JsonConvert.SerializeObject(modInfoUMM, settings);
                    File.WriteAllText(Path.Combine(destinationFolder, "Info.json"), json);

                    break;
                case BuildTarget.BepInEx:
                    ModInfoBIE modInfoBIE = modInfo.modInfoBIE;

                    Debug.Log("Renaming!");
                    string infoFilePath = modInfoBIE.BIPModInfoCSPath;
                    if (!File.Exists(infoFilePath))
                    {
                        Debug.LogError($".cs not found at {infoFilePath}");
                        return;
                    }

                    string[] lines = File.ReadAllLines(infoFilePath);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains("PLUGIN_GUID"))
                            lines[i] = $"        public const string PLUGIN_GUID = \"{modInfoBIE.GUID}\";";
                        else if (lines[i].Contains("PLUGIN_NAME"))
                            lines[i] = $"        public const string PLUGIN_NAME = \"{modInfoBIE.PluginName}\";";
                        else if (lines[i].Contains("PLUGIN_VERSION"))
                            lines[i] = $"        public const string PLUGIN_VERSION = \"{modInfoBIE.PluginVersion}\";";
                    }
                    File.WriteAllLines(infoFilePath, lines);
                    Debug.Log($"Updated EditorCustomModulesInfo.cs with {modInfoBIE.PluginName}");
                    break;
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Finds, copies, and cleans up compiled assemblies.
        /// </summary>
        private static async Task ProcessAssemblies(ThunderKit.Core.Manifests.Manifest manifest, string fallbackOutputPath, string destinationFolder, bool IncludePDBfile = false)
        {
            var assemblyDatums = manifest.Data.OfType<AssemblyDefinitions>().ToList();
            if (!assemblyDatums.Any())
            {
                Debug.LogWarning("No AssemblyDefinitions found in the manifest. Skipping assembly deployment.");
                return;
            }

            Directory.CreateDirectory(destinationFolder);
            Debug.Log($"Deploying assemblies to: {destinationFolder}");

            foreach (var datum in assemblyDatums)
            {
                string assemblySourcePath = Path.Combine(fallbackOutputPath, "Libraries");

                if (datum.StagingPaths != null && datum.StagingPaths.Any())
                {
                    foreach (string path in datum.StagingPaths)
                    {
                        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                        {
                            assemblySourcePath = path;
                            break;
                        }
                    }
                }

                if (!Directory.Exists(assemblySourcePath))
                {
                    Debug.LogError($"Assembly source directory not found: {assemblySourcePath}. Aborting assembly deployment.");
                    return;
                }

                foreach (var assemblyDefinition in datum.definitions)
                {
                    string assemblyFileName = $"{assemblyDefinition.name}.dll";
                    string sourceFile = Path.Combine(assemblySourcePath, assemblyFileName);

                    if (File.Exists(sourceFile))
                    {
                        string destFile = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));
                        File.Copy(sourceFile, destFile, true);
                        Debug.Log($"Copied assembly: {Path.GetFileName(sourceFile)}");
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find assembly file: {sourceFile}");
                    }

                    if (IncludePDBfile)
                    {
                        string assemblyPDB = $"{assemblyDefinition.name}.pdb";
                        string sourcePDBFile = Path.Combine(assemblySourcePath, assemblyPDB);

                        if (File.Exists(sourcePDBFile))
                        {
                            string destPDBFile = Path.Combine(destinationFolder, Path.GetFileName(sourcePDBFile));
                            File.Copy(sourcePDBFile, destPDBFile, true);
                            Debug.Log($"Copied PDB file: {Path.GetFileName(sourcePDBFile)}");
                        }
                        else
                        {
                            Debug.LogWarning($"Could not find PDB file: {sourcePDBFile}");
                        }
                    }
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Finds, copies, and cleans up asset bundles.
        /// </summary>
        private static async Task ProcessAssetBundles(ThunderKit.Core.Manifests.Manifest manifest, string fallbackOutputPath, string destinationFolder)
        {
            static void CopyFileIfExists(string sourceFile, string destinationFolder)
            {
                if (File.Exists(sourceFile))
                {
                    string destFile = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));
                    File.Copy(sourceFile, destFile, true);
                    Debug.Log($"Copied asset bundle: {Path.GetFileName(sourceFile)}");
                }
                else
                {
                    Debug.LogWarning($"Could not find asset bundle file: {sourceFile}");
                }
            }

            var assetBundleDatums = manifest.Data.OfType<AssetBundleDefinitions>().ToList();
            if (!assetBundleDatums.Any())
            {
                Debug.Log("No AssetBundleDefinitions found in manifest. Skipping asset bundle deployment.");
                return;
            }

            Directory.CreateDirectory(destinationFolder);
            Debug.Log($"Deploying asset bundles to: {destinationFolder}");

            foreach (var datum in assetBundleDatums)
            {
                string assetBundleSourcePath = Path.Combine(fallbackOutputPath, "AssetBundleStaging");

                if (datum.StagingPaths != null && datum.StagingPaths.Any())
                {
                    foreach (string path in datum.StagingPaths)
                    {
                        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                        {
                            assetBundleSourcePath = path;
                            break;
                        }
                    }
                }

                if (!Directory.Exists(assetBundleSourcePath))
                {
                    Debug.LogError($"AssetBundle staging directory not found: {assetBundleSourcePath}. Aborting asset bundle deployment.");
                    return;
                }

                foreach (var assetBundle in datum.assetBundles)
                {
                    string sourceFile = Path.Combine(assetBundleSourcePath, assetBundle.assetBundleName);
                    string sourceFileManifest = Path.Combine(assetBundleSourcePath, (assetBundle.assetBundleName + ".manifest"));
                    CopyFileIfExists(sourceFile, destinationFolder);
                    CopyFileIfExists(sourceFileManifest, destinationFolder);
                }
            }
            await Task.CompletedTask;
        }


        /// <summary>
        /// Launches an external executable.
        /// </summary>
        /// <param name="exePath">The full path to the executable file.</param>
        private static void LaunchExecutable(string exePath)
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath),
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(startInfo);
        }
    }
}
