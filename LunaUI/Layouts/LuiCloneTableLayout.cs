
using System.Drawing;

using Newtonsoft.Json;

namespace LunaUI.Layouts;

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
        float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - Size.Width * Pivot.X;
        float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - Size.Height * Pivot.Y;

        RenderLayoutRect(g, op, x, y);

        float next_x = x;
        float next_y = y;
        int xt = 0;

        var block_list = SubLayouts.Count > 1 ? SubLayouts : placeHolderLayouts;
        for (int i = 0; i < block_list.Count; i++)
        {
            var child = block_list[i];
            RenderOption next = (RenderOption)op.Clone();
            next.SetRect((int)next_x, (int)next_y, Size.Width / TableColumns, Size.Height / TableRows);
            child.Render(g, next);

            next_x += child.Size.Width + ItemPaddingX;
            xt++;
            if (xt >= TableColumns)
            {
                xt = 0;
                next_x = x;
                next_y += child.Size.Height + ItemPaddingY;
            }
        }

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
    }

    public override LuiLayout DeepClone()
    {
        return new LuiCloneTableLayout(this);
    }
}
