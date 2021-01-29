using System.Collections.Generic;
using System.Numerics;

namespace VolumeIntersection
{
    /// <summary>
    /// Three dimensional volumetric data.
    /// </summary>
    public class VolumeData3D : VolumeData<Vector3, Cell3D, Edge3D>
    {
        /// <summary>
        /// Dimension of this data.
        /// </summary>
        public const int Dimension = 3;

        public VolumeData3D()
        {
            base.Cells = new List<Cell3D>();
        }

        /// <summary>
        /// Creates a copy of vertices.
        /// </summary>
        /// <typeparam name="TInVertex">Type of input vertices.</typeparam>
        /// <param name="vertices">Vertices.</param>
        /// <returns>Vertices in internal format.</returns>
        protected override Vector3[] CopyVertices<TVector>(List<TVector> vertices)
        {
            // Save vertices to an array 
            var vertexArray = new Vector3[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                var position = vertices[i].Position;
                vertexArray[i] = new Vector3((float)position[0], (float)position[1], (float)position[2]);
            }

            return vertexArray;
        }

        /// <summary>
        /// Computes a centroid of a cell defined by vertex indices.
        /// </summary>
        /// <param name="indices">Vertex indices.</param>
        /// <param name="vertices">Vertices.</param>
        /// <returns>The centroid.</returns>
        protected override Vector3 ComputeCentroid(int[] indices, Vector3[] vertices)
        {
            // Compute centroid of the cell
            var centroid = new Vector3();
            for (int i = 0; i < indices.Length; i++)
            {
                centroid.X += vertices[indices[i]].X;
                centroid.Y += vertices[indices[i]].Y;
                centroid.Z += vertices[indices[i]].Z;
            }

            centroid.X /= indices.Length;
            centroid.Y /= indices.Length;
            centroid.Z /= indices.Length;

            return centroid;
        }

        /// <summary>
        /// Computes an edge of a triangulation.
        /// </summary>
        /// <param name="vertices">Vertices of the triangulation.</param>
        /// <param name="indices">Indices of an edge.</param>
        /// <param name="cell">Cell that should contain the edge.</param>
        /// <returns>Created edge.</returns>
        protected override Edge3D ComputeTriangleEdge(Vector3[] vertices, int[] indices, Cell3D cell)
        {
            var v1 = vertices[indices[0]] - vertices[indices[1]];
            var v2 = vertices[indices[2]] - vertices[indices[1]];

            var normal = Vector3.Cross(v1, v2);

            float c = -Vector3.Dot(normal, vertices[indices[0]]);

            if (Vector3.Dot(normal, cell.Centroid) + c < 0)
            {
                normal = -normal;
                c = -c;
            }

            // Create new edge
            var edge = new Edge3D()
            {
                Normal = normal,
                C = c,
                Source = cell
            };

            cell.Edges.Add(edge);

            return edge;
        }

        /// <summary>
        /// Adds a cell to the provided array.
        /// If the array already contains the cell this method returns it.
        /// </summary>
        /// <typeparam name="TInVector">Type of input vertices.</typeparam>
        /// <param name="generator">Generator of a voronoi cell.</param>
        /// <param name="cells">Cells dictionary.</param>
        /// <returns>The cell.</returns>
        protected override Cell3D AddCellToDictionary<TVector>(TVector generator, Cell3D[] cells)
        {
            Cell3D cell = cells[generator.Index];
            if (cell == null)
            {
                var position = generator.Position;

                cell = new Cell3D()
                {
                    Centroid = new Vector3((float)position[0], (float)position[1], (float)position[2]),
                    VoronoiIndex = generator.Index,
                    TriangleIndex = -1
                };

                cells[generator.Index] = cell;
            }

            return cell;
        }

        /// <summary>
        /// Computes an edge of a voronoi diagram.
        /// </summary>
        /// <param name="sourcePosition">Position of a generator of a source cell.</param>
        /// <param name="targetPosition">Position of a generator of a target cell.</param>
        /// <param name="sourceCell">Source cell.</param>
        /// <param name="targetCell">Target cel.</param>
        protected override void ComputeVoronoiEdge(double[] sourcePosition, double[] targetPosition, Cell3D sourceCell, Cell3D targetCell)
        {
            // Compute a vector that points towards the source position
            var normal = new Vector3((float)(sourcePosition[0] - targetPosition[0]), (float)(sourcePosition[1] - targetPosition[1]), (float)(sourcePosition[2] - targetPosition[2]));

            // Compute a point in the middle between the two positions
            var middlePosition = new Vector3((float)(sourcePosition[0] + targetPosition[0]) / 2, (float)(sourcePosition[1] + targetPosition[1]) / 2, (float)(sourcePosition[2] + targetPosition[2]) / 2);

            // Compute last element of a half space standard form that separates the source and target positions and goes through the middle position
            float c = -Vector3.Dot(normal, middlePosition);

            // Add the neighbor
            sourceCell.Edges.Add(new Edge3D()
            {
                Normal = normal,
                C = c,
                Source = sourceCell,
                Target = targetCell
            });

            // Add the neighbor
            targetCell.Edges.Add(new Edge3D()
            {
                Normal = -normal,
                C = -c,
                Source = targetCell,
                Target = sourceCell
            });
        }
    }
}
