using System.ComponentModel;
using System.Drawing;
using LunaUI.Layouts;
using Newtonsoft.Json;

namespace LunaUI
{
    [Serializable]
    public class LuiColorLayer : LuiLayout
    {
        [JsonProperty("color")]
        [Browsable(false)]
        public int _color { get; set; } = Color.Black.ToArgb();

        [JsonIgnore]
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
            {
                return;
            }

            float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - Size.Width * Pivot.X;
            float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - Size.Height * Pivot.Y;

            g.FillRectangle(new SolidBrush(Color), Rectangle.FromLTRB((int)x, (int)y, (int)x + Size.Width, (int)y + Size.Height));

            RenderLayoutRect(g, op, x, y);

            RenderOption next = (RenderOption)op.Clone();
            next.SetRect((int)x, (int)y, Size.Width, Size.Height);
            RenderChilds(g, next);
        }

        protected LuiColorLayer(LuiColorLayer copy) : base(copy)
        {
            this.Color = copy.Color;
        }

        public override LuiLayout DeepClone()
        {
            return new LuiColorLayer(this);
        }
    }
}
