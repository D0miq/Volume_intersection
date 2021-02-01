using MIConvexHull;
using System;
using System.Collections.Generic;
using VolumeIntersectionLibrary.DataStructures._3D;
using VolumeIntersectionLibrary.Intersection.HalfSpaceRemoval;

namespace VolumeIntersectionLibrary.Intersection
{
    /// <summary>
    /// Class that computes intersections between three dimensional triangulation and voronoi diagram.
    /// </summary>
    public class VolumeIntersection3D : VolumeIntersection<Vector3D, Cell3D, Edge3D, VolumeData3D>
    {
        /// <summary>
        /// Creates a new instance with a specific half space removal algorithm.
        /// </summary>
        /// <param name="halfSpaceRemoval">Half space removal algorithm.</param>
        public VolumeIntersection3D(IHalfSpaceRemoval<Vector3D, Cell3D, Edge3D> halfSpaceRemoval) : base(halfSpaceRemoval)
        {
            base.Dimension = 3;
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
        /// Triangulate cell with the provided vertices.
        /// </summary>
        /// <param name="vertices">The vertices of the cell.</param>
        /// <returns>The triangulation.</returns>
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
