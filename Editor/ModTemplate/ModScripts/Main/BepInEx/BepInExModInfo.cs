namespace ADOFAIModdingHelper.ModTemplate
{
	public static partial class ModTemplateMain
	{
        public const string BepInExModInfo = @"#if BEPINEX
namespace [[ModName]]
{
    public static class [[ModName]]Info
    {
        // Don't Modify these 3 fields!! They are there so the Mod Info can hook into them and change during build!! 
        public const string PLUGIN_GUID = ""BepInEx.[[ModName]]"";
        public const string PLUGIN_NAME = ""[[ModName]]"";
        public const string PLUGIN_VERSION = ""1.0.0"";
    }
}
#endif
";
	}
}
