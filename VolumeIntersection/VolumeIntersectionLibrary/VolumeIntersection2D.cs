using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace VolumeIntersection
{
    public class VolumeIntersection2D : VolumeIntersection<Vector2, Cell2D, Edge2D, VolumeData2D>
    {
        protected override List<Edge2D> RemoveHalfSpaces(Cell2D c1, Cell2D c2, out List<Vector2> intersectionPoints)
        {
            var usedHalfSpaces = new Dictionary<Edge2D, HashSet<Vector2>>();
            var usedIntersectingPoints = new HashSet<Vector2>(new Vector2Comparer());

            var halfSpaces = new List<Edge2D>(c1.Edges.Count + c2.Edges.Count);
            halfSpaces.AddRange(c1.Edges);
            halfSpaces.AddRange(c2.Edges);

            for (int i = 0; i < halfSpaces.Count; i++)
            {
                for (int j = i + 1; j < halfSpaces.Count; j++)
                {
                    var intersectionPoint = FindIntersectionPoint(new List<Edge2D>() { halfSpaces[i], halfSpaces[j] });
                    var success = intersectionPoint.X != float.NaN && intersectionPoint.Y != float.NaN;

                    if (success && c1.Contains(intersectionPoint) && c2.Contains(intersectionPoint))
                    {
                        if (usedHalfSpaces.TryGetValue(halfSpaces[i], out HashSet<Vector2> vertices))
                        {
                            vertices.Add(intersectionPoint);
                        }
                        else
                        {
                            var newVertices = new HashSet<Vector2>(new Vector2Comparer());
                            newVertices.Add(intersectionPoint);
                            usedHalfSpaces.Add(halfSpaces[i], newVertices);
                        }

                        if (usedHalfSpaces.TryGetValue(halfSpaces[j], out vertices))
                        {
                            vertices.Add(intersectionPoint);
                        }
                        else
                        {
                            var newVertices = new HashSet<Vector2>(new Vector2Comparer());
                            newVertices.Add(intersectionPoint);
                            usedHalfSpaces.Add(halfSpaces[j], newVertices);
                        }

                        usedIntersectingPoints.Add(intersectionPoint);
                    }
                }
            }

            var toRemove = usedHalfSpaces.Where(pair => pair.Value.Count != 2).ToList();
            foreach (var pair in toRemove)
            {
                usedHalfSpaces.Remove(pair.Key);
            }

            intersectionPoints = usedIntersectingPoints.ToList();
            return usedHalfSpaces.Keys.ToList();
        }

        protected override void FindCentroid(List<Vector2> vertices, Cell2D cell)
        {
            float areaSum = 0;

            var centroid = new Vector2();

            var triangles = this.Triangulate(vertices);

            foreach (var triangle in triangles)
            {
                float area = 0;

                var vertex1 = triangle.Vertices[0].Position;
                var vertex2 = triangle.Vertices[1].Position;
                var vertex3 = triangle.Vertices[2].Position; 

                area = (float)Math.Abs(((vertex2[0] - vertex1[0]) * (vertex3[1] - vertex2[1])) - ((vertex2[1] - vertex1[1]) * (vertex3[0] - vertex2[0]))) / 2;

                centroid.X += (float)((vertex1[0] + vertex2[0] + vertex3[0]) / 3) * area;
                centroid.Y += (float)((vertex1[1] + vertex2[1] + vertex3[1]) / 3) * area;

                areaSum += area;
            }

            centroid.X /= areaSum;
            centroid.Y /= areaSum;

            cell.Weight = areaSum;
            cell.Centroid = centroid;
        }

        private Vector2 FindIntersectionPoint(List<Edge2D> edges)
        {
            var w = (edges[0].Normal.X * edges[1].Normal.Y) - (edges[0].Normal.Y * edges[1].Normal.X);

            if(w != 0)
            {
                return new Vector2(
                ((edges[0].Normal.Y * edges[1].C) - (edges[0].C * edges[1].Normal.Y)) / w,
                ((edges[0].Normal.X * edges[1].C) - (edges[0].C * edges[1].Normal.X)) / w);
            } else
            {
                return new Vector2(float.NaN);
            }
        }

        private IEnumerable<DefaultTriangulationCell<MIVertex>> Triangulate(List<Vector2> vertices)
        {
            IEnumerable<DefaultTriangulationCell<MIVertex>> triangles = null;

            MIVertex[] miVertices = new MIVertex[vertices.Count];
            for(int i = 0; i < vertices.Count; i++)
            {
                miVertices[i] = new MIVertex(vertices[i].X, vertices[i].Y);
            }

            if(vertices.Count == 3)
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
