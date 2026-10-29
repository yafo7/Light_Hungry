using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LightHungry.EditorTools {
    public static class InstallCameraViews {
        const string Root = "Assets/LightHungry/";
        [MenuItem("Light Hungry/Enable four camera views in existing InteractionLab")]
        public static void Install() {
            var scene = EditorSceneManager.OpenScene(Root + "Scenes/InteractionLab.unity", OpenSceneMode.Single);
            var session = UnityEngine.Object.FindObjectOfType<LabSession>();
            var camera = Camera.main;
            if (!session || !session.player || !camera) throw new Exception("Existing lab must have its authored player, session and camera.");
            if (!AssetDatabase.IsValidFolder(Root + "Materials")) AssetDatabase.CreateFolder(Root.TrimEnd('/'), "Materials");
            var world = UnityEngine.Object.FindObjectOfType<SpatialLabWorld>();
            if (!world) world = new GameObject("Spatial Lab").AddComponent<SpatialLabWorld>();
            world.spriteMaterial = Material("SpatialSprite", AssetDatabase.LoadAssetAtPath<Shader>(Root + "Runtime/SpatialSprite.shader"));
            world.actorMaterial = Material("SpatialActor", AssetDatabase.LoadAssetAtPath<Shader>(Root + "Runtime/SpatialActor.shader"));
            world.solidMaterial = Material("SpatialSolid", AssetDatabase.LoadAssetAtPath<Shader>(Root + "Runtime/SpatialSolid.shader"));
            world.backdropMaterial = Material("SpatialBackdrop", Shader.Find("Unlit/Texture"));
            var background = GameObject.Find("Background");
            if (background && background.TryGetComponent<SpriteRenderer>(out var sr)) world.backdropMaterial.mainTexture = sr.sprite.texture;
            var controller = camera.GetComponent<CameraViewController>();
            if (!controller) controller = camera.gameObject.AddComponent<CameraViewController>();
            controller.targetCamera = camera;
            controller.player = session.player;
            camera.orthographic = true;
            camera.transform.SetPositionAndRotation(new Vector3(0,0,-40), Quaternion.identity);
            camera.farClipPlane = 120;
            EditorUtility.SetDirty(world.backdropMaterial);
            EditorUtility.SetDirty(world);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("LIGHT_HUNGRY_CAMERA_VIEWS_INSTALLED: existing scene patched, prefab sources preserved");
        }

        static Material Material(string name, Shader shader) {
            if (!shader) throw new Exception("Missing shader for " + name);
            string path = Root + "Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material) { material = new Material(shader); AssetDatabase.CreateAsset(material,path); }
            return material;
        }

        public static void InstallAndBuild() { Install(); BuildInteractionLab.ValidateAndBuild(); }
    }
}
