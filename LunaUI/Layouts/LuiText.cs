using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;

namespace LunaUI
{
    public enum TextRenderMode
    {
        Default, CharWise
    }

    public class LuiText : LuiLayout
    {
        static readonly Dictionary<char, Dictionary<char, float>> CharSpacing = new();

        static LuiText()
        {
            // todo: sept this into font config file
            CharSpacing['0'] = new();
            CharSpacing['0']['1'] = 0.7f;
            CharSpacing['1'] = new();
            CharSpacing['1']['0'] = 0.75f;
            CharSpacing['1']['1'] = 0.4f;
            CharSpacing['1']['2'] = 0.7f;
            CharSpacing['1']['3'] = 0.6f;
            CharSpacing['1']['4'] = 0.7f;
            CharSpacing['1']['5'] = 0.7f;
            CharSpacing['1']['6'] = 0.7f;
            CharSpacing['1']['7'] = 0.7f;
            CharSpacing['1']['8'] = 0.7f;
            CharSpacing['1']['9'] = 0.7f;
            CharSpacing['1']['\''] = 0.7f;
            CharSpacing['2'] = new();
            CharSpacing['2']['4'] = 0.7f;
            CharSpacing['4'] = new();
            CharSpacing['4']['1'] = 0.8f;
            CharSpacing['4']['9'] = 0.9f;
            CharSpacing['7'] = new();
            CharSpacing['7']['5'] = 0.9f;
            CharSpacing['7']['6'] = 0.8f;
            CharSpacing['9'] = new();
            CharSpacing['9']['4'] = 0.9f;
            CharSpacing['9']['5'] = 0.9f;
            CharSpacing['9']['6'] = 0.9f;
            CharSpacing['9']['7'] = 0.9f;
            CharSpacing['\''] = new();
            CharSpacing['\'']['0'] = 0.7f;
            CharSpacing['\'']['1'] = 0.6f;
            CharSpacing['\'']['2'] = 0.7f;
            CharSpacing['\'']['3'] = 0.7f;
            CharSpacing['\'']['4'] = 0.7f;
            CharSpacing['\'']['5'] = 0.7f;
            CharSpacing['\'']['6'] = 0.7f;
            CharSpacing['\'']['7'] = 0.7f;
            CharSpacing['\'']['8'] = 0.7f;
            CharSpacing['\'']['9'] = 0.7f;
        }

        public string PlaceHolder { get; set; } = "";

        public TextRenderMode TextRenderMode { get; set; } = TextRenderMode.Default;

        public float Kerning { get; set; } = 1f;

        [XmlIgnore, Category("Font")]
        private Font? _font;

        [XmlIgnore, Category("Font")]
        public Font FontSet
        {
            get => _font;
            set
            {
                _font = value;
                Font = _font.FontFamily.Name;
                FontSize = _font.Size;
                FontStyle = _font.Style;
            }
        }

        [Category("Font")]
        public string Font { get; set; } = "Exo";

        [Category("Font")]
        public float FontSize { get; set; } = 10f;

        [Category("Font")]
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;

        [XmlElement, Browsable(false)]
        public int _color { get; set; } = Color.Black.ToArgb();

        [XmlIgnore]
        public Color Color
        {
            get => Color.FromArgb(_color);
            set => _color = value.ToArgb();
        }
        public StringAlignment Align { get; set; } = StringAlignment.Near;
        public StringAlignment LineAlign { get; set; } = StringAlignment.Near;
        public bool HasShade { get; set; } = false;

        [XmlElement, Browsable(false)]
        public int _shadeColor { get; set; } = 0;

        [XmlIgnore]
        public Color ShadeColor
        {
            get => Color.FromArgb(_shadeColor);
            set => _shadeColor = value.ToArgb();
        }
        public float ShadeDisplacement { get; set; } = 1.0f;
        public float ShadeSize { get; set; } = 1.0f;

        public bool HasBorder { get; set; } = false;

        [XmlElement, Browsable(false)]
        public int _borderColor { get; set; } = 0;

