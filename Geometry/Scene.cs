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
using static JA.Geometry.Helpers;
using System.Drawing.Imaging;

namespace JA.Geometry
{

    /// <summary>
    /// Defines a Scene object for raytracing rendering.
    /// </summary>
    /// <remarks>
    /// <para>This code is based on the article "Understandable RayTracing in 256 lines of bare C++"
    /// <blockquote>
    /// link: <a href="https://github.com/ssloy/tinyraytracer/wiki">https://github.com/ssloy/tinyraytracer/wiki</a>
    /// </blockquote>
    /// </para>
    /// </remarks>
    public class Scene
    {
        #region	Factory
        public Scene()
        {
            this.Spheres = new List<Sphere>();
            this.Lights = new List<Light>();
            this.Background = RGB(0.2f, 0.7f, 0.8f);
            this.MaxDepth = 4;
        }
        #endregion

        #region Properties
        public Color Background { get;  }
        public List<Sphere> Spheres { get; }
        public List<Light> Lights { get; }
        public int MaxDepth { get; set; }
        #endregion

        #region Methods

        public void Add(Sphere sphere) => Spheres.Add(sphere);
        public void Add(Light light) => Lights.Add(light);

        public Vector3 Reflect(Vector3 incident, Vector3 normal) => Vector3.Normalize(Vector3.Reflect(incident, normal));
        public Vector3 Refract(Vector3 incident, Vector3 normal, float refractive_index)
        {
            var cosi = -Max(-1f, Min(1f, Vector3.Dot(incident, normal)));
            float etai = 1, etat = refractive_index;
            var n = normal;
            if (cosi<0)
            {
                cosi = -cosi;
                n = -normal;
                etai = refractive_index;
                etat = 1;
            }
            var eta = etai/etat;
            var k = 1 - eta*eta*(1-cosi*cosi);
            return k<0 ? Vector3.Zero : Vector3.Normalize(incident*eta + n*(eta*cosi - (float)Sqrt(k)));
        }


        public bool Intersect(Ray ray, out Vector3 hit, out Vector3 normal, out Material material)
        {
            float min_distance = float.MaxValue;
            hit = Vector3.Zero;
            normal = Vector3.Zero;
            material = null;
            foreach (var item in Spheres)
            {
                float distance = min_distance;
                if (item.Intersect(ray, ref distance) && distance < min_distance)
                {
                    min_distance = distance;
                    hit = ray.Along(distance);
                    normal = Vector3.Normalize(hit - item.Center);
                    material = item.Material;
                }
            }

            var checkerboard_dist = float.MaxValue;
            if (Abs(ray.Direction.Y)>1e-3f)
            {
                float d = -(ray.Origin.Y+4)/ray.Direction.Y; // the checkerboard plane has equation y = -4
                Vector3 pt = ray.Along(d);
                if (d>0 && Abs(pt.X)<10 && pt.Z<-10 && pt.Z>-30 && d<min_distance)
                {
                    checkerboard_dist = d;
                    hit = pt;
                    normal = Vector3.UnitY;
                    var color = ((int)(.5f*hit.X+1000) + (int)(.5f*hit.Z)) % 2 > 0 ? Color.White.ToVector()*0.3f : Color.Orange.ToVector()*0.3f;
                    material = new Material(color.RGB());
                }
            }
            return Min(min_distance, checkerboard_dist)<1000;
        }
        public Vector3 CastRay(Vector2 position, int depth = 0) => CastRay(Ray.Generate(position), depth);
        public Vector3 CastRay(Ray ray, int depth = 0)
        {
            if (depth<MaxDepth && Intersect(ray, out Vector3 point, out Vector3 normal, out Material material))
            {
                var reflect_dir = Reflect(ray.Direction, normal);
                var reflect = Ray.Emit(point, reflect_dir, normal);
                var reflect_color = CastRay(reflect, depth+1);

                var refract_dir = Refract(ray.Direction, normal, material.RefractiveIndex);
                var refract = Ray.Emit(point, refract_dir, normal);
                var refract_color = CastRay(refract, depth+1);

                float diffuse_intensity = 0f, specular_light_intensity = 0;
                Vector3 light_color = Vector3.Zero;
                foreach (var light in Lights)
                {
                    var light_pos = light.Position;
                    var rel_light_pos = light_pos - point;
                    float light_distance = rel_light_pos.Length();
                    var light_dir = rel_light_pos/light_distance;

                    // checking if the point lies in the shadow of the lights[i]
                    var shadow_ray = Ray.Emit(point, light_dir, normal);

                    if (Intersect(shadow_ray, out Vector3 shadow_pt, out Vector3 shadow_normal, out Material temp_material)
                        && (shadow_pt-shadow_ray.Origin).Length() < light_distance )
                    {
                        continue;
                    }

                    var diffuse_part = light.Intensity * Max(0f, Vector3.Dot(light_dir, normal));
                    var specular_part = light.Intensity*((float)Pow(Max(0f, -Vector3.Dot(Vector3.Reflect(-light_dir, normal), ray.Direction)), material.SpecularExponent));
                    diffuse_intensity += diffuse_part;
                    specular_light_intensity += specular_part;
                    light_color += specular_part*light.Color.ToVector();
                }
                var max = Max(light_color.X, Max(light_color.Y, light_color.Z));
                if (max>0)
                {
                    light_color = light_color/max;
                }
                Vector3 color = material.DiffuseColor.ToVector()*diffuse_intensity*material.Albedo.Diffuse
                    + light_color*specular_light_intensity*material.Albedo.Specular 
                    + reflect_color*material.Albedo.Reflection
                    + refract_color*material.Albedo.Refraction;

                return material.Opacity*color + (1-material.Opacity)*Background.ToVector();
            }
            return Background.ToVector();
        }

        /// <summary>
        /// Renders the scene into a bitmap. Calls <see cref="CastRay(Vector2, int)"/> for each pixel in the image.
        /// </summary>
        /// <param name="target">The target size of the render (in pixels).</param>
        /// <returns>A bitmap with the same size as the target</returns>
        /// <seealso cref="Camera"/>
        /// <seealso cref="FrameBuffer"/>
        public Bitmap Render(Size target, PixelFormat format = PixelFormat.Format32bppRgb)
        {
            var camera = new Camera(target);
            var buffer = new FrameBuffer(this, camera);
            return buffer.Render(format);
        }

        #endregion
    }
}
