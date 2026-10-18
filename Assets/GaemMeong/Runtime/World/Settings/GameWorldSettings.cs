using UnityEngine;

namespace GaemMeong.World.Settings
{
    [CreateAssetMenu(
        fileName = "GameWorldSettings",
        menuName = "GaemMeong/World Generation Settings")]
    public sealed class GameWorldSettings : ScriptableObject
    {
        [SerializeField]
        private string seedText = "gaemmeong";

        [SerializeField, Min(1)]
        private int worldWidth = 512;

        [SerializeField, Min(1)]
        private int worldLength = 512;

        [SerializeField, Min(1)]
        private int chunkSize = 64;

        [SerializeField]
        private TerrainPreset terrainPreset = TerrainPreset.Balanced;

        [SerializeField]
        private LandmassPreset landmassPreset = LandmassPreset.Continents;

        [SerializeField, Min(0.01f)]
        private float heightScale = 0.25f;

        public string SeedText => seedText;

        public int WorldWidth => worldWidth;

        public int WorldLength => worldLength;

        public int ChunkSize => chunkSize;

        public TerrainPreset TerrainPreset => terrainPreset;

        public LandmassPreset LandmassPreset => landmassPreset;

        public float HeightScale => heightScale;

        public WorldGenerationConfig CreateConfig()
        {
            return WorldGenerationConfig.Create(
                seedText,
                worldWidth,
                worldLength,
                chunkSize,
                terrainPreset,
                landmassPreset,
                heightScale);
        }
    }
}
