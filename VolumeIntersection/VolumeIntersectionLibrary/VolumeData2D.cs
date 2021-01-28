using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VolumeIntersection
{
    /// <summary>
    /// 
    /// </summary>
    public class VolumeData2D : VolumeData<Vector2, Cell2D, Edge2D>
    {
        /// <summary>
        /// Dimension of this data.
        /// </summary>
        public const int Dimension = 2; 

        /// <summary>
        /// Creates a copy of vertices.
        /// </summary>
        /// <typeparam name="TVector"></typeparam>
        /// <param name="vertices"></param>
        /// <returns></returns>
        protected override Vector2[] CopyVertices<TVector>(List<TVector> vertices)
        {
            // Save vertices to an array 
            var vertexArray = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                var position = vertices[i].Position;
                vertexArray[i] = new Vector2((float)position[0], (float)position[1]);
            }

            return vertexArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        protected override Vector2 ComputeCentroid(int[] indices, Vector2[] vertices)
        {
            // Compute centroid of the cell
            var centroid = new Vector2();
            for (int i = 0; i < indices.Length; i++)
            {
                centroid.X += vertices[indices[i]].X;
                centroid.Y += vertices[indices[i]].Y;
            }

            centroid.X /= indices.Length;
            centroid.Y /= indices.Length;

            return centroid;
        }

        protected override Edge2D ComputeTriangleEdge(Vector2[] vertices, int[] indices, Cell2D cell)
        {
            var normal = new Vector2();

            // Compute determinant 3x3
            float order = vertices[indices[0]].X * (vertices[indices[1]].Y - cell.Centroid.Y)
                - vertices[indices[0]].Y * (vertices[indices[1]].X - cell.Centroid.X)
                + (vertices[indices[1]].X * cell.Centroid.Y - cell.Centroid.X * vertices[indices[1]].Y);

            if (order < 0)
            {
                normal.X = -vertices[indices[1]].Y - vertices[indices[0]].Y;
                normal.Y = vertices[indices[1]].X - vertices[indices[0]].X;
            }
            else
            {
                normal.X = -vertices[indices[0]].Y - vertices[indices[1]].Y;
                normal.Y = vertices[indices[0]].X - vertices[indices[1]].X;
            }

            float c = Vector2.Dot(normal, vertices[indices[0]]);

            var edge = new Edge2D()
            {
                Normal = normal,
                C = c,
                Source = cell
            };

            // Create new edge
            cell.Edges.Add(edge);

            return edge;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TVector"></typeparam>
        /// <param name="generator"></param>
        /// <param name="cells"></param>
        /// <returns></returns>
        protected override Cell2D AddCellToDictionary<TVector>(TVector generator, Cell2D[] cells)
        {
            Cell2D cell = cells[generator.Index];
            if (cell == null)
            {
                var position = generator.Position;

                cells[generator.Index] = new Cell2D()
                {
                    Centroid = new Vector2((float)position[0], (float)position[1])
                };
            }

            return cell;
        }

        protected override void ComputeVoronoiEdge(double[] sourcePosition, double[] targetPosition, Cell2D sourceCell, Cell2D targetCell)
        {
            // Compute a vector that points towards the source position
            var normal = new Vector2((float)(sourcePosition[0] - targetPosition[0]), (float)(sourcePosition[1] - targetPosition[1]));

            // Compute a point in the middle between the two positions
            var middlePosition = new Vector2((float)(sourcePosition[0] + targetPosition[0]) / 2, (float)(sourcePosition[1] + targetPosition[1]) / 2);

            // Compute last element of a half space standard form that separates the source and target positions and goes through the middle position
            float c = Vector2.Dot(normal, middlePosition);

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
