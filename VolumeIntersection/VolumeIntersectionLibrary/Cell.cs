using System.Collections.Generic;

namespace VolumeIntersection
{
    /// <summary>
    /// Volume cell.
    /// </summary>
    /// <typeparam name="TVector">Vector type.</typeparam>
    public class Cell<TVector> where TVector : IVector
    {
        /// <summary>
        /// Centroid of this cell.
        /// </summary>
        public TVector Centroid { get; set; }

        /// <summary>
        /// Edges of this cell.
        /// </summary>
        public List<Edge<TVector>> Edges { get; set; }

        /// <summary>
        /// What triangle cell was a parent of this cell?
        /// </summary>
        public int TriangleIndex { get; set; }

        /// <summary>
        /// What voronoi cell was a parent of this cell?
        /// </summary>
        public int VoronoiIndex { get; set; }

        /// <summary>
        /// Weight (volume) of this cell.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Was this cell visited during calculations? 
        /// </summary>
        internal bool Visited { get; set; }

        /// <summary>
        /// Creates a new cell
        /// </summary>
        public Cell()
        {
            this.Edges = new List<Edge<TVector>>();
        }

        /// <summary>
        /// Checks if this cell contains the given point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>True - cell contains the point, false otherwise.</returns>
        public bool Contains(TVector point)
        {
            var pointPosition = point.Position;

            foreach (var edge in Edges)
            {
                // Check position of the point. Compares to eps to avoid errors with points really close to an edge. 
                // In this case the dir would be near 0.
                var dir = MathUtils.Dot(edge.Normal.Position, pointPosition) - edge.C;
                if (dir < -MathUtils.Eps)
                {
                    return false;
                }
            }

            return true;
        }
    }
}