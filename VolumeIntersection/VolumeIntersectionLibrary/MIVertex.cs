using MIConvexHull;

namespace VolumeIntersection
{
    /// <summary>
    /// Vertex used with MIConvexHull library. 
    /// Only for internal usage.
    /// </summary>
    internal class MIVertex : IVertex
    {
        /// <summary>
        /// Gets or sets a position of this vertex.
        /// </summary>
        public double[] Position { get; set; }

        /// <summary>
        /// Creates a two dimensional vertex.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public MIVertex(double x, double y)
        {
            Position = new double[] { x, y };
        }

        /// <summary>
        /// Creates a three dimensional vertex.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        public MIVertex(double x, double y, double z)
        {
            Position = new double[] { x, y, z };
        }
    }
}
