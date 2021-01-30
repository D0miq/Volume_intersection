using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VolumeIntersection
{
    /// <summary>
    /// Volumetric data.
    /// </summary>
    /// <typeparam name="TVector">Vector type.</typeparam>
    /// <typeparam name="TCell">Cell type.</typeparam>
    /// <typeparam name="TEdge">Edge type.</typeparam>
    public abstract class VolumeData<TVector, TCell, TEdge> where TCell : Cell<TVector, TEdge>, new() where TEdge : Edge<TVector, TCell>
    {
        /// <summary>
        /// Gets or sets a list of cells.
        /// </summary>
        public List<TCell> Cells { get; set; }

        /// <summary>
        /// Creates a copy of vertices.
        /// </summary>
        /// <typeparam name="TInVertex">Type of input vertices.</typeparam>
        /// <param name="vertices">Vertices.</param>
        /// <returns>Vertices in internal format.</returns>
        protected abstract TVector[] CopyVertices<TInVertex>(List<TInVertex> vertices) where TInVertex : IVertex;

        /// <summary>
        /// Computes a centroid of a cell defined by vertex indices.
        /// </summary>
        /// <param name="indices">Vertex indices.</param>
        /// <param name="vertices">Vertices.</param>
        /// <returns>The centroid.</returns>
        protected abstract TVector ComputeCentroid(int[] indices, TVector[] vertices);

        /// <summary>
        /// Computes an edge of a triangulation.
        /// </summary>
        /// <param name="vertices">Vertices of the triangulation.</param>
        /// <param name="indices">Indices of an edge.</param>
        /// <param name="cell">Cell that should contain the edge.</param>
        /// <returns>Created edge.</returns>
        protected abstract TEdge ComputeTriangleEdge(TVector[] vertices, int[] indices, TCell cell);

        /// <summary>
        /// Adds a cell to the provided array.
        /// If the array already contains the cell this method returns it.
        /// </summary>
        /// <typeparam name="TInVector">Type of input vertices.</typeparam>
        /// <param name="generator">Generator of a voronoi cell.</param>
        /// <param name="cells">Cells dictionary.</param>
        /// <returns>The cell.</returns>
        protected abstract TCell AddCellToDictionary<TInVertex>(TInVertex generator, TCell[] cells) where TInVertex : IIndexedVertex;
        
        /// <summary>
        /// Computes an edge of a voronoi diagram.
        /// </summary>
        /// <param name="sourcePosition">Position of a generator of a source cell.</param>
        /// <param name="targetPosition">Position of a generator of a target cell.</param>
        /// <param name="sourceCell">Source cell.</param>
        /// <param name="targetCell">Target cel.</param>
        protected abstract void ComputeVoronoiEdge(double[] sourcePosition, double[] targetPosition, TCell sourceCell, TCell targetCell);

        /// <summary>
        /// Creates cells of this volumetric data from a triangulation.
        /// </summary>
        /// <typeparam name="TInVertex">Type of input vertices.</typeparam>
        /// <typeparam name="TTriangleCell">Type of a triangle cell.</typeparam>
        /// <param name="vertices">Vertices.</param>
        /// <param name="cells">Triangle cells.</param>
        public void FromTriangulation<TInVertex, TTriangleCell>(List<TInVertex> vertices, List<TTriangleCell> cells) where TInVertex : IVertex where TTriangleCell : ITriangleCell
        {
            this.Cells = new List<TCell>(cells.Count);

            var edgeDictionary = new Dictionary<int[], TEdge>(new EdgeComparer());

            // Create a copy of vertices in a case the implementation of TInVertex creates a copy of its coordinates. It should make it easier for garbage collector.
            var vertexArray = this.CopyVertices(vertices);

            for (var cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                var cell = cells[cellIndex];

                // Save indices of the cell, cell might create a copy when the get is called.
                var cellIndices = cell.Indices;

                // Compute centroid of the cell
                var centroid = this.ComputeCentroid(cellIndices, vertexArray);

                // Create tetrahedron cell
                var volumeCell = new TCell()
                {
                    Centroid = centroid,
                    TriangleIndex = cellIndex,
                    VoronoiIndex = -1
                };

                // Iterate over all faces of a triangle (tetrahedron) and add them to volumetric data
                for (int i = 0; i < cellIndices.Length; i++)
                {
                    // Get an edge or face from the triangle or tetrahedron
                    int[] edgeIndices = new int[cellIndices.Length - 1];
                    for (int j = 0; j < edgeIndices.Length; j++)
                    {
                        edgeIndices[j] = cellIndices[(i + j) % cellIndices.Length];
                    }

                    // Compute edge
                    var edge = this.ComputeTriangleEdge(vertexArray, edgeIndices, volumeCell);

                    // Sort indices of the edge. It is easier to compare them inside the dictionary.
                    Array.Sort(edgeIndices);

                    // If the dictionary already contains the face, add neighbors.
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
        /// Creates cells of this volumetric data froma voronoi generators.
        /// </summary>
        /// <typeparam name="TInVertex">Type of input vertices.</typeparam>
        /// <param name="generators">Voronoi generators.</param>
        public void FromVoronoi<TInVertex>(List<TInVertex> generators) where TInVertex : IIndexedVertex
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