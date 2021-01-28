using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VolumeIntersection;

namespace TestVolumeIntersection
{
    [TestClass]
    public class VolumeDataTest
    {
        private List<Vertex> vertices;
        private List<Tetrahedron> triangles;

        [TestInitialize]
        public void Initialize()
        {
            vertices = new List<Vertex>()
            {
                new Vertex(0, 0),
                new Vertex(1, 0),
                new Vertex(0, 1)
            };

            triangles = new List<Tetrahedron>()
            {
                new Tetrahedron(0, 1, 2)
            };
        }

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

            var volumeData = IVolumeData<Vertex>.FromVoronoi(generators);
        }

        [TestMethod]
        public void TestFromTriangulationSingleTriangleCellCentroid()
        {
            var volumeData = IVolumeData<Vertex>.FromTriangulation(vertices, triangles);
            var centroid = volumeData.Cells[0].Centroid.Position;

            Assert.AreEqual(centroid[0], 1.0/3.0);
            Assert.AreEqual(centroid[1], 1.0/3.0);
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

            var volumeData = IVolumeData<Vertex>.FromTriangulation(vertices, tetrahedra);
        }

        [TestMethod]
        public void TestFromTriangulation()
        {
            var reader = new TetrahedralizationReader();
            var tetrahedralization = reader.Read("smalldata.dat");

            var volumeData = IVolumeData<Vertex>.FromTriangulation(tetrahedralization.Vertices, tetrahedralization.Indices);
        }
    }
}
