using System.Collections.Generic;
using System.Numerics;

namespace VolumeIntersection
{
    /// <summary>
    /// Edge describes a half space that separates two cells. 
    /// The normal has to point inside the source cell.
    /// 
    /// For 2D it would represent a line in the standard (implicit) form.
    /// For example ax + by >= c or ax + by <= c, where (a, b) is a normal.
    /// 
    /// For 3D it would represent a plane in the standard (implicit) form.
    /// For example ax + by + cz >= d or ax + by + cz <= d, where (a,b,c) is a normal.
    /// 
    /// The same goes for higher dimension.
    /// </summary>
    /// <typeparam name="TVector">Vector type</typeparam>
    public class Edge2D
    {
        /// <summary>
        /// Normal of this half space.
        /// </summary>
        public Vector2 Normal { get; set; }

        /// <summary>
        /// Right side of the standard form of this half space.
        /// </summary>
        public float C { get; set; }

        /// <summary>
        /// Source cell of this half space.
        /// </summary>
        public Cell2D Source { get; set; }

        /// <summary>
        /// Target cell of this half space.
        /// </summary>
        public Cell2D Target { get; set; }

        /// <summary>
        /// Compares this edge with another object.
        /// </summary>
        /// <param name="obj">Another object.</param>
        /// <returns>True - objects are equals, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is Edge2D edge &&
                   EqualityComparer<Vector2>.Default.Equals(Normal, edge.Normal) &&
                   C == edge.C;
        }

        /// <summary>
        /// Computes a hash code of this edge.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 1469261108;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(Normal);
            hashCode = hashCode * -1521134295 + C.GetHashCode();
            return hashCode;
        }
    }
}