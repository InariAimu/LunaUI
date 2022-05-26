using Newtonsoft.Json;

namespace LunaUI.Layouts;

[Serializable]
internal class TextKerningConfig
{
    [JsonProperty("text_kerning_conf")]
    public Dictionary<
        string, Dictionary<
            char, Dictionary<
                char, float>>>
        TextKerning
    { get; set; } = new();
}
