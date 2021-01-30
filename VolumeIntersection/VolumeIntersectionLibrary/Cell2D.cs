using System.Collections.Generic;

namespace VolumeIntersection
{
    /// <summary>
    /// Two dimensional volumetric cell.
    /// </summary>
    public class Cell2D : Cell<Vector2D, Edge2D>
    {
        /// <summary>
        /// Creates a new 2D cell.
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
        public override bool Contains(Vector2D point)
        {
            return base.Contains((edge) => edge.Normal.Dot(point) + edge.C);
        }
    }
}