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

namespace JA.Geometry
{

    /// <summary>
    /// Defines a Camera that transforms pixels into screen coordinates
    /// </summary>
    public class Camera
    {
        #region	Factory
        public Camera(Size target, float fov = 60)
        {
            this.Target= target;
            this.Fov= 60;
        }
        #endregion

        #region Properties
        public Size Target { get; set; }
        public float Fov { get; set; } 
        #endregion

        #region Methods
        public Vector2 PixelToScreen(int row, int column)
        {
            int ht = Target.Height, wt = Target.Width;
            float t = Helpers.Tand(Fov/2);
            float x = +(2*(column+0.5f)/(float)(wt-1)-1)*(t*wt)/ht;
            float y = -(2*(row+0.5f)/(float)(ht-1)-1)*t;
            return new Vector2(x, y);
        }

        public Vector2[] AllPixels()
        {
            int ht = Target.Height, wt = Target.Width;
            float t = Helpers.Tand(Fov/2);
            Vector2 vector;
            var buffer = new Vector2[ht*wt];
            for (int i = 0; i < ht; i++)
            {
                for (int j = 0; j < wt; j++)
                {
                    vector.X = +(2*(j+0.5f)/(float)(wt-1)-1)*(t*wt)/ht;
                    vector.Y = -(2*(i+0.5f)/(float)(ht-1)-1)*t;
                    buffer[j+i*wt] = vector;
                }
            }
            return buffer;
        }
        #endregion
    }
}
