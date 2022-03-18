using System.Drawing;
using System.Drawing.Drawing2D;

using Newtonsoft.Json;

namespace LunaUI
{

    public class LunaUI
    {
        public UIRoot? Root { get; set; } = null;

        private string workPath = "";

        public LunaUI(string workPath)
        {
            this.workPath = workPath;
            Root = new UIRoot()
            {
                Option = new RenderOption
                {
                    WorkPath = workPath,
                    CanvasSize = new Size(100, 100),
                }
            };
        }

        public LunaUI(string workPath, string xmlPath)
        {
            this.workPath = workPath;
            LoadFromJson(Path.Combine(workPath, xmlPath));

            Root.Option.WorkPath = workPath;
            Root.Option.CanvasSize = Root.Root.Size;
        }

        public void New()
        {
            if (Root is null)
                Root = new UIRoot();

            Root.Root = new();
        }

        public void SaveToJson(string path)
        {
            string js = JsonConvert.SerializeObject(Root, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            File.WriteAllText(path, js);
        }

        public void LoadFromJson(string path)
        {
            string js = File.ReadAllText(path);
            Root = JsonConvert.DeserializeObject<UIRoot>(js, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (Root is not null)
            {
                if (Root.Option is null)
                    Root.Option = new RenderOption();

                if (!string.IsNullOrEmpty(Root.Option.TextKerningConfFile))
                {
                    js = File.ReadAllText(Path.Combine(workPath, Root.Option.TextKerningConfFile));
                    Root.Option.TextKerningConfig = JsonConvert.DeserializeObject<TextKerningConfig>(js);
                }
            }
        }

        public T? GetNodeByPath<T>(string path) where T : LuiLayout
        {
            if (Root is null)
                return null;

            if (Root.Root is null)
                return null;

            if (!path.StartsWith("Root"))
                path = "Root/" + path;

            string[] ps = path.Split('\\', '/');

            var c = FindControlBase(Root.Root, ps, 0);
            if (c != null && c is T t)
                return t;

            return null;
        }

        public LuiLayout? FindControlBase(LuiLayout node, string[] paths, int level)
        {
            if (level >= paths.Length)
                return null;

            if (node.Name == paths[level])
            {
                foreach (var c in node.SubLayouts)
                {
                    var n = FindControlBase(c, paths, level + 1);
                    if (n != null)
                        return n;
                }
                return node;
            }
            return null;
        }

        public LuiLayout? GetNodeByPoint(int x, int y)
        {
            return Root.Root.GetByPoint(x, y, Root.Option);
        }

        public Image Render()
        {
            Image im = new Bitmap(Root.Root.Size.Width, Root.Root.Size.Height);
            using (Graphics g = Graphics.FromImage(im))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                Root.Root.Init(Root.Option);
                Root.Root.Render(g, Root.Option);
            }
            return im;
        }
    }
}
