using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VolumeIntersection;
using VolumeIntersection.SliceVisualisation;

namespace TestVolumeIntersection
{
    [TestClass]
    public class VolumeSlicerTest
    {
        [TestMethod]
        public void TestSliceTetrahedron()
        {
            var boundingBox = new BoundingBox<Vertex>()
            {
                Min = new Vertex(0, 0, 0),
                Max = new Vertex(1, 1, 1)
            };

            var cell = new Cell<Vertex>();

            var edge1 = new Edge<Vertex>()
            {
                Normal = new Vertex(-0.25f, -0.5f, -0.25f),
                C = -0.375f,
                Source = cell,
            };

            var edge2 = new Edge<Vertex>()
            {
                Normal = new Vertex(0, 0, 1),
                C = 0,
                Source = cell
            };

            var edge3 = new Edge<Vertex>()
            {
                Normal = new Vertex(1, 0, 0),
                C = 0,
                Source = cell
            };

            var edge4 = new Edge<Vertex>()
            {
                Normal = new Vertex(0, 1, 0),
                C = 0,
                Source = cell
            };

            cell.Edges = new List<Edge<Vertex>>() { edge1, edge2, edge3, edge4 };

            var volumeData = new VolumeData<Vertex>()
            {
                BoundingBox = boundingBox,
                Cells = new List<Cell<Vertex>>() { cell }
            };

            var bitmap = new System.Drawing.Bitmap(600, 800);
            VolumeSlicer.Slice(bitmap, 2, 0.1f, volumeData);
            bitmap.Save("bitmap.png");
        }

        [TestMethod]
        public void TestSliceRandomVoronoi()
        {
            Random rd = new Random();
            var generators = new List<Vertex>();

            for(int i = 0; i < 50; i++)
            {
                generators.Add(new Vertex(rd.NextDouble() * 2 - 1, rd.NextDouble() * 2 - 1, rd.NextDouble() * 2 - 1) { Index = i });
            }

            var volumeData = VolumeData<Vertex>.FromVoronoi(generators);
            volumeData.BoundingBox = new BoundingBox<Vertex>()
            {
                Max = new Vertex(1, 1, 1),
                Min = new Vertex(-1, -1, -1)
            };

            var bitmap = new System.Drawing.Bitmap(800, 800);
            VolumeSlicer.Slice(bitmap, 2, 0, volumeData);
            bitmap.Save("RandomVoronoi.png");
        }

        [TestMethod]
        public void TestSliceTetrahedralization()
        {
            var reader = new TetrahedralizationReader();
            var tetrahedralization = reader.Read("bigdata.dat");

            var volumeData = VolumeData<Vertex>.FromTriangulation(tetrahedralization.Vertices, tetrahedralization.Indices);

            double xMin = double.MaxValue;
            double xMax = double.MinValue;
            double yMin = double.MaxValue;
            double yMax = double.MinValue;
            double zMin = double.MaxValue;
            double zMax = double.MinValue;

            for(int i = 0; i < tetrahedralization.Vertices.Count; i++)
            {
                var vertex = tetrahedralization.Vertices[i].Position;

                if (xMin > vertex[0]) xMin = vertex[0];
                if (xMax < vertex[0]) xMax = vertex[0];
                if (yMin > vertex[1]) yMin = vertex[1];
                if (yMax < vertex[1]) yMax = vertex[1];
                if (zMin > vertex[2]) zMin = vertex[2];
                if (zMax < vertex[2]) zMax = vertex[2];
            }

            volumeData.BoundingBox = new BoundingBox<Vertex>()
            {
                Min = new Vertex(xMin, yMin, zMin),
                Max = new Vertex(xMax, yMax, zMax)
            };

            var bitmap = new System.Drawing.Bitmap(800, 600);
            VolumeSlicer.Slice(bitmap, 2, 0.6f, volumeData);
            bitmap.Save("bigdata.png");
        }
    }
}
