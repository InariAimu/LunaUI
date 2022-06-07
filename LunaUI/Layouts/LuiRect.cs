using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

using Newtonsoft.Json;

namespace LunaUI.Layouts;

public enum RectMode
{
    Normal,
    Round,
    Chamfer,
}

[Serializable]
public class LuiRect : LuiLayout
{
    [JsonProperty("color")]
    [Browsable(false)]
    public int _color { get; set; } = Color.Black.ToArgb();

    [JsonProperty("mode")]
    public RectMode Mode { get; set; } = RectMode.Normal;

    [JsonProperty("left_top")]
    [Category("Corners")]
    public int LeftTop { get; set; } = 1;

    [JsonProperty("right_top")]
    [Category("Corners")]
    public int RightTop { get; set; } = 1;

    [JsonProperty("left_bottom")]
    [Category("Corners")]
    public int LeftBottom { get; set; } = 1;

    [JsonProperty("right_bottom")]
    [Category("Corners")]
    public int RightBottom { get; set; } = 1;

    [JsonIgnore]
    public Color Color
    {
        get => Color.FromArgb(_color);
        set => _color = value.ToArgb();
    }

    public LuiRect() => Name = "Rectangle";

    public override void Render(Graphics g, RenderOption op)
    {
        if (!Visible)
            return;

        float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - GetWidth() * Pivot.X;
        float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - GetHeight() * Pivot.Y;

        var baseRect = Rectangle.FromLTRB((int)x, (int)y, (int)x + GetWidth(), (int)y + GetHeight());

        switch (Mode)
        {
            case RectMode.Normal:
                {
                    g.FillRectangle(new SolidBrush(Color), baseRect);
                }
                break;
            case RectMode.Round:
                {
                    var gp = GetRoundedRect(baseRect);
                    g.FillPath(new SolidBrush(Color), gp);
                }
                break;
            case RectMode.Chamfer:
                {
                    var gp = GetChamferedRect(baseRect);
                    g.FillPath(new SolidBrush(Color), gp);
                }
                break;
        }

        RenderLayoutRect(g, op, x, y);

        RenderOption next = (RenderOption)op.Clone();
        next.SetRect((int)x, (int)y, GetWidth(), GetHeight());
        RenderChilds(g, next);
    }

    private GraphicsPath GetRoundedRect(Rectangle rect)
    {
        float lt = LeftTop * 2;
        float rt = RightTop * 2;
        float lb = LeftBottom * 2;
        float rb = RightBottom * 2;

        GraphicsPath gp = new();
        gp.AddLine(rect.X + rt, rect.Y, rect.Right - rt, rect.Y);
        gp.AddArc(rect.Right - rt, rect.Y, rt, rt, 270, 90);

        gp.AddLine(rect.Right, rect.Y + rb, rect.Right, rect.Y - rb);
        gp.AddArc(rect.Right - rb, rect.Bottom - rb, rb, rb, 0, 90);

        gp.AddLine(rect.X + lb, rect.Bottom, rect.Right - lb, rect.Bottom);
        gp.AddArc(rect.X, rect.Bottom - lb, lb, lb, 90, 90);

        gp.AddLine(rect.X, rect.Y + lt, rect.X, rect.Bottom - lt);
        gp.AddArc(rect.X, rect.Y, lt, lt, 180, 90);
        return gp;
    }

    private GraphicsPath GetChamferedRect(Rectangle rect)
    {
        float lt = LeftTop;
        float rt = RightTop;
        float lb = LeftBottom;
        float rb = RightBottom;

        GraphicsPath gp = new();
        gp.AddLine(rect.X + lt, rect.Y, rect.Right - rt, rect.Y);
        gp.AddLine(rect.Right - rt, rect.Y, rect.Right, rect.Y + rt);

        gp.AddLine(rect.Right, rect.Y + rt, rect.Right, rect.Bottom - rb);
        gp.AddLine(rect.Right, rect.Bottom - rb, rect.Right - rb, rect.Bottom);

        gp.AddLine(rect.Right - rb, rect.Bottom, rect.X + lb, rect.Bottom);
        gp.AddLine(rect.X + lb, rect.Bottom, rect.X, rect.Bottom - lb);

        gp.AddLine(rect.X, rect.Bottom - lb, rect.X, rect.Y + lt);
        gp.AddLine(rect.X, rect.Y + lt, rect.X + lt, rect.Y);
        return gp;
    }

    protected LuiRect(LuiRect copy) : base(copy)
    {
        Color = copy.Color;
        Mode = copy.Mode;
        LeftTop = copy.LeftTop;
        LeftBottom = copy.LeftBottom;
        RightTop = copy.RightTop;
        RightBottom = copy.RightBottom;
    }

    public override LuiRect DeepClone() => new LuiRect(this);
}
