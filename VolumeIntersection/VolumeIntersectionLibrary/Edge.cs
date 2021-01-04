using System.Collections.Generic;

namespace VolumeIntersection
{
    /// <summary>
    /// Face describes a half space that separates two cells. 
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
    /// <typeparam name="TVector"></typeparam>
    public class Edge<TVector> where TVector : IVector
    {
        public TVector Normal { get; set; }

        /// <summary>
        /// Right side of the standard form of a half space.
        /// </summary>
        public double C { get; set; }

        public Cell<TVector> Source { get; set; }

        public Cell<TVector> Target { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Edge<TVector> edge &&
                   EqualityComparer<TVector>.Default.Equals(Normal, edge.Normal) &&
                   C == edge.C;
        }

        public override int GetHashCode()
        {
            int hashCode = 1469261108;
            hashCode = hashCode * -1521134295 + EqualityComparer<TVector>.Default.GetHashCode(Normal);
            hashCode = hashCode * -1521134295 + C.GetHashCode();
            return hashCode;
        }
    }
}