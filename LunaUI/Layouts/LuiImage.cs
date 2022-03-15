using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Serialization;

namespace LunaUI
{
    public enum ImageRenderMode
    {
        Default, Unscaled, Fill, Clamp
    }

    [Serializable]
    public class LuiImage : LuiLayout
    {
        [Browsable(false)]
        public string ImagePath { get; set; } = "";

        [XmlIgnore]
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

        [XmlIgnore, Browsable(false)]
        private Image? _image = null;

        public Rectangle Clip { get; set; }
        public int Alpha { get; set; } = 255;
        public float Light { get; set; } = 0;
        public float Contrast { get; set; } = 1f;
        public ImageRenderMode ImageRenderMode { get; set; } = ImageRenderMode.Default;
        public bool MirrorX { get; set; } = false;
        public bool MirrorY { get; set; } = false;

        [XmlIgnore, ReadOnly(true)]
        public string BasePath { get; set; } = "";

        [XmlIgnore, ReadOnly(true)]
        public Size ImageSize { get; set; } = new Size(0, 0);

        [XmlIgnore]
        public float Scale
        {
            get
            {
                if (this.ImageSize.Width == 0)
                    return 100;
                return (float)this.Size.Width * 100 / this.ImageSize.Width;
            }
            set
            {
                this.Size = new Size((int)(this.ImageSize.Width * value / 100), (int)(this.ImageSize.Height * value / 100));
            }
        }

        public LuiImage()
        {
            Name = "Image";
        }

        public override void Init(RenderOption op)
        {
            BasePath = op.WorkPath;
        }

        void LoadImage()
        {
            var impath = Path.Combine(BasePath, ImagePath);
            FileInfo file = new FileInfo(impath);

            if (!file.Exists)
                return;

            _image = Image.FromFile(impath);

            if (MirrorX && MirrorY)
                _image.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            else if (MirrorX)
                _image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            else if (MirrorY)
                _image.RotateFlip(RotateFlipType.RotateNoneFlipY);
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
                this.ImageSize = _image.Size;

                if (ImageRenderMode == ImageRenderMode.Unscaled)
                {
                    Size = new Size(_image.Width, _image.Height);
                }
            }

            if (!Visible)
                return;

            float x = op.CanvasLocation.X + op.CanvasSize.Width * Docking.X + ((Docking.X < 1 ? 1 : -1) * Position.X) - Size.Width * Pivot.X;
            float y = op.CanvasLocation.Y + op.CanvasSize.Height * Docking.Y + ((Docking.Y < 1 ? 1 : -1) * Position.Y) - Size.Height * Pivot.Y;

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
                        g.DrawImage(_image, Rectangle.FromLTRB((int)x, (int)y, (int)(x + Size.Width), (int)(y + Size.Height)), 0, 0, _image.Width, _image.Height, GraphicsUnit.Pixel, attributes);
                    else
                        g.DrawImage(_image, RectangleF.FromLTRB(x, y, x + Size.Width, y + Size.Height));
                }
                else if (ImageRenderMode == ImageRenderMode.Unscaled)
                {
                    if (Alpha < 255)
                        g.DrawImage(_image, Rectangle.FromLTRB((int)x, (int)y, (int)(x + _image.Width), (int)(y + _image.Height)), 0, 0, _image.Width, _image.Height, GraphicsUnit.Pixel, attributes);
                    else
                        g.DrawImage(_image, RectangleF.FromLTRB(x, y, x + _image.Width, y + _image.Height));
                }
                else if (ImageRenderMode == ImageRenderMode.Fill || ImageRenderMode == ImageRenderMode.Clamp)
                {
                    float rate = (float)_image.Width / _image.Height;
                    float rate_d = (float)Size.Width / Size.Height;
                    if ((ImageRenderMode == ImageRenderMode.Fill && rate < rate_d) || (ImageRenderMode == ImageRenderMode.Clamp && rate > rate_d))
                    {
                        float dw = _image.Width;
                        float dh = _image.Width / rate_d;
                        if (Alpha < 255)
                            g.DrawImage(_image, Rectangle.FromLTRB((int)x, (int)y, (int)(x + Size.Width), (int)(y + Size.Height)),
                                0, (_image.Height - dh) / 2, dw, (_image.Height - dh) / 2 + dh, GraphicsUnit.Pixel, attributes);
                        else
                            g.DrawImage(_image, RectangleF.FromLTRB(x, y, x + Size.Width, y + Size.Height),
                                RectangleF.FromLTRB(0, (_image.Height - dh) / 2, dw, (_image.Height - dh) / 2 + dh), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        float dh = _image.Height;
                        float dw = _image.Height * rate_d;
                        if (Alpha < 255)
                            g.DrawImage(_image, Rectangle.FromLTRB((int)x, (int)y, (int)(x + Size.Width), (int)(y + Size.Height)),
                                (_image.Width - dw) / 2, 0, (_image.Width - dw) / 2 + dw, dh, GraphicsUnit.Pixel, attributes);
                        else
                            g.DrawImage(_image, RectangleF.FromLTRB(x, y, x + Size.Width, y + Size.Height),
                                RectangleF.FromLTRB((_image.Width - dw) / 2, 0, (_image.Width - dw) / 2 + dw, dh), GraphicsUnit.Pixel);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            RenderLayoutRect(g, op, x, y);

            RenderOption next = (RenderOption)op.Clone();
            next.SetRect((int)x, (int)y, Size.Width, Size.Height);
            RenderChilds(g, next);
        }
    }
}
