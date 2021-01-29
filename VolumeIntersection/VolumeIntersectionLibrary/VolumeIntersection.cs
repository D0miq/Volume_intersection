﻿using MIConvexHull;
using System;
using System.Collections.Generic;

namespace VolumeIntersection
{
    public abstract class VolumeIntersection<TVector, TCell, TEdge, TVolumeData> where TVolumeData : VolumeData<TVector, TCell, TEdge>, new() where TCell : Cell<TVector, TEdge>, new() where TEdge : Edge<TVector, TCell>
    {
        private class Pair
        {
            public TCell FirstCell;
            public TCell SecondCell;

            public Pair(TCell firstCell, TCell secondCell)
            {
                this.FirstCell = firstCell;
                this.SecondCell = secondCell;
            }

            public override bool Equals(object obj)
            {
                return obj is Pair pair &&
                       FirstCell == pair.FirstCell && SecondCell == pair.SecondCell;
            }

            public override int GetHashCode()
            {
                int hashCode = 720972046;
                hashCode = hashCode * -1521134295 + EqualityComparer<TCell>.Default.GetHashCode(FirstCell);
                hashCode = hashCode * -1521134295 + EqualityComparer<TCell>.Default.GetHashCode(SecondCell);
                return hashCode;
            }
        }

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

            var triangulationData = new TVolumeData();
            triangulationData.FromTriangulation(vertices, cells);

            var voronoiData = new TVolumeData();
            voronoiData.FromVoronoi(generators);

            var volumeData = new TVolumeData();

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
        /// Finds intersection between two generic volumetric data.
        /// Methods starts traversing on a random point and checks if it is contained inside volumetric data.
        /// Method ignores multiple components.
        /// </summary>
        /// <typeparam name="TVector">Type of a used vector.</typeparam>
        /// <param name="volumeData1">First volumetric data.</param>
        /// <param name="volumeData2">Second volumetric data.</param>
        /// <returns>Volumetric data with intersections.</returns>
        //public VolumeData<TVector> Intersect(VolumeData<TVector> volumeData1, VolumeData<TVector> volumeData2)
        //{
        //    Random rd = new Random();
        //    int firstIndex, secondIndex;

        //    // Generate random point inside volume data and find cells that contain it
        //    do
        //    {
        //        var start = new TVector() { Position = new double[] { } };

        //        firstIndex = volumeData1.Cells.FindIndex(cell => cell.Contains(start));
        //        secondIndex = volumeData2.Cells.FindIndex(cell => cell.Contains(start));
        //    } while (firstIndex == -1 || secondIndex == -1);

        //    var intersections = FindIntersectingCells(volumeData1.Cells[firstIndex], volumeData2.Cells[secondIndex]);

        //    return new VolumeData<TVector>()
        //    {
        //        Cells = intersections
        //    };
        //}

        private List<TCell> FindIntersectingCells(TCell triangleCell, TCell voronoiCellIndex)
        { 
            var intersections = new List<TCell>();

            // Create a queue
            var visitedPairs = new HashSet<Pair>();
            var cellQueue = new Queue<Pair>();

            var pair = new Pair(triangleCell, voronoiCellIndex);
            visitedPairs.Add(pair);
            cellQueue.Enqueue(pair);

            while (cellQueue.Count != 0)
            {
                pair = cellQueue.Dequeue();

                var tempFirstCell = pair.FirstCell;
                var tempSecondCell = pair.SecondCell;

                tempFirstCell.Visited = true;
                tempSecondCell.Visited = true;

                List<TVector> intersectionPoints;
                var edges = RemoveHalfSpaces(tempFirstCell, tempSecondCell, out intersectionPoints);

                foreach (var edge in edges)
                {
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

                cell.VoronoiIndex = tempFirstCell.VoronoiIndex == -1 ? tempSecondCell.VoronoiIndex : tempFirstCell.VoronoiIndex;
                cell.TriangleIndex = tempFirstCell.TriangleIndex == -1 ? tempSecondCell.TriangleIndex : tempFirstCell.TriangleIndex;

                try
                {
                    FindCentroid(intersectionPoints, cell);
                    intersections.Add(cell);
                } catch(ConvexHullGenerationException e)
                {
                    Console.WriteLine("Cannot compute centroid. " + e.Message);
                }
            }

            return intersections;
        }

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