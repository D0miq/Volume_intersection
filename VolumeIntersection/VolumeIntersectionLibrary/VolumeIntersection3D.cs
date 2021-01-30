using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VolumeIntersection
{
    /// <summary>
    /// Class that computes intersections between three dimensional triangulation and voronoi diagram.
    /// </summary>
    public class VolumeIntersection3D : VolumeIntersection<Vector3D, Cell3D, Edge3D, VolumeData3D>
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public VolumeIntersection3D()
        {
            base.Dimension = 3;
        }

        /// <summary>
        /// Removes redundant half spaces and creates a minimal intersection of two cells.
        /// </summary>
        /// <param name="c1">First cell.</param>
        /// <param name="c2">Second cell.</param>
        /// <param name="intersectionPoints">Intersection points.</param>
        /// <returns>List of half spaces.</returns>
        protected override List<Edge3D> RemoveHalfSpaces(Cell3D c1, Cell3D c2, out List<Vector3D> intersectionPoints)
        {
            // Dictionary that saves intersection points for each edge.
            var usedHalfSpaces = new Dictionary<Edge3D, HashSet<Vector3D>>();

            // Distinct points (multiple edges can intersect in the same point and it would be bad later during a triangulation of the intersection).
            var usedIntersectingPoints = new HashSet<Vector3D>(new Vector3DComparer());

            // Save edges to a single list
            var halfSpaces = new List<Edge3D>(c1.Edges.Count + c2.Edges.Count);
            halfSpaces.AddRange(c1.Edges);
            halfSpaces.AddRange(c2.Edges);

            // Test edges with each other
            for (int i = 0; i < halfSpaces.Count; i++)
            {
                for (int j = i + 1; j < halfSpaces.Count; j++)
                {
                    for (int k = j + 1; k < halfSpaces.Count; k++)
                    {
                        // Find intersection point of three planes
                        var intersectionPoint = FindIntersectionPoint(halfSpaces[i], halfSpaces[j], halfSpaces[k]);

                        // Check if the intersection point makes sense
                        var success = !double.IsNaN(intersectionPoint.X) && !double.IsNaN(intersectionPoint.Y) && !double.IsNaN(intersectionPoint.Z);

                        // If the point is inside both cells add it to the dictionary
                        if (success && c1.Contains(intersectionPoint) && c2.Contains(intersectionPoint))
                        {
                            if (usedHalfSpaces.TryGetValue(halfSpaces[i], out HashSet<Vector3D> vertices))
                            {
                                vertices.Add(intersectionPoint);
                            }
                            else
                            {
                                var newVertices = new HashSet<Vector3D>(new Vector3DComparer());
                                newVertices.Add(intersectionPoint);
                                usedHalfSpaces.Add(halfSpaces[i], newVertices);
                            }

                            if (usedHalfSpaces.TryGetValue(halfSpaces[j], out vertices))
                            {
                                vertices.Add(intersectionPoint);
                            }
                            else
                            {
                                var newVertices = new HashSet<Vector3D>(new Vector3DComparer());
                                newVertices.Add(intersectionPoint);
                                usedHalfSpaces.Add(halfSpaces[j], newVertices);
                            }

                            if (usedHalfSpaces.TryGetValue(halfSpaces[k], out vertices))
                            {
                                vertices.Add(intersectionPoint);
                            }
                            else
                            {
                                var newVertices = new HashSet<Vector3D>(new Vector3DComparer());
                                newVertices.Add(intersectionPoint);
                                usedHalfSpaces.Add(halfSpaces[k], newVertices);
                            }

                            usedIntersectingPoints.Add(intersectionPoint);
                        }
                    }
                }
            }

            // Smallest edge in 3D is a triangle. Everything with less than three vertices is redundant. 
            // The intersection can be defined without them. So they must be deleted.
            var toRemove = usedHalfSpaces.Where(pair => pair.Value.Count < 3).ToList();
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
        protected override void FindCentroid(List<Vector3D> vertices, Cell3D cell)
        {
            double areaSum = 0;

            var centroid = new Vector3D();

            // To compute centroid of a polyhedron it needs to be triangulated
            var tetrahedrons = this.Triangulate(vertices);

            // For each tetrahedron compute its area and centroid
            foreach (var tetrahedron in tetrahedrons)
            {
                double area = 0;

                var vertex1 = tetrahedron.Vertices[0].Position;
                var vertex2 = tetrahedron.Vertices[1].Position;
                var vertex3 = tetrahedron.Vertices[2].Position;
                var vertex4 = tetrahedron.Vertices[3].Position;

                // Prepare edges of the tetrahedron
                var vector1 = new Vector3D(vertex2[0] - vertex1[0], vertex2[1] - vertex1[1], vertex2[2] - vertex1[2]);
                var vector2 = new Vector3D(vertex3[0] - vertex2[0], vertex3[1] - vertex2[1], vertex3[2] - vertex2[2]);
                var vector3 = new Vector3D(vertex4[0] - vertex3[0], vertex4[1] - vertex3[1], vertex4[2] - vertex3[2]);

                // Area of a tetrahedron is determinant of three vectors that represent different edges
                area = Math.Abs(vector1.Dot(vector2.Cross(vector3))) / 6;

                // Add weighted centroid of this tetrahedron to others
                centroid.X += (vertex1[0] + vertex2[0] + vertex3[0] + vertex4[0]) / 4 * area;
                centroid.Y += (vertex1[1] + vertex2[1] + vertex3[1] + vertex4[1]) / 4 * area;
                centroid.Z += (vertex1[2] + vertex2[2] + vertex3[2] + vertex4[2]) / 4 * area;

                // Add area of this triangle
                areaSum += area;
            }

            centroid.X /= areaSum;
            centroid.Y /= areaSum;
            centroid.Z /= areaSum;

            // Save total area of this cell
            cell.Weight = areaSum;

            // Centroid of the polyhedron
            cell.Centroid = centroid;
        }

        /// <summary>
        /// Finds an intersection point of three edges.
        /// </summary>
        /// <param name="edge1">First edge.</param>
        /// <param name="edge2">Second edge.</param>
        /// <param name="edge3">Third edge.</param>
        /// <returns>The intersection point.</returns>
        private Vector3D FindIntersectionPoint(Edge3D edge1, Edge3D edge2, Edge3D edge3)
        {
            var w = -edge1.Normal.Dot(edge2.Normal.Cross(edge3.Normal));

            if (w != 0)
            {
                return new Vector3D(
                    // First coordinate
                    (edge1.Normal.Y * ((edge2.Normal.Z * edge3.C) - (edge2.C * edge3.Normal.Z))
                    - edge1.Normal.Z * ((edge2.Normal.Y * edge3.C) - (edge2.C * edge3.Normal.Y))
                    + edge1.C * ((edge2.Normal.Y * edge3.Normal.Z) - (edge2.Normal.Z * edge3.Normal.Y))) / w,
                    // Second coordinate
                    -(edge1.Normal.X * ((edge2.Normal.Z * edge3.C) - (edge2.C * edge3.Normal.Z))
                    - edge1.Normal.Z * ((edge2.Normal.X * edge3.C) - (edge2.C * edge3.Normal.X))
                    + edge1.C * ((edge2.Normal.X * edge3.Normal.Z) - (edge2.Normal.Z * edge3.Normal.X))) / w,
                    // Third coordinate
                    (edge1.Normal.X * ((edge2.Normal.Y * edge3.C) - (edge2.C * edge3.Normal.Y))
                    - edge1.Normal.Y * ((edge2.Normal.X * edge3.C) - (edge2.C * edge3.Normal.X))
                    + edge1.C * ((edge2.Normal.X * edge3.Normal.Y) - (edge2.Normal.Y * edge3.Normal.X))) / w
                );
            }
            else
            {
                return new Vector3D(double.NaN);
            }
        }

        private IEnumerable<DefaultTriangulationCell<MIVertex>> Triangulate(List<Vector3D> vertices)
        {
            IEnumerable<DefaultTriangulationCell<MIVertex>> triangles = null;

            // Convert vertices to objects that work with MIConvexHull library
            MIVertex[] miVertices = new MIVertex[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                miVertices[i] = new MIVertex(vertices[i].X, vertices[i].Y, vertices[i].Z);
            }

            if (vertices.Count == 4)
            {
                // MIConvexHull library cannot create a triangulation from a single tetrahedron
                triangles = new List<DefaultTriangulationCell<MIVertex>>() { new DefaultTriangulationCell<MIVertex>() { Vertices = miVertices } };
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
