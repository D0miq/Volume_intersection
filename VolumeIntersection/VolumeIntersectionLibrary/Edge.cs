using System.Collections.Generic;

namespace VolumeIntersection
{
    /// <summary>
    /// Edge describes a half space that separates two cells. 
    /// The normal has to point inside the source cell.
    /// </summary>
    /// <typeparam name="TVector">Vector type.</typeparam>
    /// <typeparam name="TCell">Cell type.</typeparam>
    public abstract class Edge<TVector, TCell>
    {
        /// <summary>
        /// Normal of this half space.
        /// It has to point inside the source cell.
        /// </summary>
        public TVector Normal { get; set; }

        /// <summary>
        /// Constant element of the standard form of this half space.
        /// </summary>
        public float C { get; set; }

        /// <summary>
        /// Source cell of this half space.
        /// </summary>
        public TCell Source { get; set; }

        /// <summary>
        /// Target cell of this half space.
        /// </summary>
        public TCell Target { get; set; }

        /// <summary>
        /// Compares this edge with another object.
        /// </summary>
        /// <param name="obj">Another object.</param>
        /// <returns>True - objects are equals, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is Edge<TVector, TCell> edge &&
                   EqualityComparer<TVector>.Default.Equals(Normal, edge.Normal) &&
                   C == edge.C;
        }

        /// <summary>
        /// Computes a hash code of this edge.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 1469261108;
            hashCode = hashCode * -1521134295 + EqualityComparer<TVector>.Default.GetHashCode(Normal);
            hashCode = hashCode * -1521134295 + C.GetHashCode();
            return hashCode;
        }
    }
}
