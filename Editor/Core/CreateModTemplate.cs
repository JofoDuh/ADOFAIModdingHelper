using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Core.ScriptableObjects;
using ADOFAIModdingHelper.ModTemplate;
using ADOFAIModdingHelper.ThunderKitUtils;
using ADOFAIModdingHelper.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Pipelines.Jobs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ADOFAIModdingHelper.Core.Windows.CreateModPrompt;

namespace ADOFAIModdingHelper.Core
{
    public static class CreateModTemplate
    {
        public static void CreateModAction(CreateModPromptData prompt)
        {
            // 1. Sanitize
            prompt.ModName = IdentifierUtils.Sanitize(prompt.ModName);
            if (string.IsNullOrEmpty(prompt.ModName))
            {
                Debug.LogError("Mod name is required.");
                return;
            }

            // 2. Root folder
            string targetPath = ProjectWindowPathHelper.GetCurrentProjectWindowPath();
            string rootFolder = EnsureSubFolder(targetPath, prompt.ModName);

            // 3. Create asmdef
            var asmdefAsset = CreateAsmdef(rootFolder, prompt.ModName);

            // 4. Mod info asset
            var modInfo = CreateModInfoAsset(rootFolder, prompt);

            // 5. Assets folder
            string assetPath = string.Empty;
            if (prompt.AssetFolder) assetPath = SetupAssetSubfolders(rootFolder, prompt);

            // 6. Scenes folder
            string scenePath = string.Empty;
            if (prompt.SceneFolder) scenePath = SetupScenefolder(rootFolder, prompt);

            // 7. ThunderKit setup
            var tkPath = EnsureSubFolder(rootFolder, "ThunderKit");
            var manifest = CreateThunderKitManifest(tkPath, asmdefAsset, assetPath, scenePath, prompt);
            var pipeline = CreateThunderKitPipeline(tkPath, manifest, prompt);

            // 8. Scripts
            GenerateScripts(rootFolder, prompt);

            // 9. Add mod to ADOFAI Runner 
            AddModToADOFAIRunner(modInfo, pipeline);

            modInfo.modInfoBIE.BIPModInfoCSPath = Path.Combine(rootFolder, "ModScripts", "Main", "BepInEx", nameof(ModTemplateMain.BepInExModInfo));
            Debug.Log($"Created mod template at {rootFolder}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ProjectWindowPathHelper.OpenFolder(rootFolder);
        }

        // ----------------- Helpers -----------------

        private static void AddModToADOFAIRunner(ModInfo modInfo, Pipeline pipeline)
        {
            ADOFAIRunner.Core.Main.setting.AvailableMods.Add(new ADOFAIRunner.Core.DataStructures.ModBuild()
            {
                ModInfo = modInfo,
                Pipeline = pipeline
            });
        }
        private static AssemblyDefinitionAsset CreateAsmdef(string rootFolder, string modName)
        {
            string asmdefPath = Normalize(Path.Combine(rootFolder, $"{modName}.asmdef"));
            if (!File.Exists(asmdefPath))
            {
                File.WriteAllText(asmdefPath, GetAssemblyDefinition(modName));
                AssetDatabase.ImportAsset(asmdefPath);
            }
            return AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(asmdefPath);
        }

        private static ModInfo CreateModInfoAsset(string rootFolder, CreateModPromptData prompt)
        {
            var modInfo = ScriptableObject.CreateInstance<ModInfo>();
            if (prompt.UMMCompatibility)
            {
                modInfo.modInfoUMM = new ModInfoUMM
                {
                    Id = prompt.ModID,
                    DisplayName = prompt.ModName,
                    Author = prompt.ModAuthor,
                    Version = string.IsNullOrEmpty(prompt.ModVersion) ? "1.0.0" : prompt.ModVersion,
                    AssemblyName = $"{prompt.ModName}.dll",
                    EntryMethod = $"{prompt.ModName}.Startup.Load"
                };
            }
            else if (prompt.BepInExCompatibility)
            {
                modInfo.modInfoBIE = new ModInfoBIE
                {
                    GUID = $"BepInEx.{prompt.ModID}",
                    PluginName = prompt.ModName,
                    PluginVersion = string.IsNullOrEmpty(prompt.ModVersion) ? "1.0.0" : prompt.ModVersion,
                };
            }
            string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(rootFolder, $"{prompt.ModName}ModInfo.asset"));
            AssetDatabase.CreateAsset(modInfo, path);
            return modInfo;
        }

