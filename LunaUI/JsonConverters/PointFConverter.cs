using System.Drawing;

using Newtonsoft.Json;

namespace LunaUI.JsonConverters
{
    public class PointFConverter : JsonConverter<PointF>
    {
        public override void WriteJson(JsonWriter writer, PointF value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value.X}, {value.Y}");
        }

        public override PointF ReadJson(JsonReader reader, Type objectType, PointF existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string? s = (string?)reader.Value;
            if (s is null)
            {
                return new PointF(0, 0);
            }

            float x = 0;
            float y = 0;
            var sd = s.Split(',');
            float.TryParse(sd[0], out x);
            float.TryParse(sd[1], out y);
            return new PointF(x, y);
        }
    }
}
