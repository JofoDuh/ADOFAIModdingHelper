using UnityEngine;

namespace ADOFAIModdingHelper.Core
{
    //[CreateAssetMenu(fileName = "AMHSetting", menuName = "ADOFAI Modding Helper/Settings", order = 1)]
    public class Setting : ScriptableObject
    {
        public void Initialize()
        {
            //string rootPath = Directory.GetParent(Application.dataPath).FullName;
            //string gitignorePath = Path.Combine(rootPath, ".gitignore");

            //if (!File.Exists(gitignorePath))
            //{
            //    Debug.LogWarning($".gitignore not found at {gitignorePath}");
            //    return;
            //}

            //string header = "# Jofo Setting";
            //string rule = "/[Aa]ssets/ADOFAIRunnerSettings";

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

            //    Debug.Log("Added ADOFAIRunnerSettings ignore rule to .gitignore");
            //}
            //else
            //{
            //    Debug.Log("Rule already exists in .gitignore");
            //}
        }
    }
}