        private static string SetupAssetSubfolders(string root, CreateModPromptData prompt)
        {
            prompt.AdditionalAssetFolders ??= new List<string>();

            string assetPath = EnsureSubFolder(root, "Assets");

            var folders = new List<string>();
            if (prompt.AudioFolder) folders.Add("Audio");
            if (prompt.Texture2dFolder) folders.Add("Textures2D");
            if (prompt.PrefabsFolder) folders.Add("Prefabs");
            if (prompt.ScriptsFolder) folders.Add("Scripts");
            if (prompt.FontsFolder) folders.Add("Fonts");
            if (prompt.MaterialsFolder) folders.Add("Materials");
            if (prompt.ShadersFolder) folders.Add("Shaders");

            foreach (var folder in folders)
            {
                prompt.AdditionalAssetFolders.Add(folder);
            }

            var seen = new Dictionary<string, int>();
            for (int i = 0; i < prompt.AdditionalAssetFolders.Count; i++)
            {
                string folder = prompt.AdditionalAssetFolders[i];
                if (string.IsNullOrEmpty(folder)) continue;

                if (seen.TryGetValue(folder, out int count))
                {
                    count++;
                    prompt.AdditionalAssetFolders[i] = $"{folder}_{count}";
                    seen[folder] = count;
                }
                else
                {
                    seen[folder] = 0;
                }

                EnsureSubFolder(assetPath, prompt.AdditionalAssetFolders[i]);
            }

            return assetPath;
        }

        private static string SetupScenefolder(string root, CreateModPromptData prompt) 
        { 
            string scenePath = EnsureSubFolder(root, "Scenes"); 
            if (prompt.SceneTemplate) 
            { 
                var sceneTemplatePath = Path.Combine(scenePath, $"{prompt.ModName}Scene.unity"); 
                File.Copy(Path.Combine(Constants.AMHScenePath, "TemplateScene.unity"), sceneTemplatePath); 
                AssetDatabase.ImportAsset(sceneTemplatePath); 
            } 
            return scenePath; 
        }

        private static Manifest CreateThunderKitManifest(string tkPath, AssemblyDefinitionAsset asmdef, string assetPath, string scenePath, CreateModPromptData prompt)
        {
            var manifest = ScriptableObject.CreateInstance<Manifest>();
            ModManifestUtils.CreateModManifest(manifest, 0,
                AssetDatabase.GenerateUniqueAssetPath(Path.Combine(tkPath, $"{prompt.ModName}{nameof(Manifest)}.asset")));

            manifest.Identity.Name = prompt.ModName;
            manifest.Identity.Author = prompt.ModAuthor;
            manifest.Identity.Version = prompt.ModVersion;

            var assemblyDatum = ModManifestUtils.GetManifestDatum<AssemblyDefinitions>(manifest);
            var defs = assemblyDatum.definitions?.ToList() ?? new List<AssemblyDefinitionAsset>();
            defs.Add(asmdef);
            assemblyDatum.definitions = defs.ToArray();

            var assetBundleDef = new AssetBundleDefinition
            {
                assetBundleName = $"{prompt.ModName}_assets.bundle",
            };
            if (!string.IsNullOrEmpty(assetPath))
            {
                assetBundleDef.assets = new UnityEngine.Object[] { AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object)) };
            }

            var bundleDatum = ModManifestUtils.GetManifestDatum<AssetBundleDefinitions>(manifest);
            var bundles = bundleDatum.assetBundles?.ToList() ?? new List<AssetBundleDefinition>();
            bundles.Add(assetBundleDef);

