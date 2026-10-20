using System;
using GaemMeong.World.Data;

namespace GaemMeong.World.Generation
{
    public static class WorldArrayChecksum
    {
        private const ulong FnvOffsetBasis = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        public static ulong ComputeElevation(GameWorldData world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            ulong hash = FnvOffsetBasis;
            MixInt32(ref hash, world.Grid.Width);
            MixInt32(ref hash, world.Grid.Length);

            for (int index = 0; index < world.Elevation.Length; index++)
            {
                ushort value = unchecked((ushort)world.Elevation[index]);
                MixByte(ref hash, (byte)value);
                MixByte(ref hash, (byte)(value >> 8));
            }

            return hash;
        }

        public static ulong ComputeWorldData(GameWorldData world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            ulong hash = FnvOffsetBasis;
            MixInt32(ref hash, world.Grid.Width);
            MixInt32(ref hash, world.Grid.Length);

            for (int index = 0; index < world.Elevation.Length; index++)
            {
                ushort elevation = unchecked((ushort)world.Elevation[index]);
                MixByte(ref hash, (byte)elevation);
                MixByte(ref hash, (byte)(elevation >> 8));
                MixByte(ref hash, (byte)world.Water[index]);
            }

            return hash;
        }

        private static void MixInt32(ref ulong hash, int value)
        {
            uint bits = unchecked((uint)value);
            MixByte(ref hash, (byte)bits);
            MixByte(ref hash, (byte)(bits >> 8));
            MixByte(ref hash, (byte)(bits >> 16));
            MixByte(ref hash, (byte)(bits >> 24));
        }

        private static void MixByte(ref ulong hash, byte value)
        {
            hash ^= value;
            hash *= FnvPrime;
        }
    }
}
