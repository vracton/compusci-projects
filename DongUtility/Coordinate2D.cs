namespace DongUtility
{
    /// <summary>
    /// A simple two-dimensional integer coordinate
    /// </summary>
    public struct Coordinate2D(int x, int y) : IEquatable<Coordinate2D>
    {
        public int X { get; set; } = x;
        public int Y { get; set; } = y;



        public override readonly bool Equals(object? obj)
        {
            return obj is Coordinate2D coord && Equals(coord);
        }

        public readonly bool Equals(Coordinate2D other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        static public bool operator ==(Coordinate2D lhs, Coordinate2D rhs)
        {
            return lhs.Equals(rhs);
        }

        static public bool operator !=(Coordinate2D lhs, Coordinate2D rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override readonly string ToString()
        {
            return "[ " + X + ", " + Y + " ]";
        }
    }
}

