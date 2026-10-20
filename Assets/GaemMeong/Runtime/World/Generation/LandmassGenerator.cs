using System;
using GaemMeong.World.Data;
using GaemMeong.World.Noise;
using GaemMeong.World.Settings;

namespace GaemMeong.World.Generation
{
    public sealed class LandmassGenerator
    {
        public void Generate(GameWorldData world, WorldGenerationDebugData debugData, WorldGenerationConfig config)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (debugData == null) throw new ArgumentNullException(nameof(debugData));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (world.Grid.TileCount != debugData.BaseNoise.Length)
                throw new ArgumentException("Debug data size must match the world size.", nameof(debugData));

            debugData.LandmassMask = new float[world.Grid.TileCount];
            debugData.MountainMask = new float[world.Grid.TileCount];
            debugData.Slope = new float[world.Grid.TileCount];

            ulong landSeed = WorldSeedHasher.Derive(config.WorldSeed, SeedDomain.Landmass);
            ulong coastSeed = WorldSeedHasher.Derive(config.WorldSeed, SeedDomain.Coastline);
            ulong mountainSeed = WorldSeedHasher.Derive(config.WorldSeed, SeedDomain.MountainMask);
            double mapScale = Math.Min(config.WorldWidth, config.WorldLength);
            double landWavelength = Math.Max(24.0, mapScale / 2.8);
            double coastWavelength = Math.Max(12.0, mapScale / 8.0);
            float threshold = GetLandThreshold(config.LandmassPreset);
            float mountainThreshold = GetMountainThreshold(config.TerrainPreset);

            for (int z = 0; z < world.Grid.Length; z++)
            {
                for (int x = 0; x < world.Grid.Width; x++)
                {
                    int index = world.Grid.GetIndex(x, z);
                    double nx = ((x + 0.5) / config.WorldWidth) * 2.0 - 1.0;
                    double nz = ((z + 0.5) / config.WorldLength) * 2.0 - 1.0;
                    double edgeDistance = Math.Sqrt((nx * nx) + (nz * nz));
                    float landNoise = FractalNoise2D.SampleNormalized(x, z, landSeed, landWavelength, 3, 2.0, 0.52);
                    float coastNoise = FractalNoise2D.SampleNormalized(x, z, coastSeed, coastWavelength, 2, 2.0, 0.5);
                    float score = (landNoise * 0.65f) + (coastNoise * 0.15f) + ((float)Math.Max(0.0, 1.0 - edgeDistance) * 0.20f);
                    debugData.LandmassMask[index] = score;

                    float mountainNoise = FractalNoise2D.SampleNormalized(x, z, mountainSeed, landWavelength / 2.0, 4, 2.0, 0.5);
                    debugData.MountainMask[index] = SmoothStep(mountainThreshold, mountainThreshold + 0.22f, mountainNoise);
                }
            }

            ApplyElevation(world, debugData, config, threshold);
            ClassifyConnectedOcean(world, debugData.LandmassMask, threshold);
            CalculateSlope(world, debugData);
        }

        private static void ApplyElevation(GameWorldData world, WorldGenerationDebugData debugData, WorldGenerationConfig config, float landThreshold)
        {
            float terrainRange = config.TerrainPreset switch
            {
                TerrainPreset.Plains => 10f,
                TerrainPreset.Mountainous => 43f,
                _ => 25f,
            };

            for (int index = 0; index < world.Grid.TileCount; index++)
            {
                float baseNoise = debugData.BaseNoise[index];
                if (debugData.LandmassMask[index] >= landThreshold)
                {
                    float elevation = 1f + (baseNoise * terrainRange) + (debugData.MountainMask[index] * terrainRange * 0.62f);
                    world.Elevation[index] = (short)Math.Min(WorldGenerationConstants.MaximumElevation, Math.Round(elevation));
                }
                else
                {
                    float depth = 1f + ((1f - baseNoise) * 15f);
                    world.Elevation[index] = (short)-Math.Min(15f, depth);
                }
            }
        }

