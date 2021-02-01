using System.Collections.Generic;
using VolumeIntersectionLibrary;

namespace VolumeIntersectionExample
{
    /// <summary>
    /// Triangulation.
    /// </summary>
    class Triangulation
    {
        /// <summary>
        /// Vertices.
        /// </summary>
        public List<Vertex> Vertices { get; set; }

        /// <summary>
        /// Triangles.
        /// </summary>
        public List<Triangle> Indices { get; set; }
    }

    /// <summary>
    /// Triangle.
    /// </summary>
    class Triangle : ITriangleCell
    {
        /// <summary>
        /// Indices.
        /// </summary>
        public int[] Indices { get; }

        /// <summary>
        /// Creates a new triangle.
        /// </summary>
        /// <param name="v1">First vertex.</param>
        /// <param name="v2">Second vertex.</param>
        /// <param name="v3">Third vertex.</param>
        public Triangle(int v1, int v2, int v3)
        {
            this.Indices = new int[] { v1, v2, v3 };
        }

        /// <summary>
        /// Create a new tetrahedron.
        /// </summary>
        /// <param name="v1">First vertex.</param>
        /// <param name="v2">Second vertex.</param>
        /// <param name="v3">Third vertex.</param>
        /// <param name="v4">Fourth vertex.</param>
        public Triangle(int v1, int v2, int v3, int v4)
        {
            this.Indices = new int[] { v1, v2, v3, v4 };
        }
    }
}
