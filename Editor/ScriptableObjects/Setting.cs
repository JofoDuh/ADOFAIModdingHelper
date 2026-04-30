using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.ScriptableObjects
{
    public class Setting : ScriptableObject
    {
        private static Setting _config;

        public static Setting Config
        {
            get
            {
                if (_config)
                    return _config;
                _config = AssetDatabase.LoadAssetAtPath<Setting>(Constants.settingsFolder + "/AMHSettings.asset");
                if (_config) return _config;

                _config = CreateInstance<Setting>();
                if (!Directory.Exists(Constants.settingsFolder))
                {
                    Directory.CreateDirectory(Constants.settingsFolder);
                }
                AssetDatabase.CreateAsset(_config, Constants.settingsFolder + "/AMHSettings.asset");

                return _config;
            }
        }

        public GameImporter Importer = new GameImporter();
        public string ADOFAIPath;
        public bool SeperateBuildTabs;
        private List<ModToolsConfig> _allModsCache;

        public List<ModToolsConfig> AllMods
        {
            get
            {
                if (_allModsCache != null) return _allModsCache;
                _allModsCache = AssetDatabase.FindAssets("t:ModToolsConfig")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<ModToolsConfig>)
                    .Where(config => config != null)
                    .ToList();
                return _allModsCache;
            }
        }

        public ModToolsConfig CurrentConfig;

        public void RunAppWithoutConfig() => Process.Start(Config.ADOFAIPath);
        private void OnEnable()
        {
            EditorApplication.projectChanged += InvalidateCache;
        }
        private void OnDisable()
        {
            EditorApplication.projectChanged -= InvalidateCache;
        }
        private void InvalidateCache() => _allModsCache = null;
    }
}