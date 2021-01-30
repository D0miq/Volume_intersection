using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VolumeIntersection
{
    /// <summary>
    /// Class that computes intersections between two dimensional triangulation and voronoi diagram.
    /// </summary>
    public class VolumeIntersection2D : VolumeIntersection<Vector2D, Cell2D, Edge2D, VolumeData2D>
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
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
        protected override List<Edge2D> RemoveHalfSpaces(Cell2D c1, Cell2D c2, out List<Vector2D> intersectionPoints)
        {
            // Dictionary that saves intersection points for each edge.
            var usedHalfSpaces = new Dictionary<Edge2D, HashSet<Vector2D>>();

            // Distinct points (multiple edges can intersect in the same point and it would be bad later during a triangulation of the intersection).
            var usedIntersectingPoints = new HashSet<Vector2D>(new Vector2DComparer());

            // Save edges to a single list
            var halfSpaces = new List<Edge2D>(c1.Edges.Count + c2.Edges.Count);
            halfSpaces.AddRange(c1.Edges);
            halfSpaces.AddRange(c2.Edges);

            // Test edges with each other
            for (int i = 0; i < halfSpaces.Count; i++)
            {
                for (int j = i + 1; j < halfSpaces.Count; j++)
                {
                    // Find intersection point of two lines
                    var intersectionPoint = FindIntersectionPoint(halfSpaces[i], halfSpaces[j]);

                    // Check if the intersection point makes sense
                    var success = !double.IsNaN(intersectionPoint.X) && !double.IsNaN(intersectionPoint.Y);

                    // If the point is inside both cells add it to the dictionary
                    if (success && c1.Contains(intersectionPoint) && c2.Contains(intersectionPoint))
                    {
                        if (usedHalfSpaces.TryGetValue(halfSpaces[i], out HashSet<Vector2D> vertices))
                        {
                            vertices.Add(intersectionPoint);
                        }
                        else
                        {
                            var newVertices = new HashSet<Vector2D>(new Vector2DComparer());
                            newVertices.Add(intersectionPoint);
                            usedHalfSpaces.Add(halfSpaces[i], newVertices);
                        }

                        if (usedHalfSpaces.TryGetValue(halfSpaces[j], out vertices))
                        {
                            vertices.Add(intersectionPoint);
                        }
                        else
                        {
                            var newVertices = new HashSet<Vector2D>(new Vector2DComparer());
                            newVertices.Add(intersectionPoint);
                            usedHalfSpaces.Add(halfSpaces[j], newVertices);
                        }

                        usedIntersectingPoints.Add(intersectionPoint);
                    }
                }
            }

            // Cells that have 1 intersection point are redundant. The intersection can be defined without them. So they must be deleted.
            // More then 2 intersection points don't make sence in 2D.
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
        protected override void FindCentroid(List<Vector2D> vertices, Cell2D cell)
        {
            // TODO: Triangulation of 2D vertices doesn't work there is missing code for 4 vertices.
            // Remove return when finished.
            return;

            double areaSum = 0;
            var centroid = new Vector2D();

            // To compute centroid of a polygon it needs to be triangulated
            var triangles = this.Triangulate(vertices);

            // For each triangle compute its area and centroid
            foreach (var triangle in triangles)
            {
                double area = 0;

                var vertex1 = triangle.Vertices[0].Position;
                var vertex2 = triangle.Vertices[1].Position;
                var vertex3 = triangle.Vertices[2].Position; 

                area = Math.Abs(((vertex2[0] - vertex1[0]) * (vertex3[1] - vertex2[1])) - ((vertex2[1] - vertex1[1]) * (vertex3[0] - vertex2[0]))) / 2;

                // Add weighted centroid of this triangle to others
                centroid.X += (vertex1[0] + vertex2[0] + vertex3[0]) / 3 * area;
                centroid.Y += (vertex1[1] + vertex2[1] + vertex3[1]) / 3 * area;

                // Add area of this triangle
                areaSum += area;
            }

            centroid.X /= areaSum;
            centroid.Y /= areaSum;

            // Save total area of this cell
            cell.Weight = areaSum;

            // Centroid of the polygonal cell
            cell.Centroid = centroid;
        }

        /// <summary>
        /// Finds an intersection point of two lines.
        /// </summary>
        /// <param name="edge1">First edge (line).</param>
        /// <param name="edge2">Second edge (line).</param>
        /// <returns>The intersection point</returns>
        private Vector2D FindIntersectionPoint(Edge2D edge1, Edge2D edge2)
        {
            var w = (edge1.Normal.X * edge2.Normal.Y) - (edge1.Normal.Y * edge2.Normal.X);

            if (w != 0)
            {
                return new Vector2D(
                    ((edge1.Normal.Y * edge2.C) - (edge1.C * edge2.Normal.Y)) / w,
                    -((edge1.Normal.X * edge2.C) - (edge1.C * edge2.Normal.X)) / w
                );
            }
            else
            {
                return new Vector2D(double.NaN);
            }
        }

        /// <summary>
        /// Triangulate cell with the provided vertices.
        /// </summary>
        /// <param name="vertices">The vertices of the cell.</param>
        /// <returns>The triangulation.</returns>
        private IEnumerable<DefaultTriangulationCell<MIVertex>> Triangulate(List<Vector2D> vertices)
        {
            IEnumerable<DefaultTriangulationCell<MIVertex>> triangles = null;

            // Convert vertices to objects that work with MIConvexHull library
            MIVertex[] miVertices = new MIVertex[vertices.Count];
            for(int i = 0; i < vertices.Count; i++)
            {
                miVertices[i] = new MIVertex(vertices[i].X, vertices[i].Y);
            }

            if(vertices.Count == 3)
            {
                // MIConvexHull library cannot create a triangulation from a single triangle
                triangles = new List<DefaultTriangulationCell<MIVertex>>() { new DefaultTriangulationCell<MIVertex>() { Vertices = miVertices } };
            }
            if(vertices.Count == 4)
            {
                // There are problems with 4 vertices in MIConvexHull algorithms. There are multiple issues on their github
                // TODO Separate four unordered vertices to two triangles
            }
            else
            {
                // More vertices are OK so let the library to triangulate them
                var triangulation = Triangulation.CreateDelaunay(miVertices);
                triangles = triangulation.Cells;
            }

            return triangles;
        }
    }
}
