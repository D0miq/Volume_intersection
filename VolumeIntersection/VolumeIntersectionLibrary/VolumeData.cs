using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VolumeIntersection
{
    public class VolumeData<TVector> where TVector : IVector, new()
    {
        public BoundingBox<TVector> BoundingBox { get; set; }

        public List<Cell<TVector>> Cells { get; set; }

        public static VolumeData<TVector> FromTriangulation<TCell>(List<TVector> vertices, List<TCell> cells) where TCell : ITriangleCell
        {
            if(vertices.Count < 3)
            {
                throw new ArgumentException("Not enough vertices.");
            }

            if(cells.Count < 1)
            {
                throw new ArgumentException("Not enough cells.");
            }

            // Save dimension of the provided vertices
            int vertexDimension = vertices[0].Position.Length;

            var edgeDictionary = new Dictionary<int[], Edge<TVector>>(new EdgeComparer());
            var volumeCells = new List<Cell<TVector>>();

            foreach (var cell in cells)
            {
                // Save indices of the cell
                var cellIndices = cell.Indices;

                // Find vertices of the cell
                var cellVertices = new double[cellIndices.Length][];
                for(int i = 0; i < cellVertices.Length; i++)
                {
                    cellVertices[i] = vertices[cellIndices[i]].Position;
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
                    Centroid = new TVector() { Position = centroid }
                };

                // Iterate over all faces of a cell and add them to volumetric data
                for (int i = 0; i < cellVertices.Length; i++)
                {
                    // Get vertices of the face
                    double[][] edgeVertices = new double[cellVertices.Length - 1][];
                    for (int j = 0; j < edgeVertices.Length; j++)
                    {
                        edgeVertices[j] = cellVertices[(i + j) % cellVertices.Length];
                    }

                    // Create array that holds elements of a standard (implicit) form of a half space in n dimensions
                    double[] halfSpace = new double[vertexDimension + 1];

                    // Compute half space standard form from vertices that generates it.
                    // It can be calculated with a system of linear equations. I use determinant to do it.
                    // Example of the calculation of a plane from three vertices:
                    //  | i   j  k l |
                    //  | a1 b1 c1 1 |
                    //  | a2 b2 c2 1 |
                    //  | a3 b3 c3 1 |
                    // i, j, k, l are vectors with just one coordinate set to 1
                    // i = (1, 0, 0, 0)
                    // j = (0, 1, 0, 0)
                    // k = (0, 0, 1, 0)
                    // l = (0, 0, 0, 1)
                    // a, b, c are coordinates of a vertex
                    // Elements of a standard form of a half space are computed as determinant of this matrix. 
                    // However I have no means to combine vectors and scalars inside the matrix so I use a way around it.
                    // I use Laplace expansion to calculate result of the determinant above and  
                    // compute determinants of its submatrices directly and assign results to the elements of the standard form.
                    // Submatrices are
                    // | b1 c1 1 | | a1 c1 1 | | a1 b1 1 | | a1 b1 c1 |
                    // | b2 c2 1 | | a2 c2 1 | | a2 b2 1 | | a2 b2 c2 |
                    // | b3 c3 1 | | a3 c3 1 | | a3 b3 1 | | a3 b3 c3 |

                    // Compute determinant for each variable of the half space standard form
                    for (int j = 0; j < halfSpace.Length; j++)
                    {
                        // Create a submatrix
                        double[] m = new double[vertexDimension * vertexDimension];

                        int matrixIndex = 0;

                        // Fill in values of the submatrix
                        for (int k = 0; k < vertexDimension; k++)
                        {
                            // Skip k-th coordinate when its column shouldn't be inside the submatrix
                            if (j != k)
                            {
                                for (int l = 0; l < edgeVertices.Length; l++)
                                {
                                    m[(matrixIndex * edgeVertices.Length) + l] = edgeVertices[l][k];
                                }

                                matrixIndex++;
                            }
                        }

                        // Add 1 to the last collumn, except the last submatrix.
                        if(j != halfSpace.Length - 1)
                        {
                            for (int k = 0; k < edgeVertices.Length; k++)
                            {
                                m[((vertexDimension - 1) * edgeVertices.Length) + k] = 1;
                            }
                        }

                        // Compute determinant of the submatrix
                        Matrix<double> matrix = Matrix<double>.Build.Dense(vertexDimension, vertexDimension, m);
                        halfSpace[j] = (j % 2) == 0 ? 1 * matrix.Determinant() : -1 * matrix.Determinant();
                    }

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

                    // Get a triangle face from the tetrahedron
                    int[] edgeIndices = new int[edgeVertices.Length];
                    for (int j = 0; j < edgeIndices.Length; j++)
                    {
                        edgeIndices[j] = cellIndices[(i + j) % cellVertices.Length];
                    }

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
                // TODO Compute centroid
                cell = new Cell<TVector>();
                dic[generator.Index] = cell;
            }

            return cell;
        }
    }
}