using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TestVolumeIntersection
{
    class TetrahedralizationReader
    {
        /// <summary>
        /// Separator between values on a single line.
        /// </summary>
        private static char[] separator = new[] { ' ' };

        public Tetrahedralization Read(string path)
        {
            using (var streamReader = new StreamReader(path))
            {
                // Read vertices.
                int vertexCount = this.ParseInt(streamReader.ReadLine());
                var vertices = this.ReadVertices(streamReader, vertexCount);

                // Read tetrahedra.
                int tetrahedraCount = this.ParseInt(streamReader.ReadLine());
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
                vertices.Add(this.ParseVertex(streamReader.ReadLine()));
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
                int[] line = this.ParseIntVector(streamReader.ReadLine());
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

        /// <summary>
        /// Reads a line and parses it as an integer.
        /// </summary>
        /// <param name="reader">Reader of a file.</param>
        /// <returns>Integer value.</returns>
        private int ParseInt(string line)
        {
            if (!Int32.TryParse(line, out int value))
            {
                throw new FormatException();
            }

            return value;
        }

        /// <summary>
        /// Reads a line and parses it as a 3D vector, whose values are saperated by characters from <see cref="separator"/>.
        /// </summary>
        /// <param name="reader">Reader of a file.</param>
        /// <returns>3D vector.</returns>
        private Vertex ParseVertex(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                throw new FormatException("The given line cannot be empty or null!");
            }

            string[] values = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != 3)
            {
                throw new FormatException("Vector should have three elements!");
            }

            var vector = new double[3];
            for (int i = 0; i < 3; i++)
            {
                bool correct = float.TryParse(values[i], NumberStyles.Float, new CultureInfo("en-us"), out float value);
                if (!correct)
                {
                    throw new FormatException("Element of the vector hasn't been parsed!");
                }

                vector[i] = value;
            }

            return new Vertex(vector);
        }

        /// <summary>
        /// Reads a line and parses it as a tetrahedron, whose values are saperated by characters from <see cref="separator"/>. Tetrahedron has 4 values.
        /// </summary>
        /// <param name="reader">Reader of a file.</param>
        /// <returns>A tetrahedron.</returns>
        private int[] ParseIntVector(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                throw new FormatException("The given line cannot be empty or null!");
            }

            string[] values = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != 4)
            {
                throw new FormatException("Vector should have four elements!");
            }

            var intVector = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (!Int32.TryParse(values[i], out int value))
                {
                    throw new FormatException("Element of the vector hasn't been parsed!");
                }

                intVector[i] = value;
            }

            return intVector;
        }
    }
}
