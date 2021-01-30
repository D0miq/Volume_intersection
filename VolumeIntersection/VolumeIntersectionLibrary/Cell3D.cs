using System.Collections.Generic;

namespace VolumeIntersection
{
    /// <summary>
    /// Three dimensional volumetric cell.
    /// </summary>
    public class Cell3D : Cell<Vector3D, Edge3D>
    {
        /// <summary>
        /// Creates a new 3D cell.
        /// </summary>
        public Cell3D()
        {
            this.Edges = new List<Edge3D>();
        }

        /// <summary>
        /// Checks if this cell contains the given point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>True - cell contains the point, false otherwise.</returns>
        public override bool Contains(Vector3D point)
        {
            return base.Contains((edge) => edge.Normal.Dot(point) + edge.C);
        }
    }
}
