using MIConvexHull;
using System;
using System.Collections.Generic;

namespace VolumeIntersection
{
    /// <summary>
    /// Generic class for intersections of volumetric data.
    /// </summary>
    /// <typeparam name="TVector">Vector type.</typeparam>
    /// <typeparam name="TCell">Cell type.</typeparam>
    /// <typeparam name="TEdge">Edge type.</typeparam>
    /// <typeparam name="TVolumeData">Data type.</typeparam>
    public abstract class VolumeIntersection<TVector, TCell, TEdge, TVolumeData> where TVolumeData : VolumeData<TVector, TCell, TEdge>, new() where TCell : Cell<TVector, TEdge>, new() where TEdge : Edge<TVector, TCell>
    {
        /// <summary>
        /// Pair of cells used for traversal.
        /// </summary>
        private class Pair
        {
            /// <summary>
            /// First cell.
            /// </summary>
            public TCell FirstCell;

            /// <summary>
            /// Second cell.
            /// </summary>
            public TCell SecondCell;

            /// <summary>
            /// Creates a new pair of cells.
            /// </summary>
            /// <param name="firstCell">First cell.</param>
            /// <param name="secondCell">Second cell.</param>
            public Pair(TCell firstCell, TCell secondCell)
            {
                this.FirstCell = firstCell;
                this.SecondCell = secondCell;
            }

            /// <summary>
            /// Compares this pair to another object..
            /// </summary>
            /// <param name="obj">Other object.</param>
            /// <returns>True - objects are equals, false otherwise.</returns>
            public override bool Equals(object obj)
            {
                return obj is Pair pair &&
                       FirstCell == pair.FirstCell && SecondCell == pair.SecondCell;
            }

            /// <summary>
            /// Gets hash code of this pair.
            /// </summary>
            /// <returns>The hash code.</returns>
            public override int GetHashCode()
            {
                int hashCode = 720972046;
                hashCode = hashCode * -1521134295 + EqualityComparer<TCell>.Default.GetHashCode(FirstCell);
                hashCode = hashCode * -1521134295 + EqualityComparer<TCell>.Default.GetHashCode(SecondCell);
                return hashCode;
            }
        }

        /// <summary>
        /// Data dimension.
        /// </summary>
        protected int Dimension;

        /// <summary>
        /// Removes redundant half spaces and creates a minimal intersection of two cells.
        /// </summary>
        /// <param name="c1">First cell.</param>
        /// <param name="c2">Second cell.</param>
        /// <param name="intersectionPoints">Intersection points.</param>
        /// <returns>List of half spaces.</returns>
        protected abstract List<TEdge> RemoveHalfSpaces(TCell c1, TCell c2, out List<TVector> intersectionPoints);

        /// <summary>
        /// Finds centroid of a cell.
        /// </summary>
        /// <param name="vertices">Vertices of the cell.</param>
        /// <param name="cell">The cell.</param>
        protected abstract void FindCentroid(List<TVector> vertices, TCell cell);

        /// <summary>
        /// Finds intersection between triangulation (tetrahedralization) and voronoi diagram.
        /// Method starts traversing from a random triangle (tetrahedron) and checks for different components of a triangulation (tetrahedralization).
        /// </summary>
        /// <typeparam name="TInVector">Type of an input vector.</typeparam>
        /// <typeparam name="TInCell">Type of a triangle (tetrahedron).</typeparam>
        /// <typeparam name="TIndexedVector">Type of an indexed vector used for voronoi generators.</typeparam>
        /// <param name="vertices">Vertices of a triangulation (tetrahedralization).</param>
        /// <param name="cells">Cells of a triangulation (tetrahedralization).</param>
        /// <param name="generators">Voronoi generators.</param>
        /// <returns>Volumetric data with intersections.</returns>
        public TVolumeData Intersect<TInVector, TInCell, TIndexedVector>(List<TInVector> vertices, List<TInCell> cells, List<TIndexedVector> generators) where TInVector : IVertex where TInCell : ITriangleCell where TIndexedVector : IIndexedVertex
        {
            // Test dimensions
            var triangleDimension = this.GetDimension(vertices);
            var voronoiDimension = this.GetDimension(generators);

            if (triangleDimension != this.Dimension || voronoiDimension != this.Dimension)
            {
                throw new ArgumentException("Triangulation vertices and voronoi generators must have the same dimension.");
            }

            // Create data from triangulation.
            var triangulationData = new TVolumeData();
            triangulationData.FromTriangulation(vertices, cells);

            // Create data from voronoi.
            var voronoiData = new TVolumeData();
            voronoiData.FromVoronoi(generators);

            var volumeData = new TVolumeData();

            // Check for each triangulation cell, if it has been visited. In case triangulation is separated to multiple components.
            for (int i = 0; i < triangulationData.Cells.Count; i++)
            {
                var triangulationCell = triangulationData.Cells[i];

                if (triangulationCell.Visited)
                {
                    continue;
                }

                // Find voronoi cell that contains centroid of the triangulation cell
                var voronoiCell = voronoiData.Cells.Find(cell => cell.Contains(triangulationCell.Centroid));

                // Find intersection between all traversable cells and their neighbors
                var intersections = FindIntersectingCells(triangulationCell, voronoiCell);

                volumeData.Cells.AddRange(intersections);
            }

            return volumeData;
        }

