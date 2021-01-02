using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolumeIntersection;

namespace TestVolumeIntersection
{
    class Tetrahedralization
    {
        public List<Vertex> Vertices { get; set; }
        public List<Tetrahedron> Indices { get; set; }
    }

    class Tetrahedron : ITriangleCell
    {
        public int[] Indices { get; }

        public Tetrahedron(int v1, int v2, int v3)
        {
            this.Indices = new int[] { v1, v2, v3 };
        }

        public Tetrahedron(int v1, int v2, int v3, int v4)
        {
            this.Indices = new int[] { v1, v2, v3, v4 };
        }
    }
}
