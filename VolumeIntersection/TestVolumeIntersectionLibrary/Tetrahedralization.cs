using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolumeUnion;

namespace TestVolumeUnion
{
    class Tetrahedralization
    {
        public List<Vertex> Vertices { get; set; }
        public List<Tetrahedron> Indices { get; set; }
    }

    class Tetrahedron : ITetrahedron
    {
        public int V1 { get; set; }

        public int V2 { get; set; }

        public int V3 { get; set; }

        public int V4 { get; set; }

        public int[] Indices => new int[] { V1, V2, V3, V4 };

        public Tetrahedron(int v1, int v2, int v3, int v4)
        {
            this.V1 = v1;
            this.V2 = v2;
            this.V3 = v3;
            this.V4 = v4;
        }
    }
}
