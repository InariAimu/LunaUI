
using LunaUI;

namespace LunaEdit
{
    public partial class Editor : Form
    {
        LunaUI.LunaUI? uiObject = null;

        public Editor()
        {
            InitializeComponent();
        }

        private void Editor_Load(object sender, EventArgs e)
        {
            contextMenuStrip1.Items.Clear();

            contextMenuStrip1.Items.Add(new ToolStripMenuItem("Add Empty", null, new EventHandler((s, e) => { Add<LunaUI.LuiLayout>(); })));
            contextMenuStrip1.Items.Add(new ToolStripMenuItem("Add Image", null, new EventHandler((s, e) => { Add<LunaUI.LuiImage>(); })));
            contextMenuStrip1.Items.Add(new ToolStripMenuItem("Add Text", null, new EventHandler((s, e) => { Add<LunaUI.LuiText>(); })));
            contextMenuStrip1.Items.Add(new ToolStripMenuItem("Add ColorLayer", null, new EventHandler((s, e) => { Add<LunaUI.LuiColorLayer>(); })));
            contextMenuStrip1.Items.Add(new ToolStripMenuItem("Add ListLayout", null, new EventHandler((s, e) => { Add<LunaUI.LuiListLayout>(); })));

            contextMenuStrip1.Items.Add(new ToolStripSeparator());

            contextMenuStrip1.Items.Add(new ToolStripMenuItem("Delete", null, new EventHandler(OnDeleteNode)));

            //option.WorkPath = @"E:\gitlab\aimubot\bot_shared_data\";
            option.CanvasSize = new Size(100, 100);
        }

        private void Editor_Shown(object sender, EventArgs e)
        {
        }

        private void OnDeleteNode(object? sender, EventArgs e)
        {
            var t = selectedNode as LuiLayout;
            if (t.Parent != null)
            {
                selectedNode = t.Parent;
                t.Parent.Childs.Remove(t);

                _needToRebuildTree = true;
                UpdateUI();
            }
        }

        private void testToolStripMenuItem1_Click(object sender, EventArgs e)
        {
        }

        private void NewLuiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiObject == null)
                uiObject = new();

            uiObject.New();
            uiObject.Option = option;

            uiObject.Root.Name = "Root";
            uiObject.Root.Size = new Size(100, 100);
            this.propertyGrid1.SelectedObject = uiObject.Root;

            _needToRebuildTree = true;
            UpdateUI();
        }

        object? selectedNode;

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selectedNode != null)
                {
                    (selectedNode as LuiLayout).ShowLayoutRect = false;
                }

                selectedNode = e.Node.Tag;
                (selectedNode as LuiLayout).ShowLayoutRect = true;
                this.propertyGrid1.SelectedObject = selectedNode;

                UpdatePictureBox();
            }
            else if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(this.Location.X + treeView1.Location.X + e.X, this.Location.Y + treeView1.Location.Y + e.Y + 25);
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int x = hScrollBar1.Value;
            int y = vScrollBar1.Value;
            Image i = pictureBox1.Image;
            pictureBox1.Location = new Point(picbox_location.X - x, picbox_location.Y - y);
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int x = hScrollBar1.Value;
            int y = vScrollBar1.Value;
            pictureBox1.Location = new Point(picbox_location.X - x, picbox_location.Y - y);
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            option.CanvasSize = uiObject.Root.Size;
            UpdateUI();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionFrm optionFrm = new OptionFrm();
            optionFrm.option = option;
            optionFrm.ShowDialog();
        }

        private void OpenImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedNode is LunaUI.LuiImage img)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "All Files|*.*";
                ofd.InitialDirectory = option.WorkPath;
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    img.ImgPath = ofd.FileName[uiObject.Option.WorkPath.Length..];
                }
            }
            UpdateUI();
        }

        private void OpenLuiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Lui File|*.xml";
            ofd.InitialDirectory = option.WorkPath;
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                uiObject = new();
                uiObject.Option = option;
                uiObject.LoadFromFile(ofd.FileName);
                option.CanvasSize = uiObject.Root.Size;
                curr_file_path = ofd.FileName;

                _needToRebuildTree = true;
                UpdateUI();
            }
        }

        string curr_file_path = "";

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (curr_file_path != "")
            {
                uiObject.SaveToFile(curr_file_path);
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Lui File|*.xml";
                sfd.InitialDirectory = option.WorkPath;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    uiObject.SaveToFile(sfd.FileName);
                    curr_file_path = sfd.FileName;
                    UpdateUI();
                }
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Lui File|*.xml";
            sfd.InitialDirectory = option.WorkPath;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                uiObject.SaveToFile(sfd.FileName);
                curr_file_path = sfd.FileName;
                UpdateUI();
            }
        }

        private void ExportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image|*.jpg";
            sfd.InitialDirectory = option.WorkPath;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Image im = uiObject.Render();
                im.Save(sfd.FileName);
            }
        }

        private void Editor_Move(object sender, EventArgs e)
        {
        }

        private void Editor_Activated(object sender, EventArgs e)
        {
        }

        private void Editor_Deactivate(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;

            var ctrl = uiObject.GetByPoint(x, y);
            if (ctrl != null)
            {
                var tn = RecrusiveFindTreeNode(ctrl, treeView1.Nodes[0]);
                if (tn != null)
                {
                    treeView1.SelectedNode = tn;
                    if (selectedNode != null)
                    {
                        (selectedNode as LuiLayout).ShowLayoutRect = false;
                    }

                    selectedNode = tn.Tag;
                    (selectedNode as LuiLayout).ShowLayoutRect = true;
                    this.propertyGrid1.SelectedObject = selectedNode;

                    UpdatePictureBox();
                }
            }

        }
    }
}
