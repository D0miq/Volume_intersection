using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VolumeIntersection;

namespace TestVolumeIntersection
{
    [TestClass]
    public class VolumeDataTest
    {
        [TestMethod]
        public void TestFromVoronoi()
        {
            var generators = new List<Vertex>()
            {
                new Vertex(1, 0, 0) { Index = 0 },
                new Vertex(0, 0, 0) { Index = 1 },
                new Vertex(-1, 0, 0) { Index = 2 },
                new Vertex(0, 1, 0) { Index = 3 },
                new Vertex(0, -1, 0) { Index = 4 },
                new Vertex(0, 0, 1) { Index = 5 },
                new Vertex(0, 0, -1) { Index = 6 }
            };

            var volumeData = VolumeData<Vertex>.FromVoronoi(generators);
        }

        [TestMethod]
        public void TestFromTriangulationSingleTriangle()
        {
            var vertices = new List<Vertex>()
            {
                new Vertex(0, 0),
                new Vertex(1, 0),
                new Vertex(0, 1)
            };

            var tetrahedra = new List<Tetrahedron>()
            {
                new Tetrahedron(0, 1, 2)
            };

            var volumeData = VolumeData<Vertex>.FromTriangulation(vertices, tetrahedra);
        }

        [TestMethod]
        public void TestFromTriangulationSingleTetrahedron()
        {
            var vertices = new List<Vertex>()
            {
                new Vertex(0, 0, 0),
                new Vertex(1, 0, 0),
                new Vertex(0, 1, 0),
                new Vertex(0, 0, 1)
            };

            var tetrahedra = new List<Tetrahedron>()
            {
                new Tetrahedron(0, 1, 2, 3)
            };

            var volumeData = VolumeData<Vertex>.FromTriangulation(vertices, tetrahedra);
        }

        [TestMethod]
        public void TestFromTriangulation()
        {
            var reader = new TetrahedralizationReader();
            var tetrahedralization = reader.Read("smalldata.dat");

            var volumeData = VolumeData<Vertex>.FromTriangulation(tetrahedralization.Vertices, tetrahedralization.Indices);
        }
    }
}
