using System.Drawing;

using Newtonsoft.Json;

namespace LunaUI
{
    public enum ListAlignMode
    {
        Normal,
        Even,
    }

    public enum ListDirection
    {
        Horizontal,
        Vertical
    }

    [Serializable]
    public class LuiListLayout : LuiLayout
    {
        [JsonProperty("list_direction")]
        public ListDirection ListDirection { get; set; } = ListDirection.Horizontal;

        [JsonProperty("clip")]
        public ListAlignMode AlignMode { get; set; } = ListAlignMode.Normal;

        [JsonProperty("item_padding")]
        public int ItemPadding { get; set; } = 0;

        [JsonProperty("item_distance")]
        public int ItemDistance { get; set; } = 1;

        public LuiListLayout()
        {
            Name = "ListLayout";
        }

        public override void Render(Graphics g, RenderOption op)
        {
            float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - Size.Width * Pivot.X;
            float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - Size.Height * Pivot.Y;

            RenderLayoutRect(g, op, x, y);

            float next_x = x;
            float next_y = y;
            for (int i = 0; i < SubLayouts.Count; i++)
            {
                var child = SubLayouts[i];
                RenderOption next = (RenderOption)op.Clone();
                next.SetRect((int)next_x, (int)next_y, Size.Width, Size.Height);
                child.Render(g, next);

                if (AlignMode == ListAlignMode.Normal)
                {
                    if (ListDirection == ListDirection.Horizontal)
                        next_x += child.Size.Width + ItemPadding;
                    else
                        next_y += child.Size.Height + ItemPadding;
                }
                else if (AlignMode == ListAlignMode.Even)
                {
                    if (ListDirection == ListDirection.Horizontal)
                        next_x += ItemDistance + ItemPadding;
                    else
                        next_y += ItemDistance + ItemPadding;
                }
            }
        }
        protected LuiListLayout(LuiListLayout copy) : base(copy)
        {
            ListDirection = copy.ListDirection;
            AlignMode = copy.AlignMode;
            ItemPadding = copy.ItemPadding;
            ItemDistance = copy.ItemDistance;
        }

        public override LuiLayout DeepClone()
        {
            return new LuiListLayout(this);
        }
    }
}
