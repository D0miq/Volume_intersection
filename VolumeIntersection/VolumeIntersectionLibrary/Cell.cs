using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeIntersection
{
    public class Cell<TVector> where TVector : IVector
    {
        public TVector Centroid { get; set; }

        public List<Edge<TVector>> Edges { get; set; }

        public int VoronoiIndex { get; set; }

        public double Weight { get; set; }

        public bool Visited { get; set; }

        public Cell()
        {
            this.Edges = new List<Edge<TVector>>();
        }

        public bool Contains(TVector point)
        {
            var pointPosition = point.Position;

            foreach (var edge in Edges)
            {
                var dir = MathUtils.Dot(edge.Normal.Position, pointPosition) - edge.C;
                if (dir < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}