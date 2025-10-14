namespace ADOFAIModdingHelper.ModTemplate
{
	public static partial class ModTemplateMain
	{
        public const string BepInExMain = @"#if BEPINEX
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using [[ModName]].Settings;
using [[ModName]].Utilities;
using System.IO;
using UnityEngine;

namespace [[ModName]]
{
    // This is just here so the writing convention of ""Main.Logger.Log()"" doesn't screw up when switching build type
    public static class LoggerExtensions
    {
        public static void Log(this ManualLogSource logger, string message)
        {
            logger.LogInfo(message);
        }
    }

    [BepInPlugin([[ModName]]Info.PLUGIN_GUID, [[ModName]]Info.PLUGIN_NAME, [[ModName]]Info.PLUGIN_VERSION)]
    public class Main : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static Setting ModSetting;
        internal static Harmony MainHarmony;
        internal static AssetBundle Assets, Scenes;
        internal static bool IsEnabled = false;
        internal static string ModPath;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.Log($""Plugin {[[ModName]]Info.PLUGIN_GUID} is loaded!"");
        }

        private void OnEnable()
        {
            IsEnabled = true;
            ModPath = Path.GetDirectoryName(Info.Location);
            ModSetting = Setting.Load();
            Assets = AssetBundle.LoadFromFile(FilesUtility.GetAssetBundlePath(""[[ModName]]_assets.bundle"", ModPath)); // Use if needed to load your asset bundle :D

            MainHarmony = new Harmony([[ModName]]Info.PLUGIN_GUID);
            MainHarmony.PatchAll();
        }

        private void OnDisable()
        {
            ModSetting.Save();
            IsEnabled = false;

            if (MainHarmony != null)
            {
                MainHarmony.UnpatchAll([[ModName]]Info.PLUGIN_GUID);
                MainHarmony = null;
            }

            if (Assets != null)
            {
                Assets.Unload(true);
                Assets = null;
            }
        }
    }
}
#endif
";
	}
}