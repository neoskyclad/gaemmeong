using System;

namespace GaemMeong.World.Coordinates
{
    public sealed class ChunkLayout
    {
        public ChunkLayout(int worldWidth, int worldLength, int chunkSize)
        {
            if (worldWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(worldWidth));
            }

            if (worldLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(worldLength));
            }

            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize));
            }

            if (worldWidth % chunkSize != 0 || worldLength % chunkSize != 0)
            {
                throw new ArgumentException("World dimensions must be divisible by the chunk size.");
            }

            WorldWidth = worldWidth;
            WorldLength = worldLength;
            ChunkSize = chunkSize;
            CountX = worldWidth / chunkSize;
            CountZ = worldLength / chunkSize;
            Count = checked(CountX * CountZ);
        }

        public int WorldWidth { get; }

        public int WorldLength { get; }

        public int ChunkSize { get; }

        public int CountX { get; }

        public int CountZ { get; }

        public int Count { get; }

        public ChunkCoordinate GetChunkForTile(int tileX, int tileZ)
        {
            ValidateTileCoordinate(tileX, tileZ);
            return new ChunkCoordinate(tileX / ChunkSize, tileZ / ChunkSize);
        }

        public TileCoordinate GetLocalTile(int tileX, int tileZ)
        {
            ValidateTileCoordinate(tileX, tileZ);
            return new TileCoordinate(tileX % ChunkSize, tileZ % ChunkSize);
        }

        public int GetChunkIndex(int chunkX, int chunkZ)
        {
            ValidateChunkCoordinate(chunkX, chunkZ);
            return (chunkZ * CountX) + chunkX;
        }

        public ChunkCoordinate GetChunkCoordinate(int chunkIndex)
        {
            if (chunkIndex < 0 || chunkIndex >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));
            }

            int z = chunkIndex / CountX;
            int x = chunkIndex - (z * CountX);
            return new ChunkCoordinate(x, z);
        }

        public ChunkBounds GetBounds(int chunkX, int chunkZ)
        {
            ValidateChunkCoordinate(chunkX, chunkZ);
            return new ChunkBounds(
                chunkX * ChunkSize,
                chunkZ * ChunkSize,
                ChunkSize,
                ChunkSize);
        }

        private void ValidateTileCoordinate(int tileX, int tileZ)
        {
            if (tileX < 0 || tileX >= WorldWidth || tileZ < 0 || tileZ >= WorldLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tileX),
                    $"Tile coordinate ({tileX}, {tileZ}) is outside the chunk layout.");
            }
        }

        private void ValidateChunkCoordinate(int chunkX, int chunkZ)
        {
            if (chunkX < 0 || chunkX >= CountX || chunkZ < 0 || chunkZ >= CountZ)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(chunkX),
                    $"Chunk coordinate ({chunkX}, {chunkZ}) is outside the chunk layout.");
            }
        }
    }
}
