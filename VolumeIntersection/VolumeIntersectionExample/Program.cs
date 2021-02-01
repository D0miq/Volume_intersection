using System;
using System.Collections.Generic;
using System.IO;
using VolumeIntersectionLibrary;
using VolumeIntersectionLibrary.DataStructures._2D;
using VolumeIntersectionLibrary.DataStructures._3D;
using VolumeIntersectionLibrary.Intersection;
using VolumeIntersectionLibrary.Intersection.HalfSpaceRemoval;
using VolumeIntersectionLibrary.IO;
using VolumeIntersectionLibrary.Visualisation;

namespace VolumeIntersectionExample
{
    /// <summary>
    /// Class that runs examples.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Creates a boudning box of 3D vertices.
        /// </summary>
        /// <param name="vertices">Vertices.</param>
        /// <returns>The bounding box.</returns>
        private static BoundingBox3D CreateBoundingBox3D(List<Vertex> vertices)
        {
            var min = new Vector3D(double.MaxValue);
            var max = new Vector3D(double.MinValue);

            // Find min and max positions of vertices
            for (int i = 0; i < vertices.Count; i++)
            {
                var position = vertices[i].Position;

                if (min.X > position[0])
                {
                    min.X = position[0];
                }

                if (min.Y > position[1])
                {
                    min.Y = position[1];
                }

                if(min.Z > position[2])
                {
                    min.Z = position[2];
                }

                if (max.X < position[0])
                {
                    max.X = position[0];
                }

                if (max.Y < position[1])
                {
                    max.Y = position[1];
                }

                if(max.Z < position[2])
                {
                    max.Z = position[2];
                }
            }

            // Assing min and max.
            return new BoundingBox3D(min, max);
        }

        /// <summary>
        /// Creates a bounding box from 2D vertices.
        /// </summary>
        /// <param name="vertices">Vertices.</param>
        /// <returns>Bounding box.</returns>
        private static BoundingBox2D CreateBoundingBox2D(List<Vertex> vertices)
        {
            var min = new Vector2D(double.MaxValue);
            var max = new Vector2D(double.MinValue);

            // Find min and max positions of vertices
            for (int i = 0; i < vertices.Count; i++)
            {
                var position = vertices[i].Position;

                if(min.X > position[0])
                {
                    min.X = position[0];
                }

                if(min.Y > position[1])
                {
                    min.Y = position[1];
                }

                if(max.X < position[0])
                {
                    max.X = position[0];
                }

                if(max.Y < position[1])
                {
                    max.Y = position[1];
                }
            }

            // Assing min and max.
            return new BoundingBox2D(min, max);
        }

        /// <summary>
        /// Test voronoi volumetric data in 3D
        /// </summary>
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

        /// <summary>
        /// Test triangulation volumetric data in 2D.
        /// </summary>
        public static void TestFromTriangulationSingleTriangle()
        {
            var vertices = new List<Vertex>()
            {
                new Vertex(0, 0),
                new Vertex(1, 0),
                new Vertex(0, 1)
            };

            var triangles = new List<Triangle>()
            {
                new Triangle(0, 1, 2)
            };

            var volumeData = new VolumeData2D();
            volumeData.FromTriangulation(vertices, triangles);
        }

        /// <summary>
        /// Test triangulation volumetric data in 3D.
        /// </summary>
        public static void TestFromTriangulationSingleTetrahedron()
        {
            var vertices = new List<Vertex>()
            {
                new Vertex(0, 0, 0),
                new Vertex(1, 0, 0),
                new Vertex(0, 1, 0),
                new Vertex(0, 0, 1)
            };

            var tetrahedra = new List<Triangle>()
            {
                new Triangle(0, 1, 2, 3)
            };

            var volumeData = new VolumeData3D();
            volumeData.FromTriangulation(vertices, tetrahedra);
        }

