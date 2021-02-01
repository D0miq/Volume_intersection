using System;
using System.Collections.Generic;

namespace VolumeIntersectionLibrary
{
    /// <summary>
    /// Compares two edges defined as an array of integers.
    /// </summary>
    internal class EdgeComparer : IEqualityComparer<int[]>
    {
        /// <summary>
        /// Compares two edges.
        /// </summary>
        /// <param name="x">The first edge</param>
        /// <param name="y">The second edge</param>
        /// <returns>True - edges contains same indices, false otherwise.</returns>
        public bool Equals(int[] x, int[] y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null && y != null || x != null && y == null)
            {
                return false;
            }

            if(x.Length != y.Length)
            {
                return false;
            }

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Computes a hash code of an edge.
        /// </summary>
        /// <param name="obj">The edge.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(int[] obj)
        {
            if (obj == null) throw new ArgumentNullException("Object cannot be null.");

            int hash = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                hash += hash * 23 + obj[i];
            }

            return hash;
        }
    }
}
