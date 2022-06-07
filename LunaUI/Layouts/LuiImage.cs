using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

using Newtonsoft.Json;

namespace LunaUI.Layouts;

public enum ImageRenderMode
{
    /// <summary>
    /// Strech image to fill <see cref="LuiLayout.Size"/> rectangle.
    /// </summary>
    Default,

    /// <summary>
    /// Original image size.
    /// </summary>
    Unscaled,

    /// <summary>
    /// Fill image unstreched (or uniform resize) to <see cref="LuiLayout.Size"/> rectangle.
    /// </summary>
    Fill,

    /// <summary>
    /// Unstreched fit image into shortmost side of <see cref="LuiLayout.Size"/> rectangle.
    /// </summary>
    Clamp
}

[Serializable]
public class LuiImage : LuiLayout
{
    /// <summary>
    /// Relative path for image file.
    /// </summary>
    [Browsable(false)]
    [JsonProperty("img")]
    [JsonConverter(typeof(JsonConverters.WindowsPathConverter))]
    public string ImagePath { get; set; } = "";

    [JsonIgnore]
    public string ImgPath
    {
        get => ImagePath;
        set
        {
            ImagePath = value;

            LoadImage();

            Clip = new Rectangle(0, 0, _image.Width, _image.Height);

            if (Size.Width == 0 && Size.Height == 0)
                Size = _image.Size;
        }
    }

    [JsonIgnore]
    [Browsable(false)]
    private Image? _image = null;

    [JsonProperty("clip")]
    public Rectangle Clip { get; set; }

    [Category("Adjust")]
    [JsonProperty("alpha")]
    public int Alpha { get; set; } = 255;

    [Category("Adjust")]
    [JsonProperty("lightness")]
    public float Light { get; set; } = 0;

    [Category("Adjust")]
    [JsonProperty("contrast")]
    public float Contrast { get; set; } = 1f;

    [JsonProperty("image_render_mode")]
    public ImageRenderMode ImageRenderMode { get; set; } = ImageRenderMode.Default;

    [Category("Adjust")]
    [JsonProperty("mirror_x")]
    public bool MirrorX { get; set; } = false;

    [Category("Adjust")]
    [JsonProperty("mirror_y")]
    public bool MirrorY { get; set; } = false;

    [JsonIgnore]
    [ReadOnly(true)]
    public string BasePath { get; set; } = "";

    [JsonIgnore]
    [ReadOnly(true)]
    public Size ImageSize { get; set; } = new Size(0, 0);

    [JsonIgnore]
    public float Scale
    {
        get
        {
            if (ImageSize.Width == 0)
                return 100;

            return (float)Size.Width * 100 / ImageSize.Width;
        }
        set
        {
            Size = new Size((int)(ImageSize.Width * value / 100), (int)(ImageSize.Height * value / 100));
        }
    }

    public LuiImage()
    {
        Name = "Image";
    }

    public override void Init(RenderOption op)
    {
        base.Init(op);
        BasePath = op.WorkPath;
    }

    void LoadImage()
    {
        string? impath = Path.Combine(BasePath, ImagePath);
        FileInfo file = new(impath);

        if (!file.Exists)
            return;

        _image = Image.FromFile(impath);

        if (MirrorX && MirrorY)
            _image.RotateFlip(RotateFlipType.RotateNoneFlipXY);
        else if (MirrorX)
        {
            _image.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }
        else if (MirrorY)
        {
            _image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }
    }

