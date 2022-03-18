using System.Drawing;

using Newtonsoft.Json;

namespace LunaUI.JsonConverters
{
    internal class ColorConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string? s = (string?)reader.Value;
            if (s is null)
            {
                return Color.Black;
            }

            string[] cs = s.Split(',');
            if (cs.Length == 4)
            {
                return Color.FromArgb(Convert.ToInt32(cs[0]), Convert.ToInt32(cs[1]), Convert.ToInt32(cs[2]), Convert.ToInt32(cs[3]));
            }

            return Color.FromName(s);
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            if (value.IsKnownColor)
            {
                writer.WriteValue(value.ToKnownColor().ToString());
            }
            else
            {
                writer.WriteValue($"{value.A}, {value.R}, {value.G}, {value.B}");
            }
        }
    }
}
