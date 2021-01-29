using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace VolumeIntersection
{
    public class VolumeIntersection3D : VolumeIntersection<Vector3, Cell3D, Edge3D, VolumeData3D>
    {
        protected override List<Edge3D> RemoveHalfSpaces(Cell3D c1, Cell3D c2, out List<Vector3> intersectionPoints)
        {
            var usedHalfSpaces = new Dictionary<Edge3D, HashSet<Vector3>>();
            var usedIntersectingPoints = new HashSet<Vector3>(new Vector3Comparer());

            var halfSpaces = new List<Edge3D>(c1.Edges.Count + c2.Edges.Count);
            halfSpaces.AddRange(c1.Edges);
            halfSpaces.AddRange(c2.Edges);

            for (int i = 0; i < halfSpaces.Count; i++)
            {
                for (int j = i + 1; j < halfSpaces.Count; j++)
                {
                    for (int k = j + 1; k < halfSpaces.Count; k++)
                    {
                        var intersectionPoint = FindIntersectionPoint(new List<Edge3D>() { halfSpaces[i], halfSpaces[j], halfSpaces[k] });
                        var success = intersectionPoint.X != float.NaN && intersectionPoint.Y != float.NaN && intersectionPoint.Z != float.NaN;

                        if (success && c1.Contains(intersectionPoint) && c2.Contains(intersectionPoint))
                        {
                            if (usedHalfSpaces.TryGetValue(halfSpaces[i], out HashSet<Vector3> vertices))
                            {
                                vertices.Add(intersectionPoint);
                            }
                            else
                            {
                                var newVertices = new HashSet<Vector3>(new Vector3Comparer());
                                newVertices.Add(intersectionPoint);
                                usedHalfSpaces.Add(halfSpaces[i], newVertices);
                            }

                            if (usedHalfSpaces.TryGetValue(halfSpaces[j], out vertices))
                            {
                                vertices.Add(intersectionPoint);
                            }
                            else
                            {
                                var newVertices = new HashSet<Vector3>(new Vector3Comparer());
                                newVertices.Add(intersectionPoint);
                                usedHalfSpaces.Add(halfSpaces[j], newVertices);
                            }

                            if (usedHalfSpaces.TryGetValue(halfSpaces[k], out vertices))
                            {
                                vertices.Add(intersectionPoint);
                            }
                            else
                            {
                                var newVertices = new HashSet<Vector3>(new Vector3Comparer());
                                newVertices.Add(intersectionPoint);
                                usedHalfSpaces.Add(halfSpaces[k], newVertices);
                            }

                            usedIntersectingPoints.Add(intersectionPoint);
                        }
                    }
                }
            }

            var toRemove = usedHalfSpaces.Where(pair => pair.Value.Count < 3).ToList();
            foreach (var pair in toRemove)
            {
                usedHalfSpaces.Remove(pair.Key);
            }

            intersectionPoints = usedIntersectingPoints.ToList();
            return usedHalfSpaces.Keys.ToList();
        }

        protected override void FindCentroid(List<Vector3> vertices, Cell3D cell)
        {
            float areaSum = 0;

            var centroid = new Vector3();

            var triangles = this.Triangulate(vertices);

            foreach (var triangle in triangles)
            {
                float area = 0;

                var vertex1 = triangle.Vertices[0].Position;
                var vertex2 = triangle.Vertices[1].Position;
                var vertex3 = triangle.Vertices[2].Position;
                var vertex4 = triangle.Vertices[3].Position;

                double[] vector1 = { vertex2[0] - vertex1[0], vertex2[1] - vertex1[1], vertex2[2] - vertex1[2] };
                double[] vector2 = { vertex3[0] - vertex2[0], vertex3[1] - vertex2[1], vertex3[2] - vertex2[2] };
                double[] vector3 = { vertex4[0] - vertex3[0], vertex4[1] - vertex3[1], vertex4[2] - vertex3[2] };

                area = (float)Math.Abs(
                    vector1[0] * ((vector2[1] * vector3[2]) - (vector2[2] * vector3[1])) 
                    - vector1[1] * ((vector2[0] * vector3[2]) - (vector2[2] * vector3[0]))
                    + vector1[2] * ((vector2[0] * vector3[1]) - (vector2[1] * vector3[0]))
                    ) / 6;

                centroid.X += (float)((vertex1[0] + vertex2[0] + vertex3[0] + vertex4[0]) / 4) * area;
                centroid.Y += (float)((vertex1[1] + vertex2[1] + vertex3[1] + vertex4[1]) / 4) * area;
                centroid.Z += (float)((vertex1[2] + vertex2[2] + vertex3[2] + vertex4[2]) / 4) * area;

                areaSum += area;
            }

            centroid.X /= areaSum;
            centroid.Y /= areaSum;
            centroid.Z /= areaSum;

            cell.Weight = areaSum;
            cell.Centroid = centroid;
        }

        private Vector3 FindIntersectionPoint(List<Edge3D> edges)
        {
            // Compute half space standard form from vertices that generates it.
            // It can be calculated with a system of linear equations. I use determinant to do it.
            // Example of the calculation of a plane from three vertices:
            //  | i   j  k l |
            //  | a1 b1 c1 1 |
            //  | a2 b2 c2 1 |
            //  | a3 b3 c3 1 |
            // i, j, k, l are vectors with just one coordinate set to 1
            // i = (1, 0, 0, 0)
            // j = (0, 1, 0, 0)
            // k = (0, 0, 1, 0)
            // l = (0, 0, 0, 1)
            // a, b, c are coordinates of a vertex
            // Elements of a standard form of a half space are computed as determinant of this matrix. 
            // However I have no means to combine vectors and scalars inside the matrix so I use a way around it.
            // I use Laplace expansion to calculate result of the determinant above and  
            // compute determinants of its submatrices directly and assign results to the elements of the standard form.
            // Submatrices are
            // | b1 c1 1 | | a1 c1 1 | | a1 b1 1 | | a1 b1 c1 |
            // | b2 c2 1 | | a2 c2 1 | | a2 b2 1 | | a2 b2 c2 |
            // | b3 c3 1 | | a3 c3 1 | | a3 b3 1 | | a3 b3 c3 |


            var w = edges[0].Normal.X * ((edges[1].Normal.Y * edges[2].Normal.Z) - (edges[1].Normal.Z * edges[2].Normal.Y))
                - edges[0].Normal.Y * ((edges[1].Normal.X * edges[2].Normal.Z) - (edges[1].Normal.Z * edges[2].Normal.X))
                + edges[0].Normal.Z * ((edges[1].Normal.X * edges[2].Normal.Y) - (edges[1].Normal.Y * edges[2].Normal.X));

            if (w != 0)
            {
                return new Vector3(
                // First coordinate
                (edges[0].Normal.Y * ((edges[1].Normal.Z * edges[2].C) - (edges[1].C * edges[2].Normal.Z))
                - edges[0].Normal.Z * ((edges[1].Normal.Y * edges[2].C) - (edges[1].C * edges[2].Normal.Y))
                + edges[0].C * ((edges[1].Normal.Y * edges[2].Normal.Z) - (edges[1].Normal.Z * edges[2].Normal.Y))) / w,
                // Second coordinate
                (edges[0].Normal.X * ((edges[1].Normal.Z * edges[2].C) - (edges[1].C * edges[2].Normal.Z))
                - edges[0].Normal.Z * ((edges[1].Normal.X * edges[2].C) - (edges[1].C * edges[2].Normal.X))
                + edges[0].C * ((edges[1].Normal.X * edges[2].Normal.Z) - (edges[1].Normal.Z * edges[2].Normal.X))) / w,
                // Third coordinate
                (edges[0].Normal.X * ((edges[1].Normal.Y * edges[2].C) - (edges[1].C * edges[2].Normal.Y))
                - edges[0].Normal.Y * ((edges[1].Normal.X * edges[2].C) - (edges[1].C * edges[2].Normal.X))
                + edges[0].C * ((edges[1].Normal.X * edges[2].Normal.Y) - (edges[1].Normal.Y * edges[2].Normal.X))) / w
                );
            }
            else
            {
                return new Vector3(float.NaN);
            }
        }

        private IEnumerable<DefaultTriangulationCell<MIVertex>> Triangulate(List<Vector3> vertices)
        {
            IEnumerable<DefaultTriangulationCell<MIVertex>> triangles = null;

            MIVertex[] miVertices = new MIVertex[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                miVertices[i] = new MIVertex(vertices[i].X, vertices[i].Y);
            }

            if (vertices.Count == 4)
            {
                triangles = new List<DefaultTriangulationCell<MIVertex>>() { new DefaultTriangulationCell<MIVertex>() { Vertices = miVertices } };
            }
            else
            {
                var triangulation = Triangulation.CreateDelaunay(miVertices);
                triangles = triangulation.Cells;
            }

            return triangles;
        }
    }
}
