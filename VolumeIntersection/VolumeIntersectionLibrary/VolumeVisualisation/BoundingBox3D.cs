namespace VolumeIntersection.VolumeVisualisation
{
    /// <summary>
    /// 3D boundin box.
    /// </summary>
    public class BoundingBox3D
    {
        /// <summary>
        /// Minimal coordinates.
        /// </summary>
        public Vector3D Min { get; set; }

        /// <summary>
        /// Maximal coordinates.
        /// </summary>
        public Vector3D Max { get; set; }

        /// <summary>
        /// Creates a new boudning box.
        /// </summary>
        /// <param name="min">Minimal coordinates.</param>
        /// <param name="max">Maximal coordinates.</param>
        public BoundingBox3D(Vector3D min, Vector3D max)
        {
            Min = min;
            Max = max;
        }
    }
}