        [XmlIgnore]
        public Color BorderColor
        {
            get => Color.FromArgb(_borderColor);
            set => _borderColor = value.ToArgb();
        }
        public float BorderDisplacement { get; set; } = 1.0f;

        public bool SetSizeByPlaceholder { get; set; } = false;

        [XmlIgnore, Browsable(false)]
        public string? Text { get; set; } = null;

        public LuiText()
        {
            Name = "Text";
        }

        public override void Render(Graphics g, RenderOption op)
        {
            if (!Visible)
                return;

            float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + ((Docking.X < 1 ? 1 : -1) * Position.X) - Size.Width * Pivot.X;
            float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + ((Docking.Y < 1 ? 1 : -1) * Position.Y) - Size.Height * Pivot.Y;

            Font f = new(Font, FontSize, FontStyle, GraphicsUnit.Pixel);
            if (SetSizeByPlaceholder)
                AutoSetSizeByPlaceholder(g, f);

            SolidBrush b = new(Color);
            StringFormat sf = new();
            sf.Alignment = Align;
            sf.LineAlignment = LineAlign;

            string temp_str = string.IsNullOrEmpty(Text) ? PlaceHolder : Text;

            if (HasShade)
            {
                b.Color = ShadeColor;

                GraphicsPath p_s = new GraphicsPath();

                float size = g.DpiY * f.SizeInPoints / 72;

                if (TextRenderMode == TextRenderMode.Default)
                {
                    p_s.AddString(temp_str, f.FontFamily, (int)f.Style, size,
                    RectangleF.FromLTRB(x + ShadeDisplacement, y + ShadeDisplacement, x + Size.Width + ShadeDisplacement, y + Size.Height + ShadeDisplacement), sf);
                }
                else if (TextRenderMode == TextRenderMode.CharWise)
                {
                    var str_size = MeasureString(g, temp_str, f, Kerning);
                    float sx = x;
                    float sy = y;

                    if (Align == StringAlignment.Center)
                        sx = x + Size.Width / 2 - str_size.Width / 2;
                    else if (Align == StringAlignment.Far)
                        sx = x + Size.Width - str_size.Width;

                    if (LineAlign == StringAlignment.Center)
                        sy = y + Size.Height / 2 - str_size.Height / 2;
                    else if (LineAlign == StringAlignment.Far)
                        sy = y + Size.Height - str_size.Height;

                    CharWiseAddToPath(g, p_s, temp_str, f, b, sx + ShadeDisplacement, sy + ShadeDisplacement, Kerning);
                }

                if (ShadeSize > 1)
                    g.DrawPath(new Pen(b, ShadeSize), p_s);

                g.FillPath(b, p_s);
            }

            if (HasBorder)
            {
                b.Color = BorderColor;

                GraphicsPath p = new GraphicsPath();

                float size = g.DpiY * f.SizeInPoints / 72;
                float dp = -0.5f;

                if (TextRenderMode == TextRenderMode.Default)
                {
                    p.AddString(temp_str, f.FontFamily, (int)f.Style, size,
                    RectangleF.FromLTRB(x + dp, y + dp, x + Size.Width + dp, y + Size.Height + dp), sf);
                }
                else if (TextRenderMode == TextRenderMode.CharWise)
                {
                    var str_size = MeasureString(g, temp_str, f, Kerning);
                    float sx = x;
                    float sy = y;

                    if (Align == StringAlignment.Center)
                        sx = x + Size.Width / 2 - str_size.Width / 2;
                    else if (Align == StringAlignment.Far)
                        sx = x + Size.Width - str_size.Width;

                    if (LineAlign == StringAlignment.Center)
                        sy = y + Size.Height / 2 - str_size.Height / 2;
                    else if (LineAlign == StringAlignment.Far)
                        sy = y + Size.Height - str_size.Height;

                    CharWiseAddToPath(g, p, temp_str, f, b, sx, sy, Kerning);
                }

                g.DrawPath(new Pen(b, BorderDisplacement), p);
                g.FillPath(b, p);
            }

            b.Color = Color;
            if (TextRenderMode == TextRenderMode.Default)
            {
                g.DrawString(temp_str, f, b,
                    RectangleF.FromLTRB(x, y, x + Size.Width, y + Size.Height), sf);
            }
            else if (TextRenderMode == TextRenderMode.CharWise)
            {
                var size = MeasureString(g, temp_str, f, Kerning);
                float sx = x;
                float sy = y;

                if (Align == StringAlignment.Center)
                    sx = x + Size.Width / 2 - size.Width / 2;
                else if (Align == StringAlignment.Far)
                    sx = x + Size.Width - size.Width;

                if (LineAlign == StringAlignment.Center)
                    sy = y + Size.Height / 2 - size.Height / 2;
                else if (LineAlign == StringAlignment.Far)
                    sy = y + Size.Height - size.Height;

                CharWiseDrawString(g, temp_str, f, b, sx, sy, Kerning);
            }

            RenderLayoutRect(g, op, x, y);

            RenderOption next = (RenderOption)op.Clone();
            next.SetRect((int)x, (int)y, Size.Width, Size.Height);
            RenderChilds(g, next);
        }

