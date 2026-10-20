using System;
using GaemMeong.World.Generation;
using GaemMeong.World.Settings;
using NUnit.Framework;

namespace GaemMeong.Tests.EditMode
{
    public sealed class WorldGeneratorTests
    {
        [TestCase(TerrainPreset.Plains, LandmassPreset.Continents)]
        [TestCase(TerrainPreset.Plains, LandmassPreset.Archipelago)]
        [TestCase(TerrainPreset.Plains, LandmassPreset.Pangaea)]
        [TestCase(TerrainPreset.Balanced, LandmassPreset.Continents)]
        [TestCase(TerrainPreset.Balanced, LandmassPreset.Archipelago)]
        [TestCase(TerrainPreset.Balanced, LandmassPreset.Pangaea)]
        [TestCase(TerrainPreset.Mountainous, LandmassPreset.Continents)]
        [TestCase(TerrainPreset.Mountainous, LandmassPreset.Archipelago)]
        [TestCase(TerrainPreset.Mountainous, LandmassPreset.Pangaea)]
        public void Generate_AllPresetCombinationsProduceValidWorld(TerrainPreset terrain, LandmassPreset landmass)
        {
            WorldGenerationOutput output = new WorldGenerator().Generate(CreateConfig(terrain, landmass));

            Assert.That(output.World.Elevation.Length, Is.EqualTo(128 * 128));
            Assert.That(output.DebugData.LandmassMask.Length, Is.EqualTo(128 * 128));
            Assert.That(output.DebugData.MountainMask.Length, Is.EqualTo(128 * 128));
            Assert.That(output.DebugData.Slope.Length, Is.EqualTo(128 * 128));

            int oceanTiles = 0;
            int dryTiles = 0;
            int positiveElevationTiles = 0;
            for (int index = 0; index < output.World.Elevation.Length; index++)
            {
                Assert.That(output.World.Elevation[index], Is.InRange(-16, 48));
                Assert.That(output.DebugData.LandmassMask[index], Is.InRange(0f, 1f));
                Assert.That(output.DebugData.MountainMask[index], Is.InRange(0f, 1f));
                Assert.That(output.DebugData.Slope[index], Is.InRange(0f, 1f));
                if (output.World.Water[index] == GaemMeong.World.Data.WaterType.Ocean)
                {
                    oceanTiles++;
                    Assert.That(output.World.Elevation[index], Is.LessThanOrEqualTo(0));
                }
                else
                {
                    dryTiles++;
                }
                if (output.World.Elevation[index] > 0) positiveElevationTiles++;
            }

            Assert.That(oceanTiles, Is.GreaterThan(0));
            Assert.That(dryTiles, Is.GreaterThan(0));
            Assert.That(positiveElevationTiles, Is.GreaterThan(0));
        }

        [Test]
        public void Generate_SameSeedAndSettingsProduceSameChecksum()
        {
            WorldGenerationConfig config = CreateConfig(TerrainPreset.Balanced, LandmassPreset.Continents);
            WorldGenerationOutput first = new WorldGenerator().Generate(config);
            WorldGenerationOutput second = new WorldGenerator().Generate(config);

            Assert.That(second.Checksum, Is.EqualTo(first.Checksum));
            CollectionAssert.AreEqual(first.World.Elevation, second.World.Elevation);
            CollectionAssert.AreEqual(first.World.Water, second.World.Water);
        }

        [Test]
        public void Generate_PangaeaHasMoreLandThanArchipelagoForSameSeed()
        {
            WorldGenerator generator = new WorldGenerator();
            WorldGenerationOutput archipelago = generator.Generate(CreateConfig(TerrainPreset.Balanced, LandmassPreset.Archipelago));
            WorldGenerationOutput pangaea = generator.Generate(CreateConfig(TerrainPreset.Balanced, LandmassPreset.Pangaea));

            Assert.That(CountDry(archipelago), Is.LessThan(CountDry(pangaea)));
        }

        [Test]
        public void WorldChecksumIncludesWaterClassification()
        {
            WorldGenerationOutput output = new WorldGenerator().Generate(CreateConfig(TerrainPreset.Balanced, LandmassPreset.Continents));
            ulong originalChecksum = WorldArrayChecksum.ComputeWorldData(output.World);
            output.World.Water[0] = output.World.Water[0] == GaemMeong.World.Data.WaterType.Ocean
                ? GaemMeong.World.Data.WaterType.Dry
                : GaemMeong.World.Data.WaterType.Ocean;

            Assert.That(WorldArrayChecksum.ComputeWorldData(output.World), Is.Not.EqualTo(originalChecksum));
        }

        private static int CountDry(WorldGenerationOutput output)
        {
            int count = 0;
            for (int index = 0; index < output.World.Water.Length; index++)
            {
                if (output.World.Water[index] == GaemMeong.World.Data.WaterType.Dry) count++;
            }
            return count;
        }

        private static WorldGenerationConfig CreateConfig(TerrainPreset terrain, LandmassPreset landmass)
        {
            return WorldGenerationConfig.Create("stage-four", 128, 128, 64, terrain, landmass, 0.25f);
        }
    }
}
