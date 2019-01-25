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

namespace JA.Geometry
{

    /// <summary>
    /// Defines a Sphere
    /// </summary>
    public struct Sphere
    {
        #region	Factory
        public Sphere(Vector3 center, float radius) : this(center, radius, Material.Default) { }
        public Sphere(Vector3 center, float radius, Material material)
        {
            this.Center= center;
            this.Radius = radius;
            this.Material = material;
        }
        #endregion

        #region Properties        
        /// <summary>
        /// The sphere center
        /// </summary>
        public Vector3 Center { get; }
        /// <summary>
        /// The sphere radius
        /// </summary>
        public float Radius { get; }

        public Material Material { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Check if ray hits the sphere.
        /// </summary>
        /// <param name="line">The ray.</param>
        /// <param name="t0">The output parameter for the hit location t0.</param>
        /// <returns></returns>
        public bool Intersect(Ray line, ref float t0)
        {
            Vector3 L = Center - line.Origin;
            var tca = Vector3.Dot(L, line.Direction);
            var d2 = Vector3.Dot(L,L) - tca*tca;
            if (d2 > Radius*Radius)
            {
                return false;
            }
            var thc = (float)Sqrt(Radius*Radius - d2);
            t0 = tca - thc;
            var t1 = tca + thc;
            if (t0 < 0)
            {
                t0 = t1;
            }
            if (t0 < 0)
            {
                return false;
            }
            return true;
        }

        #endregion

    }
}
