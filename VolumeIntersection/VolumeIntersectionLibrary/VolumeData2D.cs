using System.Collections.Generic;

namespace VolumeIntersection
{
    /// <summary>
    /// Two dimensional volumetric data.
    /// </summary>
    public class VolumeData2D : VolumeData<Vector2D, Cell2D, Edge2D>
    {
        /// <summary>
        /// Dimension of this data.
        /// </summary>
        public const int Dimension = 2;

        /// <summary>
        /// Creates a new two dimensional volumetric data.
        /// </summary>
        public VolumeData2D()
        {
            base.Cells = new List<Cell2D>();
        }

        /// <summary>
        /// Creates a copy of vertices.
        /// </summary>
        /// <typeparam name="TInVertex">Type of input vertices.</typeparam>
        /// <param name="vertices">Vertices.</param>
        /// <returns>Vertices in internal format.</returns>
        protected override Vector2D[] CopyVertices<TVector>(List<TVector> vertices)
        {
            // Save vertices to an array 
            var vertexArray = new Vector2D[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                var position = vertices[i].Position;
                vertexArray[i] = new Vector2D(position[0], position[1]);
            }

            return vertexArray;
        }

        /// <summary>
        /// Computes a centroid of a cell defined by vertex indices.
        /// </summary>
        /// <param name="indices">Vertex indices.</param>
        /// <param name="vertices">Vertices.</param>
        /// <returns>The centroid.</returns>
        protected override Vector2D ComputeCentroid(int[] indices, Vector2D[] vertices)
        {
            // Compute centroid of the cell
            var centroid = new Vector2D();
            for (int i = 0; i < indices.Length; i++)
            {
                centroid.X += vertices[indices[i]].X;
                centroid.Y += vertices[indices[i]].Y;
            }

            centroid.X /= indices.Length;
            centroid.Y /= indices.Length;

            return centroid;
        }

        /// <summary>
        /// Computes an edge of a triangulation.
        /// </summary>
        /// <param name="vertices">Vertices of the triangulation.</param>
        /// <param name="indices">Indices of an edge.</param>
        /// <param name="cell">Cell that should contain the edge.</param>
        /// <returns>Created edge.</returns>
        protected override Edge2D ComputeTriangleEdge(Vector2D[] vertices, int[] indices, Cell2D cell)
        {
            var normal = new Vector2D(vertices[indices[1]].Y - vertices[indices[0]].Y, -(vertices[indices[1]].X - vertices[indices[0]].X));
            double c = -normal.Dot(vertices[indices[0]]);

            // Check winding order of the edge and compute normal accordingly.
            if (normal.Dot(cell.Centroid) + c < 0)
            {
                normal = -normal;
                c = -c;
            }

            // Create new edge
            var edge = new Edge2D()
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
        protected override Cell2D AddCellToDictionary<TVector>(TVector generator, Cell2D[] cells)
        {
            Cell2D cell = cells[generator.Index];
            if (cell == null)
            {
                var position = generator.Position;

                cell = new Cell2D()
                {
                    Centroid = new Vector2D(position[0], position[1]),
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
        protected override void ComputeVoronoiEdge(double[] sourcePosition, double[] targetPosition, Cell2D sourceCell, Cell2D targetCell)
        {
            // Compute a vector that points towards the source position
            var normal = new Vector2D(sourcePosition[0] - targetPosition[0], sourcePosition[1] - targetPosition[1]);

            // Compute a point in the middle between the two positions
            var middlePosition = new Vector2D((sourcePosition[0] + targetPosition[0]) / 2, (sourcePosition[1] + targetPosition[1]) / 2);

            // Compute last element of a half space standard form that separates the source and target positions and goes through the middle position
            double c = -normal.Dot(middlePosition);

            // Add the neighbor
            sourceCell.Edges.Add(new Edge2D()
            {
                Normal = normal,
                C = c,
                Source = sourceCell,
                Target = targetCell
            });

            // Add the neighbor
            targetCell.Edges.Add(new Edge2D()
            {
                Normal = -normal,
                C = -c,
                Source = targetCell,
                Target = sourceCell
            });
        }
    }
}
