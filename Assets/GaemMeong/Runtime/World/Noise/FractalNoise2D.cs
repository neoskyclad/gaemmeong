using System;

namespace GaemMeong.World.Noise
{
    public static class FractalNoise2D
    {
        public static float SampleNormalized(
            double worldX,
            double worldZ,
            ulong seed,
            double baseWavelength,
            int octaves,
            double lacunarity,
            double persistence)
        {
            ValidateParameters(worldX, worldZ, baseWavelength, octaves, lacunarity, persistence);

            double amplitude = 1.0;
            double frequency = 1.0 / baseWavelength;
            double total = 0.0;
            double theoreticalAmplitude = 0.0;

            for (int octave = 0; octave < octaves; octave++)
            {
                ulong octaveSeed = seed ^ ((ulong)octave * 0x9E3779B97F4A7C15UL);
                double sample = GradientNoise2D.Sample(worldX * frequency, worldZ * frequency, octaveSeed);
                total += sample * amplitude;
                theoreticalAmplitude += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            double normalized = 0.5 + (0.5 * total / theoreticalAmplitude);
            if (normalized < 0.0)
            {
                normalized = 0.0;
            }
            else if (normalized > 1.0)
            {
                normalized = 1.0;
            }

            return (float)normalized;
        }

        private static void ValidateParameters(
            double worldX,
            double worldZ,
            double baseWavelength,
            int octaves,
            double lacunarity,
            double persistence)
        {
            if (double.IsNaN(worldX) || double.IsInfinity(worldX))
            {
                throw new ArgumentOutOfRangeException(nameof(worldX));
            }

            if (double.IsNaN(worldZ) || double.IsInfinity(worldZ))
            {
                throw new ArgumentOutOfRangeException(nameof(worldZ));
            }

            if (double.IsNaN(baseWavelength) || double.IsInfinity(baseWavelength) || baseWavelength <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseWavelength));
            }

            if (octaves <= 0 || octaves > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(octaves));
            }

            if (double.IsNaN(lacunarity) || double.IsInfinity(lacunarity) || lacunarity <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(lacunarity));
            }

            if (double.IsNaN(persistence) || double.IsInfinity(persistence) || persistence < 0.0 || persistence > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(persistence));
            }
        }
    }
}
