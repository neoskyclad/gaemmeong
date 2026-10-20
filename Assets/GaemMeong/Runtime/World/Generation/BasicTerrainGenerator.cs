using System;
using GaemMeong.World.Data;
using GaemMeong.World.Noise;
using GaemMeong.World.Settings;

namespace GaemMeong.World.Generation
{
    public sealed class BasicTerrainGenerator
    {
        public void Generate(GameWorldData world, WorldGenerationDebugData debugData, WorldGenerationConfig config)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            if (debugData == null)
            {
                throw new ArgumentNullException(nameof(debugData));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (world.Grid.TileCount != debugData.BaseNoise.Length)
            {
                throw new ArgumentException("Debug data size must match the world size.", nameof(debugData));
            }

            if (world.Grid.Width != config.WorldWidth
                || world.Grid.Length != config.WorldLength
                || world.Chunks.ChunkSize != config.ChunkSize)
            {
                throw new ArgumentException("World data must match the generation configuration.", nameof(world));
            }

            ulong terrainSeed = WorldSeedHasher.Derive(config.WorldSeed, SeedDomain.Terrain);
            double elevationRange = WorldGenerationConstants.MaximumElevation - WorldGenerationConstants.MinimumElevation;

            for (int z = 0; z < world.Grid.Length; z++)
            {
                for (int x = 0; x < world.Grid.Width; x++)
                {
                    int index = world.Grid.GetIndex(x, z);
                    float normalizedNoise = FractalNoise2D.SampleNormalized(
                        x,
                        z,
                        terrainSeed,
                        GenerationSettings.BaseWavelength,
                        GenerationSettings.Octaves,
                        GenerationSettings.Lacunarity,
                        GenerationSettings.Persistence);

                    debugData.BaseNoise[index] = normalizedNoise;
                    double elevation = WorldGenerationConstants.MinimumElevation + (normalizedNoise * elevationRange);
                    elevation = Math.Round(elevation, MidpointRounding.AwayFromZero);
                    elevation = Math.Max(WorldGenerationConstants.MinimumElevation, elevation);
                    elevation = Math.Min(WorldGenerationConstants.MaximumElevation, elevation);
                    world.Elevation[index] = (short)elevation;
                }
            }
        }
    }
}
