using GaemMeong.World.Coordinates;
using NUnit.Framework;

namespace GaemMeong.Tests
{
    public sealed class ChunkLayoutTests
    {
        [Test]
        public void PrototypeLayoutContainsEightByEightChunks()
        {
            ChunkLayout layout = new ChunkLayout(512, 512, 64);

            Assert.That(layout.CountX, Is.EqualTo(8));
            Assert.That(layout.CountZ, Is.EqualTo(8));
            Assert.That(layout.Count, Is.EqualTo(64));
        }

        [Test]
        public void EveryPrototypeTileMapsIntoExactlyOneExpectedChunk()
        {
            ChunkLayout layout = new ChunkLayout(512, 512, 64);

            for (int z = 0; z < layout.WorldLength; z++)
            {
                for (int x = 0; x < layout.WorldWidth; x++)
                {
                    ChunkCoordinate chunk = layout.GetChunkForTile(x, z);
                    TileCoordinate local = layout.GetLocalTile(x, z);
                    ChunkBounds bounds = layout.GetBounds(chunk.X, chunk.Z);

                    if (!bounds.Contains(x, z))
                    {
                        Assert.Fail($"Tile ({x}, {z}) is not inside chunk {chunk} bounds.");
                    }

                    TileCoordinate roundTrip = bounds.LocalToWorld(local.X, local.Z);
                    if (roundTrip.X != x || roundTrip.Z != z)
                    {
                        Assert.Fail($"Tile ({x}, {z}) round-tripped as {roundTrip}.");
                    }
                }
            }
        }

        [Test]
        public void EveryChunkIndexRoundTrips()
        {
            ChunkLayout layout = new ChunkLayout(512, 512, 64);

            for (int index = 0; index < layout.Count; index++)
            {
                ChunkCoordinate coordinate = layout.GetChunkCoordinate(index);
                Assert.That(layout.GetChunkIndex(coordinate.X, coordinate.Z), Is.EqualTo(index));
            }
        }

        [Test]
        public void PartialEdgeChunksAreRejectedForPrototype()
        {
            Assert.Throws<System.ArgumentException>(() => new ChunkLayout(500, 512, 64));
            Assert.Throws<System.ArgumentException>(() => new ChunkLayout(512, 500, 64));
        }
    }
}
