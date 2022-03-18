using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

using Newtonsoft.Json;

namespace LunaUI
{
    public enum TextRenderMode
    {
        /// <summary>
        /// String is rendered as a whole block.
        /// </summary>
        Default,

        /// <summary>
        /// String is rendered charwisely. Adjust <see cref="LuiText.Kerning"/> to control distance between characters.
        /// </summary>
        CharWise
    }

    /// <summary>
    /// A layout for text rendering.
    /// </summary>
    [Serializable]
    public class LuiText : LuiLayout
    {
        /// <summary>
        /// Use as a placeholder for text in LuiEditor. Use this in your app to set text is not recommended.
        /// For app use, use <see cref="LuiText.Text"/> instead.
        /// </summary>
        [JsonProperty("placeholder")]
        public string PlaceHolder { get; set; } = "";

        [JsonProperty("text_render_mode")]
        public TextRenderMode TextRenderMode { get; set; } = TextRenderMode.Default;

        /// <summary>
        /// Controls distance between characters.
        /// Takes effect when <see cref="LuiText.TextRenderMode"/> is setted to <see cref="TextRenderMode.CharWise"/>.
        /// </summary>
        [JsonProperty("kerning")] public float Kerning { get; set; } = 1f;

        [JsonIgnore]
        [Category("Font")]
        private Font? _font;

        /// <summary>
        /// Used by ProperyGrid to set font values graphically in LuiEditor.
        /// DO NOT set this manually.
        /// </summary>
        [JsonIgnore]
        [Category("Font")]
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

        /// <summary>
        /// Font name. When not sure what name to use, please set it in LuiEdior by FontSet Property.
        /// </summary>
        [JsonProperty("font_name")]
        [Category("Font")]
        public string Font { get; set; } = "Exo";

        [JsonProperty("font_pixel_size")]
        [Category("Font")]
        public float FontSize { get; set; } = 10f;

        [JsonProperty("font_style")]
        [Category("Font")]
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;

        [JsonIgnore]
        [Browsable(false)]
        public int _color { get; set; } = Color.Black.ToArgb();

        /// <summary>
        /// Text color.
        /// </summary>
        [JsonProperty("text_color")]
        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public Color Color
        {
            get => Color.FromArgb(_color);
            set => _color = value.ToArgb();
        }

        [JsonProperty("text_align_horizontal")]
        public StringAlignment Align { get; set; } = StringAlignment.Near;

        [JsonProperty("text_align_vertical")]
        public StringAlignment LineAlign { get; set; } = StringAlignment.Near;

        [JsonProperty("shade_enabled")]
        public bool HasShade { get; set; } = false;

        [JsonIgnore]
        [Browsable(false)]
        public int _shadeColor { get; set; } = 0;

        [JsonProperty("shade_color")]
        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public Color ShadeColor
        {
            get => Color.FromArgb(_shadeColor);
            set => _shadeColor = value.ToArgb();
        }

        [JsonProperty("shade_offset")]
        public float ShadeDisplacement { get; set; } = 1.0f;

        [JsonProperty("shade_size")]
        public float ShadeSize { get; set; } = 1.0f;

        [JsonProperty("border_enabled")]
        public bool HasBorder { get; set; } = false;

        [JsonIgnore]
        [Browsable(false)]
        public int _borderColor { get; set; } = 0;

        [JsonProperty("border_color")]
        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public Color BorderColor
        {
            get => Color.FromArgb(_borderColor);
            set => _borderColor = value.ToArgb();
        }

        [JsonProperty("border_offset")]
        public float BorderDisplacement { get; set; } = 1.0f;

        /// <summary>
        /// <see cref="LuiLayout.Size"/> is set by <see cref="LuiText.PlaceHolder"/> string rectangle when enabled.
        /// </summary>
        [JsonProperty("auto_size_by_placeholder")]
        public bool SetSizeByPlaceholder { get; set; } = false;

        /// <summary>
        /// When render result image, please set this instead of <see cref="LuiText.PlaceHolder"/>
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public string? Text { get; set; } = null;

        [JsonIgnore]
        private TextKerningConfig? textKerningCfg = null;

        public LuiText()
        {
            Name = "Text";
        }

        public override void Render(Graphics g, RenderOption op)
        {
            if (!Visible)
            {
                return;
            }

            float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - Size.Width * Pivot.X;
            float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - Size.Height * Pivot.Y;

            Font f = new(Font, FontSize, FontStyle, GraphicsUnit.Pixel);
            if (SetSizeByPlaceholder)
            {
                AutoSetSizeByPlaceholder(g, f);
            }

            SolidBrush b = new(Color);
            StringFormat sf = new();
            sf.Alignment = Align;
            sf.LineAlignment = LineAlign;

            string temp_str = Text ?? PlaceHolder;

            if (TextRenderMode == TextRenderMode.CharWise)
            {
                textKerningCfg = op.TextKerningConfig;
            }

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
                    {
                        sx = x + Size.Width / 2 - str_size.Width / 2;
                    }
                    else if (Align == StringAlignment.Far)
                    {
                        sx = x + Size.Width - str_size.Width;
                    }

                    if (LineAlign == StringAlignment.Center)
                    {
                        sy = y + Size.Height / 2 - str_size.Height / 2;
                    }
                    else if (LineAlign == StringAlignment.Far)
                    {
                        sy = y + Size.Height - str_size.Height;
                    }

                    CharWiseAddToPath(g, p_s, temp_str, f, b, sx + ShadeDisplacement, sy + ShadeDisplacement, Kerning);
                }

                if (ShadeSize > 1)
                {
                    g.DrawPath(new Pen(b, ShadeSize), p_s);
                }

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
                    {
                        sx = x + Size.Width / 2 - str_size.Width / 2;
                    }
                    else if (Align == StringAlignment.Far)
                    {
                        sx = x + Size.Width - str_size.Width;
                    }

                    if (LineAlign == StringAlignment.Center)
                    {
                        sy = y + Size.Height / 2 - str_size.Height / 2;
                    }
                    else if (LineAlign == StringAlignment.Far)
                    {
                        sy = y + Size.Height - str_size.Height;
                    }

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
                {
                    sx = x + Size.Width / 2 - size.Width / 2;
                }
                else if (Align == StringAlignment.Far)
                {
                    sx = x + Size.Width - size.Width;
                }

                if (LineAlign == StringAlignment.Center)
                {
                    sy = y + Size.Height / 2 - size.Height / 2;
                }
                else if (LineAlign == StringAlignment.Far)
                {
                    sy = y + Size.Height - size.Height;
                }

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

        public float GetSpacing(char c1, char c2)
        {
            if (textKerningCfg is null)
            {
                return 1;
            }

            if (textKerningCfg.TextKerning.TryGetValue(Font, out var font_conf))
            {
                if (font_conf.TryGetValue(c1, out var char_spacing))
                {
                    if (char_spacing.TryGetValue(c2, out float f))
                    {
                        return f;
                    }
                }
            }
            return 1;
        }

        public void CharWiseAddToPath(Graphics g, GraphicsPath p, string s, Font f, Brush b, float x, float y, float kerning)
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

        public void CharWiseDrawString(Graphics g, string s, Font f, Brush b, float x, float y, float kerning)
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

        public Size MeasureString(Graphics g, string s, Font f, float kerning)
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
