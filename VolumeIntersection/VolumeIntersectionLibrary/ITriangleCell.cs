namespace VolumeIntersection
{
    /// <summary>
    /// N dimensional triangle cells (can be a triangle or tetrahedron).
    /// </summary>
    public interface ITriangleCell
    {
        /// <summary>
        /// Indices of vertices.
        /// </summary>
        int[] Indices { get; }
    }
}
