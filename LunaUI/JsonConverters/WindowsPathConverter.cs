using Newtonsoft.Json;

namespace LunaUI.JsonConverters
{
    public class WindowsPathConverter : JsonConverter<string>
    {
        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Replace('\\', '/'));
        }

        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (reader.Value as string).Replace('/', '\\');
        }
    }
}