            if (!string.IsNullOrEmpty(scenePath) && File.Exists(Path.Combine(scenePath, $"{prompt.ModName}Scene.unity")))
            {
                var sceneBundleDef = new AssetBundleDefinition
                {
                    assetBundleName = $"{prompt.ModName}_scenes.bundle"
                };
                sceneBundleDef.assets = new UnityEngine.Object[] { AssetDatabase.LoadAssetAtPath(Path.Combine(scenePath, $"{prompt.ModName}Scene.unity"), typeof(UnityEngine.Object)) };
                bundles.Add(sceneBundleDef);
            }
            bundleDatum.assetBundles = bundles.ToArray();
            EditorUtility.SetDirty(manifest);
            return manifest;
        }

        private static Pipeline CreateThunderKitPipeline(string tkPath, Manifest manifest, CreateModPromptData prompt)
        {
            var pipeline = ScriptableObject.CreateInstance<Pipeline>();
            ModPipelineUtils.CreateModPipeline(pipeline, 0,
                AssetDatabase.GenerateUniqueAssetPath(Path.Combine(tkPath, $"{prompt.ModName}{nameof(Pipeline)}.asset")));

            pipeline.manifest = manifest;

            var deleteJobAsm = ModPipelineUtils.GetPipelineJob<Delete>(pipeline, 0);
            deleteJobAsm.Path = Constants.TKDefaultLibrariesPath;

            var deleteJobBundle = ModPipelineUtils.GetPipelineJob<Delete>(pipeline, 1);
            deleteJobBundle.Path = Constants.TKDefaultAssetBundleStagingPath;

            EditorUtility.SetDirty(pipeline);

            return pipeline;
        }

        private static void GenerateScripts(string root, CreateModPromptData prompt)
        {
            string modScripts = EnsureSubFolder(root, "ModScripts");

            // Main
            string mainPath = EnsureSubFolder(modScripts, "Main");
            string settingPath = EnsureSubFolder(modScripts, "Setting");

            if (prompt.UMMCompatibility)
            {
                CreateCSFile(EnsureSubFolder(mainPath, "UMM"), "Main", ModTemplateMain.UMMMain, prompt);
                CreateCSFile(EnsureSubFolder(mainPath, "UMM"), nameof(ModTemplateMain.StartUp), ModTemplateMain.StartUp, prompt);

                CreateCSFile(EnsureSubFolder(settingPath, "UMM"), "Setting", ModTemplateMain.UMMSetting, prompt);
            }
            if (prompt.BepInExCompatibility)
            {
                CreateCSFile(EnsureSubFolder(mainPath, "BepInEx"), "Main", ModTemplateMain.BepInExMain, prompt);
                CreateCSFile(EnsureSubFolder(mainPath, "BepInEx"), nameof(ModTemplateMain.BepInExModInfo), ModTemplateMain.BepInExModInfo, prompt);

                CreateCSFile(EnsureSubFolder(settingPath, "BepInEx"), "Setting", ModTemplateMain.BepInExSetting, prompt);
            }
            // Patches
            CreateCSFile(EnsureSubFolder(modScripts, "Patches"), nameof(ModTemplateMain.ExamplePatch), ModTemplateMain.ExamplePatch, prompt);

            // Utilities
            string utils = EnsureSubFolder(modScripts, "Utilities");
            CreateCSFile(utils, nameof(ModTemplateMain.Reflections), ModTemplateMain.Reflections, prompt);
            CreateCSFile(utils, nameof(ModTemplateMain.JsonSerializer), ModTemplateMain.JsonSerializer, prompt);
            CreateCSFile(utils, nameof(ModTemplateMain.FilesUtilities), ModTemplateMain.FilesUtilities, prompt);
        }

        private static void CreateCSFile(string path, string fileName, string raw, CreateModPromptData prompt)
        {
            string content = raw.Replace("[[ModName]]", prompt.ModName);
            string filePath = Path.Combine(path, $"{fileName}.cs");
            File.WriteAllText(filePath, content);
            AssetDatabase.ImportAsset(filePath);
        }

        private static string EnsureSubFolder(string parent, string name)
        {
            string sub = Normalize(Path.Combine(parent, name));
            if (!AssetDatabase.IsValidFolder(sub))
                AssetDatabase.CreateFolder(parent, name);
            return sub;
        }

        private static string Normalize(string path) => path.Replace("\\", "/");

        private static string GetAssemblyDefinition(string modName) => $@"{{
    ""name"": ""{modName}"",
    ""rootNamespace"": """",
    ""references"": [],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": true,
    ""precompiledReferences"": [
        ""0Harmony.dll"",
        ""UnityModManager.dll"",
        ""BepInEx.dll"",
        ""Newtonsoft.Json.dll"",
        ""Assembly-CSharp.dll"",
        ""Assembly-CSharp-firstpass.dll"",
        ""RDTools.dll""
    ],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";
    }
}
