using ADOFAIModdingHelper.Core.ScriptableObjects; 
using ThunderKit.Core.Pipelines;
using UnityEngine;

namespace ADOFAIRunner.Core.DataStructures
{
    [System.Serializable]
    public class ModBuild
    {
        [SerializeField] public ModInfo ModInfo;
        [SerializeField] public Pipeline Pipeline;
    }
}