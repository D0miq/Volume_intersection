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

        public static double Det(double[,] m)
        {
            if(m.GetLength(0) != m.GetLength(1))
            {
                throw new ArgumentException("Both dimensions must be the same.");
            }


        }

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
