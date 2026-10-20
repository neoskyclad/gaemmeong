using System;
using GaemMeong.World.Data;
using GaemMeong.World.Settings;

namespace GaemMeong.World.Generation
{
    public sealed class WorldGenerator
    {
        private readonly BasicTerrainGenerator basicTerrainGenerator = new BasicTerrainGenerator();
        private readonly LandmassGenerator landmassGenerator = new LandmassGenerator();

        public WorldGenerationOutput Generate(WorldGenerationConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            GameWorldData world = new GameWorldData(config);
            WorldGenerationDebugData debugData = new WorldGenerationDebugData(world.Grid.TileCount);
            basicTerrainGenerator.Generate(world, debugData, config);
            landmassGenerator.Generate(world, debugData, config);

            return new WorldGenerationOutput(world, debugData, WorldArrayChecksum.ComputeWorldData(world));
        }
    }
}
