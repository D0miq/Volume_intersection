using System.Collections.Generic;
using System.Linq;
using VolumeIntersectionLibrary.DataStructures._3D;

namespace VolumeIntersectionLibrary.Intersection.HalfSpaceRemoval
{
    /// <summary>
    /// Brute force algorithm that removes half spaces and creates an intersection in 3D.
    /// </summary>
    public class BruteForceHalfSpaceRemoval3D : IHalfSpaceRemoval<Vector3D, Cell3D, Edge3D>
    {
        /// <summary>
        /// Removes redundant half spaces and creates a minimal intersection of two cells.
        /// </summary>
        /// <param name="triangleCell">Triangle cell.</param>
        /// <param name="voronoiCell">Voronoi cell.</param>
        /// <param name="intersectionPoints">Intersection points.</param>
        /// <returns>List of half spaces.</returns>
        public List<Edge3D> RemoveHalfSpaces(Cell3D triangleCell, Cell3D voronoiCell, out List<Vector3D> intersectionPoints)
        {
            // Dictionary that saves intersection points for each edge.
            var usedHalfSpaces = new Dictionary<Edge3D, HashSet<Vector3D>>();

            // Distinct points (multiple edges can intersect in the same point and it would be bad later during a triangulation of the intersection).
            var usedIntersectingPoints = new HashSet<Vector3D>(new Vector3DComparer());

            // Save edges to a single list
            var halfSpaces = new List<Edge3D>(triangleCell.Edges.Count + voronoiCell.Edges.Count);
            halfSpaces.AddRange(triangleCell.Edges);
            halfSpaces.AddRange(voronoiCell.Edges);

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
                        if (success && triangleCell.Contains(intersectionPoint) && voronoiCell.Contains(intersectionPoint))
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
    }
}
