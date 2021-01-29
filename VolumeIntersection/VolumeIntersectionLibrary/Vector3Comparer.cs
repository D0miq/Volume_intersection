using System;
using System.Collections.Generic;
using System.Numerics;

namespace VolumeIntersection
{
    /// <summary>
    /// Comparer that compares two 3D vectors.
    /// </summary>
    internal class Vector3Comparer : IEqualityComparer<Vector3>
    {
        /// <summary>
        /// Compares two vectors. Two vectors are equals when their coordinates are within a treshold.
        /// </summary>
        /// <param name="x">First vector.</param>
        /// <param name="y">Second vector.</param>
        /// <returns>True - vectors are equals, false otherwise.</returns>
        public bool Equals(Vector3 x, Vector3 y)
        {
            return Math.Abs(x.X - y.X) < MathUtils.Eps && Math.Abs(x.Y - y.Y) < MathUtils.Eps && Math.Abs(x.Z - y.Z) < MathUtils.Eps;
        }

        /// <summary>
        /// Gets a hash code of the given vector.
        /// </summary>
        /// <param name="obj">The vector.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(Vector3 obj)
        {
            if (obj == null) throw new ArgumentNullException("Object cannot be null.");

            int hash = 17;
            hash += hash * 23 + EqualityComparer<float>.Default.GetHashCode(obj.X);
            hash += hash * 23 + EqualityComparer<float>.Default.GetHashCode(obj.Y);
            hash += hash * 23 + EqualityComparer<float>.Default.GetHashCode(obj.Z);
            return hash;
        }
    }
}
