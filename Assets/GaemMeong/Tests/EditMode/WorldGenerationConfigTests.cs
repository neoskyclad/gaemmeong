using GaemMeong.World.Settings;
using NUnit.Framework;
using UnityEngine;

namespace GaemMeong.Tests
{
    public sealed class WorldGenerationConfigTests
    {
        [Test]
        public void PrototypeConfigurationExposesExpectedDerivedValues()
        {
            WorldGenerationConfig config = CreateValidConfig();

            Assert.That(config.SeedText, Is.EqualTo("test-seed"));
            Assert.That(config.TileCount, Is.EqualTo(262144));
            Assert.That(config.ChunkCountX, Is.EqualTo(8));
            Assert.That(config.ChunkCountZ, Is.EqualTo(8));
            Assert.That(config.GenerationVersion, Is.EqualTo(1));
        }

        [Test]
        public void ScriptableObjectDefaultsCreatePrototypeConfiguration()
        {
            GameWorldSettings settings = ScriptableObject.CreateInstance<GameWorldSettings>();

            try
            {
                WorldGenerationConfig config = settings.CreateConfig();
                Assert.That(config.WorldWidth, Is.EqualTo(512));
                Assert.That(config.WorldLength, Is.EqualTo(512));
                Assert.That(config.ChunkSize, Is.EqualTo(64));
                Assert.That(config.TerrainPreset, Is.EqualTo(TerrainPreset.Balanced));
                Assert.That(config.LandmassPreset, Is.EqualTo(LandmassPreset.Continents));
                Assert.That(config.HeightScale, Is.EqualTo(0.25f));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [TestCase(0, 512, 64)]
        [TestCase(512, 0, 64)]
        [TestCase(512, 512, 0)]
        public void NonPositiveDimensionsAreRejected(int width, int length, int chunkSize)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                WorldGenerationConfig.Create(
                    "seed",
                    width,
                    length,
                    chunkSize,
                    TerrainPreset.Balanced,
                    LandmassPreset.Continents,
                    0.25f));
        }

        [Test]
        public void DimensionsMustBeDivisibleByChunkSize()
        {
            Assert.Throws<System.ArgumentException>(() =>
                WorldGenerationConfig.Create(
                    "seed",
                    500,
                    512,
                    64,
                    TerrainPreset.Balanced,
                    LandmassPreset.Continents,
                    0.25f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositiveHeightScaleIsRejected(float heightScale)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                WorldGenerationConfig.Create(
                    "seed",
                    512,
                    512,
                    64,
                    TerrainPreset.Balanced,
                    LandmassPreset.Continents,
                    heightScale));
        }

        [Test]
        public void EmptySeedIsRejected()
        {
            Assert.Throws<System.ArgumentException>(() =>
                WorldGenerationConfig.Create(
                    " ",
                    512,
                    512,
                    64,
                    TerrainPreset.Balanced,
                    LandmassPreset.Continents,
                    0.25f));
        }

        private static WorldGenerationConfig CreateValidConfig()
        {
            return WorldGenerationConfig.Create(
                "test-seed",
                512,
                512,
                64,
                TerrainPreset.Balanced,
                LandmassPreset.Continents,
                0.25f);
        }
    }
}
