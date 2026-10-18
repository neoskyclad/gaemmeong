using System;

namespace GaemMeong.World.Coordinates
{
    public sealed class WorldGrid
    {
        public WorldGrid(int width, int length)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "World width must be positive.");
            }

            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "World length must be positive.");
            }

            long tileCount = (long)width * length;
            if (tileCount > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "World tile count exceeds the supported array size.");
            }

            Width = width;
            Length = length;
            TileCount = (int)tileCount;
        }

        public int Width { get; }

        public int Length { get; }

        public int TileCount { get; }

        public bool IsInBounds(int x, int z)
        {
            return x >= 0 && x < Width && z >= 0 && z < Length;
        }

        public int GetIndex(int x, int z)
        {
            if (!IsInBounds(x, z))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(x),
                    $"Tile coordinate ({x}, {z}) is outside a {Width} x {Length} world.");
            }

            return (z * Width) + x;
        }

        public bool TryGetIndex(int x, int z, out int index)
        {
            if (!IsInBounds(x, z))
            {
                index = -1;
                return false;
            }

            index = (z * Width) + x;
            return true;
        }

        public TileCoordinate GetCoordinate(int index)
        {
            if (index < 0 || index >= TileCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            int z = index / Width;
            int x = index - (z * Width);
            return new TileCoordinate(x, z);
        }
    }
}
