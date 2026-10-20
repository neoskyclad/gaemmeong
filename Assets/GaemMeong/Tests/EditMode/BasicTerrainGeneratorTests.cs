using GaemMeong.World.Data;
using GaemMeong.World.Generation;
using GaemMeong.World.Settings;
using NUnit.Framework;

namespace GaemMeong.Tests
{
    public sealed class BasicTerrainGeneratorTests
    {
        [Test]
        public void SameSeedProducesSameElevationAndChecksum()
        {
            WorldGenerationConfig config = CreateConfig("deterministic-123");
            GameWorldData firstWorld = new GameWorldData(config);
            GameWorldData secondWorld = new GameWorldData(config);

            new BasicTerrainGenerator().Generate(
                firstWorld,
                new WorldGenerationDebugData(config.TileCount),
                config);
            new BasicTerrainGenerator().Generate(
                secondWorld,
                new WorldGenerationDebugData(config.TileCount),
                config);

            CollectionAssert.AreEqual(firstWorld.Elevation, secondWorld.Elevation);
            Assert.That(WorldArrayChecksum.ComputeElevation(firstWorld), Is.EqualTo(
                WorldArrayChecksum.ComputeElevation(secondWorld)));
        }

        [Test]
        public void GeneratedElevationAndNoiseStayWithinSupportedRanges()
        {
            WorldGenerationConfig config = CreateConfig("range-check");
            GameWorldData world = new GameWorldData(config);
            WorldGenerationDebugData debugData = new WorldGenerationDebugData(config.TileCount);

            new BasicTerrainGenerator().Generate(world, debugData, config);

            for (int index = 0; index < world.Elevation.Length; index++)
            {
                Assert.That(world.Elevation[index], Is.InRange(
                    WorldGenerationConstants.MinimumElevation,
                    WorldGenerationConstants.MaximumElevation));
                Assert.That(debugData.BaseNoise[index], Is.InRange(0f, 1f));
            }
        }

        [Test]
        public void DifferentSeedChangesGeneratedElevation()
        {
            WorldGenerationConfig firstConfig = CreateConfig("first-seed");
            WorldGenerationConfig secondConfig = CreateConfig("second-seed");
            GameWorldData firstWorld = new GameWorldData(firstConfig);
            GameWorldData secondWorld = new GameWorldData(secondConfig);

            BasicTerrainGenerator generator = new BasicTerrainGenerator();
            generator.Generate(firstWorld, new WorldGenerationDebugData(firstConfig.TileCount), firstConfig);
            generator.Generate(secondWorld, new WorldGenerationDebugData(secondConfig.TileCount), secondConfig);

            Assert.That(WorldArrayChecksum.ComputeElevation(firstWorld), Is.Not.EqualTo(
                WorldArrayChecksum.ComputeElevation(secondWorld)));
        }

        [Test]
        public void GeneratorRejectsMismatchedDebugData()
        {
            WorldGenerationConfig config = CreateConfig("mismatch");
            GameWorldData world = new GameWorldData(config);

            Assert.Throws<System.ArgumentException>(() => new BasicTerrainGenerator().Generate(
                world,
                new WorldGenerationDebugData(64),
                config));
        }

        private static WorldGenerationConfig CreateConfig(string seed)
        {
            return WorldGenerationConfig.Create(
                seed,
                128,
                128,
                64,
                TerrainPreset.Balanced,
                LandmassPreset.Continents,
                0.25f);
        }
    }
}
