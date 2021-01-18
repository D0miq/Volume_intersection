using MathNet.Numerics.LinearAlgebra;
using System;

namespace VolumeIntersection
{
    /// <summary>
    /// Math operations.
    /// </summary>
    internal static class MathUtils
    {
        /// <summary>
        /// Eps used for eps tests.
        /// </summary>
        public const double Eps = 1E-10;

        /// <summary>
        /// Computes linear equation with determinants.
        /// </summary>
        /// <param name="vectors">Matrix.</param>
        /// <returns>Result.</returns>
        public static double[] LinearEquationsDet(double[][] vectors)
        {
            int dimension = vectors[0].Length;
            int subMatrixSize = dimension - 1;
            // Create array that holds elements of a standard (implicit) form of a half space in n dimensions
            double[] halfSpace = new double[dimension];

            // Compute half space standard form from vertices that generates it.
            // It can be calculated with a system of linear equations. I use determinant to do it.
            // Example of the calculation of a plane from three vertices:
            //  | i   j  k l |
            //  | a1 b1 c1 1 |
            //  | a2 b2 c2 1 |
            //  | a3 b3 c3 1 |
            // i, j, k, l are vectors with just one coordinate set to 1
            // i = (1, 0, 0, 0)
            // j = (0, 1, 0, 0)
            // k = (0, 0, 1, 0)
            // l = (0, 0, 0, 1)
            // a, b, c are coordinates of a vertex
            // Elements of a standard form of a half space are computed as determinant of this matrix. 
            // However I have no means to combine vectors and scalars inside the matrix so I use a way around it.
            // I use Laplace expansion to calculate result of the determinant above and  
            // compute determinants of its submatrices directly and assign results to the elements of the standard form.
            // Submatrices are
            // | b1 c1 1 | | a1 c1 1 | | a1 b1 1 | | a1 b1 c1 |
            // | b2 c2 1 | | a2 c2 1 | | a2 b2 1 | | a2 b2 c2 |
            // | b3 c3 1 | | a3 c3 1 | | a3 b3 1 | | a3 b3 c3 |

            // Compute determinant for each variable of the half space standard form
            for (int j = 0; j < halfSpace.Length; j++)
            {
                // Create a submatrix
                double[] m = new double[subMatrixSize * subMatrixSize];

                int matrixIndex = 0;

                // Fill in values of the submatrix
                for (int k = 0; k < dimension; k++)
                {
                    // Skip k-th coordinate when its column shouldn't be inside the submatrix
                    if (j != k)
                    {
                        for (int l = 0; l < vectors.Length; l++)
                        {
                            m[(matrixIndex * vectors.Length) + l] = vectors[l][k];
                        }

                        matrixIndex++;
                    }
                }

                // Compute determinant of the submatrix
                Matrix<double> matrix = Matrix<double>.Build.Dense(subMatrixSize, subMatrixSize, m);
                halfSpace[j] = (j % 2) == 0 ? 1 * matrix.Determinant() : -1 * matrix.Determinant();
            }

            return halfSpace;
        }

        /// <summary>
        /// Computes a dot product of two vectors.
        /// </summary>
        /// <param name="v1">First vector.</param>
        /// <param name="v2">Second vector.</param>
        /// <returns>The dot product.</returns>
        public static double Dot(double[] v1, double[] v2)
        {
            if(v1.Length != v2.Length)
            {
                throw new ArgumentException("Lengths have to be the same.");
            }

            double dot = 0;
            for(int i = 0; i < v1.Length; i++)
            {
                dot += v1[i] * v2[i];
            }

            return dot;
        }

        /// <summary>
        /// Remaps value from one interval to another.
        /// </summary>
        /// <param name="value">Value that should be remapped.</param>
        /// <param name="low1">Low limit of the first interval.</param>
        /// <param name="high1">High limit of the first interval.</param>
        /// <param name="low2">Low limit of the second interval.</param>
        /// <param name="high2">High limit of the second interval.</param>
        /// <returns>Remapped value.</returns>
        public static double Remap(double value, double low1, double high1, double low2, double high2)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }
    }
}
