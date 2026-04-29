using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Core;
using System.Collections.Generic;
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
        public List<ModToolsConfig> AllMods
        {
            get
            {
                return AssetDatabase.FindAssets("t:ModToolsConfig")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ModToolsConfig>)
                .Where(config => config != null)
                .ToList();
            }
        }

        public ModToolsConfig CurrentConfig;

        private void OnValidate()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void Initialize()
        {
            //DefineSymbolToggler.SetBuild(0);
            //string rootPath = Directory.GetParent(Application.dataPath).FullName;
            //string gitignorePath = Path.Combine(rootPath, ".gitignore");

            //if (!File.Exists(gitignorePath))
            //{
            //    Debug.LogWarning($".gitignore not found at {gitignorePath}");
            //    return;
            //}

            //string header = "# ADOFAI Modding Helper Setting";
            //string rule = "/[Aa]ssets/AMHSettings";

            //string all = File.ReadAllText(gitignorePath);

            //bool endsWithNewline = all.EndsWith("\n") || all.EndsWith("\r");

            //if (!all.Contains(rule))
            //{
            //    using (StreamWriter sw = File.AppendText(gitignorePath))
            //    {
            //        if (!endsWithNewline) sw.WriteLine();
            //        sw.WriteLine("");
            //        sw.WriteLine(header);
            //        sw.Write(rule);
            //    }

            //    Debug.Log("Added AMHSettings ignore rule to .gitignore");
            //}
            //else
            //{
            //    Debug.Log("Rule already exists in .gitignore");
            //}
        }
    }
}