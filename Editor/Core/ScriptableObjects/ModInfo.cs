using ADOFAIModdingHelper.Common;
using UnityEngine;

namespace ADOFAIModdingHelper.Core.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ModInfo", menuName = Constants.ADOFAIModdingHelperRoot + "Mod Info", order = 1)]
    public class ModInfo : ScriptableObject
    {
        public ModInfoUMM modInfoUMM;
        public ModInfoBIE modInfoBIE;
    }

    [System.Serializable]
    public class ModInfoUMM
    {
        public string Id;
        public string DisplayName;
        public string Author;
        public string Version;
        public string ManagerVersion;
        public string GameVersion;
        public string[] Requirements;
        public string[] LoadAfter;
        public string AssemblyName;
        public string EntryMethod;
        public string HomePage;
        public string Repository;
        public string ContentType;
    }

    [System.Serializable]
    public class ModInfoBIE
    {
        public string GUID;
        public string PluginName;
        public string PluginVersion;
        public string BIPModInfoCSPath;
    }
}