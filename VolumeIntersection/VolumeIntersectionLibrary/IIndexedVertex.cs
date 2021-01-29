using MIConvexHull;

namespace VolumeIntersection
{
    /// <summary>
    /// Vertex which also contains its index.
    /// It is used to compute a volumetric data from voronoi generators.
    /// </summary>
    public interface IIndexedVertex : IVertex
    {
        /// <summary>
        /// Gets index of this vector.
        /// </summary>
        int Index { get; }
    }
}
