﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolumeUnion;

namespace TestVolumeUnion
{
    class Vertex : IVector
    {
        public double[] Position { get; set; }

        public int Index { get; set; }

        public Vertex()
        {
        }

        public Vertex(double x, double y, double z)
        {
            this.Position = new double[] { x, y, z };
        }

        public Vertex(double[] position)
        {
            this.Position = position;
        }
    }
}
