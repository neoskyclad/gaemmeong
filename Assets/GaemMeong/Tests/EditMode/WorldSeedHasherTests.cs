using GaemMeong.World.Generation;
using NUnit.Framework;

namespace GaemMeong.Tests
{
    public sealed class WorldSeedHasherTests
    {
        [Test]
        public void NumericSeedTextIsParsedAsUnsignedInteger()
        {
            Assert.That(WorldSeedHasher.Parse("123456789"), Is.EqualTo(123456789UL));
        }

        [Test]
        public void StringSeedUsesUtf8Fnv1a64()
        {
            Assert.That(WorldSeedHasher.HashUtf8("hello"), Is.EqualTo(0xA430D84680AABD0BUL));
            Assert.That(WorldSeedHasher.Parse("gaemmeong"), Is.EqualTo(WorldSeedHasher.HashUtf8("gaemmeong")));
        }

        [Test]
        public void SeedTextWhitespaceDoesNotChangeParsedSeed()
        {
            Assert.That(WorldSeedHasher.Parse(" 123 "), Is.EqualTo(WorldSeedHasher.Parse("123")));
            Assert.That(WorldSeedHasher.Parse(" gaemmeong "), Is.EqualTo(WorldSeedHasher.Parse("gaemmeong")));
        }

        [Test]
        public void DifferentDomainsAndCoordinatesProduceDifferentHashes()
        {
            ulong seed = WorldSeedHasher.Parse("gaemmeong");

            Assert.That(WorldSeedHasher.Derive(seed, SeedDomain.Terrain), Is.Not.EqualTo(
                WorldSeedHasher.Derive(seed, SeedDomain.MountainMask)));
            Assert.That(WorldSeedHasher.HashCoordinates(seed, 10, 20), Is.Not.EqualTo(
                WorldSeedHasher.HashCoordinates(seed, 10, 21)));
        }

        [Test]
        public void EmptyStringSeedIsRejected()
        {
            Assert.Throws<System.ArgumentException>(() => WorldSeedHasher.Parse(" "));
        }
    }
}
