namespace VolumeIntersection.VolumeVisualisation
{
    /// <summary>
    /// 2D bounding box.
    /// </summary>
    public class BoundingBox2D
    {
        /// <summary>
        /// Minimal coordinates.
        /// </summary>
        public Vector2D Min { get; set; }

        /// <summary>
        /// Maximmal coordinates.
        /// </summary>
        public Vector2D Max { get; set; }

        /// <summary>
        /// Creates a new bounding box.
        /// </summary>
        /// <param name="min">Minimal coordinates.</param>
        /// <param name="max">Maximal coordinates.</param>
        public BoundingBox2D(Vector2D min, Vector2D max)
        {
            Min = min;
            Max = max;
        }
    }
}