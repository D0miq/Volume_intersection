using System;
using System.Collections.Generic;

namespace VolumeIntersectionLibrary.DataStructures
{
    /// <summary>
    /// Volumetric cell.
    /// </summary>
    /// <typeparam name="TVector">Vector type.</typeparam>
    /// <typeparam name="TEdge">Edge type.</typeparam>
    public abstract class Cell<TVector, TEdge>
    {
        /// <summary>
        /// Centroid of this cell.
        /// </summary>
        public TVector Centroid { get; set; }

        /// <summary>
        /// Edges of this cell.
        /// </summary>
        public List<TEdge> Edges { get; set; }

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
        /// Checks if this cell contains the provided point.
        /// </summary>
        /// <param name="point">Point that is checked.</param>
        /// <returns>True - cell contains the point, false otherwise.</returns>
        public abstract bool Contains(TVector point);

        /// <summary>
        /// Checks if a result of the provided direction function is contained in this cell.
        /// </summary>
        /// <param name="dirFunction">Direction function.</param>
        /// <returns>True - cell contains the point, false otherwise.</returns>
        protected bool Contains(Func<TEdge, double> dirFunction)
        {
            foreach (var edge in this.Edges)
            {
                // Check position of the point. Compares to eps to avoid errors with points really close to an edge. 
                // In this case the dir would be near -0.
                var dir = dirFunction(edge);
                if (dir < -MathUtils.Eps)
                {
                    return false;
                }
            }

            return true;
        }
    }
}