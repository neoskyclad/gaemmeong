using System;
using System.IO;
using GaemMeong.Debugging;
using GaemMeong.World.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GaemMeong.Editor
{
    public static class WorldGenerationPrototypeSceneBuilder
    {
        private const string RootFolder = "Assets/GaemMeong";
        private const string SettingsFolder = RootFolder + "/Settings";
        private const string MaterialFolder = RootFolder + "/Materials";
        private const string SceneFolder = RootFolder + "/Scenes";
        private const string SettingsPath = SettingsFolder + "/WorldGenerationPrototypeSettings.asset";
        private const string MaterialPath = MaterialFolder + "/WorldGenerationPreview.mat";
        private const string ScenePath = SceneFolder + "/WorldGenerationPrototype.unity";

        [MenuItem("GaemMeong/World Generation/Build Prototype Scene")]
        public static void BuildFromMenu()
        {
            if (File.Exists(ScenePath)
                && !EditorUtility.DisplayDialog(
                    "Rebuild World Generation Prototype?",
                    "This recreates the prototype scene and replaces manual scene edits. The settings and material assets are retained.",
                    "Rebuild",
                    "Cancel"))
            {
                return;
            }
            Build();
        }

        public static void BuildFromCommandLine()
        {
            try
            {
                Build();
                Debug.Log($"World generation prototype scene created at {ScenePath}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        private static void Build()
        {
            EnsureFolder(SettingsFolder);
            EnsureFolder(MaterialFolder);
            EnsureFolder(SceneFolder);

            GameWorldSettings settings = AssetDatabase.LoadAssetAtPath<GameWorldSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<GameWorldSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(SettingsPath);
                settings = AssetDatabase.LoadAssetAtPath<GameWorldSettings>(SettingsPath);
            }
            if (settings == null || !EditorUtility.IsPersistent(settings))
                throw new InvalidOperationException("The prototype settings asset was not saved as a persistent Unity asset.");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) throw new InvalidOperationException("URP Unlit shader was not found.");
            if (material == null)
            {
                material = new Material(shader) { name = "World Generation Preview" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
            {
                material.shader = shader;
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(MaterialPath);
            material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            settings = AssetDatabase.LoadAssetAtPath<GameWorldSettings>(SettingsPath);
            if (settings == null)
                throw new InvalidOperationException("The prototype settings asset could not be reloaded after asset import.");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject controllerObject = new GameObject("World Generation Preview");
            WorldGenerationPreviewController controller = controllerObject.AddComponent<WorldGenerationPreviewController>();
            controller.Configure(settings, material);
            EditorUtility.SetDirty(controller);

            CreateCamera(settings);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException($"Failed to save prototype scene at {ScenePath}.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = controllerObject;
        }

        private static void CreateCamera(GameWorldSettings settings)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.075f, 0.1f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(settings.WorldWidth, settings.WorldLength) * 0.62f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 3000f;
            float centerX = settings.WorldWidth * 0.5f;
            float centerZ = settings.WorldLength * 0.5f;
            cameraObject.transform.position = new Vector3(centerX, 680f, centerZ - 780f);
            cameraObject.transform.LookAt(new Vector3(centerX, 0f, centerZ));
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            string folder = path.Substring(separator + 1);
            EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
