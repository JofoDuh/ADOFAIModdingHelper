//Original Code from https://github.com/ADOFAI-gg/ADOFAI-Modding-Toolkit
using ADOFAIModdingHelper.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ADOFAIModdingHelper.ScriptableObjects
{
    public class ModToolsConfig : ScriptableObject
    {
        public string BuildDirectory;

        public bool openModInfoFoldout;

        public bool skipAssetBundleBuild;

        public bool createZip;

        public bool developmentBuild = true;
        public bool generateDebugSymbols;

        public bool buildEveryPlatform;
        public BuildTarget[] serializedBuildPlatforms;

        public bool copyToDirectory;

        public bool runApplication;
        public bool runApplicationThroughSteam;

        public int deleteBuildsExceptLastN;
        public bool automaticallyDeleteBuilds;

        public ModInfo modInfo;

        public List<AssemblyDefinitionAsset> AssemblyDefinitions;
        public List<string> PrecompAssemblies;
        public List<string> AssetBundles;

        public string RepositoryLink;
        public string IssuesLink;
        public string PullRequestsLink;

        public string ScenesPath;

        private HashSet<BuildTarget> _buildTargets;
        public HashSet<BuildTarget> BuildPlatforms
        {
            get
            {
                if (_buildTargets == null)
                    return _buildTargets ??= serializedBuildPlatforms?.ToHashSet() ?? new();

                return _buildTargets;
            }
            set
            {
                _buildTargets = value;
                serializedBuildPlatforms = value.ToArray();
            }
        }

        public readonly ModBuilder ModBuilder = new();

        public void BuildMod(string copyDestination)
        {
            ModBuilder.SkipAssetBundleBuild = skipAssetBundleBuild;
            ModBuilder.DevelopmentBuild = developmentBuild;
            ModBuilder.GenerateDebugSymbols = generateDebugSymbols;

            ModBuilder.modInfo = modInfo;
            ModBuilder.AssemblyDefinitions = AssemblyDefinitions;
            ModBuilder.AssetBundles = RemoveNoneFromList(AssetBundles);
            ModBuilder.PrecompAssemblies = RemoveNoneFromList(PrecompAssemblies);

            ModBuilder.Build(copyDestination, buildEveryPlatform, BuildPlatforms)
                .ContinueWith(task =>
                {
                    if (createZip)
                    {
                        using var stream =
                            new FileStream(Path.Combine(Path.GetDirectoryName(task.Result)!, modInfo?.Id ?? $"{name}_{GetInstanceID()}" + ".zip"),
                                FileMode.Create);
                        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

                        foreach (var file in Directory.GetFiles(task.Result, "*", SearchOption.AllDirectories))
                        {
                            archive.CreateEntryFromFile(file,
                                Path.Combine(modInfo.Id, Path.GetRelativePath(task.Result, file)));
                        }
                    }

                    if (automaticallyDeleteBuilds)
                        DeleteBuilds(1);

                    RunApp();
                });
        }

        private List<string> RemoveNoneFromList(List<string> list)
        {
            var newlist = new List<string>(list);
            newlist.RemoveAll(item => item == "None");
            return newlist;
        }
        public void RunApp()
        {
            if (runApplication)
            {
                if (runApplicationThroughSteam)
                {
                    Process.Start("steam://rungameid/977950");
                }
                else
                {
                    Process.Start(Setting.Config.ADOFAIPath);
                }
            }
        }
        public void DeleteBuilds(int? saveLeast = null)
        {
            var buildDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", modInfo?.Id ?? $"{name}_{GetInstanceID()}");

            if (Directory.Exists(buildDir))
            {
                var except = deleteBuildsExceptLastN;

                if (except == 0)
                {
                    Directory.Delete(buildDir, true);
                    Directory.CreateDirectory(buildDir);

                    var zipPath = Path.Combine(buildDir, modInfo?.Id ?? $"{name}_{GetInstanceID()}" + ".zip");

                    if (File.Exists(zipPath))
                        File.Delete(zipPath);
                }
                else
                {
                    var buildDirectories = new DirectoryInfo(buildDir).GetDirectories()
                        .OrderByDescending(d => d.CreationTimeUtc)
                        .ToList();

                    for (var i = Math.Max(saveLeast ?? 0, Math.Max(0, except)); i < buildDirectories.Count; i++)
                    {
                        buildDirectories[i].Delete(true);
                    }
                }
            }
        }

        public void ApplyPreset(string preset)
        {
            switch (preset)
            {
                case "Debug":
                    copyToDirectory = true;
                    buildEveryPlatform = false;
                    developmentBuild = true;
                    generateDebugSymbols = true;
                    createZip = false;
                    runApplication = true;
                    break;
                case "Release":
                    buildEveryPlatform = true;
                    developmentBuild = false;
                    generateDebugSymbols = false;
                    createZip = true;
                    break;
                case "Clear":
                    skipAssetBundleBuild = false;
                    buildEveryPlatform = false;
                    developmentBuild = false;
                    generateDebugSymbols = false;
                    copyToDirectory = false;
                    createZip = false;
                    runApplication = false;
                    runApplicationThroughSteam = false;
                    break;
            }
        }
    }
}