using System;
using GaemMeong.World.Coordinates;
using GaemMeong.World.Settings;

namespace GaemMeong.World.Data
{
    public sealed class GameWorldData
    {
        public GameWorldData(WorldGenerationConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Grid = new WorldGrid(config.WorldWidth, config.WorldLength);
            Chunks = new ChunkLayout(config.WorldWidth, config.WorldLength, config.ChunkSize);
            Elevation = new short[Grid.TileCount];
            Water = new WaterType[Grid.TileCount];
        }

        public WorldGrid Grid { get; }

        public ChunkLayout Chunks { get; }

        public short[] Elevation { get; }

        public WaterType[] Water { get; }
    }
}
