using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VolumeIntersection
{
    public class VolumeIntersection2D : VolumeIntersection<Vector2, Cell2D, Edge2D, VolumeData2D>
    {
        protected override void RemoveHalfSpaces()
        {
            throw new NotImplementedException();
        }

        protected override void FindCentroid(List<Vector2> vertices, Cell2D cell)
        {
            double areaSum = 0;

            var centroid = new Vector2();

            var triangles = this.Triangulate(vertices);

            foreach (var triangle in triangles)
            {
                double area = 0;

                double[][] triangleEdges = new double[triangle.Vertices.Length - 1][];

                var vertexPosition = triangle.Vertices[0].Position;

                int index = 0;

                for (int i = 1; i < triangle.Vertices.Length; i++)
                {
                    triangleEdges[index] = new double[this.dimension];
                    var nextVertexPosition = triangle.Vertices[i].Position;
                    for (int j = 0; j < this.dimension; j++)
                    {
                        triangleEdges[index][j] = nextVertexPosition[j] - vertexPosition[j];
                    }

                    index++;
                }

                area = Math.Abs(MathUtils.Determinant(triangleEdges)) / 2;

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

            // Compute centroid of the triangle
            for (int i = 0; i < centroid.Length; i++)
            {
                centroid[i] /= areaSum;
            }

            cell.Weight = areaSum;
            cell.Centroid = new TVector()
            {
                Position = centroid
            };
        }
    }
}
