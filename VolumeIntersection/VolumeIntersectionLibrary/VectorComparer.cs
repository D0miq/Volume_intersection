using System;
using System.Collections.Generic;
using System.Text;

namespace VolumeIntersection
{
    class VectorComparer<TVector> : IEqualityComparer<TVector> where TVector : IVector
    {
        public bool Equals(TVector x, TVector y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null && y != null || x != null && y == null)
            {
                return false;
            }

            var xPosition = x.Position;
            var yPosition = y.Position;

            if (xPosition.Length != yPosition.Length)
            {
                return false;
            }

            for(int i = 0; i < xPosition.Length; i++)
            {
                if(Math.Abs(xPosition[i] - yPosition[i]) > MathUtils.Eps)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(TVector obj)
        {
            if (obj == null) throw new ArgumentNullException("Object cannot be null.");

            var position = obj.Position;

            int hash = 17;
            for (int i = 0; i < position.Length; i++)
            {
                hash += hash * 23 + EqualityComparer<double>.Default.GetHashCode(position[i]);
            }

            return hash;
        }
    }
}
