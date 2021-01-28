using System;
using System.Collections.Generic;

namespace VolumeIntersection
{
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
        public float Weight { get; set; }

        /// <summary>
        /// Was this cell visited during calculations? 
        /// </summary>
        internal bool Visited { get; set; }

        public abstract bool Contains(TVector point);

        /// <summary>
        /// Checks if this cell contains the provided direction function.
        /// </summary>
        /// <param name="edges">The list of edges.</param>
        /// <param name="dirFunction">Direction function.</param>
        /// <returns>True - cell contains the point, false otherwise.</returns>
        protected bool Contains(Func<TEdge, float> dirFunction)
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
