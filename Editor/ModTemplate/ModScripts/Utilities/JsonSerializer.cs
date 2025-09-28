namespace ADOFAIModdingHelper.ModTemplate
{
	public static partial class ModTemplateMain
	{
        public const string JsonSerializer = @"using Newtonsoft.Json;
using System;
using UnityEngine;

namespace [[ModName]].Utilities
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(""x"");
            writer.WriteValue(value.x);
            writer.WritePropertyName(""y"");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propName = (string)reader.Value;
                    reader.Read();
                    if (propName == ""x"") x = Convert.ToSingle(reader.Value);
                    else if (propName == ""y"") y = Convert.ToSingle(reader.Value);
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }
            return new Vector2(x, y);
        }
    }

    public class ColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(""r"");
            writer.WriteValue(value.r);
            writer.WritePropertyName(""g"");
            writer.WriteValue(value.g);
            writer.WritePropertyName(""b"");
            writer.WriteValue(value.b);
            writer.WritePropertyName(""a"");
            writer.WriteValue(value.a);
            writer.WriteEndObject();

        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float r = 0, g = 0, b = 0, a = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propName = (string)reader.Value;
                    reader.Read();
                    if (propName == ""r"") r = Convert.ToSingle(reader.Value);
                    else if (propName == ""g"") g = Convert.ToSingle(reader.Value);
                    else if (propName == ""b"") b = Convert.ToSingle(reader.Value);
                    else if (propName == ""a"") a = Convert.ToSingle(reader.Value);
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }
            return new Color(r, g, b, a);
        }
    }
}";
	}
}

