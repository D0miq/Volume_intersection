using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolumeIntersection;
using VolumeIntersection.SliceVisualisation;

namespace VolumeIntersectionExample
{
    class Program
    {
        static void Main(string[] args)
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

            var volumeData = VolumeIntersection<Vertex>.Intersect(tetrahedralization.Vertices, tetrahedralization.Indices, generators);

            //bitmap = new System.Drawing.Bitmap(800, 800);
            //VolumeSlicer.Slice(bitmap, 2, 0.4, volumeData, boundingBox);
            //bitmap.Save("intersectExample.png");
        }
    }
}
