using MathNet.Numerics.LinearAlgebra;
using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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

        protected abstract void RemoveHalfSpaces();

        protected abstract void FindCentroid(List<TVector> vertices, TCell cell);

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
        public TVolumeData Intersect<TInVector, TInCell, TIndexedVector>(List<TInVector> vertices, List<TInCell> cells, List<TIndexedVector> generators) where TInVector : IVector where TInCell : ITriangleCell where TIndexedVector : IIndexedVector
        {
            // Test dimensions
            var triangleDimension = this.GetDimension(vertices);
            var voronoiDimension = this.GetDimension(generators);

            if (triangleDimension != 2 || voronoiDimension != 2)
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

                List<TVector> intersectingPoints;
                var edges = RemoveHalfSpaces(tempFirstCell, tempSecondCell, out intersectingPoints);

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
                    FindCentroid(intersectingPoints, cell);
                    intersections.Add(cell);
                } catch(ConvexHullGenerationException)
                {
                    Console.WriteLine("Intersection is too small.");
                }
            }

            return intersections;
        }

        private List<TEdge> RemoveHalfSpaces(TCell c1, TCell c2, out List<TVector> intersectionPoints)
        {
            var usedHalfSpaces = new Dictionary<TEdge, HashSet<TVector>>();
            var usedIntersectingPoints = new HashSet<TVector>(new VectorComparer<TVector>());

            var halfSpaces = new List<TEdge>(c1.Edges.Count + c2.Edges.Count);
            halfSpaces.AddRange(c1.Edges);
            halfSpaces.AddRange(c2.Edges);

            if (this.dimension == 2)
            {
                for (int i = 0; i < halfSpaces.Count; i++)
                {
                    for (int j = i + 1; j < halfSpaces.Count; j++)
                    {
                        var success = FindIntersectionPoint(new List<Edge2D<TVector>>() { halfSpaces[i], halfSpaces[j] }, out TVector intersectionPoint);

                        if (success && (c1.Contains(intersectionPoint) && c2.Contains(intersectionPoint)))
                        {
                            if(usedHalfSpaces.TryGetValue(halfSpaces[i], out HashSet<TVector> vertices))
                            {
                                vertices.Add(intersectionPoint);
                            } else
                            {
                                var newVertices = new HashSet<TVector>(new VectorComparer<TVector>());
                                newVertices.Add(intersectionPoint);
                                usedHalfSpaces.Add(halfSpaces[i], newVertices);
                            }

                            if(usedHalfSpaces.TryGetValue(halfSpaces[j], out vertices))
                            {
                                vertices.Add(intersectionPoint);
                            } else
                            {
                                var newVertices = new HashSet<TVector>(new VectorComparer<TVector>());
                                newVertices.Add(intersectionPoint);
                                usedHalfSpaces.Add(halfSpaces[j], newVertices);
                            }

                            usedIntersectingPoints.Add(intersectionPoint);
                        }
                    }
                }

                var toRemove = usedHalfSpaces.Where(pair => pair.Value.Count != 2).ToList();
                foreach (var pair in toRemove)
                {
                    usedHalfSpaces.Remove(pair.Key);
                }
            }
            else if (this.dimension == 3)
            {
                for (int i = 0; i < halfSpaces.Count; i++)
                {
                    for (int j = i + 1; j < halfSpaces.Count; j++)
                    {
                        for (int k = j + 1; k < halfSpaces.Count; k++)
                        {
                            var success = FindIntersectionPoint(new List<Edge2D<TVector>>() { halfSpaces[i], halfSpaces[j], halfSpaces[k] }, out TVector intersectionPoint);

                            if (success && (c1.Contains(intersectionPoint) && c2.Contains(intersectionPoint)))
                            {
                                if (usedHalfSpaces.TryGetValue(halfSpaces[i], out HashSet<TVector> vertices))
                                {
                                    vertices.Add(intersectionPoint);
                                }
                                else
                                {
                                    var newVertices = new HashSet<TVector>(new VectorComparer<TVector>());
                                    newVertices.Add(intersectionPoint);
                                    usedHalfSpaces.Add(halfSpaces[i], newVertices);
                                }

                                if (usedHalfSpaces.TryGetValue(halfSpaces[j], out vertices))
                                {
                                    vertices.Add(intersectionPoint);
                                }
                                else
                                {
                                    var newVertices = new HashSet<TVector>(new VectorComparer<TVector>());
                                    newVertices.Add(intersectionPoint);
                                    usedHalfSpaces.Add(halfSpaces[j], newVertices);
                                }

                                if (usedHalfSpaces.TryGetValue(halfSpaces[k], out vertices))
                                {
                                    vertices.Add(intersectionPoint);
                                }
                                else
                                {
                                    var newVertices = new HashSet<TVector>(new VectorComparer<TVector>());
                                    newVertices.Add(intersectionPoint);
                                    usedHalfSpaces.Add(halfSpaces[k], newVertices);
                                }

                                usedIntersectingPoints.Add(intersectionPoint);
                            }
                        }
                    }
                }

                var toRemove = usedHalfSpaces.Where(pair => pair.Value.Count < 3).ToList();
                foreach (var pair in toRemove)
                {
                    usedHalfSpaces.Remove(pair.Key);
                }
            }

            intersectionPoints = usedIntersectingPoints.ToList();
            return usedHalfSpaces.Keys.ToList();
        }

        private bool FindIntersectionPoint(List<TEdge> edges, out TVector intersection)
        {
            //double[][] vectors = new double[edges.Count][];

            //for (int i = 0; i < edges.Count; i++)
            //{
            //    vectors[i] = new double[this.dimension + 1];
            //    var normal = edges[i].Normal.Position;

            //    for (int j = 0; j < this.dimension; j++)
            //    {
            //        vectors[i][j] = normal[j];
            //    }

            //    vectors[i][this.dimension] = -edges[i].C;
            //}

            //double[] x = MathUtils.LinearEquationsDet(vectors);

            //double[] position = new double[this.dimension];
            //for (int i = 0; i < position.Length; i++)
            //{
            //    position[i] = x[i] / x[this.dimension];
            //}

            double[][] rows = new double[edges.Count][];
            double[] b = new double[edges.Count];
            for (int i = 0; i < edges.Count; i++)
            {
                rows[i] = edges[i].Normal.Position;
                b[i] = edges[i].C;
            }

            var matrix = Matrix<double>.Build.DenseOfRowArrays(rows);
            var bVector = Vector<double>.Build.Dense(b);

            var position = matrix.Solve(bVector).ToArray();

            intersection = new TVector() { Position = position };

            //return x[normalDimension] != 0;

            for (int i = 0; i < position.Length; i++)
            {
                if (double.IsNaN(position[i])) return false;
            }

            return true;
        }

        private IEnumerable<DefaultTriangulationCell<TVector>> Triangulate<TVector>(List<TVector> vertices) where TVector : IVector
        {
            IEnumerable<DefaultTriangulationCell<TVector>> triangles = null;

            if (vertices.Count == 3 || vertices.Count == 4)
            {
                triangles = new List<DefaultTriangulationCell<TVector>>() { new DefaultTriangulationCell<TVector>() { Vertices = vertices.ToArray() } };

                //if (dimension == 2)
                //{
                //    triangles = new List<DefaultTriangulationCell<TVector>>() { 
                //        new DefaultTriangulationCell<TVector>() { Vertices = vertices.ToArray() },
                //        new DefaultTriangulationCell<TVector>() {Vertices = }
                //    };
                //}
                //else
                //{
                //    triangles = new List<DefaultTriangulationCell<TVector>>() { new DefaultTriangulationCell<TVector>() { Vertices = vertices.ToArray() } };
                //}
            }
            else
            {
                var triangulation = Triangulation.CreateDelaunay(vertices);
                triangles = triangulation.Cells;
            }

            return triangles;
        }

        private int GetDimension<TInVector>(List<TInVector> vertices) where TInVector : IVector
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