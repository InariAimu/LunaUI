
using LunaUI;

namespace LunaEdit
{
    public partial class Editor : Form
    {
        private bool _needToRebuildTree = false;

        void UpdateTreeView()
        {
            var tw = treeView1;
            if (uiObject == null)
            {
                return;
            }

            if (_needToRebuildTree)
            {
                tw.Nodes.Clear();

                TreeNode root = new(uiObject.Root.Root.Name);
                root.Tag = uiObject.Root.Root;
                RecrusiveAddTreeNode(uiObject.Root.Root, root);

                tw.Nodes.Add(root);

                _needToRebuildTree = false;
            }
            //tw.ExpandAll();
        }

        void RecrusiveAddTreeNode(LuiLayout cb, TreeNode root)
        {
            foreach (var c in cb.SubLayouts)
            {
                TreeNode t = new(c.Name);
                t.Tag = c;
                RecrusiveAddTreeNode(c, t);
                root.Nodes.Add(t);
            }
        }

        TreeNode? RecrusiveFindTreeNode(LuiLayout cb, TreeNode treeNode)
        {
            if (treeNode.Tag.Equals(cb))
            {
                return treeNode;
            }

            foreach (TreeNode t in treeNode.Nodes)
            {
                var tn = RecrusiveFindTreeNode(cb, t);
                if (tn != null)
                {
                    return tn;
                }
            }
            return null;
        }

        void Add<T>() where T : LuiLayout, new()
        {
            var t = selectedNode as LuiLayout;
            var node = new T
            {
                Parent = t
            };
            t.SubLayouts.Add(node);

            var tn = RecrusiveFindTreeNode(t, treeView1.Nodes[0]);
            if (tn != null)
            {
                TreeNode rt = new(node.Name);
                rt.Tag = node;
                tn.Nodes.Add(rt);
                tn.Expand();
            }
            else
            {
                _needToRebuildTree = true;
                UpdateTreeView();
            }
        }

        Point picbox_location;
        Point picbox_size;

        void UpdateUI()
        {
            UpdateTreeView();
            UpdatePictureBox();

            Text = $"[{curr_file_path}] - {uiObject.Root.Option.CanvasSize.Width}x{uiObject.Root.Option.CanvasSize.Height} - LuiEditor";
        }

        void UpdatePictureBox()
        {
            pictureBox1.Size = uiObject.Root.Root.Size;

            try
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }

                Image im = uiObject.Render();
                pictureBox1.Image = im;
                picbox_location = new Point(hScrollBar1.Location.X, vScrollBar1.Location.Y);
                picbox_size = new Point(hScrollBar1.Size.Width, vScrollBar1.Size.Height);

                hScrollBar1.Minimum = 0;
                hScrollBar1.Maximum = im.Width - picbox_size.X;

                vScrollBar1.Minimum = 0;
                vScrollBar1.Maximum = im.Height - picbox_size.Y;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
