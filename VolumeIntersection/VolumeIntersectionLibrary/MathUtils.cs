using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeIntersection
{
    static class MathUtils
    {
        public const double Eps = 0.0000001f;

        public static double Det3x3(double[,] m)
        {
            if (m.GetLength(0) != 3 || m.GetLength(1) != 3)
            {
                throw new ArgumentException("Both dimensions must be 3 values long.");
            }

            return m[0, 0] * ((m[1, 1] * m[2, 2]) - (m[2, 1] * m[1, 2])) - m[0, 1] * (m[1, 0] * m[2, 2] - m[2, 0] * m[1, 2]) + m[0, 2] * (m[1, 0] * m[2, 1] - m[2, 0] * m[1, 1]);
        }

        public static double Det4x4(double[,] m)
        {
            if (m.GetLength(0) != 4 || m.GetLength(1) != 4)
            {
                throw new ArgumentException("Both dimensions must be 4 values long.");
            }

            double[,] sub1 = { { m[1, 1], m[1, 2], m[1, 3] }, { m[2, 1], m[2, 2], m[2, 3] }, { m[3, 1], m[3, 2], m[3, 3] } };
            double[,] sub2 = { { m[1, 0], m[1, 2], m[1, 3] }, { m[2, 0], m[2, 2], m[2, 3] }, { m[3, 0], m[3, 2], m[3, 3] } };
            double[,] sub3 = { { m[1, 0], m[1, 1], m[1, 3] }, { m[2, 0], m[2, 1], m[2, 3] }, { m[3, 0], m[3, 1], m[3, 3] } };
            double[,] sub4 = { { m[1, 0], m[1, 1], m[1, 2] }, { m[2, 0], m[2, 1], m[2, 2] }, { m[3, 0], m[3, 1], m[3, 2] } };

            return m[0, 0] * Det3x3(sub1) - m[0, 1] * Det3x3(sub2) + m[0, 2] * Det3x3(sub3) - m[0, 3] * Det3x3(sub4);
        }

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


        public static double Length(double[] v)
        {
            return Math.Sqrt(LengthSquared(v));
        }

        public static double LengthSquared(double[] v)
        {
            double norm = 0;
            for (int i = 0; i < v.Length; i++)
            {
                var t = v[i];
                norm += t * t;
            }

            return norm;
        }

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

        public static double[] CrossProduct(double[] v1, double[] v2)
        {
            return new double[] { v1[1] * v2[2] - v1[2] * v2[1], -(v1[0] * v2[2] - v1[2] * v2[0]), v1[0] * v2[1] - v1[1] * v2[0] };
        }

        public static double DistancePointPlane(double[] n, double d, double[] v)
        {
            return Dot(n, v) - d;
        }

        public static double Remap(double value, double low1, double high1, double low2, double high2)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }
    }
}
