using System;
using System.IO;
using GaemMeong.Debugging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GaemMeong.Editor
{
    public static class RendererValidationSceneBuilder
    {
        private const string MaterialFolder = "Assets/GaemMeong/Materials";
        private const string SceneFolder = "Assets/GaemMeong/Scenes";
        private const string MaterialPath = MaterialFolder + "/RendererValidation.mat";
        private const string ScenePath = SceneFolder + "/RendererValidation.unity";
        private const string UrpUnlitShaderName = "Universal Render Pipeline/Unlit";
        private const int CaptureSize = 512;

        [MenuItem("GaemMeong/World Generation/Build Renderer Validation Scene")]
        public static void BuildFromMenu()
        {
            Build();
        }

        public static void BuildFromCommandLine()
        {
            try
            {
                Build();
                Debug.Log($"Renderer validation scene created at {ScenePath}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        public static void CaptureFromCommandLine()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Camera camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            RendererValidationMesh validationMesh = UnityEngine.Object.FindFirstObjectByType<RendererValidationMesh>();
            if (camera == null || validationMesh == null)
            {
                throw new InvalidOperationException("The renderer validation scene is missing its camera or mesh.");
            }

            validationMesh.Rebuild();
            RenderTexture renderTexture = RenderTexture.GetTemporary(
                CaptureSize,
                CaptureSize,
                24,
                RenderTextureFormat.ARGB32);
            Texture2D capture = new Texture2D(CaptureSize, CaptureSize, TextureFormat.RGBA32, false);
            RenderTexture previousActive = RenderTexture.active;

            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                capture.ReadPixels(new Rect(0f, 0f, CaptureSize, CaptureSize), 0, 0);
                capture.Apply();

                Color32[] pixels = capture.GetPixels32();
                int foregroundPixels = CountForegroundPixels(pixels);
                int minimumForegroundPixels = pixels.Length / 100;
                if (foregroundPixels < minimumForegroundPixels)
                {
                    throw new InvalidOperationException(
                        $"The captured frame contains only {foregroundPixels} foreground pixels; " +
                        $"expected at least {minimumForegroundPixels}.");
                }

                string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/RendererValidation.png"));
                File.WriteAllBytes(outputPath, capture.EncodeToPNG());
                Debug.Log($"Renderer validation capture passed with {foregroundPixels} foreground pixels: {outputPath}");
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
                UnityEngine.Object.DestroyImmediate(capture);
            }
        }

        private static void Build()
        {
            EnsureFolder(MaterialFolder);
            EnsureFolder(SceneFolder);

            Material material = CreateOrUpdateMaterial();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject meshObject = new GameObject("Renderer Validation Mesh");
            RendererValidationMesh validationMesh = meshObject.AddComponent<RendererValidationMesh>();
            validationMesh.SetMaterial(material);
            validationMesh.Rebuild();

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);
            camera.fieldOfView = 45f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            cameraObject.transform.position = new Vector3(4f, 3.5f, -4f);
            cameraObject.transform.LookAt(Vector3.zero);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Failed to save renderer validation scene at {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = meshObject;
        }

        private static Material CreateOrUpdateMaterial()
        {
            Shader shader = Shader.Find(UrpUnlitShaderName);
            if (shader == null)
            {
                throw new InvalidOperationException($"Shader '{UrpUnlitShaderName}' was not found.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = "Renderer Validation",
                };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            Color validationColor = new Color(0.18f, 0.72f, 0.42f, 1f);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", validationColor);
            }
            else
            {
                material.color = validationColor;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static int CountForegroundPixels(Color32[] pixels)
        {
            Color32 background = pixels[0];
            int foregroundPixels = 0;

            for (int index = 1; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                int difference = Math.Abs(pixel.r - background.r)
                    + Math.Abs(pixel.g - background.g)
                    + Math.Abs(pixel.b - background.b);
                if (difference > 24)
                {
                    foregroundPixels++;
                }
            }

            return foregroundPixels;
        }

        private static void EnsureFolder(string assetFolderPath)
        {
            string[] parts = assetFolderPath.Split('/');
            string currentPath = parts[0];

            for (int index = 1; index < parts.Length; index++)
            {
                string nextPath = currentPath + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[index]);
                }

                currentPath = nextPath;
            }
        }
    }
}
