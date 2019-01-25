#define PARALLEL

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;

using static System.Math;

namespace JA.Geometry
{

    /// <summary>
    /// Defines a FrameBuffer that hold color information for each pixel.
    /// </summary>
    public class FrameBuffer
    {
        #region	Factory
        public FrameBuffer(Size size) : this(size.Width, size.Height) { }
        public FrameBuffer(int width, int height)
        {
            this.Height = height;
            this.Width = width;
            this.Pixels = new Vector3[width*height];
        }
        public FrameBuffer(FrameBuffer other)
        {
            this.Height = other.Height;
            this.Width = other.Width;
            this.Pixels = other.Pixels.Clone() as Vector3[];
        }
        public FrameBuffer(Scene scene, Camera camera) : this(camera.Target)
        {
#if !DEBUG
            var opts = new ParallelOptions()
            {
                // MaxDegreeOfParallelism = 4,
            };
            Parallel.For(0, Height, opts, (i) =>
            {
                for (int j = 0; j < Width; j++)
                {
                    var pos = camera.PixelToScreen(i, j);
                    this[i, j] = scene.CastRay(pos, 0);
                }
            });
#else        
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    var pos = camera.PixelToScreen(i, j);
                    this[i, j] = scene.CastRay(pos, 0);
                }
            }
#endif
        }
        #endregion

        #region Properties
        public int Width { get; }
        public int Height { get; }
        public Vector3 this[int row, int column]
        {
            get => Pixels[column + row*Width];
            set => Pixels[column + row*Width] = value;
        }
        public Vector3[] Pixels { get; private set; }
        #endregion

        #region Methods

        public Bitmap Render(PixelFormat format)
        {
            var img = new Bitmap(Width, Height, format);
            int wt = img.Width, ht = img.Height;
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, img.PixelFormat);
            byte bitsPerPixel;
            bool has_alpha;
            switch (data.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    bitsPerPixel = 24;
                    has_alpha = false;
                    break;
                case PixelFormat.Format32bppRgb:
                    bitsPerPixel = 32;
                    has_alpha = false;
                    break;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    bitsPerPixel = 32;
                    has_alpha = true;
                    break;
                default:
                    throw new NotSupportedException("Pixel format is not supported.");
            }
            unsafe
            {
                byte* scan0 = (byte*)data.Scan0.ToPointer();
                for (int i = 0; i < ht; i++)
                {
                    for (int j = 0; j < wt; j++)
                    {
                        Color color = this[i, j].RGB();
                        byte* pixel = scan0 + i*data.Stride + j*bitsPerPixel/8;
                        pixel[0] = color.B;
                        pixel[1] = color.G;
                        pixel[2] = color.R;
                        if (has_alpha)
                        {
                            pixel[3] = color.A;
                        }
                    }
                }
            }

            img.UnlockBits(data);
            return img;
        }

        #endregion
    }
}
