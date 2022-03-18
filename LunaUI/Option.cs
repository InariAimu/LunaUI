using System.Drawing;

namespace LunaUI
{
    public class Option : ICloneable
    {
        public string WorkPath { get; set; } = string.Empty;
        public Point CanvasLocation { get; set; }
        public Size CanvasSize { get; set; }
        public bool ShowRect { get; set; } = false;

        public object Clone()
        {
            var op = new Option
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
