namespace VolumeIntersection
{
    /// <summary>
    /// Bounding box.
    /// </summary>
    /// <typeparam name="TVector"></typeparam>
    public class BoundingBox<TVector> where TVector : IVector
    {
        /// <summary>
        /// Minimal point.
        /// </summary>
        public TVector Min { get; set; }
        
        /// <summary>
        /// Maximmal point.
        /// </summary>
        public TVector Max { get; set; }
    }
}