        private static void ClassifyConnectedOcean(GameWorldData world, float[] landmassMask, float threshold)
        {
            int width = world.Grid.Width;
            int length = world.Grid.Length;
            bool[] visited = new bool[world.Grid.TileCount];
            int[] queue = new int[world.Grid.TileCount];
            int head = 0;
            int tail = 0;

            for (int x = 0; x < width; x++)
            {
                EnqueueWater(x, 0, world, landmassMask, threshold, visited, queue, ref tail);
                EnqueueWater(x, length - 1, world, landmassMask, threshold, visited, queue, ref tail);
            }
            for (int z = 1; z < length - 1; z++)
            {
                EnqueueWater(0, z, world, landmassMask, threshold, visited, queue, ref tail);
                EnqueueWater(width - 1, z, world, landmassMask, threshold, visited, queue, ref tail);
            }

            while (head < tail)
            {
                int index = queue[head++];
                int x = index % width;
                int z = index / width;
                if (x > 0) EnqueueWater(x - 1, z, world, landmassMask, threshold, visited, queue, ref tail);
                if (x + 1 < width) EnqueueWater(x + 1, z, world, landmassMask, threshold, visited, queue, ref tail);
                if (z > 0) EnqueueWater(x, z - 1, world, landmassMask, threshold, visited, queue, ref tail);
                if (z + 1 < length) EnqueueWater(x, z + 1, world, landmassMask, threshold, visited, queue, ref tail);
            }

            for (int index = 0; index < world.Grid.TileCount; index++)
            {
                world.Water[index] = visited[index] ? WaterType.Ocean : WaterType.Dry;
            }
        }

        private static void EnqueueWater(int x, int z, GameWorldData world, float[] landmassMask, float threshold, bool[] visited, int[] queue, ref int tail)
        {
            int index = world.Grid.GetIndex(x, z);
            if (visited[index] || landmassMask[index] >= threshold) return;
            visited[index] = true;
            world.Elevation[index] = (short)Math.Min(-1, (int)world.Elevation[index]);
            world.Water[index] = WaterType.Ocean;
            queue[tail++] = index;
        }

        private static void CalculateSlope(GameWorldData world, WorldGenerationDebugData debugData)
        {
            int width = world.Grid.Width;
            int length = world.Grid.Length;
            for (int z = 0; z < length; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    short elevation = world.Elevation[world.Grid.GetIndex(x, z)];
                    int maxDelta = 0;
                    if (x > 0) maxDelta = Math.Max(maxDelta, Math.Abs(elevation - world.Elevation[world.Grid.GetIndex(x - 1, z)]));
                    if (x + 1 < width) maxDelta = Math.Max(maxDelta, Math.Abs(elevation - world.Elevation[world.Grid.GetIndex(x + 1, z)]));
                    if (z > 0) maxDelta = Math.Max(maxDelta, Math.Abs(elevation - world.Elevation[world.Grid.GetIndex(x, z - 1)]));
                    if (z + 1 < length) maxDelta = Math.Max(maxDelta, Math.Abs(elevation - world.Elevation[world.Grid.GetIndex(x, z + 1)]));
                    debugData.Slope[world.Grid.GetIndex(x, z)] = Math.Min(1f, maxDelta / 16f);
                }
            }
        }

        private static float GetLandThreshold(LandmassPreset preset)
        {
            return preset switch
            {
                LandmassPreset.Archipelago => 0.56f,
                LandmassPreset.Pangaea => 0.42f,
                _ => 0.49f,
            };
        }

        private static float GetMountainThreshold(TerrainPreset preset)
        {
            return preset switch
            {
                TerrainPreset.Plains => 0.76f,
                TerrainPreset.Mountainous => 0.47f,
                _ => 0.62f,
            };
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float t = Math.Max(0f, Math.Min(1f, (value - edge0) / (edge1 - edge0)));
            return t * t * (3f - (2f * t));
        }
    }
}
