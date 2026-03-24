namespace DongUtility
{
    /// <summary>
    /// A simple three-dimensional integer coordinate
    /// </summary>
    public struct Coordinate3D(int x, int y, int z) : IEquatable<Coordinate3D>
    {
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
        public int Z { get; set; } = z;

        public override readonly bool Equals(object? obj)
        {
            return obj is Coordinate3D coord && Equals(coord);
        }

        public readonly bool Equals(Coordinate3D other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        /// <summary>
        /// Converts the coordinate to a vector (of doubles)
        /// </summary>
        public readonly Vector ToVector()
        {
            return new Vector(X, Y, Z);
        }

        static public bool operator ==(Coordinate3D lhs, Coordinate3D rhs)
        {
            return lhs.Equals(rhs);
        }

        static public bool operator !=(Coordinate3D lhs, Coordinate3D rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override readonly string ToString()
        {
            return "[ " + X + ", " + Y + ", " + Z + " ]";
        }
    }
}

