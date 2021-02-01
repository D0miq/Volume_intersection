using System.Collections.Generic;
using System.Linq;
using VolumeIntersectionLibrary.DataStructures._2D;

namespace VolumeIntersectionLibrary.Intersection.HalfSpaceRemoval
{
    /// <summary>
    /// Brute force algorithm that removes half spaces and creates an intersection in 2D.
    /// </summary>
    public class BruteForceHalfSpaceRemoval2D : IHalfSpaceRemoval<Vector2D, Cell2D, Edge2D>
    {
        /// <summary>
        /// Removes redundant half spaces and creates a minimal intersection of two cells.
        /// </summary>
        /// <param name="triangleCell">Triangle cell.</param>
        /// <param name="voronoiCell">Voronoi cell.</param>
        /// <param name="intersectionPoints">Intersection points.</param>
        /// <returns>List of half spaces.</returns>
        public List<Edge2D> RemoveHalfSpaces(Cell2D triangleCell, Cell2D voronoiCell, out List<Vector2D> intersectionPoints)
        {
            // Dictionary that saves intersection points for each edge.
            var usedHalfSpaces = new Dictionary<Edge2D, HashSet<Vector2D>>();

            // Distinct points (multiple edges can intersect in the same point and it would be bad later during a triangulation of the intersection).
            var usedIntersectingPoints = new HashSet<Vector2D>(new Vector2DComparer());

            // Save edges to a single list
            var halfSpaces = new List<Edge2D>(triangleCell.Edges.Count + voronoiCell.Edges.Count);
            halfSpaces.AddRange(triangleCell.Edges);
            halfSpaces.AddRange(voronoiCell.Edges);

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
                    if (success && triangleCell.Contains(intersectionPoint) && voronoiCell.Contains(intersectionPoint))
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
    }
}
