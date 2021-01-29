using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VolumeIntersection
{
    public class VolumeIntersection3D : VolumeIntersection<Vector3, Cell3D, Edge3D, VolumeData3D>
    {
        protected override void RemoveHalfSpaces()
        {
            throw new NotImplementedException();
        }

        protected override void FindCentroid(List<Vector3> vertices, Cell3D cell)
        {
            float areaSum = 0;

            var centroid = new Vector3();

            var triangles = this.Triangulate(vertices);

            foreach (var triangle in triangles)
            {
                float area = 0;

                float[] triangleEdges = new float[(triangle.Vertices.Length - 1) * dimension] ;

                var vertexPosition = triangle.Vertices[0].Position;

                int index = 0;

                for (int i = 1; i < triangle.Vertices.Length; i++)
                {
                    var nextVertexPosition = triangle.Vertices[i].Position;

                    for (int j = 0; j < this.dimension; j++)
                    {
                        triangleEdges[index][j] = nextVertexPosition[j] - vertexPosition[j];
                    }

                    index++;
                }

                area = Math.Abs(MathUtils.Det3x3(triangleEdges)) / 6;

                // Compute centroid of the triangle
                for (int i = 0; i < centroid.Length; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < triangle.Vertices.Length; j++)
                    {
                        sum += triangle.Vertices[j].Position[i];
                    }

                    centroid[i] = sum / triangle.Vertices.Length * area;
                }

                areaSum += area;
            }

            centroid.X /= areaSum;
            centroid.Y /= areaSum;
            centroid.Z /= areaSum;

            cell.Weight = areaSum;
            cell.Centroid = centroid;
        }
    }
}
