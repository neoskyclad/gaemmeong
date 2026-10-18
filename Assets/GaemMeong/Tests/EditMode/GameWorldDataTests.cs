using GaemMeong.World.Data;
using GaemMeong.World.Settings;
using NUnit.Framework;

namespace GaemMeong.Tests
{
    public sealed class GameWorldDataTests
    {
        [Test]
        public void PrototypeDataAllocatesFlatSoaArrays()
        {
            WorldGenerationConfig config = WorldGenerationConfig.Create(
                "test-seed",
                512,
                512,
                64,
                TerrainPreset.Balanced,
                LandmassPreset.Continents,
                0.25f);

            GameWorldData world = new GameWorldData(config);

            Assert.That(world.Elevation.Length, Is.EqualTo(config.TileCount));
            Assert.That(world.Water.Length, Is.EqualTo(config.TileCount));
            Assert.That(world.Chunks.Count, Is.EqualTo(64));
            Assert.That(world.Water[0], Is.EqualTo(WaterType.Dry));
            Assert.That(world.Elevation[0], Is.EqualTo(0));
        }

        [Test]
        public void NullConfigurationIsRejected()
        {
            Assert.Throws<System.ArgumentNullException>(() => new GameWorldData(null));
        }
    }
}
