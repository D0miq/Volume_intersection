using System;
using System.Collections.Generic;
using System.Text;

namespace VolumeIntersection
{
    class FaceComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null && y != null || x != null && y == null)
            {
                return false;
            }

            if(x.Length != y.Length)
            {
                return false;
            }

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(int[] obj)
        {
            if (obj == null) throw new ArgumentNullException("Object cannot be null.");

            int hash = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                hash += hash * 23 + obj[i];
            }

            return hash;
        }
    }
}
