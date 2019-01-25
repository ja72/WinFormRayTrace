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
using System.Drawing.Imaging;
using System.Drawing;

namespace JA.Geometry
{

    /// <summary>
    /// Defines static function helpers and extensions
    /// </summary>
    public static class Helpers
    {
        static readonly Random rng = new Random();

        /// <summary>
        /// The largest value where <c>1+ε == 1</c>.
        /// This Equals <c>Math.Pow(2,-53)</c> since the mantissa is 52 bits.
        /// </summary>
        public const double MachinePrecision = 0.000000000000000111022302462516;
        /// <summary>
        /// This equals <c>Math.Pow(2,-51)</c> ignoring the last bit of the mantissa
        /// </summary>
        public const double TinyNumber = 0.000000000000000444089209850063;
        /// <summary>
        /// This equals <c>Math.Pow(2,1024)</c>
        /// </summary>
        public const double BigNumber = 8.98846567431158E+307;
        /// <summary>
        /// This equals <c>Math.Pow(2,-27)</c>. It is the smallest number that might not affect
        /// the results of trigonometric operation.
        /// </summary>
        /// <remarks>
        /// If <c>Cos(π-x)==-1</c> then x is 2× this
        /// if <c>Sin(x)==x</c> then x is 4× this
        /// </remarks>
        public const double TrigonometricPrecision = 0.00000000745058059692383;
        public const float deg = (float)(PI/180);
        public static float Tand(float degrees) => (float)Tan(PI/180*degrees);
        public static float ToFloat(this int i, int n) => ((float)i)/(n-1);
        public static byte ToByte(this float x) => (byte)(255*x);
        public static Color RGB(float r, float g, float b) => Color.FromArgb((int)(255*r), (int)(255*g), (int)(255*b));
        public static Color RGB(float a, float r, float g, float b)
        {
            var max = Max(r, Max(g, b));
            if (max>1)
            {
                r /= max;
                g /= max;
                b /= max;
            }
            return Color.FromArgb(
                (int)Round(255*Max(0f, Min(1f, a))),
                (int)Round(255*Max(0f, Min(1f, r))),
                (int)Round(255*Max(0f, Min(1f, g))),
                (int)Round(255*Max(0f, Min(1f, b))));
        }
        public static Vector3 ToVector(this Color color) => new Vector3(color.R/255f, color.G/255f, color.B/255f);
        public static Color RGB(this Vector3 color, float alpha = 1) => RGB(alpha, color.X, color.Y, color.Z);
        public static Color Add(this Color color, Color other)
            => RGB(Max(color.A, other.A)/255f, (color.R+other.R)/510f, (color.G+other.G)/510f, (color.B+other.B)/510f);
        public static Color Scale(this Color color, float factor) 
            => RGB(color.A/255f, factor*(color.R/255f), factor*(color.G/255f), factor*(color.B/255f));
        public static byte MaxColor(this Color color) => Max(color.R, Max(color.G, color.B));
        public static byte MinColor(this Color color) => Min(color.R, Min(color.G, color.B));
        public static float MaxValue(this Vector3 vector) => Max(vector.X, Max(vector.Y, vector.Z));
        public static float MinValue(this Vector3 vector) => Min(vector.X, Min(vector.Y, vector.Z));
        public static float MaxAbsValue(this Vector3 vector) => Max(Abs(vector.X), Max(Abs(vector.Y), Abs(vector.Z)));

        public static float ClampValue(this float x, float min_value = 0, float max_value = 1)
            => Max(min_value, Min(max_value, x));

        public static Vector2 ClampValue(this Vector2 vector, float min_value = 0, float max_value = 1)
            => new Vector2(
                Max(min_value, Min(max_value, vector.X)),
                Max(min_value, Min(max_value, vector.Y)));

        public static Vector3 ClampValue(this Vector3 vector, float min_value = 0, float max_value = 1)
            => new Vector3(
                Max(min_value, Min(max_value, vector.X)),
                Max(min_value, Min(max_value, vector.Y)),
                Max(min_value, Min(max_value, vector.Z)));

        public static Vector3 Jitter(this Vector3 along, float angle)
        {
            var l = along.Length();
            var du = (float)(along.Length() * Tan((2*rng.NextDouble()-1)*angle));
            var dv = (float)(along.Length() * Tan((2*rng.NextDouble()-1)*angle));
            var (u, v) = Orthogonal(along);
            return Vector3.Normalize(du*u + dv*v + along)*l;
        }

        public static (Vector3 u, Vector3 v) Orthogonal(this Vector3 along)
        {
            var temp = new Vector3[] {
                Vector3.Cross(Vector3.UnitX, along),
                Vector3.Cross(Vector3.UnitY, along),
                Vector3.Cross(Vector3.UnitZ, along)
            };
            var metric = temp.Select((vec) => vec.Length()).ToArray();
            var index = MaxIndex(metric);

            var u = Vector3.Normalize(temp[index]);
            var v = Vector3.Normalize(Vector3.Cross(u, along));

            return (u, v);
        }

        public static int MinIndex<T>(params T[] items) where T : IComparable<T>
        {
            int index =0;
            T min = items[0];
            for (int i = 1; i < items.Length; i++)
            {
                if (items[i].CompareTo(min)<0)
                {
                    min = items[i];
                    index = i;
                }
            }
            return index;
        }
        public static int MaxIndex<T>(params T[] items) where T : IComparable<T>
        {
            int index = 0;
            T min = items[0];
            for (int i = 1; i < items.Length; i++)
            {
                if (items[i].CompareTo(min)>0)
                {
                    min = items[i];
                    index = i;
                }
            }
            return index;
        }
    }
}
