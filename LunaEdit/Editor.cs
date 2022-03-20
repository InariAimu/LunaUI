
using LunaUI;

using Newtonsoft.Json;

namespace LunaEdit
{
    public partial class Editor : Form
    {
        LunaUI.LunaUI? uiObject = null;
        UserConfig? userConfig = null;

        object? selectedNode;
        string curr_file_path = "";

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


            if (new FileInfo("config.json").Exists)
            {
                string js = File.ReadAllText("config.json");
                userConfig = JsonConvert.DeserializeObject<UserConfig>(js);
            }
            else
            {
                userConfig = new UserConfig()
                {
                    WorkPath = Environment.CurrentDirectory
                };
                string js = JsonConvert.SerializeObject(userConfig);
                File.WriteAllText("config.json", js);
            }
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
                t.Parent.SubLayouts.Remove(t);

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
            {
                uiObject = new(userConfig.WorkPath);
            }

            uiObject.New();

            uiObject.Root.Root.Name = "Root";
            uiObject.Root.Root.Size = new Size(100, 100);
            propertyGrid1.SelectedObject = uiObject.Root.Root;

            _needToRebuildTree = true;
            UpdateUI();
        }

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
                propertyGrid1.SelectedObject = selectedNode;

                UpdatePictureBox();
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (selectedNode != null)
                {
                    (selectedNode as LuiLayout).ShowLayoutRect = false;
                }

                selectedNode = e.Node.Tag;
                (selectedNode as LuiLayout).ShowLayoutRect = true;
                propertyGrid1.SelectedObject = selectedNode;

                treeView1.SelectedNode = e.Node;
                contextMenuStrip1.Show(Location.X + treeView1.Location.X + e.X + 8, Location.Y + treeView1.Location.Y + e.Y + 55);
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int x = hScrollBar1.Value;
            int y = vScrollBar1.Value;
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
            if (uiObject == null)
            {
                uiObject = new(userConfig.WorkPath);
            }

            uiObject.Root.Option.CanvasSize = uiObject.Root.Root.Size;
            UpdateUI();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiObject == null)
            {
                uiObject = new(userConfig.WorkPath);
            }

            OptionFrm optionFrm = new OptionFrm();
            optionFrm.option = uiObject.Root.Option;
            optionFrm.ShowDialog();
        }

        private void OpenImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedNode is LunaUI.LuiImage img)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "All Files|*.*";
                ofd.InitialDirectory = userConfig.WorkPath;
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    img.ImgPath = ofd.FileName[uiObject.Root.Option.WorkPath.Length..];
                }
            }
            UpdateUI();
        }

        private void OpenLuiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Lui File|*.json";
            ofd.InitialDirectory = userConfig.WorkPath;
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                uiObject = new LunaUI.LunaUI(userConfig.WorkPath);
                uiObject.LoadFromJson(ofd.FileName);
                uiObject.Root.Option.CanvasSize = uiObject.Root.Root.Size;
                uiObject.Root.Option.WorkPath = userConfig.WorkPath;
                curr_file_path = ofd.FileName;

                _needToRebuildTree = true;
                UpdateUI();
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiObject == null)
            {
                MessageBox.Show("No working file.");
                return;
            }

            if (curr_file_path != "")
            {
                uiObject.SaveToJson(curr_file_path);
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Lui File|*.json";
                sfd.InitialDirectory = userConfig.WorkPath;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    uiObject.SaveToJson(sfd.FileName);
                    curr_file_path = sfd.FileName;
                    UpdateUI();
                }
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiObject == null)
            {
                MessageBox.Show("No working file.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Lui File|*.json";
            sfd.InitialDirectory = userConfig.WorkPath;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                uiObject.SaveToJson(sfd.FileName);
                curr_file_path = sfd.FileName;
                UpdateUI();
            }
        }

        private void ExportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiObject == null)
            {
                MessageBox.Show("No working file.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image|*.jpg,*.png";
            sfd.InitialDirectory = userConfig.WorkPath;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Image im = uiObject.Render();
                im.Save(sfd.FileName);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;

            if (uiObject == null)
            {
                MessageBox.Show("No working file.");
                return;
            }

            var ctrl = uiObject.GetNodeByPoint(x, y);
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
                    propertyGrid1.SelectedObject = selectedNode;

                    UpdatePictureBox();
                }
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Preferences_Click(object sender, EventArgs e)
        {
            if (userConfig is null)
            {
                userConfig = new UserConfig()
                {
                    WorkPath = Environment.CurrentDirectory
                };
            }

            OptionFrm optionFrm = new OptionFrm();
            optionFrm.option = userConfig;
            optionFrm.ShowDialog();

            string js = JsonConvert.SerializeObject(userConfig);
            File.WriteAllText("config.json", js);
        }
    }
}
