using GaemMeong.World.Data;

namespace GaemMeong.World.Generation
{
    public sealed class WorldGenerationOutput
    {
        public WorldGenerationOutput(GameWorldData world, WorldGenerationDebugData debugData, ulong checksum)
        {
            World = world;
            DebugData = debugData;
            Checksum = checksum;
        }

        public GameWorldData World { get; }

        public WorldGenerationDebugData DebugData { get; }

        public ulong Checksum { get; }
    }
}
