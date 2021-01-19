using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VolumeIntersection
{
    public class VolumeIntersection<TVector> where TVector : IVector, new()
    {
        private class Pair
        {
            public Cell<TVector> FirstCell;
            public Cell<TVector> SecondCell;

            public Pair(Cell<TVector> firstCell, Cell<TVector> secondCell)
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
                hashCode = hashCode * -1521134295 + EqualityComparer<Cell<TVector>>.Default.GetHashCode(FirstCell);
                hashCode = hashCode * -1521134295 + EqualityComparer<Cell<TVector>>.Default.GetHashCode(SecondCell);
                return hashCode;
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
        public VolumeData<TVector> Intersect<TCell>(List<TVector> vertices, List<TCell> cells, List<TVector> generators) where TCell : ITriangleCell
        {
            // Create volumetric data from triangulation and voronoi diagram
            var triangulationVolumeData = VolumeData<TVector>.FromTriangulation(vertices, cells);
            var voronoiVolumeData = VolumeData<TVector>.FromVoronoi(generators);

            // Create volumetric data for an intersection between the triangulation and voronoi diagram
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

        private List<Cell<TVector>> FindIntersectingCells(Cell<TVector> triangleCell, Cell<TVector> voronoiCellIndex)
        { 
            var intersections = new List<Cell<TVector>>();

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
                    if (edge.Source == tempFirstCell && edge.Target != null)
                    {
                        pair = new Pair(edge.Target, tempSecondCell);
                    }
                    else if(edge.Source == tempSecondCell && edge.Target != null)
                    {
                        pair = new Pair(tempFirstCell, edge.Target);
                    }

                    if (visitedPairs.Add(pair))
                    {
                        cellQueue.Enqueue(pair);
                    }
                }

                var cell = new Cell<TVector>()
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

        private void FindCentroid(List<TVector> vertices, Cell<TVector> cell)
        {
            int dimension = vertices[0].Position.Length;

            double areaSum = 0;

            double[] centroid = new double[dimension];

            var triangles = Triangulate(dimension, vertices);
            
            foreach (var triangle in triangles)
            {
                double area = 0;

                double[][] triangleEdges = new double[triangle.Vertices.Length - 1][];

                var vertexPosition = triangle.Vertices[0].Position;

                int index = 0;

                for (int i = 1; i < triangle.Vertices.Length; i++)
                {
                    triangleEdges[index] = new double[dimension];
                    var nextVertexPosition = triangle.Vertices[i].Position;
                    for (int j = 0; j < dimension; j++)
                    {
                        triangleEdges[index][j] = nextVertexPosition[j] - vertexPosition[j];
                    }

                    index++;
                }

                if (dimension == 2)
                {
                    area = Math.Abs(MathUtils.Determinant(triangleEdges)) / 2;
                }
                else if (dimension == 3)
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

        private List<Edge<TVector>> RemoveHalfSpaces(Cell<TVector> c1, Cell<TVector> c2, out List<TVector> intersectionPoints)
        {
            var usedHalfSpaces = new Dictionary<Edge<TVector>, HashSet<TVector>>();
            var usedIntersectingPoints = new HashSet<TVector>(new VectorComparer<TVector>());

            var halfSpaces = new List<Edge<TVector>>(c1.Edges.Count + c2.Edges.Count);
            halfSpaces.AddRange(c1.Edges);
            halfSpaces.AddRange(c2.Edges);

            var dimension = c1.Centroid.Position.Length;

            if (dimension == 2)
            {
                for (int i = 0; i < halfSpaces.Count; i++)
                {
                    for (int j = i + 1; j < halfSpaces.Count; j++)
                    {
                        var success = FindIntersectionPoint(new List<Edge<TVector>>() { halfSpaces[i], halfSpaces[j] }, out TVector intersectionPoint);

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
            else if (dimension == 3)
            {
                for (int i = 0; i < halfSpaces.Count; i++)
                {
                    for (int j = i + 1; j < halfSpaces.Count; j++)
                    {
                        for (int k = j + 1; k < halfSpaces.Count; k++)
                        {
                            var success = FindIntersectionPoint(new List<Edge<TVector>>() { halfSpaces[i], halfSpaces[j], halfSpaces[k] }, out TVector intersectionPoint);

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

        private bool FindIntersectionPoint(List<Edge<TVector>> edges, out TVector intersection)
        {
            int normalDimension = edges[0].Normal.Position.Length;

            double[][] vectors = new double[edges.Count][];

            for (int i = 0; i < edges.Count; i++)
            {
                vectors[i] = new double[normalDimension + 1];
                var normal = edges[i].Normal.Position;

                for (int j = 0; j < normalDimension; j++)
                {
                    vectors[i][j] = normal[j];
                }

                vectors[i][normalDimension] = -edges[i].C;
            }

            double[] x = MathUtils.LinearEquationsDet(vectors);

            double[] position = new double[normalDimension];
            for (int i = 0; i < position.Length; i++)
            {
                position[i] = x[i] / x[normalDimension];
            }

            intersection = new TVector() { Position = position };

            return x[normalDimension] != 0;
        }

        private IEnumerable<DefaultTriangulationCell<TVector>> Triangulate<TVector>(int dimension, List<TVector> vertices) where TVector : IVector
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
    }
}