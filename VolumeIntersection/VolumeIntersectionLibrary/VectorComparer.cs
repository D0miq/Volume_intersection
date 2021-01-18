using System;
using System.Collections.Generic;

namespace VolumeIntersection
{
    /// <summary>
    /// Compares two vectors. The comparison supportss a deviation in a calculation
    /// </summary>
    /// <typeparam name="TVector">Vector type.</typeparam>
    internal class VectorComparer<TVector> : IEqualityComparer<TVector> where TVector : IVector
    {
        /// <summary>
        /// Compares two vectors. Two vectors are equals when their coordinates are within a treshold.
        /// </summary>
        /// <param name="x">First vector.</param>
        /// <param name="y">Second vector.</param>
        /// <returns>True - vectors are equals, false otherwise.</returns>
        public bool Equals(TVector x, TVector y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null && y != null || x != null && y == null)
            {
                return false;
            }

            var xPosition = x.Position;
            var yPosition = y.Position;

            if (xPosition.Length != yPosition.Length)
            {
                return false;
            }

            for(int i = 0; i < xPosition.Length; i++)
            {
                if(Math.Abs(xPosition[i] - yPosition[i]) > MathUtils.Eps)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a hash code of the given vector.
        /// </summary>
        /// <param name="obj">The vector.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(TVector obj)
        {
            if (obj == null) throw new ArgumentNullException("Object cannot be null.");

            var position = obj.Position;

            int hash = 17;
            for (int i = 0; i < position.Length; i++)
            {
                hash += hash * 23 + EqualityComparer<double>.Default.GetHashCode(position[i]);
            }

            return hash;
        }
    }
}
