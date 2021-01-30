namespace VolumeIntersection
{
    /// <summary>
    /// Structure for a 2D vector.
    /// </summary>
    public struct Vector2D
    {
        /// <summary>
        /// X coordinate.
        /// </summary>
        public double X;

        /// <summary>
        /// Y coordinate.
        /// </summary>
        public double Y;

        /// <summary>
        /// Creates a new 2D vector with the same value in both coordinates.
        /// </summary>
        /// <param name="value">The value saved to X and Y.</param>
        public Vector2D(double value)
        {
            X = Y = value;
        }

        /// <summary>
        /// Creates a new 2D vector with its coordinates.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Computes dot product of two vectors.
        /// </summary>
        /// <param name="other">Other vector.</param>
        /// <returns>The dot product.</returns>
        public double Dot(Vector2D other)
        {
            return X * other.X + Y * other.Y;
        }

        /// <summary>
        /// Unary minus operator.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <returns>New vector with opposite coordinates.</returns>
        public static Vector2D operator -(Vector2D a)
        {
            return new Vector2D(-a.X, -a.Y);
        }
    }
}
