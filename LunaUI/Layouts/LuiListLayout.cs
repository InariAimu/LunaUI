using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaUI
{
    public enum ListAlignMode
    {
        Normal,
        Even,
    }

    public class LuiListLayout : LuiLayout
    {
        public ListAlignMode AlignMode { get; set; } = ListAlignMode.Normal;
        public int ItemPadding { get; set; } = 0;
        public int ItemDistance { get; set; } = 1;

        public LuiListLayout()
        {
            Name = "ListLayout";
        }

        public override void Render(Graphics g, RenderOption op)
        {
            float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + ((Docking.X < 1 ? 1 : -1) * Position.X) - Size.Width * Pivot.X;
            float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + ((Docking.Y < 1 ? 1 : -1) * Position.Y) - Size.Height * Pivot.Y;

            RenderLayoutRect(g, op, x, y);

            float next_x = x;
            float next_y = y;
            for (int i = 0; i < Childs.Count; i++)
            {
                var child = Childs[i];
                RenderOption next = (RenderOption)op.Clone();
                next.SetRect((int)next_x, (int)next_y, Size.Width, Size.Height);
                child.Render(g, next);

                if (AlignMode == ListAlignMode.Normal)
                {
                    next_y += child.Size.Height + ItemPadding;
                }
                else if (AlignMode == ListAlignMode.Even)
                {
                    next_y += ItemDistance + ItemPadding;
                }
            }
        }
    }
}
