using UnityEngine;
using UnityEngine.Rendering;

namespace GaemMeong.Debugging
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RendererValidationMesh : MonoBehaviour
    {
        [SerializeField]
        private Material material;

        private Mesh generatedMesh;

        private void OnEnable()
        {
            Rebuild();
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                Rebuild();
            }
        }

        private void OnDisable()
        {
            ClearGeneratedMesh();
        }

        private void OnDestroy()
        {
            ClearGeneratedMesh();
        }

        public void SetMaterial(Material validationMaterial)
        {
            material = validationMaterial;
            ApplyRendererSettings();
        }

        [ContextMenu("Rebuild Validation Mesh")]
        public void Rebuild()
        {
            ClearGeneratedMesh();

            generatedMesh = new Mesh
            {
                name = "Renderer Validation Grid",
                hideFlags = HideFlags.DontSave,
                vertices = CreateVertices(),
                triangles = CreateTriangles(),
                uv = CreateUvs(),
            };
            generatedMesh.RecalculateNormals();
            generatedMesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = generatedMesh;
            ApplyRendererSettings();
        }

        private void ApplyRendererSettings()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private void ClearGeneratedMesh()
        {
            if (generatedMesh == null)
            {
                return;
            }

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh == generatedMesh)
            {
                meshFilter.sharedMesh = null;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(generatedMesh);
            }

            generatedMesh = null;
        }

        private static Vector3[] CreateVertices()
        {
            return new[]
            {
                new Vector3(-1f, 0.0f, -1f),
                new Vector3( 0f, 0.3f, -1f),
                new Vector3( 1f, 0.0f, -1f),
                new Vector3(-1f, 0.2f,  0f),
                new Vector3( 0f, 1.5f,  0f),
                new Vector3( 1f, 0.4f,  0f),
                new Vector3(-1f, 0.0f,  1f),
                new Vector3( 0f, 0.5f,  1f),
                new Vector3( 1f, 0.1f,  1f),
            };
        }

        private static int[] CreateTriangles()
        {
            return new[]
            {
                0, 3, 1,
                1, 3, 4,
                1, 4, 2,
                2, 4, 5,
                3, 6, 4,
                4, 6, 7,
                4, 7, 5,
                5, 7, 8,
            };
        }

        private static Vector2[] CreateUvs()
        {
            return new[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(0.5f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(1.0f, 0.5f),
                new Vector2(0.0f, 1.0f),
                new Vector2(0.5f, 1.0f),
                new Vector2(1.0f, 1.0f),
            };
        }
    }
}
