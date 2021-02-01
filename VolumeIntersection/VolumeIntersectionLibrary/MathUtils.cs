namespace VolumeIntersectionLibrary
{
    /// <summary>
    /// Math operations.
    /// </summary>
    internal static class MathUtils
    {
        /// <summary>
        /// Eps used for eps tests.
        /// </summary>
        public const double Eps = 1E-10;

        /// <summary>
        /// Remaps value from one interval to another.
        /// </summary>
        /// <param name="value">Value that should be remapped.</param>
        /// <param name="low1">Low limit of the first interval.</param>
        /// <param name="high1">High limit of the first interval.</param>
        /// <param name="low2">Low limit of the second interval.</param>
        /// <param name="high2">High limit of the second interval.</param>
        /// <returns>Remapped value.</returns>
        public static double Remap(double value, double low1, double high1, double low2, double high2)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }
    }
}
