using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

using Newtonsoft.Json;

using RoyT.TrueType;
using RoyT.TrueType.Helpers;

namespace LunaUI.Layouts;

public enum TextRenderMode
{
    /// <summary>
    /// String is rendered as a whole block.
    /// </summary>
    Default,

    /// <summary>
    /// String is rendered charwisely. Adjust <see cref="LuiText.CharSpacingRate"/> to control distance between characters.
    /// </summary>
    CharWise,
}

public enum TextKerningMode
{
    /// <summary>
    /// Default GDI+ or Skia font kerning mode.
    /// </summary>
    Default,

    /// <summary>
    /// Determine kerning by RoyT.TrueType.
    /// </summary>
    Font,

    /// <summary>
    /// Using kerning_config.json for kerning.
    /// </summary>
    Custom,
}

/// <summary>
/// A layout for text rendering.
/// </summary>
[Serializable]
public class LuiText : LuiLayout
{
    /// <summary>
    /// Used as a placeholder for text in LuiEditor for sizing purpose. 
    /// Using this in your app to set text is not recommended.
    /// For app use, use <see cref="Text"/> instead.
    /// </summary>
    [JsonProperty("placeholder")]
    public string PlaceHolder { get; set; } = "";

    [JsonProperty("text_render_mode")]
    [Category("Font Kerning")]
    public TextRenderMode TextRenderMode { get; set; } = TextRenderMode.Default;

    /// <summary>
    /// Text kerning mode. Normal for default.
    /// For some font that do not have GPOS table, use <see cref="TextKerningMode.Font"/>.
    /// For custom kerning control, use <see cref="TextKerningMode.Custom"/>.
    /// </summary>
    [JsonProperty("text_kerning_mode")]
    [Category("Font Kerning")]
    public TextKerningMode TextKerningMode { get; set; } = TextKerningMode.Default;

    /// <summary>
    /// Controls distance between characters.
    /// Takes effect when <see cref="TextRenderMode"/> is setted to <see cref="TextRenderMode.CharWise"/>.
    /// </summary>
    [JsonProperty("kerning")]
    [Category("Font Kerning")]
    public float CharSpacingRate { get; set; } = 1f;

    /// <summary>
    /// This is a factor for kerning scale.
    /// Bigger value for more char spacing.
    /// </summary>
    [JsonProperty("kerning_base")]
    [Category("Font Kerning")]
    public float KerningBase { get; set; } = 1300f;

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

    [JsonIgnore]
    private TrueTypeFont? _ttf = null;

    [JsonProperty("font_path")]
    [Category("Font Kerning")]
    public string FontPath { get; set; } = "";

    [JsonProperty("font_pixel_size")]
    [Category("Font")]
    public float FontSize { get; set; } = 10f;

    [JsonProperty("font_style")]
    [Category("Font")]
    public FontStyle FontStyle { get; set; } = FontStyle.Regular;

    private int _color = Color.Black.ToArgb();

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

    private int _shadeColor = 0;

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

    private int _borderColor = 0;

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
    /// <see cref="LuiLayout.Size"/> is set by <see cref="PlaceHolder"/> string rectangle when enabled.
    /// </summary>
    [JsonProperty("auto_size_by_placeholder")]
    public bool SetSizeByPlaceholder { get; set; } = false;

    /// <summary>
    /// When render result image, please set this instead of <see cref="PlaceHolder"/>
    /// </summary>
    [JsonIgnore]
    [Browsable(false)]
    public string? Text { get; set; } = null;

    private TextKerningConfig? _textKerningCfg = null;

    private readonly Dictionary<char, SizeF> _glyphSize = new();

    public LuiText()
    {
        Name = "Text";
    }

