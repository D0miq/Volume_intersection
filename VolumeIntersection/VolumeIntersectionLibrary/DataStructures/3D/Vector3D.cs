namespace VolumeIntersectionLibrary.DataStructures._3D
{
    /// <summary>
    /// Structure for a 3D vector.
    /// </summary>
    public struct Vector3D
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
        /// Z coordinate.
        /// </summary>
        public double Z;

        /// <summary>
        /// Creates a new 3D vector with the same value in all three coordinates.
        /// </summary>
        /// <param name="value">The value that will be set to X, Y and Z.</param>
        public Vector3D(double value)
        {
            X = Y = Z = value;
        }

        /// <summary>
        /// Creates a new 3D vector with its coordinates.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Computes dot product of two vectors.
        /// </summary>
        /// <param name="other">Other vector.</param>
        /// <returns>The dot product.</returns>
        public double Dot(Vector3D other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        /// <summary>
        /// Computes cross product of two vectors.
        /// </summary>
        /// <param name="other">Other vector.</param>
        /// <returns>The cross product.</returns>
        public Vector3D Cross(Vector3D other)
        {
            return new Vector3D(Y * other.Z - Z * other.Y, -(X * other.Z - Z * other.X), X * other.Y - Y * other.X);
        }

        /// <summary>
        /// Unary minus operator.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <returns>New vector with opposite coordinates.</returns>
        public static Vector3D operator -(Vector3D a)
        {
            return new Vector3D(-a.X, -a.Y, -a.Z);
        }

        /// <summary>
        /// Adds elements of two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>New vector with added elements.</returns>
        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Subtracts elements of two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>New vector with subtracted elements.</returns>
        public static Vector3D operator -(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
    }
}
