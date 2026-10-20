using System;
using System.Globalization;
using System.Text;

namespace GaemMeong.World.Generation
{
    public static class WorldSeedHasher
    {
        private const ulong FnvOffsetBasis = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        public static ulong Parse(string seedText)
        {
            if (string.IsNullOrWhiteSpace(seedText))
            {
                throw new ArgumentException("Seed text cannot be empty.", nameof(seedText));
            }

            string normalized = seedText.Trim();
            if (ulong.TryParse(normalized, NumberStyles.None, CultureInfo.InvariantCulture, out ulong numericSeed))
            {
                return numericSeed;
            }

            return HashUtf8(normalized);
        }

        public static ulong HashUtf8(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            ulong hash = FnvOffsetBasis;
            for (int index = 0; index < bytes.Length; index++)
            {
                hash ^= bytes[index];
                hash *= FnvPrime;
            }

            return hash;
        }

        public static ulong Derive(ulong worldSeed, SeedDomain domain)
        {
            return SplitMix64.Mix(worldSeed ^ (ulong)domain);
        }

        public static ulong HashCoordinates(ulong seed, int x, int z)
        {
            ulong xBits = unchecked((uint)x);
            ulong zBits = unchecked((uint)z);
            ulong mixedX = SplitMix64.Mix(xBits + 0x9E3779B97F4A7C15UL);
            ulong mixedZ = SplitMix64.Mix(zBits + 0xD1B54A32D192ED03UL);
            return SplitMix64.Mix(seed ^ mixedX ^ RotateLeft(mixedZ, 29));
        }

        private static ulong RotateLeft(ulong value, int amount)
        {
            return (value << amount) | (value >> (64 - amount));
        }
    }
}
