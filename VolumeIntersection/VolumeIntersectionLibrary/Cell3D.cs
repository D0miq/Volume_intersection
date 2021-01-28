using System.Collections.Generic;
using System.Numerics;

namespace VolumeIntersection
{
    public class Cell3D : Cell
    {
        /// <summary>
        /// Centroid of this cell.
        /// </summary>
        public Vector3 Centroid { get; set; }

        /// <summary>
        /// Edges of this cell.
        /// </summary>
        public List<Edge3D> Edges { get; set; }

        /// <summary>
        /// Creates a new cell
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
        public bool Contains(Vector3 point)
        {
            return base.Contains(this.Edges, (edge) => Vector3.Dot(edge.Normal, point) - edge.C);
        }
    }
}
