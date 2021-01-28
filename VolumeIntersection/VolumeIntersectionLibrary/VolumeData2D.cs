using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VolumeIntersection
{
    /// <summary>
    /// 
    /// </summary>
    public class VolumeData2D : IVolumeData
    {
        /// <summary>
        /// Dimension of this data.
        /// </summary>
        public const int Dimension = 2; 

        /// <summary>
        /// Gets or sets a list of cells.
        /// </summary>
        public List<Cell2D> Cells { get; set; }

        /// <summary>
        /// Creates a copy of vertices.
        /// </summary>
        /// <typeparam name="TVector"></typeparam>
        /// <param name="vertices"></param>
        /// <returns></returns>
        private Vector2[] CopyVertices<TVector>(List<TVector> vertices) where TVector : IVector
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
        private Vector2 ComputeCentroid(int[] indices, Vector2[] vertices)
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TVector"></typeparam>
        /// <param name="generator"></param>
        /// <param name="cells"></param>
        /// <returns></returns>
        private Cell2D AddCellToDictionary<TVector>(TVector generator, Cell2D[] cells) where TVector : IIndexedVector
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TVector"></typeparam>
        /// <typeparam name="TCell"></typeparam>
        /// <param name="vertices"></param>
        /// <param name="cells"></param>
        public void FromTriangulation<TVector, TCell>(List<TVector> vertices, List<TCell> cells) where TVector : IVector where TCell : ITriangleCell
        {
            this.Cells = new List<Cell2D>(cells.Count);

            var edgeDictionary = new Dictionary<int[], Edge2D>(new EdgeComparer());

            var vertexArray = this.CopyVertices(vertices);

            for (var cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                var cell = cells[cellIndex];

                // Save indices of the cell
                var cellIndices = cell.Indices;

                // Compute centroid of the cell
                var centroid = this.ComputeCentroid(cellIndices, vertexArray);

                // Create tetrahedron cell
                var volumeCell = new Cell2D()
                {
                    Centroid = centroid
                };

                // Iterate over all faces of a cell and add them to volumetric data
                for (int i = 0; i < cellIndices.Length; i++)
                {
                    // Get a triangle face from the tetrahedron
                    int[] edgeIndices = new int[cellIndices.Length - 1];
                    for (int j = 0; j < edgeIndices.Length; j++)
                    {
                        edgeIndices[j] = cellIndices[(i + j) % cellIndices.Length];
                    }

                    var normal = new Vector2();

                    // Compute determinant 3x3
                    float order = vertexArray[edgeIndices[0]].X * (vertexArray[edgeIndices[1]].Y - centroid.Y) 
                        - vertexArray[edgeIndices[0]].Y * (vertexArray[edgeIndices[1]].X - centroid.X) 
                        + (vertexArray[edgeIndices[1]].X * centroid.Y - centroid.X * vertexArray[edgeIndices[1]].Y);

                    if (order < 0)
                    {
                        normal.X = -vertexArray[edgeIndices[1]].Y - vertexArray[edgeIndices[0]].Y;
                        normal.Y = vertexArray[edgeIndices[1]].X - vertexArray[edgeIndices[0]].X;
                    }
                    else
                    {
                        normal.X = -vertexArray[edgeIndices[0]].Y - vertexArray[edgeIndices[1]].Y;
                        normal.Y = vertexArray[edgeIndices[0]].X - vertexArray[edgeIndices[1]].X;
                    }

                    float c = Vector2.Dot(normal, vertexArray[edgeIndices[0]]);

                    // Create new edge
                    var edge = new Edge2D()
                    {
                        Normal = normal,
                        C = c,
                        Source = volumeCell
                    };

                    volumeCell.Edges.Add(edge);

                    Array.Sort(edgeIndices);

                    // If dictionary contained the face before add neighbors
                    if (edgeDictionary.TryGetValue(edgeIndices, out Edge2D neighborEdge))
                    {
                        edge.Target = neighborEdge.Source;
                        neighborEdge.Target = volumeCell;
                    }
                    else
                    {
                        edgeDictionary.Add(edgeIndices, edge);
                    }
                }

                this.Cells.Add(volumeCell);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TVector"></typeparam>
        /// <param name="generators"></param>
        public void FromVoronoi<TVector>(List<TVector> generators) where TVector : IIndexedVector
        {
            var cells = new Cell2D[generators.Count];

            // Create voronoi diagram from the generators.
            var voronoiMesh = VoronoiMesh.Create(generators);

            // Iteratate over all voronoi vertices.
            // They are represented as tetrahedra in the implementation of the voronoi diagram.
            // So each vertex of the tetrahedron is a generator of an adjecent voronoi cell.
            foreach (var tetrahedron in voronoiMesh.Vertices)
            {
                // Iterate over all adjecent generators.
                for (int i = 0; i < tetrahedron.Vertices.Length; i++)
                {
                    // Create a voronoi cell for the generator or find it if the generator has already been visited.
                    Cell2D sourceCell = this.AddCellToDictionary(tetrahedron.Vertices[i], cells);

                    // From the comments above follows that vertices of the tetrahedron and their corresponding voronoi cells are each other's neighbors.
                    // So iterate over neighbors of the current source cell.
                    for (int j = i + 1; j < tetrahedron.Vertices.Length; j++)
                    {
                        // Create a voronoi cell for the neighboring generator or find it if the generator has already been visited.
                        Cell2D targetCell = this.AddCellToDictionary(tetrahedron.Vertices[j], cells);

                        // Check if the target cell has already been added to the neighbors of the source cell.
                        // If it hasn't create a new edge separating them.
                        if (!sourceCell.Edges.Any(edge => edge.Target == targetCell))
                        {
                            // Get position of the source and target
                            // The source is the cell and the target its neigbor
                            var sourcePosition = tetrahedron.Vertices[i].Position;
                            var targetPosition = tetrahedron.Vertices[j].Position;

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
            }

            this.Cells = cells.ToList();
        }
    }
}
