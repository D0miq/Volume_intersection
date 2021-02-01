using VolumeIntersectionLibrary;

namespace VolumeIntersectionExample
{
    /// <summary>
    /// Indexed vertex
    /// </summary>
    class Vertex : IIndexedVertex
    {
        /// <summary>
        /// Vertex position.
        /// </summary>
        public double[] Position { get; set; }

        /// <summary>
        /// Vertex index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Creates a new 2D vertex.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        public Vertex(double x, double y)
        {
            this.Position = new double[] { x, y };
        }

        /// <summary>
        /// Creates a new 3D vertex.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="z">Z position.</param>
        public Vertex(double x, double y, double z)
        {
            this.Position = new double[] { x, y, z };
        }

        /// <summary>
        /// Creates a new vertex from an array.
        /// </summary>
        /// <param name="position">Array with positions.</param>
        public Vertex(double[] position)
        {
            this.Position = position;
        }
    }
}
