using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using VolumeIntersection;
using VolumeIntersection.SliceVisualisation;

namespace TestVolumeIntersection
{
    [TestClass]
    public class VolumeIntersectionTest
    {
        [TestMethod]
        public void TestIntersect()
        {
            var vertices = new List<Vertex>()
            {
                new Vertex(0, 0),
                new Vertex(1, 0),
                new Vertex(0, 1)
            };

            var triangles = new List<Tetrahedron>()
            {
                new Tetrahedron(0, 1, 2)
            };

            var generators = new List<Vertex>()
            {
                new Vertex(1, 0) { Index = 0 },
                new Vertex(0, 0) { Index = 1 },
                new Vertex(-1, 0) { Index = 2 },
                new Vertex(0, 1) { Index = 3 },
                new Vertex(0, -1) { Index = 4 },
            };

            var volumeData = VolumeIntersection.VolumeIntersection<Vertex>.Intersect(vertices, triangles, generators);
        }

        [TestMethod]
        public void TestIntersectSmallData()
        {
            var reader = new TetrahedralizationReader();
            var tetrahedralization = reader.Read("smalldata.dat");

            //var tetrahedralizationData = VolumeData<Vertex>.FromTriangulation(tetrahedralization.Vertices, tetrahedralization.Indices);
            //var bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, tetrahedralizationData);
            //bitmap.Save("smalldata.png");

            Random rd = new Random();
            var generators = new List<Vertex>();

            for (int i = 0; i < 5; i++)
            {
                generators.Add(new Vertex(rd.NextDouble() * 0.6 + 0.1, rd.NextDouble() * 0.9, rd.NextDouble() * 0.7 + 0.2) { Index = i });
            }

            //var voronoiData = VolumeData<Vertex>.FromVoronoi(generators);
            //voronoiData.BoundingBox = tetrahedralizationData.BoundingBox;
            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, voronoiData);
            //bitmap.Save("voronoiSmallData.png");

            var volumeData = VolumeIntersection<Vertex>.Intersect(tetrahedralization.Vertices, tetrahedralization.Indices, generators);

            //volumeData.BoundingBox = tetrahedralizationData.BoundingBox;

            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, volumeData);
            //bitmap.Save("intersectSmallData.png");
        }

        [TestMethod]
        public void TestIntersectSmallDataMultiple()
        {
            var reader = new TetrahedralizationReader();
            var tetrahedralization = reader.Read("smalldata.dat");

            var tetrahedralizationData = VolumeData<Vertex>.FromTriangulation(tetrahedralization.Vertices, tetrahedralization.Indices);
            var boundingBox = new BoundingBox<Vertex>(tetrahedralization.Vertices, 3);

            for(int i = 0; i < 10; i++)
            {
                var bitmap = new System.Drawing.Bitmap(800, 800);
                VolumeSlicer.Slice(bitmap, 2, 0.2 + i * 0.05, tetrahedralizationData, boundingBox);
                bitmap.Save("Intersect" + i + "/smalldata.png");
            }

            Random rd = new Random();
            var generators = new List<Vertex>();

            for (int i = 0; i < 5; i++)
            {
                generators.Add(new Vertex(rd.NextDouble() * 0.6 + 0.1 , rd.NextDouble() * 0.9, rd.NextDouble() * 0.7 + 0.2) { Index = i });
            }

            var voronoiData = VolumeData<Vertex>.FromVoronoi(generators);
            for (int i = 0; i < 10; i++)
            {
                var bitmap = new System.Drawing.Bitmap(800, 800);
                VolumeSlicer.Slice(bitmap, 2, 0.2 + i * 0.05, voronoiData, boundingBox);
                bitmap.Save("Intersect" + i + "/voronoiSmallData.png");
            }

            var volumeData = VolumeIntersection<Vertex>.Intersect(tetrahedralization.Vertices, tetrahedralization.Indices, generators);

            for (int i = 0; i < 10; i++)
            {
                var bitmap = new System.Drawing.Bitmap(800, 800);
                VolumeSlicer.Slice(bitmap, 2, 0.2 + i * 0.05, volumeData, boundingBox);
                bitmap.Save("Intersect" + i + "/intersectSmallData.png");
            }
        }

        [TestMethod]
        public void TestIntersectionExampleData()
        {
            var parser = new Parser();
            var generators = new List<Vertex>();

            using (var streamReader = new StreamReader("export000.txt"))
            {
                int i = 0;
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    var vertex = parser.ParseVertex(line);
                    vertex.Index = i;
                    generators.Add(vertex);
                    i++;
                }
            }

            //var voronoiData = VolumeData<Vertex>.FromVoronoi(generators);
            //var bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, voronoiData);
            //bitmap.Save("export000.png");

            var reader = new TetrahedralizationReader();
            var tetrahedralization = reader.Read("example.dat");

            for (int i = 0; i < tetrahedralization.Vertices.Count; i++)
            {
                var position = tetrahedralization.Vertices[i].Position;
            }

            for(int i = 0; i < tetrahedralization.Indices.Count; i++)
            {
                var indices = tetrahedralization.Indices[i].Indices;

                for(int j = 0; j < indices.Length; j++)
                {
                    indices[j] = indices[j] - 1;
                }
            }

            //var tetrahedralizationData = VolumeData<Vertex>.FromTriangulation(tetrahedralization.Vertices, tetrahedralization.Indices);
            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, tetrahedralizationData);
            //bitmap.Save("example.png");

            var volumeData = VolumeIntersection.VolumeIntersection<Vertex>.Intersect(tetrahedralization.Vertices, tetrahedralization.Indices, generators);
            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, volumeData);
            //bitmap.Save("intersectExample.png");

        }
    }
}
