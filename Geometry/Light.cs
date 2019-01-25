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
        static readonly Random rng = new Random();
        #region	Factory
        public Light(Vector3 position, float intensity, float jitter = 0)
            : this(Color.White, position, intensity, jitter) { }
        public Light(Color color, Vector3 position, float intensity, float jitter = 0)
        {
            this.Color = color;
            this.Position = position;
            this.Intensity= intensity;
            this.Jitter = jitter;
        }
        #endregion

        #region Properties
        public Color Color { get; }
        public Vector3 Position { get; }
        public float Intensity { get; }
        public float Jitter { get; }
        #endregion

        #region Methods
        public Vector3 JitteredPosition()
        {
            var delta = new Vector3((float)rng.NextDouble()*2-1, (float)rng.NextDouble()*2-1, (float)rng.NextDouble()*2-1);
            return Position + Jitter * delta;
        }
        #endregion
    }
}
