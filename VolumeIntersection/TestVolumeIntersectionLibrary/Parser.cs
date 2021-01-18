using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestVolumeIntersection
{

    class Parser
    {
        /// <summary>
        /// Separator between values on a single line.
        /// </summary>
        private static char[] separator = new[] { ' ' };

        /// <summary>
        /// Reads a line and parses it as an integer.
        /// </summary>
        /// <param name="reader">Reader of a file.</param>
        /// <returns>Integer value.</returns>
        public int ParseInt(string line)
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
        public Vertex ParseVertex(string line)
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
        public int[] ParseIntVector(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                throw new FormatException("The given line cannot be empty or null!");
            }

            string[] values = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length < 4)
            {
                throw new FormatException("Vector should have at least four elements!");
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
