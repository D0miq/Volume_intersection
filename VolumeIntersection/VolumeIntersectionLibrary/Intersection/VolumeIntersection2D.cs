using MIConvexHull;
using System;
using System.Collections.Generic;
using VolumeIntersectionLibrary.DataStructures._2D;
using VolumeIntersectionLibrary.Intersection.HalfSpaceRemoval;

namespace VolumeIntersectionLibrary.Intersection
{
    /// <summary>
    /// Class that computes intersections between two dimensional triangulation and voronoi diagram.
    /// </summary>
    public class VolumeIntersection2D : VolumeIntersection<Vector2D, Cell2D, Edge2D, VolumeData2D>
    {
        /// <summary>
        /// Creates a new instance with a specific half space removal algorithm.
        /// </summary>
        /// <param name="halfSpaceRemoval">Half space removal algorithm.</param>
        public VolumeIntersection2D(IHalfSpaceRemoval<Vector2D, Cell2D, Edge2D> halfSpaceRemoval) : base(halfSpaceRemoval)
        {
            base.Dimension = 2;
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
