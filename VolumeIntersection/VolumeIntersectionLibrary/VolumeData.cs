using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VolumeIntersection
{
    /// <summary>
    /// Volumetric data.
    /// </summary>
    /// <typeparam name="TVector">Vector type</typeparam>
    public abstract class VolumeData<TVector, TCell, TEdge> where TCell : Cell<TVector, TEdge>, new() where TEdge : Edge<TVector, TCell>
    {
        /// <summary>
        /// Gets or sets a list of cells.
        /// </summary>
        public List<TCell> Cells { get; set; }

        /// <summary>
        /// Creates a copy of vertices.
        /// </summary>
        /// <typeparam name="TVector"></typeparam>
        /// <param name="vertices"></param>
        /// <returns></returns>
        protected abstract TVector[] CopyVertices<TInVector>(List<TInVector> vertices) where TInVector : IVector;

        protected abstract TVector ComputeCentroid(int[] indices, TVector[] vertices);

        protected abstract TEdge ComputeTriangleEdge(TVector[] vertices, int[] indices, TCell cell);

        protected abstract TCell AddCellToDictionary<TInVector>(TInVector generator, TCell[] cells) where TInVector : IIndexedVector;

        protected abstract void ComputeVoronoiEdge(double[] sourcePosition, double[] targetPosition, TCell sourceCell, TCell targetCell);

        public void FromTriangulation<TInVector, TTriangleCell>(List<TInVector> vertices, List<TTriangleCell> cells) where TInVector : IVector where TTriangleCell : ITriangleCell
        {
            this.Cells = new List<TCell>(cells.Count);

            var edgeDictionary = new Dictionary<int[], TEdge>(new EdgeComparer());

            var vertexArray = this.CopyVertices(vertices);

            for (var cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                var cell = cells[cellIndex];

                // Save indices of the cell
                var cellIndices = cell.Indices;

                // Compute centroid of the cell
                var centroid = this.ComputeCentroid(cellIndices, vertexArray);

                // Create tetrahedron cell
                var volumeCell = new TCell()
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

                    var edge = this.ComputeTriangleEdge(vertexArray, edgeIndices, volumeCell);

                    Array.Sort(edgeIndices);

                    // If dictionary contained the face before add neighbors
                    if (edgeDictionary.TryGetValue(edgeIndices, out TEdge neighborEdge))
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
        public void FromVoronoi<TInVector>(List<TInVector> generators) where TInVector : IIndexedVector
        {
            var cells = new TCell[generators.Count];

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
                    TCell sourceCell = this.AddCellToDictionary(tetrahedron.Vertices[i], cells);

                    // From the comments above follows that vertices of the tetrahedron and their corresponding voronoi cells are each other's neighbors.
                    // So iterate over neighbors of the current source cell.
                    for (int j = i + 1; j < tetrahedron.Vertices.Length; j++)
                    {
                        // Create a voronoi cell for the neighboring generator or find it if the generator has already been visited.
                        TCell targetCell = this.AddCellToDictionary(tetrahedron.Vertices[j], cells);

                        // Check if the target cell has already been added to the neighbors of the source cell.
                        // If it hasn't create a new edge separating them.
                        if (!sourceCell.Edges.Any(edge => edge.Target == targetCell))
                        {
                            // Get position of the source and target
                            // The source is the cell and the target its neigbor
                            var sourcePosition = tetrahedron.Vertices[i].Position;
                            var targetPosition = tetrahedron.Vertices[j].Position;

                            this.ComputeVoronoiEdge(sourcePosition, targetPosition, sourceCell, targetCell);
                        }
                    }
                }
            }

            this.Cells = cells.ToList();
        }
    }
}