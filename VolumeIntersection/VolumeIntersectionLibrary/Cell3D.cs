using System.Collections.Generic;
using System.Numerics;

namespace VolumeIntersection
{
    public class Cell3D : Cell<Vector3, Edge3D>
    {
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
        public override bool Contains(Vector3 point)
        {
            return base.Contains((edge) => Vector3.Dot(edge.Normal, point) - edge.C);
        }
    }
}
