using System.Collections.Generic;
using System.Numerics;

namespace VolumeIntersection
{
    public class Edge3D
    {
        /// <summary>
        /// Normal of this half space.
        /// </summary>
        public Vector3 Normal { get; set; }

        /// <summary>
        /// Right side of the standard form of this half space.
        /// </summary>
        public float C { get; set; }

        /// <summary>
        /// Source cell of this half space.
        /// </summary>
        public Cell3D Source { get; set; }

        /// <summary>
        /// Target cell of this half space.
        /// </summary>
        public Cell3D Target { get; set; }

        /// <summary>
        /// Compares this edge with another object.
        /// </summary>
        /// <param name="obj">Another object.</param>
        /// <returns>True - objects are equals, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is Edge3D edge &&
                   EqualityComparer<Vector3>.Default.Equals(Normal, edge.Normal) &&
                   C == edge.C;
        }

        /// <summary>
        /// Computes a hash code of this edge.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 1469261108;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(Normal);
            hashCode = hashCode * -1521134295 + C.GetHashCode();
            return hashCode;
        }
    }
}
