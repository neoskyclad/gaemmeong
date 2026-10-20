using GaemMeong.World.Generation;
using GaemMeong.World.Noise;
using NUnit.Framework;

namespace GaemMeong.Tests
{
    public sealed class FractalNoise2DTests
    {
        [Test]
        public void SamplesAreDeterministicAndNormalized()
        {
            ulong seed = WorldSeedHasher.Parse("noise-test");
            float first = FractalNoise2D.SampleNormalized(128.5, 256.25, seed, 192.0, 4, 2.0, 0.5);
            float second = FractalNoise2D.SampleNormalized(128.5, 256.25, seed, 192.0, 4, 2.0, 0.5);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first, Is.InRange(0f, 1f));
        }

        [Test]
        public void AdjacentWorldSamplesRemainContinuousAcrossChunkBoundary()
        {
            ulong seed = WorldSeedHasher.Parse("boundary-test");
            float beforeBoundary = FractalNoise2D.SampleNormalized(63.99, 128.0, seed, 192.0, 4, 2.0, 0.5);
            float afterBoundary = FractalNoise2D.SampleNormalized(64.01, 128.0, seed, 192.0, 4, 2.0, 0.5);

            Assert.That(System.Math.Abs(afterBoundary - beforeBoundary), Is.LessThan(0.01f));
        }

        [Test]
        public void DifferentSeedsProduceDifferentSamples()
        {
            float first = FractalNoise2D.SampleNormalized(87.0, 119.0, 1UL, 192.0, 4, 2.0, 0.5);
            float second = FractalNoise2D.SampleNormalized(87.0, 119.0, 2UL, 192.0, 4, 2.0, 0.5);

            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void InvalidNoiseParametersAreRejected()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                FractalNoise2D.SampleNormalized(0, 0, 1UL, 0, 4, 2.0, 0.5));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                FractalNoise2D.SampleNormalized(0, 0, 1UL, 192.0, 0, 2.0, 0.5));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                FractalNoise2D.SampleNormalized(0, 0, 1UL, 192.0, 4, 2.0, 2.0));
        }
    }
}
