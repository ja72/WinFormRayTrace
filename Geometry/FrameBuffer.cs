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

using static System.Math;
using System.Drawing;
using System.Drawing.Imaging;

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
#if PARALLEL
            var opts = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 6,
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
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var pos = camera.PixelToScreen(i, j);
                    this[i, j] = scene.CastRay(pos, 0);
                }
            }
#endif
        }
        public FrameBuffer(int width, int height, DrawPixel draw) : this(width, height)
        {
#if PARALLEL
            var opts = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 6,                
            };
            Parallel.For(0, height, opts, (i) =>
            {                
                for (int j = 0; j < width; j++)
                {
                    this[i, j] = draw(i, j);
                }
            });
#else        
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    this[i, j] = draw(i,j);
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
        public void Blur(float strength = 0)
        {
            var corner_strength = 0.3f;
            var b = strength/(4*(corner_strength+1));
            var a = corner_strength*b;
            var c = 1-strength;

            var buffer = new Vector3[Width*Height];

            // | tl tm tr |
            // | ml mm mr |
            // | bl bm br |
            Vector3 tl, tm, tr, ml, mm, mr, bl, bm, br;

            for (int k = 0; k < buffer.Length; k++)
            {
                int i = k/Width;
                int j = k%Width;
                mm = Pixels[j + i * Width];

                tl = i>0 && j>0 ? this[i-1, j-1] : mm;
                ml = j>0 ? this[i, j-1] : mm;
                bl = j>0 && i<Height-1 ? this[i+1, j-1] : mm;
                tm = i>0 ? this[i-1, j] : mm;
                tr = i>0 && j<Width-1 ? this[i-1, j+1] : mm;
                bm = i<Height-1 ? this[i+1, j] : mm;
                br = i<Height-1 && j<Width-1 ? this[i+1, j+1] : mm;
                mr = j<Width-1 ? this[i, j+1] : mm;
                buffer[k] = a*tl+b*tm+a*tr+b*ml+c*mm+b*mr+a*bl+b*bm+a*br;
            }

            Pixels = buffer;
        }

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
