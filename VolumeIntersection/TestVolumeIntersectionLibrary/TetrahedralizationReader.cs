using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TestVolumeIntersection
{
    class TetrahedralizationReader
    {
        private Parser parser;

        public TetrahedralizationReader()
        {
            parser = new Parser();
        }

        public Tetrahedralization Read(string path)
        {
            using (var streamReader = new StreamReader(path))
            {
                // Read vertices.
                int vertexCount = this.parser.ParseInt(streamReader.ReadLine());
                var vertices = this.ReadVertices(streamReader, vertexCount);

                // Read tetrahedra.
                int tetrahedraCount = this.parser.ParseInt(streamReader.ReadLine());
                var tetrahedra = this.ReadTetrahedra(streamReader, tetrahedraCount);

                return new Tetrahedralization()
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
        private List<Tetrahedron> ReadTetrahedra(StreamReader streamReader, int tetrahedraCount)
        {
            List<Tetrahedron> tetrahedra = new List<Tetrahedron>(tetrahedraCount);
            for (int i = 0; i < tetrahedraCount; i++)
            {
                int[] line = this.parser.ParseIntVector(streamReader.ReadLine());
                tetrahedra.Add(new Tetrahedron(line[0], line[1], line[2], line[3]));
            }

            return tetrahedra;
        }

        //private void ReadNeighbors(StreamReader streamReader, int neigborsCount, List<Tetrahedron> tetrahedra)
        //{
        //    for (int i = 0; i < neigborsCount; i++)
        //    {
        //        int[] line = this.ParseIntVector(streamReader.ReadLine());

        //        var tempNeighbors = new List<Tetrahedron>(line.Length);
        //        for (int j = 0; j < line.Length; j++)
        //        {
        //            if (line[j] >= 0)
        //            {
        //                tempNeighbors.Add(tetrahedra[line[j]]);
        //            }
        //        }

        //        tetrahedra[i].Neighbors = tempNeighbors;
        //    }
        //}
    }
}
