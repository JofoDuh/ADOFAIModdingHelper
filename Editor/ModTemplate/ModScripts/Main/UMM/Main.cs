namespace ADOFAIModdingHelper.ModTemplate
{
	public static partial class ModTemplateMain
	{
        public const string UMMMain = @"#if UNITYMODMANAGER
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using [[ModName]].Utilities;
using [[ModName]].Settings;

namespace [[ModName]]
{
    public static class Main
    {
        // Unity Mod Manager fields
        public static UnityModManager.ModEntry ModEntry;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static string ModPath;

        // Mod fields
        public static bool IsEnabled;
        public static AssetBundle Assets, Scenes;
        public static Harmony MainHarmony;
        public static Setting ModSetting;

        internal static void Setup(UnityModManager.ModEntry modEntry)
        {
            ModPath = modEntry.Path;
            ModEntry = modEntry;
            Logger = modEntry.Logger;
            ModSetting = Setting.Load(modEntry);
            modEntry.OnToggle = OnToggle;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            IsEnabled = value;
            if (IsEnabled)
            {
                Assets = AssetBundle.LoadFromFile(FilesUtility.GetAssetBundlePath(""[[ModName]]_assets.bundle"")); // Use if needed to load your asset bundle :D

                MainHarmony = new Harmony(modEntry.Info.Id);
                MainHarmony.PatchAll();
                modEntry.OnGUI = OnGUI;
                modEntry.OnSaveGUI = OnSaveGUI;
            }
            else
            {
                MainHarmony.UnpatchAll();
            }
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (GUILayout.Button(""Hello World!""))
            {
                Logger.Log(""Hello World!"");
            }
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            ModSetting.Save(modEntry);
        }
    }
}
#endif";
	}
}
