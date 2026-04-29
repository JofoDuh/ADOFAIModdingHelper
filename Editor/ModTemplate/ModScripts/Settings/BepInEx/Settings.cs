namespace ADOFAIModdingHelper.ModTemplate
{
	public static partial class ModTemplateMain
	{
        public const string BepInExSetting = @"#if BEPINEX
using Newtonsoft.Json;
using System;
using [[ModName]].Utilities;
using System.Collections.Generic;
using System.IO;

namespace [[ModName]].Settings
{
    public class Setting
    {
        public void Save()
        {
            var filepath = Path.Combine(Main.ModPath, typeof(Setting).Name + "".json"");
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter> { new Vector2Converter(), new ColorConverter() }
                };
                var json = JsonConvert.SerializeObject(this, settings);
                File.WriteAllText(filepath, json);
            }
            catch (Exception e)
            {
                Main.Logger.Log(e.ToString());
            }
        }

        public static Setting Load()
        {
            var filepath = Path.Combine(Main.ModPath, typeof(Setting).Name + "".json"");

            if (!File.Exists(filepath))
            {
                var newSetting = new Setting();
                return newSetting;
            }

            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter> { new Vector2Converter(), new ColorConverter() },
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                var json = File.ReadAllText(filepath);
                var setting = JsonConvert.DeserializeObject<Setting>(json, settings) ?? new Setting();
                return setting;
            }
            catch (Exception e)
            {
                Main.Logger.Log(""Failed to load settings: "" + e);
                var newSetting = new Setting();
                return newSetting;
            }
        }
    }
}
#endif";
	}
}