    public override void Render(Graphics g, RenderOption op)
    {
        if (_image == null)
        {
            BasePath = op.WorkPath;
            LoadImage();
        }

        if (_image != null)
        {
            ImageSize = _image.Size;

            if (ImageRenderMode == ImageRenderMode.Unscaled)
                Size = new Size(_image.Width, _image.Height);
        }

        if (!Visible)
            return;

        var w = GetWidth();
        var h = GetHeight();

        float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + Position.X - w * Pivot.X;
        float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + Position.Y - h * Pivot.Y;

        try
        {
            float[][] nArray = {new float[] {Contrast, 0, 0, 0, 0},
                            new float[] {0, Contrast, 0, 0, 0},
                            new float[] {0, 0, Contrast, 0, 0},
                            new float[] {0, 0, 0, (float)Alpha/255, 0},
                            new float[] { Light, Light, Light, 0, 1}};

            ColorMatrix matrix = new ColorMatrix(nArray);
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            if (ImageRenderMode == ImageRenderMode.Default)
            {
                if (Alpha < 255)
                    g.DrawImage(_image, Rectangle.FromLTRB((int)x, (int)y, (int)(x + w), (int)(y + h)), 0, 0, _image.Width, _image.Height, GraphicsUnit.Pixel, attributes);
                else
                {
                    g.DrawImage(_image, RectangleF.FromLTRB(x, y, x + w, y + h));
                }
            }
            else if (ImageRenderMode == ImageRenderMode.Unscaled)
            {
                if (Alpha < 255)
                    g.DrawImage(_image, Rectangle.FromLTRB((int)x, (int)y, (int)(x + _image.Width), (int)(y + _image.Height)), 0, 0, _image.Width, _image.Height, GraphicsUnit.Pixel, attributes);
                else
                {
                    g.DrawImage(_image, RectangleF.FromLTRB(x, y, x + _image.Width, y + _image.Height));
                }
            }
            else if (ImageRenderMode == ImageRenderMode.Fill || ImageRenderMode == ImageRenderMode.Clamp)
            {
                float rate = (float)_image.Width / _image.Height;
                float rate_d = (float)w / h;
                if (ImageRenderMode == ImageRenderMode.Fill && rate < rate_d || ImageRenderMode == ImageRenderMode.Clamp && rate > rate_d)
                {
                    float dw = _image.Width;
                    float dh = _image.Width / rate_d;
                    if (Alpha < 255)
                    {
                        g.DrawImage(_image, Rectangle.FromLTRB((int)x, (int)y, (int)(x + w), (int)(y + h)),
                            0, (_image.Height - dh) / 2, dw, (_image.Height - dh) / 2 + dh, GraphicsUnit.Pixel, attributes);
                    }
                    else
                    {
                        g.DrawImage(_image, RectangleF.FromLTRB(x, y, x + w, y + h),
                            RectangleF.FromLTRB(0, (_image.Height - dh) / 2, dw, (_image.Height - dh) / 2 + dh), GraphicsUnit.Pixel);
                    }
                }
                else
                {
                    float dh = _image.Height;
                    float dw = _image.Height * rate_d;
                    if (Alpha < 255)
                    {
                        g.DrawImage(_image, Rectangle.FromLTRB((int)x, (int)y, (int)(x + w), (int)(y + h)),
                            (_image.Width - dw) / 2, 0, (_image.Width - dw) / 2 + dw, dh, GraphicsUnit.Pixel, attributes);
                    }
                    else
                    {
                        g.DrawImage(_image, RectangleF.FromLTRB(x, y, x + w, y + h),
                            RectangleF.FromLTRB((_image.Width - dw) / 2, 0, (_image.Width - dw) / 2 + dw, dh), GraphicsUnit.Pixel);
                    }
                }
            }
        }
        catch (Exception)
        {

        }

        RenderLayoutRect(g, op, x, y);

        RenderOption next = (RenderOption)op.Clone();
        next.SetRect((int)x, (int)y, Size.Width, Size.Height);
        RenderChilds(g, next);
    }

    public override void CalcSize(Graphics g, RenderOption op)
    {
        if (_image == null)
        {
            BasePath = op.WorkPath;
            LoadImage();
        }

        if (_image != null)
        {
            ImageSize = _image.Size;

            if (ImageRenderMode == ImageRenderMode.Unscaled)
                Size = new Size(_image.Width, _image.Height);
        }

        base.CalcSize(g, op);
    }

    protected LuiImage(LuiImage copy) : base(copy)
    {
        ImagePath = copy.ImagePath;
        Clip = copy.Clip;
        Alpha = copy.Alpha;
        Light = copy.Light;
        Contrast = copy.Contrast;
        ImageRenderMode = copy.ImageRenderMode;
        MirrorX = copy.MirrorX;
        MirrorY = copy.MirrorY;
        BasePath = copy.BasePath;
        ImageSize = copy.Size;
    }

    public override LuiLayout DeepClone()
    {
        return new LuiImage(this);
    }
}
 