using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.ScriptableObjects;
using ADOFAIModdingHelper.ModTemplate;
using ADOFAIModdingHelper.Utilities;
using System.Collections.Generic;
using System.IO;
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

            // 5. Mod tools config asset
            CreateModToolsConfig(rootFolder, prompt, modInfo, asmdefAsset);

            // 6. Assets folder
            string assetPath;
            if (prompt.AssetFolder) assetPath = SetupAssetSubfolders(rootFolder, prompt);

            // 7. Scenes folder
            string scenePath;
            if (prompt.SceneFolder) scenePath = SetupScenefolder(rootFolder, prompt);

            // 8. Scripts
            GenerateScripts(rootFolder, prompt);

            Debug.Log($"Created mod template at {rootFolder}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ProjectWindowPathHelper.OpenFolder(rootFolder);
        }

        // ----------------- Helpers -----------------
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
            modInfo.AssemblyName = $"{prompt.ModName}.dll";
            modInfo.EntryMethod = $"{prompt.ModName}.Startup.Load";
            modInfo.Id = prompt.ModID;
            modInfo.DisplayName = prompt.ModName;
            modInfo.Author = prompt.ModAuthor;
            modInfo.Version = string.IsNullOrEmpty(prompt.ModVersion) ? "1.0.0" : prompt.ModVersion;

            string path = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(rootFolder, $"ModInfo.asset"));
            AssetDatabase.CreateAsset(modInfo, path);
            return modInfo;
        }

        private static void CreateModToolsConfig(string rootFolder, CreateModPromptData prompt,
            ModInfo modInfo, AssemblyDefinitionAsset asmdefAsset)
        {
            var config = ScriptableObject.CreateInstance<ModToolsConfig>();

            config.modInfo = modInfo;
            config.AssemblyDefinitions = new List<AssemblyDefinitionAsset> { asmdefAsset };
            config.PrecompAssemblies = new List<string>();
            config.AssetBundles = new List<string>();

            config.skipAssetBundleBuild = true;
            config.developmentBuild = true;
            config.generateDebugSymbols = true;
            config.buildEveryPlatform = false;
            config.copyToDirectory = false;
            config.runApplication = false;
            config.createZip = false;

            config.ScenesPath = Normalize(Path.Combine(rootFolder, "Scenes"));

            string path = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(rootFolder, $"ModToolsConfig.asset"));
            AssetDatabase.CreateAsset(config, path);

            var setting = Setting.Config;
            setting.CurrentConfig = config;
            EditorUtility.SetDirty(setting);
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
                File.Copy(Path.Combine(Constants.ADOFAIModdingHelperRootPath, "Scenes", "TemplateScene.unity"), sceneTemplatePath);
                AssetDatabase.ImportAsset(sceneTemplatePath);
            }
            return scenePath;
        }

        private static void GenerateScripts(string root, CreateModPromptData prompt)
        {
            string modScripts = EnsureSubFolder(root, "ModScripts");

            string mainPath = EnsureSubFolder(modScripts, "Main");
            string settingPath = EnsureSubFolder(modScripts, "Setting");

            CreateCSFile(mainPath, "Main", ModTemplateMain.UMMMain, prompt);
            CreateCSFile(mainPath, nameof(ModTemplateMain.StartUp), ModTemplateMain.StartUp, prompt);
            CreateCSFile(settingPath, "Setting", ModTemplateMain.UMMSetting, prompt);

            CreateCSFile(EnsureSubFolder(modScripts, "Patches"), nameof(ModTemplateMain.ExamplePatch), ModTemplateMain.ExamplePatch, prompt);

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
