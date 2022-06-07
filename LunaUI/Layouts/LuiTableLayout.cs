
using System.Drawing;

using Newtonsoft.Json;

namespace LunaUI.Layouts;

public enum TableAlignMode
{
    Normal,
    Even,
}

[Serializable]
public class LuiTableLayout : LuiLayout
{
    [JsonProperty("table_rows")]
    public int TableRows { get; set; }

    [JsonProperty("table_columns")]
    public int TableColumns { get; set; }

    [JsonProperty("item_padding_x")]
    public int ItemPaddingX { get; set; } = 0;

    [JsonProperty("item_padding_y")]
    public int ItemPaddingY { get; set; } = 0;

    [JsonProperty("table_align_x")]
    public TableAlignMode TableAlignX { get; set; } = TableAlignMode.Normal;

    [JsonProperty("table_align_y")]
    public TableAlignMode TableAlignY { get; set; } = TableAlignMode.Normal;

    public LuiTableLayout()
    {
        Name = "TableLayout";
    }

    public override void Render(Graphics g, RenderOption op)
    {
        float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - GetWidth() * Pivot.X;
        float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - GetHeight() * Pivot.Y;

        RenderLayoutRect(g, op, x, y);

        for (int i = 0; i < SubLayouts.Count; i++)
        {
            int xt = i % TableColumns;
            int yt = i / TableColumns;
            int wid = (GetWidth() - ItemPaddingX * (TableColumns - 1)) / TableColumns;
            int hgt = (GetHeight() - ItemPaddingY * (TableRows - 1)) / TableRows;
            var child = SubLayouts[i];
            RenderOption next = (RenderOption)op.Clone();
            next.SetRect((int)x + xt * wid, (int)y + yt * hgt, wid, hgt);
            child.Render(g, next);
        }
    }

    protected LuiTableLayout(LuiTableLayout copy) : base(copy)
    {
        TableRows = copy.TableRows;
        TableColumns = copy.TableColumns;
        ItemPaddingX = copy.ItemPaddingX;
        ItemPaddingY = copy.ItemPaddingY;
        TableAlignX = copy.TableAlignX;
        TableAlignY = copy.TableAlignY;

        SubLayouts.Clear();
        foreach (var lo in copy.SubLayouts)
        {
            SubLayouts.Add(lo.DeepClone());
        }
    }

    public override LuiLayout DeepClone()
    {
        return new LuiTableLayout(this);
    }
}