    public override void Render(Graphics g, RenderOption op)
    {
        if (!Visible)
            return;

        float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - Size.Width * Pivot.X;
        float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - Size.Height * Pivot.Y;

        Font f = new(Font, FontSize, FontStyle, GraphicsUnit.Pixel);

        if (TextKerningMode is TextKerningMode.Font && _ttf is null)
            _ttf = TrueTypeFont.FromFile(Path.Combine(op.WorkPath, FontPath));

        if (SetSizeByPlaceholder)
            AutoSizeByPlaceholder(g, f);

        SolidBrush b = new(Color);
        StringFormat sf = new();
        sf.Alignment = Align;
        sf.LineAlignment = LineAlign;

        string strDraw = Text ?? PlaceHolder;

        if (TextRenderMode == TextRenderMode.CharWise)
            _textKerningCfg = op.TextKerningConfig;

        if (HasShade)
        {
            b.Color = ShadeColor;

            GraphicsPath gpath = new();

            float size = g.DpiY * f.SizeInPoints / 72;

            if (TextRenderMode == TextRenderMode.Default)
            {
                gpath.AddString(strDraw, f.FontFamily, (int)f.Style, size,
                    RectangleF.FromLTRB(
                        x + ShadeDisplacement,
                        y + ShadeDisplacement,
                        x + Size.Width + ShadeDisplacement,
                        y + Size.Height + ShadeDisplacement),
                    sf);
            }
            else if (TextRenderMode == TextRenderMode.CharWise)
            {
                var strSize = MeasureString(g, strDraw, f, CharSpacingRate);
                float sx = x;
                float sy = y;

                if (Align == StringAlignment.Center)
                    sx = x + Size.Width / 2 - strSize.Width / 2;
                else if (Align == StringAlignment.Far)
                {
                    sx = x + Size.Width - strSize.Width;
                }

                if (LineAlign == StringAlignment.Center)
                    sy = y + Size.Height / 2 - strSize.Height / 2;
                else if (LineAlign == StringAlignment.Far)
                {
                    sy = y + Size.Height - strSize.Height;
                }

                CharWiseAddToGraphicsPath(g, gpath, strDraw, f, sx + ShadeDisplacement, sy + ShadeDisplacement, CharSpacingRate);
            }

            if (ShadeSize > 1)
                g.DrawPath(new Pen(b, ShadeSize), gpath);

            g.FillPath(b, gpath);
        }

        if (HasBorder)
        {
            b.Color = BorderColor;

            GraphicsPath p = new();

            float size = g.DpiY * f.SizeInPoints / 72;
            float dp = -0.5f;

            if (TextRenderMode == TextRenderMode.Default)
            {
                p.AddString(strDraw, f.FontFamily, (int)f.Style, size,
                RectangleF.FromLTRB(x + dp, y + dp, x + Size.Width + dp, y + Size.Height + dp), sf);
            }
            else if (TextRenderMode == TextRenderMode.CharWise)
            {
                var str_size = MeasureString(g, strDraw, f, CharSpacingRate);
                float sx = x;
                float sy = y;

                if (Align == StringAlignment.Center)
                    sx = x + Size.Width / 2 - str_size.Width / 2;
                else if (Align == StringAlignment.Far)
                {
                    sx = x + Size.Width - str_size.Width;
                }

                if (LineAlign == StringAlignment.Center)
                    sy = y + Size.Height / 2 - str_size.Height / 2;
                else if (LineAlign == StringAlignment.Far)
                {
                    sy = y + Size.Height - str_size.Height;
                }

                CharWiseAddToGraphicsPath(g, p, strDraw, f, sx, sy, CharSpacingRate);
            }

            g.DrawPath(new Pen(b, BorderDisplacement), p);
            g.FillPath(b, p);
        }

        b.Color = Color;
        if (TextRenderMode == TextRenderMode.Default)
        {
            g.DrawString(strDraw, f, b,
                RectangleF.FromLTRB(x, y, x + Size.Width, y + Size.Height), sf);
        }
        else if (TextRenderMode == TextRenderMode.CharWise)
        {
            var size = MeasureString(g, strDraw, f, CharSpacingRate);
            float sx = x;
            float sy = y;

            if (Align == StringAlignment.Center)
                sx = x + Size.Width / 2 - size.Width / 2;
            else if (Align == StringAlignment.Far)
            {
                sx = x + Size.Width - size.Width;
            }

            if (LineAlign == StringAlignment.Center)
                sy = y + Size.Height / 2 - size.Height / 2;
            else if (LineAlign == StringAlignment.Far)
            {
                sy = y + Size.Height - size.Height;
            }

            CharWiseDrawString(g, strDraw, f, b, sx, sy, CharSpacingRate);
        }

        RenderLayoutRect(g, op, x, y);

        RenderOption next = (RenderOption)op.Clone();
        next.SetRect((int)x, (int)y, Size.Width, Size.Height);
        RenderChilds(g, next);
    }

    public void AutoSizeByPlaceholder(Graphics g, Font f)
    {
        if (TextRenderMode == TextRenderMode.Default)
        {
            var sf = g.MeasureString(PlaceHolder, f);
            Size = sf.ToSize();
        }
        else if (TextRenderMode == TextRenderMode.CharWise)
        {
            Size = MeasureString(g, PlaceHolder, f, CharSpacingRate);
        }
    }