        /// <summary>
        /// Traverse data sets to find intersections between cells.
        /// </summary>
        /// <param name="triangleCell">First triangulation cell.</param>
        /// <param name="voronoiCellIndex">First voronoi cell.</param>
        /// <returns>List of intersections.</returns>        
        private List<TCell> FindIntersectingCells(TCell triangleCell, TCell voronoiCellIndex)
        { 
            var intersections = new List<TCell>();

            // Create a hash set and queue for visited pairs of cells
            var visitedPairs = new HashSet<Pair>();
            var cellQueue = new Queue<Pair>();

            // Start with provided cells
            var pair = new Pair(triangleCell, voronoiCellIndex);
            visitedPairs.Add(pair);
            cellQueue.Enqueue(pair);

            while (cellQueue.Count != 0)
            {
                // Withdraw a pair from the queue
                pair = cellQueue.Dequeue();

                var tempFirstCell = pair.FirstCell;
                var tempSecondCell = pair.SecondCell;

                // Set visited flag for both cells 
                tempFirstCell.Visited = true;
                tempSecondCell.Visited = true;

                // Remove unnecessary edges from cells to create an intersection
                List<TVector> intersectionPoints;
                var edges = RemoveHalfSpaces(tempFirstCell, tempSecondCell, out intersectionPoints);

                // Continue traversal over each edge
                foreach (var edge in edges)
                {
                    // If edge doesn't have a target there is nowhere to go
                    if(edge.Target != null)
                    {
                        if (edge.Source == tempFirstCell)
                        {
                            pair = new Pair(edge.Target, tempSecondCell);
                        }
                        else if (edge.Source == tempSecondCell)
                        {
                            pair = new Pair(tempFirstCell, edge.Target);
                        }

                        // Check if new pair was already visited
                        if (visitedPairs.Add(pair))
                        {
                            cellQueue.Enqueue(pair);
                        }
                    }
                }

                var cell = new TCell()
                {
                    Edges = edges,
                };

                // Set parents of the intersection
                cell.VoronoiIndex = tempFirstCell.VoronoiIndex == -1 ? tempSecondCell.VoronoiIndex : tempFirstCell.VoronoiIndex;
                cell.TriangleIndex = tempFirstCell.TriangleIndex == -1 ? tempSecondCell.TriangleIndex : tempFirstCell.TriangleIndex;

                try
                {
                    // Find centroid of the intersection
                    FindCentroid(intersectionPoints, cell);
                    intersections.Add(cell);
                } catch(ConvexHullGenerationException e)
                {
                    Console.WriteLine("Cannot compute a centroid. " + e.ErrorMessage);
                }
            }

            return intersections;
        }

        /// <summary>
        /// Computes dimension of provided vertices.
        /// It takes 10 random vertices and uses their dimensions.
        /// If min and max dimension don't match InvalidOperationException is thrown.
        /// </summary>
        /// <typeparam name="TInVector">Vexter type.</typeparam>
        /// <param name="vertices">Vertices.</param>
        /// <returns>Dimension.</returns>
        private int GetDimension<TInVector>(List<TInVector> vertices) where TInVector : IVertex
        {
            int minDimension = int.MaxValue;
            int maxDimension = int.MinValue;

            Random random = new Random();
            for (int i = 0; i < 10; i++)
            {
                int index = random.Next(vertices.Count);
                int dimension = vertices[index].Position.Length;

                if (minDimension > dimension) minDimension = dimension;
                if (maxDimension < dimension) maxDimension = dimension;
            }

            if (minDimension != maxDimension)
                throw new InvalidOperationException("Vertices contains different dimensions");

            return minDimension;
        }
    }
}