using System.Collections.Generic;
using System.IO;

namespace VolumeIntersectionExample
{
    /// <summary>
    /// Read a triangulation from a file
    /// </summary>
    class TriangulationReader
    {
        /// <summary>
        /// Parser.
        /// </summary>
        private Parser parser;

        /// <summary>
        /// Create a new reader.
        /// </summary>
        public TriangulationReader()
        {
            parser = new Parser();
        }

        /// <summary>
        /// Reads a file with a triangulation.
        /// </summary>
        /// <param name="path">Path of a file.</param>
        /// <returns>Triangulation</returns>
        public Triangulation Read(string path)
        {
            using (var streamReader = new StreamReader(path))
            {
                // Read vertices.
                int vertexCount = this.parser.ParseInt(streamReader.ReadLine());
                var vertices = this.ReadVertices(streamReader, vertexCount);

                // Read tetrahedra.
                int tetrahedraCount = this.parser.ParseInt(streamReader.ReadLine());
                var tetrahedra = this.ReadTetrahedra(streamReader, tetrahedraCount);

                return new Triangulation()
                {
                    Vertices = vertices,
                    Indices = tetrahedra
                };
            }
        }

        /// <summary>
        /// Reads a list of vertices from a given text stream.
        /// </summary>
        /// <param name="streamReader">The text stream.</param>
        /// <param name="vertexCount">A number of vertices.</param>
        /// <returns>The list of vertices.</returns>
        private List<Vertex> ReadVertices(StreamReader streamReader, int vertexCount)
        {
            List<Vertex> vertices = new List<Vertex>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
            {
                vertices.Add(this.parser.ParseVertex(streamReader.ReadLine()));
            }

            return vertices;
        }

        /// <summary>
        /// Reads a list of tetrahedra from a given text stream.
        /// </summary>
        /// <param name="streamReader">The text stream</param>
        /// <param name="vectorsCount">A number of tetrahedra.</param>
        /// <returns>The list of tetrahedra.</returns>
        private List<Triangle> ReadTetrahedra(StreamReader streamReader, int tetrahedraCount)
        {
            List<Triangle> tetrahedra = new List<Triangle>(tetrahedraCount);
            for (int i = 0; i < tetrahedraCount; i++)
            {
                int[] line = this.parser.ParseIntVector(streamReader.ReadLine());
                tetrahedra.Add(new Triangle(line[0], line[1], line[2], line[3]));
            }

            return tetrahedra;
        }
    }
}