        /// <summary>
        /// Test intersections in 2D.
        /// </summary>
        public static void TestIntersect2D()
        {
            var vertices = new List<Vertex>()
            {
                new Vertex(0, 0),
                new Vertex(1, 0),
                new Vertex(0, 1)
            };

            var triangles = new List<Triangle>()
            {
                new Triangle(0, 1, 2)
            };

            var generators = new List<Vertex>()
            {
                new Vertex(1, 0) { Index = 0 },
                new Vertex(0, 0) { Index = 1 },
                new Vertex(-1, 0) { Index = 2 },
                new Vertex(0, 1) { Index = 3 },
                new Vertex(0, -1) { Index = 4 },
            };

            var volumeData = new VolumeIntersection2D(new BruteForceHalfSpaceRemoval2D()).Intersect(vertices, triangles, generators);
        }

        /// <summary>
        /// Test intersection in 3D.
        /// </summary>
        public static void TestIntersect3D()
        {
            var vertices = new List<Vertex>()
            {
                new Vertex(0, 0, 0),
                new Vertex(1, 0, 0),
                new Vertex(0, 1, 0),
                new Vertex(0, 0, 1)
            };

            var triangles = new List<Triangle>()
            {
                new Triangle(0, 1, 2, 3)
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

            var volumeData = new VolumeIntersection3D(new BruteForceHalfSpaceRemoval3D()).Intersect(vertices, triangles, generators);
        }

        /// <summary>
        /// Test intersection in 3D on a small dataset.
        /// </summary>
        public static void TestIntersectSmallData()
        {
            var reader = new TriangulationReader();
            var tetrahedralization = reader.Read("smalldata.dat");

            Random rd = new Random();
            var generators = new List<Vertex>();

            for (int i = 0; i < 5; i++)
            {
                generators.Add(new Vertex(rd.NextDouble() * 0.6 + 0.1, rd.NextDouble() * 0.9, rd.NextDouble() * 0.7 + 0.2) { Index = i });
            }

            //var boundingBox = CreateBoundingBox3D(tetrahedralization.Vertices);

            //var voronoiData = new VolumeData3D();
            //voronoiData.FromVoronoi(generators);

            //var tetrahedralizationData = new VolumeData3D();
            //tetrahedralizationData.FromTriangulation(tetrahedralization.Vertices, tetrahedralization.Indices);

            //var bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeVisualisator.Visualise3D(bitmap, 0.4, voronoiData, boundingBox);
            //bitmap.Save("voronoiSmallData.png");

            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeVisualisator.Visualise3D(bitmap, 0.4, tetrahedralizationData, boundingBox);
            //bitmap.Save("smalldata.png");

            var volumeData = new VolumeIntersection3D(new BruteForceHalfSpaceRemoval3D()).Intersect(tetrahedralization.Vertices, tetrahedralization.Indices, generators);

            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeVisualisator.Visualise3D(bitmap, 0.4, volumeData, boundingBox);
            //bitmap.Save("intersectSmallData.png");
        }

        /// <summary>
        /// Test intersections in 3D on a big dataset.
        /// </summary>
        public static void TestIntersectExampleData()
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

            var reader = new TriangulationReader();
            var tetrahedralization = reader.Read("example.dat");

            for (int i = 0; i < tetrahedralization.Indices.Count; i++)
            {
                var indices = tetrahedralization.Indices[i].Indices;

                for (int j = 0; j < indices.Length; j++)
                {
                    indices[j] = indices[j] - 1;
                }
            }

            //var boundingBox = CreateBoundingBox3D(tetrahedralization.Vertices);

            //var voronoiData = new VolumeData3D();
            //voronoiData.FromVoronoi(generators);

            var tetrahedralizationData = new VolumeData3D();
            tetrahedralizationData.FromTriangulation(tetrahedralization.Vertices, tetrahedralization.Indices);

            //var bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeVisualisator.Visualise3D(bitmap, 0.4, voronoiData, boundingBox);
            //bitmap.Save("export000.png");

            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeVisualisator.Visualise3D(bitmap, 0.4, tetrahedralizationData, boundingBox);
            //bitmap.Save("example.png");

            var volumeIntersection = new VolumeIntersection3D(new BruteForceHalfSpaceRemoval3D());
            var volumeData = volumeIntersection.Intersect(tetrahedralization.Vertices, tetrahedralization.Indices, generators);

            var writer = new VolumeData3DWriter(',');
            writer.Write("test.txt", tetrahedralizationData);

            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeVisualisator.Visualise3D(bitmap, 0.4, volumeData, boundingBox);
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
            TestIntersectExampleData();
        }
    }
}
