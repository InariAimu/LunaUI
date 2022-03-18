
using Newtonsoft.Json;

namespace LunaEdit
{
    [Serializable]
    internal class UserConfig
    {
        [JsonProperty("work_path")] public string WorkPath { get; set; } = "";

    }
}
