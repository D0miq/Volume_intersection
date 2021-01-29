namespace VolumeIntersection
{
    /// <summary>
    /// Math operations.
    /// </summary>
    internal static class MathUtils
    {
        /// <summary>
        /// Eps used for eps tests.
        /// </summary>
        public const float Eps = 1E-7F;

        /// <summary>
        /// Remaps value from one interval to another.
        /// </summary>
        /// <param name="value">Value that should be remapped.</param>
        /// <param name="low1">Low limit of the first interval.</param>
        /// <param name="high1">High limit of the first interval.</param>
        /// <param name="low2">Low limit of the second interval.</param>
        /// <param name="high2">High limit of the second interval.</param>
        /// <returns>Remapped value.</returns>
        public static float Remap(float value, float low1, float high1, float low2, float high2)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }
    }
}
