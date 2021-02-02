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
            if(vertices.Count == 3)
            {
                // Convert vertices to objects that work with MIConvexHull library
                MIVertex[] miVertices = new MIVertex[vertices.Count];
                for (int i = 0; i < vertices.Count; i++)
                {
                    miVertices[i] = new MIVertex(vertices[i].X, vertices[i].Y);
                }

                // MIConvexHull library cannot create a triangulation from a single triangle
                return new List<DefaultTriangulationCell<MIVertex>>() { new DefaultTriangulationCell<MIVertex>() { Vertices = miVertices } };
            }
            else if (vertices.Count == 4)
            {
                // There are problems with 4 vertices in MIConvexHull algorithms. There are multiple issues on their github

                Vector2D average = new Vector2D();

                for (int i = 0; i < vertices.Count; i++)
                {
                    average.X += vertices[i].X;
                    average.Y += vertices[i].Y;
                }

                average.X /= vertices.Count;
                average.Y /= vertices.Count;

                // Convert vertices to objects that work with MIConvexHull library
                List<MIVertex> miVertices = new List<MIVertex>(vertices.Count);
                for (int i = 0; i < vertices.Count; i++)
                {
                    miVertices.Add(new MIVertex(vertices[i].X, vertices[i].Y));
                }

                miVertices.Sort(Comparer<MIVertex>.Create((item1, item2) => {
                    var position1 = item1.Position;
                    var position2 = item2.Position;

                    double a1 = ((Math.Atan2(position1[0] - average.X, position1[1] - average.Y) * 180 / Math.PI) + 360) % 360;
                    double a2 = ((Math.Atan2(position2[0] - average.X, position2[1] - average.Y) * 180 / Math.PI) + 360) % 360;
                    return (int)(a1 - a2);
                }));

                var triangles = new List<DefaultTriangulationCell<MIVertex>>();

                for (int i = 1; i < miVertices.Count - 1; i++)
                {
                    MIVertex[] tempVertices = { miVertices[0], miVertices[i], miVertices[i + 1] };
                    triangles.Add(new DefaultTriangulationCell<MIVertex>() { Vertices = tempVertices });
                }

                return triangles;
            }
            else
            {
                // Convert vertices to objects that work with MIConvexHull library
                MIVertex[] miVertices = new MIVertex[vertices.Count];
                for (int i = 0; i < vertices.Count; i++)
                {
                    miVertices[i] = new MIVertex(vertices[i].X, vertices[i].Y);
                }

                // More vertices are OK so let the library to triangulate them
                var triangulation = Triangulation.CreateDelaunay(miVertices);
                return triangulation.Cells;
            }
        }
    }
}
