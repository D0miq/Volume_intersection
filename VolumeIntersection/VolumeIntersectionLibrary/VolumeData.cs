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

            var faceDictionary = new Dictionary<int[], Face<TVector>>(new FaceComparer());
            var volumeCells = new List<Cell<TVector>>();

            foreach (var cell in cells)
            {
                // Save indices of the cell
                var cellIndices = cell.Indices;

                // Save dimension of the provided cells
                int cellDimension = cellIndices.Length;

                // Find vertices of the cell
                var cellVertices = new double[cellIndices.Length][];
                for(int i = 0; i < cellIndices.Length; i++)
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

                for (int i = 0; i < cellDimension; i++)
                {
                    // Get vertices of the face
                    double[][] faceVertices = new double[cellDimension - 1][];
                    for (int j = 0; j < cellDimension - 1; j++)
                    {
                        faceVertices[j] = cellVertices[(i + j) % cellDimension];
                    }

                    double[] halfSpace = new double[cellDimension];

                    for(int j = 0; j < cellDimension; j++)
                    {
                        double[,] m = new double[vertexDimension, vertexDimension];

                        for(int k = 0; k < faceVertices.Length; k++)
                        {
                            for(int l = 0; l < vertexDimension; l++)
                            {
                                m[k, ] = faceVertices[k][l];
                            }
                        }

                        Matrix<double> matrix = DenseMatrix.OfArray(m);
                        halfSpace[j] = matrix.Determinant();
                    }
                    


                    if(faceVertices.Length == 3)
                    {
                        // Get two vectors between points that define a plane
                        double[] vector1 = { faceVertices[0][0] - faceVertices[1][0], faceVertices[0][1] - faceVertices[1][1], faceVertices[0][2] - faceVertices[1][2] };
                        double[] vector2 = { faceVertices[2][0] - faceVertices[1][0], faceVertices[2][1] - faceVertices[1][1], faceVertices[2][2] - faceVertices[1][2] };

                        // Compute normal of the triangle
                        normal = MathUtils.CrossProduct(vector1, vector2);

                        // Compute vector from centroid to one of the vertices
                        double[] vectorFromCentroid = { faceVertices[1][0] - centroid[0], faceVertices[1][1] - centroid[1], faceVertices[1][2] - centroid[2] };

                        // Check widing order
                        double order = MathUtils.Dot(normal, vectorFromCentroid);

                        // Swap order of vertices if neccessary
                        if (order > 0)
                        {
                            normal = MathUtils.CrossProduct(vector2, vector1);
                        }

                        c = MathUtils.Dot(normal, faceVertices[1]);
                    }

                    // Create new edge
                    var edge = new Face<TVector>()
                    {
                        Normal = new TVector() { Position = normal },
                        C = c,
                        Source = volumeCell
                    };

                    volumeCell.Edges.Add(edge);

                    // Get a triangle face from the tetrahedron
                    int[] face = { cellIndices[i], cellIndices[(i + 1) % cellIndices.Length], cellIndices[(i + 2) % cellIndices.Length] };
                    Array.Sort(face);

                    // If dictionary contained the face before add neighbors
                    if (faceDictionary.TryGetValue(face, out Face<TVector> neighborEdge))
                    {
                        edge.Target = neighborEdge.Source;
                        neighborEdge.Target = volumeCell;
                    }
                    else
                    {
                        faceDictionary.Add(face, edge);
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
                            for(int k = 0; k < dimension; k++)
                            {
                                middlePosition[k] = (sourcePosition[k] + targetPosition[k]) / 2;
                            };

                            // Compute last element of a half space standard form that separates the source and target positions and goes through the middle position
                            double c = MathUtils.Dot(normal, middlePosition);

                            // Add the neighbor
                            sourceCell.Edges.Add(new Face<TVector>()
                            {
                                Normal = new TVector() { Position = normal },
                                C = c,
                                Source = sourceCell,
                                Target = targetCell
                            });

                            // Change direction of the normal
                            double[] reverseNormal = new double[dimension];
                            for(int k = 0; k < dimension; k++)
                            {
                                reverseNormal[k] = -normal[k];
                            }

                            // Add the neighbor
                            targetCell.Edges.Add(new Face<TVector>()
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