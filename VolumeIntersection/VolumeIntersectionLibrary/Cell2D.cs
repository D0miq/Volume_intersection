using System.Collections.Generic;
using System.Numerics;

namespace VolumeIntersection
{
    /// <summary>
    /// Volume cell.
    /// </summary>
    public class Cell2D : Cell
    {
        /// <summary>
        /// Centroid of this cell.
        /// </summary>
        public Vector2 Centroid { get; set; }

        /// <summary>
        /// Edges of this cell.
        /// </summary>
        public List<Edge2D> Edges { get; set; }

        /// <summary>
        /// Creates a new cell
        /// </summary>
        public Cell2D()
        {
            this.Edges = new List<Edge2D>();
        }

        /// <summary>
        /// Checks if this cell contains the given point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>True - cell contains the point, false otherwise.</returns>
        public bool Contains(Vector2 point)
        {
            return base.Contains(this.Edges, (edge) => Vector2.Dot(edge.Normal, point) - edge.C);
        }
    }
}