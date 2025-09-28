using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Core.ScriptableObjects;
using ADOFAIModdingHelper.ThunderKitUtils;
using ADOFAIModdingHelper.Utilities;
using ADOFAIModdingHelper.ModTemplate;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Pipelines.Jobs;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
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
            string assetPath = SetupAssetSubfolders(rootFolder, prompt);

            // 6. ThunderKit setup
            var tkPath = EnsureSubFolder(rootFolder, "ThunderKit");
            var manifest = CreateThunderKitManifest(tkPath, asmdefAsset, assetPath, prompt);
            var pipeline = CreateThunderKitPipeline(tkPath, manifest, prompt);

            // 7. Scripts
            GenerateScripts(rootFolder, prompt);

            // 8. Add mod to ADOFAI Runner 
            AddModToADOFAIRunner(modInfo, pipeline);

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
            modInfo.modInfoUMM = new ModInfoUMM
            {
                Id = prompt.ModName,
                DisplayName = prompt.ModName,
                Author = prompt.ModAuthor,
                Version = prompt.ModVersion,
                AssemblyName = $"{prompt.ModName}.dll",
                EntryMethod = $"{prompt.ModName}.Startup.Load"
            };
            modInfo.modInfoBIE = new ModInfoBIE
            {
                GUID = $"BepInEx.{prompt.ModName}",
                PluginName = prompt.ModName,
                PluginVersion = prompt.ModVersion
            };

            string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(rootFolder, $"{prompt.ModName}ModInfo.asset"));
            AssetDatabase.CreateAsset(modInfo, path);
            return modInfo;
        }

        private static string SetupAssetSubfolders(string root, CreateModPromptData prompt)
        {
            string assetPath = EnsureSubFolder(root, "Assets");
            if (prompt.AudiosFolder) EnsureSubFolder(assetPath, "Audios");
            if (prompt.Texture2dFolder) EnsureSubFolder(assetPath, "Textures2D");
            if (prompt.PrefabFolder) EnsureSubFolder(assetPath, "Prefabs");
            if (prompt.ScriptsFolder) EnsureSubFolder(assetPath, "Scripts");
            if (prompt.FontsFolder) EnsureSubFolder(assetPath, "Fonts");
            if (prompt.MaterialsFolder) EnsureSubFolder(assetPath, "Materials");
            if (prompt.ShadersFolder) EnsureSubFolder(assetPath, "Shaders");
            return assetPath;
        }

        private static Manifest CreateThunderKitManifest(string tkPath, AssemblyDefinitionAsset asmdef, string assetPath, CreateModPromptData prompt)
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
                assets = new UnityEngine.Object[] { AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object)) }
            };
            var bundleDatum = ModManifestUtils.GetManifestDatum<AssetBundleDefinitions>(manifest);
            var bundles = bundleDatum.assetBundles?.ToList() ?? new List<AssetBundleDefinition>();
            bundles.Add(assetBundleDef);
            bundleDatum.assetBundles = bundles.ToArray();

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

            return pipeline;
        }

        private static void GenerateScripts(string root, CreateModPromptData prompt)
        {
            string modScripts = EnsureSubFolder(root, "ModScripts");

            // Main
            string mainPath = EnsureSubFolder(modScripts, "Main");
            CreateCSFile(EnsureSubFolder(mainPath, "UMM"), "Main", ModTemplateMain.UMMMain, prompt);
            CreateCSFile(EnsureSubFolder(mainPath, "UMM"), nameof(ModTemplateMain.StartUp), ModTemplateMain.StartUp, prompt);
            CreateCSFile(EnsureSubFolder(mainPath, "BepInEx"), "Main", ModTemplateMain.BepInExMain, prompt);
            CreateCSFile(EnsureSubFolder(mainPath, "BepInEx"), nameof(ModTemplateMain.BepInExModInfo), ModTemplateMain.BepInExModInfo, prompt);

            // Settings
            string settingPath = EnsureSubFolder(modScripts, "Setting");
            CreateCSFile(EnsureSubFolder(settingPath, "UMM"), "Setting", ModTemplateMain.UMMSetting, prompt);
            CreateCSFile(EnsureSubFolder(settingPath, "BepInEx"), "Setting", ModTemplateMain.BepInExSetting, prompt);

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
