using System.Collections.Generic;
using System.Numerics;

namespace VolumeIntersection
{
    /// <summary>
    /// Volume cell.
    /// </summary>
    public class Cell2D : Cell<Vector2, Edge2D>
    {
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
        public override bool Contains(Vector2 point)
        {
            return base.Contains((edge) => Vector2.Dot(edge.Normal, point) - edge.C);
        }
    }
}