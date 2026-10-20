using System;

namespace GaemMeong.World.Data
{
    public sealed class WorldGenerationDebugData
    {
        public WorldGenerationDebugData(int tileCount)
        {
            if (tileCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tileCount));
            }

            BaseNoise = new float[tileCount];
        }

        public float[] BaseNoise { get; }

        public float[] MountainMask { get; internal set; }

        public float[] LandmassMask { get; internal set; }

        public float[] Slope { get; internal set; }
    }
}
