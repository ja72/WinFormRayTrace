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
    /// Defines a Ray
    /// </summary>
    public struct Ray : IEquatable<Ray>
    {
        #region	Factory
        public Ray(Vector3 origin, Vector3 direction )
        {
            this.Origin = origin;
            this.Direction= direction;
        }
        public static Ray Generate(Vector2 position)
        {
            var dir = Vector3.Normalize(new Vector3(position, -1));
            return new Ray(Vector3.Zero, dir);
        }
        public static Ray Generate(float x, float y)
        {
            var dir = Vector3.Normalize(new Vector3(x, y, -1));
            return new Ray(Vector3.Zero, dir);
        }
        public static Ray Emit(Vector3 point, Vector3 direction, Vector3 normal)
        {
            const float offset = 1e-3f;
            var origin = Vector3.Dot(direction, normal)<0 ? point - normal*offset : point + normal*offset;
            return new Ray(origin, direction);
        }
        #endregion

        #region Properties
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        #endregion

        #region Method

        public Vector3 Along(float distance) => Origin + distance * Direction;

        public override string ToString()
        {
            return $"{Origin}+t*{Direction}";
        }

        #endregion

        #region IEquatable Members
        /// <summary>
        /// Equality overrides from <see cref="System.Object"/>
        /// </summary>
        /// <param name="obj">The object to compare this with</param>
        /// <returns>False if object is a different type, otherwise it calls <code>Equals(Ray)</code></returns>
        public override bool Equals(object obj)
        {
            if (obj is Ray other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool IsParallel(Ray other)
        {
            var cos = Vector3.Dot(Direction, other.Direction);
            var sin = Vector3.Cross(Direction, other.Direction).Length();
            return Abs(Atan2(sin, cos))<Helpers.TinyNumber;
        }

        public static bool operator ==(Ray target, Ray other) => target.Equals(other);
        public static bool operator !=(Ray target, Ray other) => !(target==other);


        /// <summary>
        /// Checks for equality among <see cref="Ray"/> classes
        /// </summary>
        /// <param name="other">The other <see cref="Ray"/> to compare it to</param>
        /// <returns>True if equal</returns>
        public bool Equals(Ray other) => Origin.Equals(other.Origin) && Direction.Equals(other.Direction);

        /// <summary>
        /// Calculates the hash code for the <see cref="Ray"/>
        /// </summary>
        /// <returns>The int hash value</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hc = -1817952719;
                hc = (-1521134295)*hc + Origin.GetHashCode();
                hc = (-1521134295)*hc + Direction.GetHashCode();
                return hc;
            }
        }

        #endregion

    }
}
