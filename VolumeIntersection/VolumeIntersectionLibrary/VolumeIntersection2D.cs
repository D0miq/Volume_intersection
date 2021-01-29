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
        public VolumeIntersection2D()
        {
            base.Dimension = 2;
        }

        /// <summary>
        /// Removes redundant half spaces and creates a minimal intersection of two cells.
        /// </summary>
        /// <param name="c1">First cell.</param>
        /// <param name="c2">Second cell.</param>
        /// <param name="intersectionPoints">Intersection points.</param>
        /// <returns>List of half spaces.</returns>
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
                    var intersectionPoint = FindIntersectionPoint(halfSpaces[i], halfSpaces[j]);
                    var success = !float.IsNaN(intersectionPoint.X) && !float.IsNaN(intersectionPoint.Y);

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

        /// <summary>
        /// Finds centroid of a cell.
        /// </summary>
        /// <param name="vertices">Vertices of the cell.</param>
        /// <param name="cell">The cell.</param>
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

        /// <summary>
        /// Finds an intersection point of two lines.
        /// </summary>
        /// <param name="edge1">First edge (line).</param>
        /// <param name="edge2">Second edge (line).</param>
        /// <returns>The intersection point</returns>
        private Vector2 FindIntersectionPoint(Edge2D edge1, Edge2D edge2)
        {
            var w = (edge1.Normal.X * edge2.Normal.Y) - (edge1.Normal.Y * edge2.Normal.X);

            if (w != 0)
            {
                return new Vector2(
                ((edge1.Normal.Y * edge2.C) - (edge1.C * edge2.Normal.Y)) / w,
                -((edge1.Normal.X * edge2.C) - (edge1.C * edge2.Normal.X)) / w);
            }
            else
            {
                return new Vector2(float.NaN);
            }

            //var edge1Vec = new Vector3(edge1.Normal.X, edge1.Normal.Y, edge1.C);
            //var edge2Vec = new Vector3(edge2.Normal.X, edge2.Normal.Y, edge2.C);

            //var cross = Vector3.Cross(edge1Vec, edge2Vec);

            //return cross.Z != 0 ? new Vector2(cross.X / cross.Z, cross.Y / cross.Z) : new Vector2(float.NaN);
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
