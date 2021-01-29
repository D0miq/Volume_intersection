using System;
using System.Collections.Generic;
using System.Numerics;

namespace VolumeIntersection
{
    /// <summary>
    /// Comparer that compares two 2D vectors.
    /// </summary>
    internal class Vector2Comparer : IEqualityComparer<Vector2>
    {
        /// <summary>
        /// Compares two vectors. Two vectors are equals when their coordinates are within a treshold.
        /// </summary>
        /// <param name="x">First vector.</param>
        /// <param name="y">Second vector.</param>
        /// <returns>True - vectors are equals, false otherwise.</returns>
        public bool Equals(Vector2 x, Vector2 y)
        {
            return Math.Abs(x.X - y.X) < MathUtils.Eps && Math.Abs(x.Y - y.Y) < MathUtils.Eps;
        }

        /// <summary>
        /// Gets a hash code of the given vector.
        /// </summary>
        /// <param name="obj">The vector.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(Vector2 obj)
        {
            int hash = 17;
            hash += hash * 23 + EqualityComparer<float>.Default.GetHashCode(obj.X);
            hash += hash * 23 + EqualityComparer<float>.Default.GetHashCode(obj.Y);
            return hash;
        }
    }
}
