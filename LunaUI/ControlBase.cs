using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace LunaUI
{
    [Serializable]
    [XmlInclude(typeof(LuiImage))]
    [XmlInclude(typeof(LuiText))]
    [XmlInclude(typeof(LuiColorLayer))]
    public class ControlBase
    {
        public string Name { get; set; } = "";
        public bool Visible { get; set; } = true;
        public Point Position { get; set; }

        [TypeConverter(typeof(PointFConverter))]
        public PointF Pivot { get; set; }
        public Size Size { get; set; }

        [TypeConverter(typeof(PointFConverter))]
        public PointF Docking { get; set; }
        public List<ControlBase> Childs { get; set; } = new List<ControlBase>();

        [XmlIgnore, Browsable(false)]
        public bool ShowLayoutRect { get; set; } = false;

        [XmlIgnore, Browsable(false)]
        public ControlBase? Parent { get; set; } = null;

        public ControlBase()
        {
            Name = "Empty";
        }

        public virtual void Init(Option op)
        {
            foreach (var child in Childs)
            {
                child.Init(op);
            }
        }

        public virtual void Render(Graphics g, Option op)
        {
            if (!Visible)
                return;

            float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + ((Docking.X < 1 ? 1 : -1) * Position.X) - Size.Width * Pivot.X;
            float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + ((Docking.Y < 1 ? 1 : -1) * Position.Y) - Size.Height * Pivot.Y;

            Option next = (Option)op.Clone();
            next.SetRect((int)x, (int)y, Size.Width, Size.Height);
            RenderChilds(g, next);

            if (ShowLayoutRect)
                g.DrawRectangle(new Pen(Color.Coral, 1.0f), x, y, Size.Width, Size.Height);
        }

        public void RenderChilds(Graphics g, Option op)
        {
            foreach (var child in Childs)
            {
                child.Render(g, op);
            }
        }

        public void Move(int dx, int dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);

            foreach (var child in Childs)
            {
                child.Position = new Point(child.Position.X + dx, child.Position.Y + dy);
            }
        }
    }
}
