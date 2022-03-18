using System.ComponentModel;
using System.Drawing;

using Newtonsoft.Json;

namespace LunaUI
{
    [Serializable]
    public class RenderOption : ICloneable
    {
        [JsonIgnore]
        [Browsable(false)]
        public string WorkPath { get; set; } = string.Empty;

        [JsonIgnore]
        [Browsable(false)]
        public Point CanvasLocation { get; set; }

        [JsonIgnore]
        [Browsable(false)]
        public Size CanvasSize { get; set; }

        [JsonIgnore]
        [Browsable(false)]
        public bool ShowRect { get; set; } = false;

        [JsonIgnore]
        [Browsable(false)]
        public TextKerningConfig? TextKerningConfig { get; set; } = null;

        [JsonProperty("text_kerning_config")]
        public string? TextKerningConfFile { get; set; } = null;

        public object Clone()
        {
            var op = new RenderOption
            {
                WorkPath = WorkPath,
                CanvasLocation = new Point(CanvasLocation.X, CanvasLocation.Y),
                CanvasSize = new Size(CanvasSize.Width, CanvasSize.Height),
                ShowRect = ShowRect
            };

            // No need to clone this
            op.TextKerningConfig = TextKerningConfig;
            return op;
        }

        public void SetRect(int x, int y, int w, int h)
        {
            CanvasLocation = new Point(x, y);
            CanvasSize = new Size(w, h);
        }
    }
}
