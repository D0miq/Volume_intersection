using System;
using System.Collections.Generic;
using System.Linq;

namespace VolumeIntersection
{
    public static class VolumeIntersection
    {
        private class Pair<TVector> where TVector : IVector
        {
            public Cell<TVector> FirstCell;
            public Cell<TVector> SecondCell;

            public Pair(Cell<TVector> firstCell, Cell<TVector> secondCell)
            {
                this.FirstCell = firstCell;
                this.SecondCell = secondCell;
            }
        }

        /// <summary>
        /// Finds intersection between triangulation (tetrahedralization) and voronoi diagram.
        /// Method starts traversing from a random triangle (tetrahedron) and checks for different components of a triangulation (tetrahedralization).
        /// </summary>
        /// <typeparam name="TVector">Type of a used vector.</typeparam>
        /// <typeparam name="TCell">Type of a used triangle (tetrahedron).</typeparam>
        /// <param name="vertices">Vertices of a triangulation (tetrahedralization).</param>
        /// <param name="cells">Cells of a triangulation (tetrahedralization).</param>
        /// <param name="generators">Voronoi generators.</param>
        /// <returns>Volumetric data with intersections.</returns>
        public static VolumeData<TVector> Intersect<TVector, TCell>(List<TVector> vertices, List<TCell> cells, List<TVector> generators) where TVector : IVector, new() where TCell : ITriangleCell
        {
            var triangulationVolumeData = VolumeData<TVector>.FromTriangulation(vertices, cells);
            var voronoiVolumeData = VolumeData<TVector>.FromVoronoi(generators);

            var volumeData = new VolumeData<TVector>()
            {
                Cells = new List<Cell<TVector>>()
            };

            for (int i = 0; i < triangulationVolumeData.Cells.Count; i++)
            {
                var triangulationCell = triangulationVolumeData.Cells[i];

                if (triangulationCell.Visited)
                {
                    continue;
                }

                var voronoiCell = voronoiVolumeData.Cells.Find(cell => cell.Contains(triangulationCell.Centroid));

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
        public static VolumeData<TVector> Intersect<TVector>(VolumeData<TVector> volumeData1, VolumeData<TVector> volumeData2) where TVector : IVector, new()
        {
            Random rd = new Random();
            int firstIndex, secondIndex;

            // Generate random point inside volume data and find cells that contain it
            do
            {
                var start = new TVector() { Position = new double[] { } };

                firstIndex = volumeData1.Cells.FindIndex(cell => cell.Contains(start));
                secondIndex = volumeData2.Cells.FindIndex(cell => cell.Contains(start));
            } while (firstIndex == -1 || secondIndex == -1);
            
            var intersections = FindIntersectingCells(volumeData1.Cells[firstIndex], volumeData2.Cells[secondIndex]);

            return new VolumeData<TVector>()
            {
                Cells = intersections
            };
        }

        private static List<Cell<TVector>> FindIntersectingCells<TVector>(Cell<TVector> firstCell, Cell<TVector> secondCell) where TVector : IVector, new()
        {
            var intersections = new List<Cell<TVector>>();

            // Create a queue
            var visitedPairs = new HashSet<Pair<TVector>>();
            var cellQueue = new Queue<Pair<TVector>>();

            var pair = new Pair<TVector>(firstCell, secondCell);
            visitedPairs.Add(pair);
            cellQueue.Enqueue(pair);

            while (cellQueue.Count != 0)
            {
                firstCell.Visited = true;
                secondCell.Visited = true;

                var edges = RemoveHalfSpaces(firstCell, secondCell);

                foreach (var edge in edges)
                {
                    if (edge.Source == firstCell)
                    {
                        pair = new Pair<TVector>(edge.Target, secondCell);
                    }
                    else
                    {
                        pair = new Pair<TVector>(firstCell, edge.Target);
                    }

                    if (visitedPairs.Add(pair))
                    {
                        cellQueue.Enqueue(pair);
                    }
                }

                var cell = new Cell<TVector>()
                {
                    Edges = edges,
                    //Centroid = FindCentroid(edges)
                };

                intersections.Add(cell);

                pair = cellQueue.Dequeue();

                firstCell = pair.FirstCell;
                secondCell = pair.SecondCell;
            }

            return intersections;
        }

        private static TVector FindCentroid<TVector>(List<Edge<TVector>> edges) where TVector : IVector
        {
            double areaSum = 0;

            for (int i = 0; i < edges.Count; i++)
            {

            }
        }

        private static List<Edge<TVector>> RemoveHalfSpaces<TVector>(Cell<TVector> c1, Cell<TVector> c2) where TVector : IVector, new()
        {
            var usedHalfSpaces = new HashSet<Edge<TVector>>();
            var halfSpaces = new List<Edge<TVector>>(c1.Edges.Count + c2.Edges.Count);
            halfSpaces.AddRange(c1.Edges);
            halfSpaces.AddRange(c2.Edges);

            var dimension = c1.Centroid.Position.Length;

            if(dimension == 2)
            {
                for (int i = 0; i < halfSpaces.Count; i++)
                {
                    for (int j = i + 1; j < halfSpaces.Count; j++)
                    {
                        var intersectionPoint = FindIntersectionPoint(new List<Edge<TVector>>() { halfSpaces[i], halfSpaces[j] });

                        if (c1.Contains(intersectionPoint) && c2.Contains(intersectionPoint))
                        {
                            usedHalfSpaces.Add(halfSpaces[i]);
                            usedHalfSpaces.Add(halfSpaces[j]);
                        }
                    }
                }
            } else if(dimension == 3)
            {
                for (int i = 0; i < halfSpaces.Count; i++)
                {
                    for (int j = i + 1; j < halfSpaces.Count; j++)
                    {
                        for (int k = j + 1; k < halfSpaces.Count; k++)
                        {
                            var intersectionPoint = FindIntersectionPoint(new List<Edge<TVector>>() { halfSpaces[i], halfSpaces[j], halfSpaces[k] });

                            if (c1.Contains(intersectionPoint) && c2.Contains(intersectionPoint))
                            {
                                usedHalfSpaces.Add(halfSpaces[i]);
                                usedHalfSpaces.Add(halfSpaces[j]);
                                usedHalfSpaces.Add(halfSpaces[k]);
                            }
                        }
                    }
                }
            }

            return usedHalfSpaces.ToList();
        }

        private static TVector FindIntersectionPoint<TVector>(List<Edge<TVector>> edges) where TVector : IVector, new()
        {
            int normalDimension = edges[0].Normal.Position.Length;

            double[][] vectors = new double[edges.Count][];

            for(int i = 0; i < edges.Count; i++)
            {
                vectors[i] = new double[normalDimension + 1];
                var normal = edges[i].Normal.Position;

                for(int j = 0; j < normalDimension; j++)
                {
                    vectors[i][j] = normal[j];
                }

                vectors[i][normalDimension] = edges[i].C;
            }

            double[] x = MathUtils.LinearEquationsDet(vectors);

            double[] position = new double[normalDimension];
            for(int i = 0; i < position.Length; i++)
            {
                position[i] = x[i] / x[normalDimension];
            }

            return new TVector() { Position = position };
        }
    }
}