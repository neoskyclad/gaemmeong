using System;
using GaemMeong.World.Generation;

namespace GaemMeong.World.Noise
{
    public static class GradientNoise2D
    {
        private const double Diagonal = 0.7071067811865475244;

        public static double Sample(double x, double z, ulong seed)
        {
            if (double.IsNaN(x) || double.IsInfinity(x))
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (double.IsNaN(z) || double.IsInfinity(z))
            {
                throw new ArgumentOutOfRangeException(nameof(z));
            }

            int x0 = checked((int)Math.Floor(x));
            int z0 = checked((int)Math.Floor(z));
            int x1 = checked(x0 + 1);
            int z1 = checked(z0 + 1);
            double localX = x - x0;
            double localZ = z - z0;

            double lowerLeft = DotGradient(seed, x0, z0, localX, localZ);
            double lowerRight = DotGradient(seed, x1, z0, localX - 1.0, localZ);
            double upperLeft = DotGradient(seed, x0, z1, localX, localZ - 1.0);
            double upperRight = DotGradient(seed, x1, z1, localX - 1.0, localZ - 1.0);

            double blendX = Fade(localX);
            double blendZ = Fade(localZ);
            double lower = Lerp(lowerLeft, lowerRight, blendX);
            double upper = Lerp(upperLeft, upperRight, blendX);
            return Clamp(Lerp(lower, upper, blendZ), -1.0, 1.0);
        }

        private static double DotGradient(ulong seed, int latticeX, int latticeZ, double offsetX, double offsetZ)
        {
            ulong hash = WorldSeedHasher.HashCoordinates(seed, latticeX, latticeZ);
            switch ((int)(hash & 7UL))
            {
                case 0: return offsetX;
                case 1: return -offsetX;
                case 2: return offsetZ;
                case 3: return -offsetZ;
                case 4: return (offsetX + offsetZ) * Diagonal;
                case 5: return (offsetX - offsetZ) * Diagonal;
                case 6: return (-offsetX + offsetZ) * Diagonal;
                default: return (-offsetX - offsetZ) * Diagonal;
            }
        }

        private static double Fade(double value)
        {
            return value * value * value * (value * (value * 6.0 - 15.0) + 10.0);
        }

        private static double Lerp(double from, double to, double amount)
        {
            return from + ((to - from) * amount);
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }
    }
}
