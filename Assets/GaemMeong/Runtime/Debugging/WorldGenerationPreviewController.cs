using System;
using System.Collections.Generic;
using GaemMeong.World.Generation;
using GaemMeong.World.Settings;
using UnityEngine;
using UnityEngine.Rendering;

namespace GaemMeong.Debugging
{
    public sealed class WorldGenerationPreviewController : MonoBehaviour
    {
        [SerializeField] private GameWorldSettings settings;
        [SerializeField] private Material previewMaterial;

        private string seedText;
        private TerrainPreset terrainPreset;
        private LandmassPreset landmassPreset;
        private PreviewMode previewMode;
        private string statusMessage;
        private GameObject generatedWorldRoot;
        private Texture2D previewTexture;
        private Material runtimeMaterial;
        private bool isReady;

        public void Configure(GameWorldSettings worldSettings, Material material)
        {
            settings = worldSettings;
            previewMaterial = material;
        }

        private enum PreviewMode
        {
            Terrain,
            Elevation,
            MountainMask,
            LandmassMask,
            Slope,
            Water,
        }

        private void Start()
        {
            if (settings == null)
            {
                statusMessage = "Assign a GameWorldSettings asset to begin.";
                return;
            }

            seedText = settings.SeedText;
            terrainPreset = settings.TerrainPreset;
            landmassPreset = settings.LandmassPreset;
            Regenerate();
        }

        private void OnGUI()
        {
            if (!isReady && settings == null)
            {
                GUI.Label(new Rect(12, 12, 460, 30), statusMessage);
                return;
            }

            GUILayout.BeginArea(new Rect(12, 12, 310, 260), GUI.skin.box);
            GUILayout.Label("GaemMeong · World Preview");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Seed", GUILayout.Width(54));
            seedText = GUILayout.TextField(seedText ?? string.Empty);
            GUILayout.EndHorizontal();
            terrainPreset = (TerrainPreset)EditorGUILayoutEnumPopup("Terrain", terrainPreset);
            landmassPreset = (LandmassPreset)EditorGUILayoutEnumPopup("Landmass", landmassPreset);
            previewMode = (PreviewMode)EditorGUILayoutEnumPopup("View", previewMode);
            if (GUILayout.Button("Regenerate")) Regenerate();
            if (!string.IsNullOrEmpty(statusMessage)) GUILayout.Label(statusMessage, GUI.skin.label);
            GUILayout.EndArea();
        }

        private void Regenerate()
        {
            try
            {
                WorldGenerationConfig config = WorldGenerationConfig.Create(
                    seedText,
                    settings.WorldWidth,
                    settings.WorldLength,
                    settings.ChunkSize,
                    terrainPreset,
                    landmassPreset,
                    settings.HeightScale);
                WorldGenerationOutput output = new WorldGenerator().Generate(config);
                RebuildTexture(output, config);
                RebuildMeshes(output, config);
                statusMessage = $"{config.WorldWidth}×{config.WorldLength} · checksum {output.Checksum:X16}";
                isReady = true;
            }
            catch (Exception exception)
            {
                statusMessage = exception.Message;
                Debug.LogException(exception, this);
            }
        }

        private void RebuildTexture(WorldGenerationOutput output, WorldGenerationConfig config)
        {
            DestroyGeneratedObject(previewTexture);
            DestroyGeneratedObject(runtimeMaterial);

            previewTexture = new Texture2D(config.WorldWidth, config.WorldLength, TextureFormat.RGBA32, false, false)
            {
                name = "Generated World Debug Texture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            Color32[] pixels = new Color32[config.TileCount];
            for (int index = 0; index < pixels.Length; index++)
            {
                pixels[index] = GetPixelColor(output, index);
            }
            previewTexture.SetPixels32(pixels);
            previewTexture.Apply(false, false);

            if (previewMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Unlit/Texture");
                if (shader == null) throw new InvalidOperationException("No supported unlit shader was found.");
                runtimeMaterial = new Material(shader);
            }
            else
            {
                runtimeMaterial = new Material(previewMaterial);
            }

            if (runtimeMaterial.HasProperty("_BaseMap")) runtimeMaterial.SetTexture("_BaseMap", previewTexture);
            else runtimeMaterial.mainTexture = previewTexture;
            if (runtimeMaterial.HasProperty("_BaseColor")) runtimeMaterial.SetColor("_BaseColor", Color.white);
            else runtimeMaterial.color = Color.white;
            runtimeMaterial.enableInstancing = true;
        }

        private void RebuildMeshes(WorldGenerationOutput output, WorldGenerationConfig config)
        {
            DestroyWorldRoot();
            generatedWorldRoot = new GameObject("Generated World");
            generatedWorldRoot.transform.SetParent(transform, false);

            for (int chunkZ = 0; chunkZ < config.ChunkCountZ; chunkZ++)
            {
                for (int chunkX = 0; chunkX < config.ChunkCountX; chunkX++)
                {
                    BuildChunk(output.World.Elevation, config, chunkX, chunkZ);
                }
            }
        }

        private void BuildChunk(short[] elevation, WorldGenerationConfig config, int chunkX, int chunkZ)
        {
            int size = config.ChunkSize;
            int rowLength = size + 1;
            Vector3[] vertices = new Vector3[rowLength * rowLength];
            Vector2[] uv = new Vector2[vertices.Length];
            List<int> triangles = new List<int>(size * size * 6);
            int startX = chunkX * size;
            int startZ = chunkZ * size;

            for (int z = 0; z <= size; z++)
            {
                for (int x = 0; x <= size; x++)
                {
                    int globalX = startX + x;
                    int globalZ = startZ + z;
                    int vertexIndex = z * rowLength + x;
                    float height = GetVertexHeight(elevation, config, globalX, globalZ);
                    vertices[vertexIndex] = new Vector3(globalX, height, globalZ);
                    uv[vertexIndex] = new Vector2(
                        (float)globalX / config.WorldWidth,
                        (float)globalZ / config.WorldLength);
                }
            }

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    int bottomLeft = (z * rowLength) + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + rowLength;
                    int topRight = topLeft + 1;
                    triangles.Add(bottomLeft);
                    triangles.Add(topLeft);
                    triangles.Add(bottomRight);
                    triangles.Add(bottomRight);
                    triangles.Add(topLeft);
                    triangles.Add(topRight);
                }
            }

