
using LunaUI;

namespace LunaEdit
{
    public partial class Editor : Form
    {
        private bool _needToRebuildTree = false;

        void UpdateTreeView()
        {
            var tw = this.treeView1;
            if (uiObject == null)
                return;

            if (_needToRebuildTree)
            {
                tw.Nodes.Clear();

                TreeNode root = new TreeNode(uiObject.Root.Name);
                root.Tag = uiObject.Root;
                RecrusiveAddTreeNode(uiObject.Root, root);

                tw.Nodes.Add(root);

                _needToRebuildTree = false;
            }
            //tw.ExpandAll();
        }

        void RecrusiveAddTreeNode(ControlBase cb, TreeNode root)
        {
            foreach (var c in cb.Childs)
            {
                TreeNode t = new TreeNode(c.Name);
                t.Tag = c;
                RecrusiveAddTreeNode(c, t);
                root.Nodes.Add(t);
            }
        }

        TreeNode? RecrusiveFindTreeNode(ControlBase cb, TreeNode root)
        {
            if (root.Tag.Equals(cb))
                return root;

            foreach (TreeNode t in root.Nodes)
            {
                return RecrusiveFindTreeNode(cb, t);
            }
            return null;
        }

        void Add<T>() where T : ControlBase, new()
        {
            var t = selectedNode as ControlBase;
            var node = new T();

            node.Parent = t;
            t.Childs.Add(node);

            var tn = RecrusiveFindTreeNode(t, treeView1.Nodes[0]);
            if (tn != null)
            {
                TreeNode rt = new TreeNode(node.Name);
                rt.Tag = node;
                tn.Nodes.Add(rt);
            }
            else
            {
                _needToRebuildTree = true;
                UpdateTreeView();
            }
        }

        Point picbox_location;
        Point picbox_size;
        LunaUI.Option option = new LunaUI.Option();

        void UpdateUI()
        {
            UpdateTreeView();
            UpdatePictureBox();

            this.Text = $"[{curr_file_path}] - {option.CanvasSize.Width}x{option.CanvasSize.Height} - LuiEditor";
        }

        void UpdatePictureBox()
        {
            pictureBox1.Size = (Size)uiObject.Root.Size;

            try
            {
                Image im = uiObject.Render();
                pictureBox1.Image = im;
                picbox_location = new Point(hScrollBar1.Location.X, vScrollBar1.Location.Y);
                picbox_size = new Point(hScrollBar1.Size.Width, vScrollBar1.Size.Height);

                this.hScrollBar1.Minimum = 0;
                this.hScrollBar1.Maximum = im.Width - picbox_size.X;

                this.vScrollBar1.Minimum = 0;
                this.vScrollBar1.Maximum = im.Height - picbox_size.Y;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
