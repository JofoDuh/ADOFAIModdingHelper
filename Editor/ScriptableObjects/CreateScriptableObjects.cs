using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Utilities;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace ADOFAIModdingHelper.ScriptableObjects
{
    public static class CreateScriptableObjects
    {
        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "ScriptableObjects/" + "Create Mod Info", false, priority: Constants.ADOFAIModdingHelperMenuPriority)]
        public static void CreateModInfo()
        {
            //CreateShitsAction<ModInfo>();
        }       
    }
}
