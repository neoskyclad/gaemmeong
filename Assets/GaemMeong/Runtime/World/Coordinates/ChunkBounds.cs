namespace GaemMeong.World.Coordinates
{
    public readonly struct ChunkBounds
    {
        public ChunkBounds(int minX, int minZ, int width, int length)
        {
            MinX = minX;
            MinZ = minZ;
            Width = width;
            Length = length;
        }

        public int MinX { get; }

        public int MinZ { get; }

        public int Width { get; }

        public int Length { get; }

        public int MaxXExclusive => MinX + Width;

        public int MaxZExclusive => MinZ + Length;

        public bool Contains(int tileX, int tileZ)
        {
            return tileX >= MinX
                && tileX < MaxXExclusive
                && tileZ >= MinZ
                && tileZ < MaxZExclusive;
        }

        public TileCoordinate LocalToWorld(int localX, int localZ)
        {
            if (localX < 0 || localX >= Width)
            {
                throw new System.ArgumentOutOfRangeException(nameof(localX));
            }

            if (localZ < 0 || localZ >= Length)
            {
                throw new System.ArgumentOutOfRangeException(nameof(localZ));
            }

            return new TileCoordinate(MinX + localX, MinZ + localZ);
        }
    }
}
