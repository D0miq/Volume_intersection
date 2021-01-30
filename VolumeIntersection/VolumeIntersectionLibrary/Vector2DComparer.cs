using System;
using System.Collections.Generic;

namespace VolumeIntersection
{
    /// <summary>
    /// Comparer that compares two 2D vectors.
    /// </summary>
    internal class Vector2DComparer : IEqualityComparer<Vector2D>
    {
        /// <summary>
        /// Compares two vectors. Two vectors are equals when their coordinates are within a treshold.
        /// </summary>
        /// <param name="x">First vector.</param>
        /// <param name="y">Second vector.</param>
        /// <returns>True - vectors are equals, false otherwise.</returns>
        public bool Equals(Vector2D x, Vector2D y)
        {
            return Math.Abs(x.X - y.X) < MathUtils.Eps && Math.Abs(x.Y - y.Y) < MathUtils.Eps;
        }

        /// <summary>
        /// Gets a hash code of the given vector.
        /// </summary>
        /// <param name="obj">The vector.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(Vector2D obj)
        {
            int hash = 17;
            hash += hash * 23 + obj.X.GetHashCode();
            hash += hash * 23 + obj.Y.GetHashCode();
            return hash;
        }
    }
}
