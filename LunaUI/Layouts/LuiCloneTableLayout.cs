
using System.Drawing;

using Newtonsoft.Json;

namespace LunaUI.Layouts;

public enum PlaceMode
{
    XFirst,
    YFirst,
}

[Serializable]
public class LuiCloneTableLayout : LuiLayout
{
    [JsonProperty("table_rows")]
    public int TableRows { get; set; }

    [JsonProperty("table_columns")]
    public int TableColumns { get; set; }

    [JsonProperty("item_padding_x")]
    public int ItemPaddingX { get; set; } = 0;

    [JsonProperty("item_padding_y")]
    public int ItemPaddingY { get; set; } = 0;

    [JsonIgnore]
    public bool AutoResizeByPlaceholder { get; set; } = false;

    [JsonIgnore]
    public int PlaceHolderCopies { get; set; } = 1;

    [JsonIgnore]
    public int PlaceHolderClones { get; set; } = 0;

    [JsonIgnore]
    private List<LuiLayout> placeHolderLayouts = new List<LuiLayout>();

    public LuiCloneTableLayout()
    {
        Name = "CloneTableLayout";
    }

    public LuiLayout? this[int index]
    {
        get
        {
            return SubLayouts[index];
        }
    }

    public override void Init(RenderOption op)
    {
        base.Init(op);

        if (PlaceHolderClones > 0)
        {
            if (SubLayouts.Count != PlaceHolderClones)
            {
                CloneLayouts(PlaceHolderClones, "clone_");
            }
            return;
        }

        if (placeHolderLayouts.Count != PlaceHolderCopies)
        {
            placeHolderLayouts.Clear();
            for (int i = 0; i < PlaceHolderCopies; i++)
            {
                placeHolderLayouts.Add(SubLayouts[0]);
            }
        }
    }

    public override void Render(Graphics g, RenderOption op)
    {
        float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - GetWidth() * Pivot.X;
        float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - GetHeight() * Pivot.Y;

        RenderLayoutRect(g, op, x, y);

        float next_x = x;
        float next_y = y;
        int xt = 0;

        var blockList = SubLayouts.Count > 1 ? SubLayouts : placeHolderLayouts;
        for (int i = 0; i < blockList.Count; i++)
        {
            var child = blockList[i];
            RenderOption next = (RenderOption)op.Clone();
            next.SetRect((int)next_x, (int)next_y, GetWidth() / TableColumns, GetHeight() / TableRows);
            child.Render(g, next);

            next_x += child.RenderSize.Width + ItemPaddingX;
            xt++;
            if (xt >= TableColumns)
            {
                xt = 0;
                next_x = x;
                next_y += child.RenderSize.Height + ItemPaddingY;
            }
        }

    }

    public T? GetSubLayout<T>(List<LuiLayout> list, int x, int y) where T : LuiLayout
    {
        var index = x + y * TableColumns;
        if (index >= list.Count)
            return null;

        return (T)list[index];
    }

    public override void CalcSize(Graphics g, RenderOption op)
    {
        RenderSize = new Size(Size.Width, Size.Height);

        if (SubLayouts.Count == 0)
            return;

        RenderOption next = (RenderOption)op.Clone();

        for (int i = 0; i < SubLayouts.Count; i++)
        {
            SubLayouts[i].CalcSize(g, next);
        }

        var blockList = SubLayouts.Count > 1 ? SubLayouts : placeHolderLayouts;

        var w = Size.Width;
        var h = Size.Height;
        var count = Math.Max(SubLayouts.Count, placeHolderLayouts.Count);
        var xcount = count > TableColumns ? TableColumns : count;
        var ycount = (count - 1) / TableColumns + 1;
        var xsize = 0;
        var ysize = 0;

        if (FlowX == FlowMode.None)
        {
            xsize = xcount * SubLayouts[0].RenderSize.Width;
            xsize += (xcount - 1) * ItemPaddingX;
        }
        else if (FlowX != FlowMode.None)
        {
            for (int i = 0; i < xcount; i++)
            {
                var l = GetSubLayout<LuiLayout>(blockList, i, 0);
                xsize += l.RenderSize.Width;
                xsize += l.PaddingRight;
                xsize += ItemPaddingX;
            }
        }

        if (FlowY == FlowMode.None)
        {
            ysize = ycount * SubLayouts[0].RenderSize.Height;
            ysize += (ycount - 1) * ItemPaddingY;
        }
        else if (FlowY != FlowMode.None)
        {
            for (int i = 0; i < ycount; i++)
            {
                var l = GetSubLayout<LuiLayout>(blockList, 0, i);
                ysize += l.RenderSize.Height;
                ysize += l.PaddingBottom;
                ysize += ItemPaddingY;
            }
        }

        w = FlowX switch
        {
            FlowMode.Auto => xsize,
            FlowMode.Shrink => Math.Min(xsize, Size.Width),
            FlowMode.Expand => Math.Max(xsize, Size.Width),
            _ => Size.Width,
        };
        h = FlowY switch
        {
            FlowMode.Auto => ysize,
            FlowMode.Shrink => Math.Min(ysize, Size.Height),
            FlowMode.Expand => Math.Max(ysize, Size.Height),
            _ => Size.Height,
        };

        RenderSize = new Size(w + PaddingRight, h + PaddingBottom);

        float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - GetWidth() * Pivot.X;
        float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - GetHeight() * Pivot.Y;
        RenderLocation = new Point((int)x, (int)y);
    }

    public void CloneLayouts(int copies, string name_prefix = "")
    {
        SubLayouts[0].Name = name_prefix + 0.ToString();
        for (int i = 1; i < copies; i++)
        {
            var l = SubLayouts[0].DeepClone();
            l.Name = name_prefix + i.ToString();
            SubLayouts.Add(l);
        }
    }

    protected LuiCloneTableLayout(LuiCloneTableLayout copy) : base(copy)
    {
        TableRows = copy.TableRows;
        TableColumns = copy.TableColumns;
        ItemPaddingX = copy.ItemPaddingX;
        ItemPaddingY = copy.ItemPaddingY;
    }

    public override LuiLayout DeepClone()
    {
        return new LuiCloneTableLayout(this);
    }
}
