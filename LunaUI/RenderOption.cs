using System.Drawing;
using System.ComponentModel;

namespace LunaUI
{
    public class RenderOption : ICloneable
    {
        public string WorkPath { get; set; } = string.Empty;

        [Browsable(false)]
        public Point CanvasLocation { get; set; }

        [Browsable(false)]
        public Size CanvasSize { get; set; }

        [Browsable(false)]
        public bool ShowRect { get; set; } = false;

        public object Clone()
        {
            var op = new RenderOption
            {
                WorkPath = WorkPath,
                CanvasLocation = new Point(CanvasLocation.X, CanvasLocation.Y),
                CanvasSize = new Size(CanvasSize.Width, CanvasSize.Height),
                ShowRect = ShowRect
            };
            return op;
        }

        public void SetRect(int x, int y, int w, int h)
        {
            CanvasLocation = new Point(x, y);
            CanvasSize = new Size(w, h);
        }
    }
}
