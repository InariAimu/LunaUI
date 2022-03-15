using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace LunaUI
{
    public class LuiColorLayer : LuiLayout
    {
        [XmlElement, Browsable(false)]
        public int _color { get; set; } = Color.Black.ToArgb();

        [XmlIgnore]
        public Color Color
        {
            get => Color.FromArgb(_color);
            set => _color = value.ToArgb();
        }

        public LuiColorLayer()
        {
            Name = "ColorLayer";
        }

        public override void Render(Graphics g, RenderOption op)
        {
            if (!Visible)
                return;

            float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + ((Docking.X < 1 ? 1 : -1) * Position.X) - Size.Width * Pivot.X;
            float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + ((Docking.Y < 1 ? 1 : -1) * Position.Y) - Size.Height * Pivot.Y;

            g.FillRectangle(new SolidBrush(Color), Rectangle.FromLTRB((int)x, (int)y, (int)x + Size.Width, (int)y + Size.Height));

            RenderLayoutRect(g, op, x, y);

            RenderOption next = (RenderOption)op.Clone();
            next.SetRect((int)x, (int)y, Size.Width, Size.Height);
            RenderChilds(g, next);
        }

    }
}
