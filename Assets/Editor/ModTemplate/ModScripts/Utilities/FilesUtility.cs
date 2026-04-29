namespace ADOFAIModdingHelper.ModTemplate
{
	public static partial class ModTemplateMain
	{
        public const string FilesUtilities = @"using System;
using System.IO;

namespace [[ModName]].Utilities
{
    public static class FilesUtility
    {
        /// <summary>
        /// Helper file method to find a specific file in mod folder.
        /// </summary>
        public static string GetAssetBundlePath(string targetFileName, string ModPath)
        {
            // This was the original method, but due to errors when generating, I opted for reflection. The main issue is that, when you, for example, set current build target to UMM but generate a mod only for BepInEx, this method screams because ""Main"" don't exist, since it has been marked out due to #if BepInEx

            //try
            //{
            //    string[] files = Directory.GetFiles(ModPath, targetFileName, SearchOption.AllDirectories);

            //    if (files.Length == 0)
            //    {
            //        Main.Logger.Log(""File not found."");
            //        return null;
            //    }
            //    else
            //    {
            //        foreach (string filePath in files)
            //        {
            //            Main.Logger.Log(""Found file at: "" + filePath);
            //        }
            //        return files[0];
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Main.Logger.Log(""Error: "" + ex.Message);
            //    return null;
            //}

            try
            {
                string[] files = Directory.GetFiles(ModPath, targetFileName, SearchOption.AllDirectories);

                if (files.Length == 0)
                {
                    CallMainLogger(""File not found."");
                    return null;
                }
                else
                {
                    foreach (string filePath in files)
                    {
                        CallMainLogger(""Found file at: "" + filePath);
                    }
                    return files[0];
                }
            }
            catch (Exception ex)
            {
                CallMainLogger(""Error: "" + ex.Message);
                return null;
            }
        }
        private static void CallMainLogger(string message)
        {
            var mainType = Reflections.GetType(""[[ModName]].Main"");
            if (mainType == null) return;

            var logger = mainType.Get(""Logger"");

            if (logger == null) return;

            logger.Method(""Log"", new object[] { message });
        }
    }
}";
	}
}    