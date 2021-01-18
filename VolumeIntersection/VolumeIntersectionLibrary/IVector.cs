using MIConvexHull;

namespace VolumeIntersection
{
    /// <summary>
    /// Interface of a vector.
    /// </summary>
    public interface IVector : IVertex
    {
        /// <summary>
        /// Gets or sets a copy of positions.
        /// </summary>
        new double[] Position { get; set; }

        /// <summary>
        /// Gets index of this vector.
        /// Should be used when the vector is used to compute a volumetric data from voronoi generators.
        /// </summary>
        int Index { get; }
    }
}
