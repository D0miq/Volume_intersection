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
    public class VolumeData<TVector> where TVector : IVector, new()
    {
        /// <summary>
        /// Cells of this data.
        /// </summary>
        public List<Cell<TVector>> Cells { get; set; }

        /// <summary>
        /// Creates a volumetric data from a triangulation.
        /// </summary>
        /// <typeparam name="TCell"></typeparam>
        /// <param name="vertices">Vertices</param>
        /// <param name="cells">Cells (triangles or tetrahedrons)</param>
        /// <returns>Volumetric data</returns>
        public static VolumeData<TVector> FromTriangulation<TCell>(List<TVector> vertices, List<TCell> cells) where TCell : ITriangleCell
        {
            if (vertices.Count < 3)
            {
                throw new ArgumentException("Not enough vertices.");
            }

            if (cells.Count < 1)
            {
                throw new ArgumentException("Not enough cells.");
            }

            // Save dimension of the provided vertices
            int vertexDimension = vertices[0].Position.Length;
            int homogenDimension = vertexDimension + 1;

            double[][] homogenVertices = new double[vertices.Count][];
            for (int i = 0; i < vertices.Count; i++)
            {
                var vertexPosition = vertices[i].Position;
                homogenVertices[i] = new double[homogenDimension];

                for (int j = 0; j < vertexDimension; j++)
                {
                    homogenVertices[i][j] = vertexPosition[j];
                }

                homogenVertices[i][vertexDimension] = 1;
            }
            
            var edgeDictionary = new Dictionary<int[], Edge<TVector>>(new EdgeComparer());
            var volumeCells = new List<Cell<TVector>>();

            for (var cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                var cell = cells[cellIndex];

                // Save indices of the cell
                var cellIndices = cell.Indices;

                // Find vertices of the cell
                var cellVertices = new double[cellIndices.Length][];
                for(int i = 0; i < cellVertices.Length; i++)
                {
                    cellVertices[i] = homogenVertices[cellIndices[i]];
                }

                // Compute centroid of the cell
                var centroid = new double[vertexDimension];
                for(int i = 0; i < centroid.Length; i++)
                {
                    for(int j = 0; j < cellVertices.Length; j++)
                    {
                        centroid[i] += cellVertices[j][i];
                    }

                    centroid[i] /= cellVertices.Length;
                }

                // Create tetrahedron cell
                var volumeCell = new Cell<TVector>()
                {
                    Centroid = new TVector() { Position = centroid },
                    VoronoiIndex = -1,
                    TriangleIndex = cellIndex
                };

                // Iterate over all faces of a cell and add them to volumetric data
                for (int i = 0; i < cellVertices.Length; i++)
                {
                    // Get a triangle face from the tetrahedron
                    int[] edgeIndices = new int[cellVertices.Length - 1];
                    for (int j = 0; j < edgeIndices.Length; j++)
                    {
                        edgeIndices[j] = cellIndices[(i + j) % cellIndices.Length];
                    }

                    // Get vertices of the face
                    double[][] edgeVertices = new double[edgeIndices.Length][];
                    for (int j = 0; j < edgeVertices.Length; j++)
                    {
                        edgeVertices[j] = homogenVertices[edgeIndices[j]];
                    }

                    var halfSpace = MathUtils.LinearEquationsDet(edgeVertices);

                    // Get normal from the standard form
                    double[] normal = new double[vertexDimension];
                    for (int j = 0; j < normal.Length; j++)
                    {
                        normal[j] = halfSpace[j];
                    }

                    double c = -halfSpace[vertexDimension];

                    // Compute vector from centroid to one of the vertices
                    double[] vectorFromCentroid = new double[vertexDimension];
                    for(int j = 0; j < vectorFromCentroid.Length; j++)
                    {
                        vectorFromCentroid[j] = edgeVertices[0][j] - centroid[j];
                    }

                    // Check widing order
                    double order = MathUtils.Dot(normal, vectorFromCentroid);

                    // Swap order of vertices if neccessary
                    if (order > 0)
                    {
                        for(int j = 0; j < normal.Length; j++)
                        {
                            normal[j] = -normal[j];
                        }

                        c = -c;
                    }

                    // Create new edge
                    var edge = new Edge<TVector>()
                    {
                        Normal = new TVector() { Position = normal },
                        C = c,
                        Source = volumeCell
                    };

                    volumeCell.Edges.Add(edge);

                    Array.Sort(edgeIndices);

                    // If dictionary contained the face before add neighbors
                    if (edgeDictionary.TryGetValue(edgeIndices, out Edge<TVector> neighborEdge))
                    {
                        edge.Target = neighborEdge.Source;
                        neighborEdge.Target = volumeCell;
                    }
                    else
                    {
                        edgeDictionary.Add(edgeIndices, edge);
                    }
                }

                volumeCells.Add(volumeCell);
            }

            return new VolumeData<TVector>()
            {
                Cells = volumeCells
            };
            
        }

        public static VolumeData<TVector> FromVoronoi(List<TVector> generators)
        {
            if(generators.Count < 2)
            {
                throw new ArgumentException("Not enough generators");
            }

            // Save dimension of the provided generators
            int dimension = generators[0].Position.Length;

            // Create voronoi diagram from the generators.
            var voronoiMesh = VoronoiMesh.Create(generators);

            // Prepare dictionary of voronoi cells
            var cellDictionary = new Cell<TVector>[generators.Count];
            
            // Iteratate over all voronoi vertices.
            // They are represented as tetrahedra in the implementation of the voronoi diagram.
            // So each vertex of the tetrahedron is a generator of an adjecent voronoi cell.
            foreach(var tetrahedron in voronoiMesh.Vertices)
            {
                // Iterate over all adjecent generators.
                for (int i = 0; i < tetrahedron.Vertices.Length; i++)
                {
                    // Create a voronoi cell for the generator or find it if the generator has already been visited.
                    Cell<TVector> sourceCell = AddCellToDictionary(tetrahedron.Vertices[i], cellDictionary);

                    // From the comments above follows that vertices of the tetrahedron and their corresponding voronoi cells are each other's neighbors.
                    // So iterate over neighbors of the current source cell.
                    for (int j = i + 1; j < tetrahedron.Vertices.Length; j++)
                    {
                        // Create a voronoi cell for the neighboring generator or find it if the generator has already been visited.
                        Cell<TVector> targetCell = AddCellToDictionary(tetrahedron.Vertices[j], cellDictionary);

                        // Check if the target cell has already been added to the neighbors of the source cell.
                        // If it hasn't create a new edge separating them.
                        if (!sourceCell.Edges.Any(edge => edge.Target == targetCell))
                        {
                            // Get position of the source and target
                            // The source is the cell and the target its neigbor
                            var sourcePosition = tetrahedron.Vertices[i].Position;
                            var targetPosition = tetrahedron.Vertices[j].Position;

                            // Compute a vector that points towards the source position
                            double[] normal = new double[dimension];
                            for(int k = 0; k < dimension; k++)
                            {
                                normal[k] = sourcePosition[k] - targetPosition[k];
                            }

                            // Compute a point in the middle between the two positions
                            double[] middlePosition = new double[dimension];
                            for(int k = 0; k < middlePosition.Length; k++)
                            {
                                middlePosition[k] = (sourcePosition[k] + targetPosition[k]) / 2;
                            };

                            // Compute last element of a half space standard form that separates the source and target positions and goes through the middle position
                            double c = MathUtils.Dot(normal, middlePosition);

                            // Add the neighbor
                            sourceCell.Edges.Add(new Edge<TVector>()
                            {
                                Normal = new TVector() { Position = normal },
                                C = c,
                                Source = sourceCell,
                                Target = targetCell
                            });

                            // Change direction of the normal
                            double[] reverseNormal = new double[dimension];
                            for(int k = 0; k < reverseNormal.Length; k++)
                            {
                                reverseNormal[k] = -normal[k];
                            }

                            // Add the neighbor
                            targetCell.Edges.Add(new Edge<TVector>()
                            {
                                Normal = new TVector() { Position = reverseNormal },
                                C = -c,
                                Source = targetCell,
                                Target = sourceCell
                            });
                        }
                    }
                }
            }

            return new VolumeData<TVector>()
            {
                Cells = cellDictionary.ToList()
            };
        }

        private static Cell<TVector> AddCellToDictionary(TVector generator, Cell<TVector>[] dic)
        {
            Cell<TVector> cell = dic[generator.Index];
            if (cell == null)
            {
                cell = new Cell<TVector>()
                {
                    Centroid = generator,
                    VoronoiIndex = generator.Index,
                    TriangleIndex = -1
                };

                dic[generator.Index] = cell;
            }

            return cell;
        }

        private static int GetDimension(List<TVector> vertices)
        {
            int minDimension = int.MaxValue;
            int maxDimension = int.MinValue;

            Random random = new Random();
            for(int i = 0; i < 10; i++)
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