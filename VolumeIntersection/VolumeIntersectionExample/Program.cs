using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolumeIntersection;

namespace VolumeIntersectionExample
{
    class Program
    {
        public static void TestFromVoronoi3D()
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

            var volumeData = new VolumeData3D();
            volumeData.FromVoronoi(generators);
        }

        public static void TestFromTriangulationSingleTriangle()
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

            var volumeData = new VolumeData2D();
            volumeData.FromTriangulation(vertices, triangles);
        }

        public static void TestFromTriangulationSingleTetrahedron()
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

            var volumeData = new VolumeData3D();
            volumeData.FromTriangulation(vertices, tetrahedra);
        }

        public static void TestIntersect2D()
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

            var volumeData = new VolumeIntersection2D().Intersect(vertices, triangles, generators);
        }

        public static void TestIntersect3D()
        {
            var vertices = new List<Vertex>()
            {
                new Vertex(0, 0, 0),
                new Vertex(1, 0, 0),
                new Vertex(0, 1, 0),
                new Vertex(0, 0, 1)
            };

            var triangles = new List<Tetrahedron>()
            {
                new Tetrahedron(0, 1, 2, 3)
            };

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

            var volumeData = new VolumeIntersection3D().Intersect(vertices, triangles, generators);
        }

        public static void TestIntersectSmallData()
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

            var volumeData = new VolumeIntersection3D().Intersect(tetrahedralization.Vertices, tetrahedralization.Indices, generators);

            //volumeData.BoundingBox = tetrahedralizationData.BoundingBox;

            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, volumeData);
            //bitmap.Save("intersectSmallData.png");
        }

        public static void TestExampleData()
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

            var reader = new TetrahedralizationReader();
            var tetrahedralization = reader.Read("example.dat");

            for (int i = 0; i < tetrahedralization.Indices.Count; i++)
            {
                var indices = tetrahedralization.Indices[i].Indices;

                for (int j = 0; j < indices.Length; j++)
                {
                    indices[j] = indices[j] - 1;
                }
            }

            //var boundingBox = new BoundingBox<Vertex>(tetrahedralization.Vertices, 3);

            //var voronoiData = VolumeData<Vertex>.FromVoronoi(generators);

            //var bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, voronoiData, boundingBox);
            //bitmap.Save("export000.png");

            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, tetrahedralizationData, boundingBox);
            //bitmap.Save("example.png");

            var volumeIntersection = new VolumeIntersection3D();

            var volumeData = volumeIntersection.Intersect(tetrahedralization.Vertices, tetrahedralization.Indices, generators);

            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, volumeData, boundingBox);
            //bitmap.Save("intersectExample.png");
        }

        private static void Main(string[] args)
        {
            //TestFromVoronoi3D();
            //TestFromTriangulationSingleTriangle();
            //TestFromTriangulationSingleTetrahedron();
            //TestIntersect2D();
            //TestIntersect3D();
            //TestIntersectSmallData();
            TestExampleData();
        }
    }
}