            Mesh mesh = new Mesh
            {
                name = $"World Chunk ({chunkX}, {chunkZ})",
                indexFormat = IndexFormat.UInt16,
                vertices = vertices,
                uv = uv,
                triangles = triangles.ToArray(),
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject chunkObject = new GameObject($"Chunk ({chunkX}, {chunkZ})");
            chunkObject.transform.SetParent(generatedWorldRoot.transform, false);
            chunkObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            chunkObject.AddComponent<MeshRenderer>().sharedMaterial = runtimeMaterial;
        }

        private Color32 GetPixelColor(WorldGenerationOutput output, int index)
        {
            float normalizedElevation = Mathf.InverseLerp(
                WorldGenerationConstants.MinimumElevation,
                WorldGenerationConstants.MaximumElevation,
                output.World.Elevation[index]);
            bool isOcean = output.World.Water[index] == GaemMeong.World.Data.WaterType.Ocean;

            switch (previewMode)
            {
                case PreviewMode.Elevation:
                    return isOcean
                        ? Color32.Lerp(new Color32(8, 28, 63, 255), new Color32(37, 112, 156, 255), normalizedElevation)
                        : Color32.Lerp(new Color32(94, 125, 76, 255), new Color32(226, 222, 199, 255), normalizedElevation);
                case PreviewMode.MountainMask:
                    return Gray(output.DebugData.MountainMask[index]);
                case PreviewMode.LandmassMask:
                    return Color32.Lerp(new Color32(14, 45, 65, 255), new Color32(111, 175, 111, 255), output.DebugData.LandmassMask[index]);
                case PreviewMode.Slope:
                    return Color32.Lerp(new Color32(35, 39, 49, 255), new Color32(246, 115, 65, 255), output.DebugData.Slope[index]);
                case PreviewMode.Water:
                    return isOcean ? new Color32(42, 119, 167, 255) : new Color32(113, 146, 86, 255);
                default:
                    if (isOcean) return new Color32(34, 91, 130, 255);
                    if (normalizedElevation > 0.80f) return new Color32(203, 198, 176, 255);
                    if (normalizedElevation > 0.48f) return new Color32(127, 143, 94, 255);
                    return new Color32(102, 143, 82, 255);
            }
        }

        private static Color32 Gray(float value)
        {
            byte channel = (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);
            return new Color32(channel, channel, channel, 255);
        }

        private static float GetVertexHeight(short[] elevation, WorldGenerationConfig config, int x, int z)
        {
            int samples = 0;
            int total = 0;
            for (int tileZ = z - 1; tileZ <= z; tileZ++)
            {
                for (int tileX = x - 1; tileX <= x; tileX++)
                {
                    if (tileX < 0 || tileX >= config.WorldWidth || tileZ < 0 || tileZ >= config.WorldLength) continue;
                    total += elevation[(tileZ * config.WorldWidth) + tileX];
                    samples++;
                }
            }
            return (total / (float)samples) * config.HeightScale;
        }

        private static object EditorGUILayoutEnumPopup(string label, Enum selected)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(72));
            string[] names = Enum.GetNames(selected.GetType());
            int selectedIndex = Array.IndexOf(names, selected.ToString());
            int nextIndex = GUILayout.Toolbar(selectedIndex, names);
            GUILayout.EndHorizontal();
            return Enum.Parse(selected.GetType(), names[Mathf.Clamp(nextIndex, 0, names.Length - 1)]);
        }

        private static void DestroyGeneratedObject(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private void DestroyWorldRoot()
        {
            if (generatedWorldRoot == null) return;
            MeshFilter[] meshFilters = generatedWorldRoot.GetComponentsInChildren<MeshFilter>();
            for (int index = 0; index < meshFilters.Length; index++)
            {
                DestroyGeneratedObject(meshFilters[index].sharedMesh);
            }
            DestroyGeneratedObject(generatedWorldRoot);
            generatedWorldRoot = null;
        }

        private void OnDestroy()
        {
            DestroyWorldRoot();
            DestroyGeneratedObject(previewTexture);
            DestroyGeneratedObject(runtimeMaterial);
        }
    }
}
