
using System.ComponentModel;
using System.Drawing;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LunaUI.Layouts;

public enum FlowMode
{
    None,
    Auto,
    Expand,
    Shrink,
}

public enum LocatingMode
{
    Absolute,
    Relative,
}

/// <summary>
/// Base class for all Layouts.
/// Also used as an empty layout.
/// </summary>
[Serializable]
public class LuiLayout
{
    /// <summary>
    /// Layout object name.
    /// </summary>
    [JsonProperty("name")]
    [Category("Layout")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Relative position of layout to it's parent layout.
    /// </summary>
    [JsonProperty("position")]
    [Category("Layout")]
    public Point Position { get; set; }

    /// <summary>
    /// Pivot of this layout for positioning.<br/>
    /// (0, 0) for topleft, (1, 0) for topright, (0.5, 0.5) for center.
    /// </summary>
    [JsonProperty("pivot")]
    [JsonConverter(typeof(JsonConverters.PointFConverter))]
    [Category("Layout"), TypeConverter(typeof(PointFConverter))]
    public PointF Pivot { get; set; }

    /// <summary>
    /// Percent position relative to parent layout.<br/>
    /// For example, <see cref="Pivot"/>(0.5, 0.5) and <see cref="Docking"/>(0.5, 0.5) will put this layout
    /// at center location in parent layout.
    /// </summary>
    [JsonProperty("docking")]
    [JsonConverter(typeof(JsonConverters.PointFConverter))]
    [Category("Layout"), TypeConverter(typeof(PointFConverter))]
    public PointF Docking { get; set; }

    /// <summary>
    /// Layout size in pixels.
    /// </summary>
    [JsonProperty("size")]
    [Category("Layout")]
    public Size Size { get; set; }

    [JsonProperty("visible")]
    [Category("Layout")]
    public bool Visible { get; set; } = true;



    [JsonProperty("flow_x")]
    [Category("Experimental")]
    public FlowMode FlowX { get; set; } = FlowMode.None;

    [JsonProperty("flow_y")]
    [Category("Experimental")]
    public FlowMode FlowY { get; set; } = FlowMode.None;

    [JsonProperty("flow_padding_bottom")]
    [Category("Experimental")]
    public int PaddingBottom { get; set; } = 0;

    [JsonProperty("flow_padding_right")]
    [Category("Experimental")]
    public int PaddingRight { get; set; } = 0;

    [JsonIgnore]
    [Category("Experimental")]
    public Size RenderSize { get; protected set; }

    [JsonIgnore]
    public Point RenderLocation { get; protected set; }


    [JsonProperty("sub_layouts")]
    public List<LuiLayout> SubLayouts { get; set; } = new List<LuiLayout>();

    [JsonIgnore]
    [Browsable(false)]
    public bool ShowLayoutRect { get; set; } = false;

    [JsonIgnore]
    [Browsable(false)]
    public LuiLayout? Parent { get; set; } = null;

    public LuiLayout()
    {
        Name = "Layout";
    }

    public virtual void Init(RenderOption op)
    {
        foreach (var child in SubLayouts)
        {
            child.Init(op);
        }
    }

    public virtual void Render(Graphics g, RenderOption op)
    {
        if (!Visible)
            return;

        float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - GetWidth() * Pivot.X;
        float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - GetHeight() * Pivot.Y;

        RenderOption next = (RenderOption)op.Clone();
        next.SetRect((int)x, (int)y, GetWidth(), GetHeight());
        RenderChilds(g, next);

        RenderLayoutRect(g, op, x, y);
    }

    public int GetWidth()
        => FlowX == FlowMode.None ? Size.Width : RenderSize.Width;

    public int GetHeight()
        => FlowY == FlowMode.None ? Size.Height : RenderSize.Height;

    public virtual void CalcSize(Graphics g, RenderOption op)
    {
        RenderSize = Size;
        float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - GetWidth() * Pivot.X;
        float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - GetHeight() * Pivot.Y;
        RenderLocation = new Point((int)x, (int)y);

        foreach (var child in SubLayouts)
        {
            child.CalcSize(g, op);
        }

        if (FlowX == FlowMode.None && FlowY == FlowMode.None)
        {
            RenderSize = Size;
            return;
        }

        int maxWidth = 0;
        int maxHeight = 0;

        foreach (var child in SubLayouts)
        {
            maxWidth = Math.Max(child.RenderLocation.X + child.RenderSize.Width, maxWidth);
            maxHeight = Math.Max(child.RenderLocation.Y + child.RenderSize.Height, maxHeight);
        }

        int w = FlowX switch
        {
            FlowMode.Auto => maxWidth,
            FlowMode.Shrink => Math.Min(maxWidth, Size.Width),
            FlowMode.Expand => Math.Max(maxWidth, Size.Width),
            _ => Size.Width,
        };
        int h = FlowY switch
        {
            FlowMode.Auto => maxHeight,
            FlowMode.Shrink => Math.Min(maxHeight, Size.Height),
            FlowMode.Expand => Math.Max(maxHeight, Size.Height),
            _ => Size.Height,
        };

        RenderSize = new Size(w + PaddingRight, h + PaddingBottom);
    }

    public LuiLayout? GetByPoint(int sx, int sy, RenderOption op)
    {
        if (!Visible)
            return null;

        float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - GetWidth() * Pivot.X;
        float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - GetHeight() * Pivot.Y;

        RenderOption next = (RenderOption)op.Clone();
        next.SetRect((int)x, (int)y, Size.Width, Size.Height);

        for (int i = SubLayouts.Count - 1; i >= 0; i--)
        {
            var child = SubLayouts[i];
            var layout = child.GetByPoint(sx, sy, next);
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

            g.DrawRectangle(new Pen(Color.Red, 1.0f), x, y, GetWidth() - 1, GetHeight() - 1);
        }
    }

    public void RenderChilds(Graphics g, RenderOption op)
    {
        foreach (var child in SubLayouts)
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

    protected LuiLayout(LuiLayout copy)
    {
        Name = copy.Name;
        Position = copy.Position;
        Pivot = copy.Pivot;
        Docking = copy.Docking;
        Size = copy.Size;
        Visible = copy.Visible;
        Parent = copy.Parent;
        FlowX = copy.FlowX;
        FlowY = copy.FlowY;
        PaddingRight = copy.PaddingRight;
        PaddingBottom = copy.PaddingBottom;

        RenderSize = copy.RenderSize;
        RenderLocation = copy.RenderLocation;

        SubLayouts = new List<LuiLayout>();
        foreach (var l in copy.SubLayouts)
        {
            SubLayouts.Add(l.DeepClone());
        }
    }

    public virtual LuiLayout DeepClone()
    {
        return new LuiLayout(this);
    }
}
