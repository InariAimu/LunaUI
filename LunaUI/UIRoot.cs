
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LunaUI
{
    [Serializable]
    public class UIRoot
    {
        [JsonProperty("ui_options")]
        public RenderOption? Option = null;

        [JsonProperty("layouts")]
        public LuiLayout? Root = null;
    }
}
