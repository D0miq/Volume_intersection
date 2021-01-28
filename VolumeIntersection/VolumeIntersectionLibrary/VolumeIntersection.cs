using MathNet.Numerics.LinearAlgebra;
using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VolumeIntersection
{
    public class VolumeIntersection<TVolumeData>
    {
        private class Pair<TCell>
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
                return obj is Pair<TCell> pair &&
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

        public TVolumeData Intersect<TVector, TCell, TIndexedVector>(List<TVector> vertices, List<TCell> cells, List<TIndexedVector> generators) where TVector : IVector where TCell : ITriangleCell where TIndexedVector : IIndexedVector
        {
            // Test dimensions
            var triangleDimension = this.GetDimension(vertices);
            var voronoiDimension = this.GetDimension(generators);

            if (triangleDimension != 2 || voronoiDimension != 2)
            {
                throw new ArgumentException("Triangulation vertices and voronoi generators must have the same dimension.");
            }

            TVolumeData triangulationData = ;
            TVolumeData voronoiData = ;

            TVolumeData volumeData = ;
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
        public VolumeData2D Intersect2D<TVector, TCell, TIndexedVector>(List<TVector> vertices, List<TCell> cells, List<TIndexedVector> generators) where TVector : IVector where TCell : ITriangleCell where TIndexedVector : IIndexedVector
        {
            // Test dimensions
            var triangleDimension = this.GetDimension(vertices);
            var voronoiDimension = this.GetDimension(generators);

            if(triangleDimension != 2 || voronoiDimension != 2)
            {
                throw new ArgumentException("Triangulation vertices and voronoi generators must have the same dimension.");
            }

            // Create volumetric data from triangulation and voronoi diagram
            var triangulationVolumeData = new VolumeData2D();
            triangulationVolumeData.FromTriangulation(vertices, cells);

            var voronoiVolumeData = new VolumeData2D();
            voronoiVolumeData.FromVoronoi(generators);

            // Create volumetric data for an intersection between the triangulation and voronoi diagram
            var volumeData = new VolumeData2D();

            for (int i = 0; i < triangulationVolumeData.Cells.Count; i++)
            {
                var triangulationCell = triangulationVolumeData.Cells[i];

                if (triangulationCell.Visited)
                {
                    continue;
                }

                // Find voronoi cell that contains centroid of the triangulation cell
                var voronoiCell = voronoiVolumeData.Cells.Find(cell => cell.Contains(triangulationCell.Centroid));

                // Find intersection between all traversable cells and their neighbors
                var intersections = FindIntersectingCells(triangulationCell, voronoiCell);

                volumeData.Cells.AddRange(intersections);
            }

            return volumeData;
        }

        public VolumeData3D Intersect3D<TVector, TCell, TIndexedVector>(List<TVector> vertices, List<TCell> cells, List<TIndexedVector> generators) where TVector : IVector where TCell : ITriangleCell where TIndexedVector : IIndexedVector
        {
            // Test dimensions
            var triangleDimension = this.GetDimension(vertices);
            var voronoiDimension = this.GetDimension(generators);

            if (triangleDimension != 3 || voronoiDimension != 3)
            {
                throw new ArgumentException("Triangulation vertices and voronoi generators must have the same dimension.");
            }

            // Create volumetric data from triangulation and voronoi diagram
            var triangulationVolumeData = new VolumeData3D();
            triangulationVolumeData.FromTriangulation(vertices, cells);

            var voronoiVolumeData = new VolumeData3D();
            voronoiVolumeData.FromVoronoi(generators);

            // Create volumetric data for an intersection between the triangulation and voronoi diagram
            var volumeData = new VolumeData3D();

            for (int i = 0; i < triangulationVolumeData.Cells.Count; i++)
            {
                var triangulationCell = triangulationVolumeData.Cells[i];

                if (triangulationCell.Visited)
                {
                    continue;
                }

                // Find voronoi cell that contains centroid of the triangulation cell
                var voronoiCell = voronoiVolumeData.Cells.Find(cell => cell.Contains(triangulationCell.Centroid));

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

        private List<Cell2D> FindIntersectingCells(Cell2D triangleCell, Cell2D voronoiCellIndex)
        { 
            var intersections = new List<Cell2D>();

            // Create a queue
            var visitedPairs = new HashSet<Pair<Cell2D>>();
            var cellQueue = new Queue<Pair<Cell2D>>();

            var pair = new Pair<Cell2D>(triangleCell, voronoiCellIndex);
            visitedPairs.Add(pair);
            cellQueue.Enqueue(pair);

            while (cellQueue.Count != 0)
            {
                pair = cellQueue.Dequeue();

                var tempFirstCell = pair.FirstCell;
                var tempSecondCell = pair.SecondCell;

                tempFirstCell.Visited = true;
                tempSecondCell.Visited = true;

                List<Vector2> intersectingPoints;
                var edges = RemoveHalfSpaces(tempFirstCell, tempSecondCell, out intersectingPoints);

                foreach (var edge in edges)
                {
                    if(edge.Target != null)
                    {
                        if (edge.Source == tempFirstCell)
                        {
                            pair = new Pair<Cell2D>(edge.Target, tempSecondCell);
                        }
                        else if (edge.Source == tempSecondCell)
                        {
                            pair = new Pair<Cell2D>(tempFirstCell, edge.Target);
                        }

                        if (visitedPairs.Add(pair))
                        {
                            cellQueue.Enqueue(pair);
                        }
                    }
                }

                var cell = new Cell2D()
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

        private void FindCentroid(List<TVector> vertices, Cell2D<TVector> cell)
        {
            double areaSum = 0;

            double[] centroid = new double[this.dimension];

            var triangles = this.Triangulate(vertices);
            
            foreach (var triangle in triangles)
            {
                double area = 0;

                double[][] triangleEdges = new double[triangle.Vertices.Length - 1][];

                var vertexPosition = triangle.Vertices[0].Position;

                int index = 0;

                for (int i = 1; i < triangle.Vertices.Length; i++)
                {
                    triangleEdges[index] = new double[this.dimension];
                    var nextVertexPosition = triangle.Vertices[i].Position;
                    for (int j = 0; j < this.dimension; j++)
                    {
                        triangleEdges[index][j] = nextVertexPosition[j] - vertexPosition[j];
                    }

                    index++;
                }

                if (this.dimension == 2)
                {
                    area = Math.Abs(MathUtils.Determinant(triangleEdges)) / 2;
                }
                else if (this.dimension == 3)
                {
                    area = Math.Abs(MathUtils.Determinant(triangleEdges)) / 6;
                }

                // Compute centroid of the triangle
                for (int i = 0; i < centroid.Length; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < triangle.Vertices.Length; j++)
                    {
                        sum += triangle.Vertices[j].Position[i];
                    }

                    centroid[i] = sum / triangle.Vertices.Length * area;
                }

                areaSum += area;
            }

            // Compute centroid of the triangle
            for (int i = 0; i < centroid.Length; i++)
            {
                centroid[i] /= areaSum;
            }

            cell.Weight = areaSum;
            cell.Centroid = new TVector()
            {
                Position = centroid
            };
        }

        private List<Edge2D> RemoveHalfSpaces(Cell2D c1, Cell2D c2, out List<TVector> intersectionPoints)
        {
            var usedHalfSpaces = new Dictionary<Edge2D<TVector>, HashSet<TVector>>();
            var usedIntersectingPoints = new HashSet<TVector>(new VectorComparer<TVector>());

            var halfSpaces = new List<Edge2D<TVector>>(c1.Edges.Count + c2.Edges.Count);
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

        private bool FindIntersectionPoint(List<Edge2D<TVector>> edges, out TVector intersection)
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

        private int GetDimension<TVector>(List<TVector> vertices) where TVector : IVector
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