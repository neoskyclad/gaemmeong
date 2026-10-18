using GaemMeong.World.Coordinates;
using NUnit.Framework;

namespace GaemMeong.Tests
{
    public sealed class WorldGridTests
    {
        [Test]
        public void EveryPrototypeCoordinateRoundTripsThroughFlatIndex()
        {
            WorldGrid grid = new WorldGrid(512, 512);

            for (int z = 0; z < grid.Length; z++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    int index = grid.GetIndex(x, z);
                    TileCoordinate coordinate = grid.GetCoordinate(index);
                    if (coordinate.X != x || coordinate.Z != z)
                    {
                        Assert.Fail($"Coordinate ({x}, {z}) round-tripped as {coordinate}.");
                    }
                }
            }

            Assert.That(grid.TileCount, Is.EqualTo(262144));
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(512, 0)]
        [TestCase(0, 512)]
        public void OutOfBoundsCoordinatesAreRejected(int x, int z)
        {
            WorldGrid grid = new WorldGrid(512, 512);

            Assert.That(grid.TryGetIndex(x, z, out int index), Is.False);
            Assert.That(index, Is.EqualTo(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => grid.GetIndex(x, z));
        }

        [Test]
        public void InvalidFlatIndicesAreRejected()
        {
            WorldGrid grid = new WorldGrid(512, 512);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => grid.GetCoordinate(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => grid.GetCoordinate(grid.TileCount));
        }
    }
}
