using System;
using GaemMeong.World.Generation;

namespace GaemMeong.World.Settings
{
    public sealed class WorldGenerationConfig
    {
        private WorldGenerationConfig(
            string seedText,
            ulong worldSeed,
            int worldWidth,
            int worldLength,
            int chunkSize,
            TerrainPreset terrainPreset,
            LandmassPreset landmassPreset,
            float heightScale)
        {
            SeedText = seedText;
            WorldSeed = worldSeed;
            WorldWidth = worldWidth;
            WorldLength = worldLength;
            ChunkSize = chunkSize;
            TerrainPreset = terrainPreset;
            LandmassPreset = landmassPreset;
            HeightScale = heightScale;
        }

        public string SeedText { get; }

        public ulong WorldSeed { get; }

        public int WorldWidth { get; }

        public int WorldLength { get; }

        public int ChunkSize { get; }

        public TerrainPreset TerrainPreset { get; }

        public LandmassPreset LandmassPreset { get; }

        public float HeightScale { get; }

        public int GenerationVersion => WorldGenerationConstants.CurrentVersion;

        public int TileCount => checked(WorldWidth * WorldLength);

        public int ChunkCountX => WorldWidth / ChunkSize;

        public int ChunkCountZ => WorldLength / ChunkSize;

        public static WorldGenerationConfig Create(
            string seedText,
            int worldWidth,
            int worldLength,
            int chunkSize,
            TerrainPreset terrainPreset,
            LandmassPreset landmassPreset,
            float heightScale)
        {
            if (string.IsNullOrWhiteSpace(seedText))
            {
                throw new ArgumentException("Seed text cannot be empty.", nameof(seedText));
            }

            if (worldWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(worldWidth), "World width must be positive.");
            }

            if (worldLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(worldLength), "World length must be positive.");
            }

            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be positive.");
            }

            if (worldWidth % chunkSize != 0 || worldLength % chunkSize != 0)
            {
                throw new ArgumentException("World dimensions must be divisible by the chunk size.");
            }

            long tileCount = (long)worldWidth * worldLength;
            if (tileCount > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(worldLength), "World tile count exceeds the supported array size.");
            }

            if (float.IsNaN(heightScale) || float.IsInfinity(heightScale) || heightScale <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(heightScale), "Height scale must be finite and positive.");
            }

            if (!Enum.IsDefined(typeof(TerrainPreset), terrainPreset))
            {
                throw new ArgumentOutOfRangeException(nameof(terrainPreset));
            }

            if (!Enum.IsDefined(typeof(LandmassPreset), landmassPreset))
            {
                throw new ArgumentOutOfRangeException(nameof(landmassPreset));
            }

            return new WorldGenerationConfig(
                seedText.Trim(),
                WorldSeedHasher.Parse(seedText),
                worldWidth,
                worldLength,
                chunkSize,
                terrainPreset,
                landmassPreset,
                heightScale);
        }
    }
}
