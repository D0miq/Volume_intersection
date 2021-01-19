using System.Collections.Generic;

namespace VolumeIntersection.SliceVisualisation
{
    /// <summary>
    /// Bounding box.
    /// </summary>
    /// <typeparam name="TVector">Vector type</typeparam>
    public class BoundingBox<TVector> where TVector : IVector, new()
    {
        /// <summary>
        /// Minimal point.
        /// </summary>
        public TVector Min { get; set; }
        
        /// <summary>
        /// Maximmal point.
        /// </summary>
        public TVector Max { get; set; }

        /// <summary>
        /// Creates a new empty bounding box.
        /// </summary>
        public BoundingBox()
        { }

        /// <summary>
        /// Creates a new bounding box of the provided vertices
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="vertexDimension">Vertex dimension</param>
        public BoundingBox(List<TVector> vertices, int vertexDimension)
        {
            // Create min and max arrays
            double[] min = new double[vertexDimension];
            double[] max = new double[vertexDimension];

            // Initialize min a max positions
            for (int i = 0; i < vertexDimension; i++)
            {
                min[i] = double.MaxValue;
                max[i] = double.MinValue;
            }

            // Find min and max positions of vertices
            for (int i = 0; i < vertices.Count; i++)
            {
                var position = vertices[i].Position;

                for (int j = 0; j < position.Length; j++)
                {
                    if (min[j] > position[j])
                    {
                        min[j] = position[j];
                    }

                    if (max[j] < position[j])
                    {
                        max[j] = position[j];
                    }
                }
            }

            // Assing min and max.
            Min = new TVector() { Position = min };
            Max = new TVector() { Position = max };
        }
    }
}