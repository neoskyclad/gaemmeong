using System;

namespace GaemMeong.World.Coordinates
{
    public readonly struct TileCoordinate : IEquatable<TileCoordinate>
    {
        public TileCoordinate(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int X { get; }

        public int Z { get; }

        public bool Equals(TileCoordinate other)
        {
            return X == other.X && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is TileCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Z;
            }
        }

        public override string ToString()
        {
            return $"({X}, {Z})";
        }

        public static bool operator ==(TileCoordinate left, TileCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TileCoordinate left, TileCoordinate right)
        {
            return !left.Equals(right);
        }
    }
}