    public float GetTrueTypeKerning(char c1, char c2)
    {
        if (TextKerningMode == TextKerningMode.Font)
        {
            if (_ttf is null || KerningBase == 0)
                return 0;
            else
            {
                float dx = KerningHelper.GetHorizontalKerning(c1, c2, _ttf);
                return dx / KerningBase;
            }
        }
        return 0;
    }

    public float GetCustomKerning(char c1, char c2)
    {
        if (_textKerningCfg is null)
            return 1;

        if (_textKerningCfg.TextKerning.TryGetValue(Font, out var font_conf))
        {
            if (font_conf.TryGetValue(c1, out var char_spacing))
            {
                if (char_spacing.TryGetValue(c2, out float f))
                    return f;
            }
        }
        return 1;
    }

    public SizeF MeasureChar(Graphics g, char ch, Font f)
    {
        if (_glyphSize.TryGetValue(ch, out var glyph_size))
            return glyph_size;
        StringBuilder sb = new(1024);
        for (int i = 0; i < 1024; i++)
        {
            sb.Append(ch);
        }
        var sz = g.MeasureString(sb.ToString(), f);
        sz = new(sz.Width / 1024f, sz.Height);
        _glyphSize[ch] = sz;
        return sz;
    }

    public void CharWiseAddToGraphicsPath(Graphics g, GraphicsPath p, string s, Font f, float x, float y, float kerningScale)
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
            var cs = MeasureChar(g, ch[i], f);
            sw += cs.Width * GetCustomKerning(ch[i], ch[i + 1]) * kerningScale + GetTrueTypeKerning(ch[i], ch[i + 1]) * f.Height;
            sh = Math.Max(sh, cs.Height);
        }
        {
            p.AddString(ch[i].ToString(), f.FontFamily, (int)f.Style, size, new PointF(x + sw + dp, y + dp), StringFormat.GenericDefault);
        }
    }

    public void CharWiseDrawString(Graphics g, string s, Font f, Brush b, float x, float y, float kerningScale)
    {
        char[] ch = s.ToCharArray();
        float sw = 0;
        float sh = 0;
        int i = 0;
        for (; i < ch.Length - 1; i++)
        {
            g.DrawString(ch[i].ToString(), f, b, x + sw, y);
            var cs = MeasureChar(g, ch[i], f);
            sw += cs.Width * GetCustomKerning(ch[i], ch[i + 1]) * kerningScale + GetTrueTypeKerning(ch[i], ch[i + 1]) * f.Height;
            sh = Math.Max(sh, cs.Height);
        }
        {
            g.DrawString(ch[i].ToString(), f, b, x + sw, y);
        }
    }

    public Size MeasureString(Graphics g, string s, Font f, float kerningScale)
    {
        char[] ch = s.ToCharArray();
        float sw = 0;
        float sh = 0;
        int i = 0;
        for (; i < ch.Length - 1; i++)
        {
            var cs = MeasureChar(g, ch[i], f);
            sw += cs.Width * GetCustomKerning(ch[i], ch[i + 1]) * kerningScale + GetTrueTypeKerning(ch[i], ch[i + 1]) * f.Height;
            sh = Math.Max(sh, cs.Height);
        }
        {
            var cs = g.MeasureString(ch[i].ToString(), f);
            sw += cs.Width;
            sh = Math.Max(sh, cs.Height);
        }

        return new Size((int)sw, (int)sh);
    }

    protected LuiText(LuiText copy) : base(copy)
    {
        PlaceHolder = copy.PlaceHolder;
        TextRenderMode = copy.TextRenderMode;
        TextKerningMode = copy.TextKerningMode;
        CharSpacingRate = copy.CharSpacingRate;
        Font = copy.Font;
        FontSize = copy.FontSize;
        FontStyle = copy.FontStyle;
        Color = copy.Color;
        Align = copy.Align;
        LineAlign = copy.LineAlign;
        HasShade = copy.HasShade;
        ShadeColor = copy.ShadeColor;
        ShadeDisplacement = copy.ShadeDisplacement;
        HasBorder = copy.HasBorder;
        BorderColor = copy.BorderColor;
        BorderDisplacement = copy.BorderDisplacement;
        SetSizeByPlaceholder = copy.SetSizeByPlaceholder;
        FontPath = copy.FontPath;
        KerningBase = copy.KerningBase;

        _textKerningCfg = copy._textKerningCfg;
    }

    public override LuiLayout DeepClone()
    {
        return new LuiText(this);
    }
}
