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
    public struct Albedo
    {
        public Albedo(float diffuse, float specular, float reflection, float refraction) 
        {
            this.Diffuse=diffuse;
            this.Specular=specular;
            this.Reflection=reflection;
            this.Refraction = refraction;
        }

        public float Diffuse { get; }
        public float Specular { get; }
        public float Reflection { get; }
        public float Refraction { get; }
        public static readonly Albedo Default = new Albedo(1, 0, 0, 0);

    }

    /// <summary>
    /// Defines a Material
    /// </summary>
    public class Material
    {
        #region	Factory
        public Material(Color diffuse, float opacity = 1) : this(diffuse, Albedo.Default, 1, 0, opacity) { }
        public Material(Color diffuse, Albedo albedo, float refractive_index, float specular_exponent, float opacity = 1)
        {
            this.DiffuseColor = diffuse;
            this.Albedo = albedo;
            this.RefractiveIndex = refractive_index;
            this.SpecularExponent = specular_exponent;
            this.Opacity = opacity;
        }

        public static readonly Material Default = new Material(Color.FromArgb(128, 48, 48), Albedo.Default, 1.0f, 20f);
        #endregion

        #region Properties
        public Color DiffuseColor { get; }
        public Albedo Albedo { get; }
        public float RefractiveIndex { get; }
        public float SpecularExponent { get; }
        public float Opacity { get; }
        #endregion

    }
}
