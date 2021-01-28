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
    }
}
