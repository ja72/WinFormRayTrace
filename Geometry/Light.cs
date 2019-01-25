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

using static System.Math;

namespace JA.Geometry
{

    /// <summary>
    /// Defines a Light
    /// </summary>
    public class Light
    {

        #region	Factory
        public Light(Vector3 position, float intensity)
            : this(Color.White, position, intensity) { }
        public Light(Color color, Vector3 position, float intensity)
        {
            this.Color = color;
            this.Position = position;
            this.Intensity= intensity;
        }
        #endregion

        #region Properties
        public Color Color { get; }
        public Vector3 Position { get; }
        public float Intensity { get; }
        #endregion

    }
}
