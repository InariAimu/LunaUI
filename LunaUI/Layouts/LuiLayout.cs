
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace LunaUI
{
    [Serializable]
    [XmlInclude(typeof(LuiImage))]
    [XmlInclude(typeof(LuiText))]
    [XmlInclude(typeof(LuiColorLayer))]
    [XmlInclude(typeof(LuiListLayout))]
    public class LuiLayout
    {
        [Category("Layout")]
        public string Name { get; set; } = "";
        
        [Category("Layout")]
        public bool Visible { get; set; } = true;

        [Category("Layout")]
        public Point Position { get; set; }

        [Category("Layout"), TypeConverter(typeof(PointFConverter))]
        public PointF Pivot { get; set; }

        [Category("Layout")]
        public Size Size { get; set; }

        [Category("Layout"), TypeConverter(typeof(PointFConverter))]
        public PointF Docking { get; set; }

        [Category("Experimental")]
        public bool XAutoSize { get; set; } = false;

        [Category("Experimental")]
        public bool YAutoSize { get; set; } = false;

        public List<LuiLayout> Childs { get; set; } = new List<LuiLayout>();

        [XmlIgnore, Browsable(false)]
        public bool ShowLayoutRect { get; set; } = false;

        [XmlIgnore, Browsable(false)]
        public LuiLayout? Parent { get; set; } = null;

        public LuiLayout()
        {
            Name = "Layout";
        }

        public virtual void Init(RenderOption op)
        {
            foreach (var child in Childs)
            {
                child.Init(op);
            }
        }

        public virtual void Render(Graphics g, RenderOption op)
        {
            if (!Visible)
                return;

            float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + ((Docking.X < 1 ? 1 : -1) * Position.X) - Size.Width * Pivot.X;
            float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + ((Docking.Y < 1 ? 1 : -1) * Position.Y) - Size.Height * Pivot.Y;

            RenderOption next = (RenderOption)op.Clone();
            next.SetRect((int)x, (int)y, Size.Width, Size.Height);
            RenderChilds(g, next);

            RenderLayoutRect(g, op, x, y);
        }

        public LuiLayout? GetByPoint(int sx, int sy, RenderOption op)
        {
            if (!Visible)
                return null;

            float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + ((Docking.X < 1 ? 1 : -1) * Position.X) - Size.Width * Pivot.X;
            float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + ((Docking.Y < 1 ? 1 : -1) * Position.Y) - Size.Height * Pivot.Y;

            RenderOption next = (RenderOption)op.Clone();
            next.SetRect((int)x, (int)y, Size.Width, Size.Height);

            for (int i = Childs.Count - 1; i >= 0; i--)
            {
                var child = Childs[i];
                LuiLayout? layout = child.GetByPoint(sx, sy, next);
                if (layout != null)
                    return layout;
            }

            Rectangle rect = new Rectangle((int)x, (int)y, Size.Width, Size.Height);
            if (rect.Contains(sx, sy))
                return this;

            return null;
        }

        public void RenderLayoutRect(Graphics g, RenderOption op, float x, float y)
        {
            if (ShowLayoutRect)
            {
                if (Parent != null)
                    g.DrawRectangle(new Pen(Color.Coral, 1.0f), op.CanvasLocation.X, op.CanvasLocation.Y, op.CanvasSize.Width - 1, op.CanvasSize.Height - 1);

                g.DrawRectangle(new Pen(Color.Red, 1.0f), x, y, Size.Width - 1, Size.Height - 1);
            }
        }

        public void RenderChilds(Graphics g, RenderOption op)
        {
            foreach (var child in Childs)
            {
                child.SetParent(this);
                child.Render(g, op);
            }
        }

        public void SetParent(LuiLayout parent) => Parent = parent;

        public void Move(int dx, int dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
        }
    }
}