        public void AutoSetSizeByPlaceholder(Graphics g, Font f)
        {
            if (TextRenderMode == TextRenderMode.Default)
            {
                var sf = g.MeasureString(PlaceHolder, f);
                Size = sf.ToSize();
            }
            else if (TextRenderMode == TextRenderMode.CharWise)
            {
                Size = MeasureString(g, PlaceHolder, f, Kerning);
            }
        }

        public static float GetSpacing(char c1, char c2)
        {
            if (CharSpacing.TryGetValue(c1, out Dictionary<char, float>? r))
            {
                if (r.TryGetValue(c2, out float f))
                    return f;
            }
            return 1;
        }

        public static void CharWiseAddToPath(Graphics g, GraphicsPath p, string s, Font f, Brush b, float x, float y, float kerning)
        {
            char[] ch = s.ToCharArray();
            float sw = 0;
            float sh = 0;
            float size = g.DpiY * f.SizeInPoints / 72;
            float dp = -0.5f;
            int i = 0;
            for (; i < ch.Length - 1; i++)
            {
                p.AddString(ch[i].ToString(), f.FontFamily, (int)f.Style, size, new PointF(x + sw + dp, y + dp), StringFormat.GenericDefault);
                var cs = g.MeasureString(ch[i].ToString(), f);
                sw += cs.Width * GetSpacing(ch[i], ch[i + 1]) * kerning;
                sh = Math.Max(sh, cs.Height);
            }
            {
                p.AddString(ch[i].ToString(), f.FontFamily, (int)f.Style, size, new PointF(x + sw + dp, y + dp), StringFormat.GenericDefault);
            }
        }

        public static void CharWiseDrawString(Graphics g, string s, Font f, Brush b, float x, float y, float kerning)
        {
            char[] ch = s.ToCharArray();
            float sw = 0;
            float sh = 0;
            int i = 0;
            for (; i < ch.Length - 1; i++)
            {
                g.DrawString(ch[i].ToString(), f, b, x + sw, y);
                var cs = g.MeasureString(ch[i].ToString(), f);
                sw += cs.Width * GetSpacing(ch[i], ch[i + 1]) * kerning;
                sh = Math.Max(sh, cs.Height);
            }
            {
                g.DrawString(ch[i].ToString(), f, b, x + sw, y);
            }
        }

        public static Size MeasureString(Graphics g, string s, Font f, float kerning)
        {
            char[] ch = s.ToCharArray();
            float sw = 0;
            float sh = 0;
            int i = 0;
            for (; i < ch.Length - 1; i++)
            {
                var cs = g.MeasureString(ch[i].ToString(), f);
                sw += cs.Width * GetSpacing(ch[i], ch[i + 1]) * kerning;
                sh = Math.Max(sh, cs.Height);
            }
            {
                var cs = g.MeasureString(ch[i].ToString(), f);
                sw += cs.Width;
                sh = Math.Max(sh, cs.Height);
            }

            return new Size((int)sw, (int)sh);
        }

    }
}
