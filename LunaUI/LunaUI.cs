using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;

using System.Diagnostics.CodeAnalysis;
namespace LunaUI
{
    public class LunaUI
    {
        public ControlBase? Root = null;
        public Option? Option = null;

        public LunaUI()
        {

        }

        public LunaUI(string workPath, string xmlPath)
        {
            LoadFromFile(Path.Combine(workPath, xmlPath));
            Option = new Option();
            Option.WorkPath = workPath;
            Option.CanvasSize = Root.Size;
        }

        public void New()
        {
            Root = new ControlBase();
        }

        public void LoadFromFile(string path)
        {
            if (File.Exists(path))
            {
                using StreamReader reader = new(path);
                XmlSerializer xmlSerializer = new(typeof(ControlBase));
                Root = (ControlBase?)xmlSerializer.Deserialize(reader);
            }
        }

        public void SaveToFile(string path)
        {
            using StreamWriter writer = new(path);
            XmlSerializer xmlSerializer = new(typeof(ControlBase));
            xmlSerializer.Serialize(writer, Root);
        }

        public T? GetNodeByPath<T>(string path) where T : ControlBase
        {
            if (!path.StartsWith("Root"))
                path = "Root/" + path;
            string[] ps = path.Split('\\', '/');
            var c = FindControlBase(Root, ps, 0);
            if (c != null && c is T t)
            {
                return t;
            }
            return null;
        }

        public ControlBase? FindControlBase(ControlBase node, string[] paths, int level)
        {
            if (level >= paths.Length)
                return null;

            if (node.Name == paths[level])
            {
                foreach (var c in node.Childs)
                {
                    var n = FindControlBase(c, paths, level + 1);
                    if (n != null)
                        return n;
                }
                return node;
            }
            return null;
        }

        public Image Render()
        {
            Image im = new Bitmap(Root.Size.Width, Root.Size.Height);
            using (Graphics g = Graphics.FromImage(im))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                Root.Init(Option);
                Root.Render(g, Option);
            }
            return im;
        }
    }
}
