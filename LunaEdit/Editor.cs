
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

            contextMenuStrip1.Items.Add(new ToolStripButton("Add Empty", null, new EventHandler((s, e) => { Add<LunaUI.ControlBase>(); })));
            contextMenuStrip1.Items.Add(new ToolStripButton("Add Image", null, new EventHandler((s, e) => { Add<LunaUI.LuiImage>(); })));
            contextMenuStrip1.Items.Add(new ToolStripButton("Add Text", null, new EventHandler((s, e) => { Add<LunaUI.LuiText>(); })));
            contextMenuStrip1.Items.Add(new ToolStripButton("Add ColorLayer", null, new EventHandler((s, e) => { Add<LunaUI.LuiColorLayer>(); })));

            contextMenuStrip1.Items.Add(new ToolStripSeparator());

            contextMenuStrip1.Items.Add(new ToolStripButton("Delete", null, new EventHandler(OnDeleteNode)));

            option.WorkPath = @"E:\gitlab\aimubot\bot_shared_data\";
            option.CanvasSize = new Size(100, 100);
        }

        private void OnDeleteNode(object? sender, EventArgs e)
        {
            var t = selectedNode as ControlBase;
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

        private void 新建Lui界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiObject == null)
                uiObject = new();

            uiObject.New();
            uiObject.Option = option;

            uiObject.Root.Name = "Root";
            uiObject.Root.Size = new Size(100, 100);
            this.propertyGrid1.SelectedObject = uiObject.Root;

            UpdateUI();
        }

        object? selectedNode;

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selectedNode != null)
                {
                    (selectedNode as ControlBase).ShowLayoutRect = false;
                }

                selectedNode = e.Node.Tag;
                (selectedNode as ControlBase).ShowLayoutRect = true;
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

        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionFrm optionFrm = new OptionFrm();
            optionFrm.option = option;
            optionFrm.ShowDialog();
        }

        private void 打开ImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedNode is LunaUI.LuiImage img)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "所有文件|*.*";
                ofd.InitialDirectory = option.WorkPath;
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    img.ImgPath = ofd.FileName[uiObject.Option.WorkPath.Length..];
                }
            }
            UpdateUI();
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "UI文件|*.xml";
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

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (curr_file_path != "")
            {
                uiObject.SaveToFile(curr_file_path);
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "UI文件|*.xml";
                sfd.InitialDirectory = option.WorkPath;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    uiObject.SaveToFile(sfd.FileName);
                    curr_file_path = sfd.FileName;
                    UpdateUI();
                }
            }
        }

        private void 另存为ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "UI文件|*.xml";
            sfd.InitialDirectory = option.WorkPath;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                uiObject.SaveToFile(sfd.FileName);
                curr_file_path = sfd.FileName;
                UpdateUI();
            }
        }

        private void 保存图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "图片|*.jpg";
            sfd.InitialDirectory = option.WorkPath;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Image im = uiObject.Render();
                im.Save(sfd.FileName);
            }
        }
    }
}
