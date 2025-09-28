using ADOFAIRunner.Core.DataStructures;
using ADOFAIRunner.DefineSymbols.Core;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ADOFAIRunner.Core
{
    [CreateAssetMenu(fileName = "ARSetting", menuName = "ADOFAI Runner/Settings", order = 1)]
    public class Setting : ScriptableObject
    {
        public string BepInExModFolderPath;
        public string BepInExExePath;

        public string UnityModManagerExePath;
        public string UMMModFolderPath;

        public string ThunderkitOutputPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "ThunderKit");

        public List<ModBuild> AvailableMods = new List<ModBuild>();
        public int AvailableModsSelectedIndex;

        public bool IncludePDBFile = true;
        public string[] AvailableBuildOptions = new string[] { "Unity Mod Manager", "BepInEx" };
        [SerializeField] int _AvailableBuildOptionsSelectedIndex;
        public  int AvailableBuildOptionsSelectedIndex
        {
            get
            {
                return _AvailableBuildOptionsSelectedIndex;
            }
            set
            {
                if (value == _AvailableBuildOptionsSelectedIndex) return;
                _AvailableBuildOptionsSelectedIndex = value;
                DefineSymbolToggler.SetBuild(value);
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void Initialize()
        {
            DefineSymbols.Core.DefineSymbolToggler.SetBuild(0);
            string rootPath = Directory.GetParent(Application.dataPath).FullName;
            string gitignorePath = Path.Combine(rootPath, ".gitignore");

            if (!File.Exists(gitignorePath))
            {
                Debug.LogWarning($".gitignore not found at {gitignorePath}");
                return;
            }

            string header = "# Jofo Setting";
            string rule = "/[Aa]ssets/ADOFAIRunnerSettings";

            string all = File.ReadAllText(gitignorePath);

            bool endsWithNewline = all.EndsWith("\n") || all.EndsWith("\r");

            if (!all.Contains(rule))
            {
                using (StreamWriter sw = File.AppendText(gitignorePath))
                {
                    if (!endsWithNewline) sw.WriteLine(); 
                    sw.WriteLine("");
                    sw.WriteLine(header);
                    sw.Write(rule);
                }

                Debug.Log("Added ADOFAIRunnerSettings ignore rule to .gitignore");
            }
            else
            {
                Debug.Log("Rule already exists in .gitignore");
            }
        }
    }
}