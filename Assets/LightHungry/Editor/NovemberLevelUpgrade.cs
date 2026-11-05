using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LightHungry.EditorTools {
    public static class NovemberLevelUpgrade {
        const string Root="Assets/LightHungry/Prefabs/";
        [MenuItem("Light Hungry/补齐新关卡功能预制体（保留现有资源）")]
        public static void EnsureAssets(){
            if(!AssetDatabase.LoadAssetAtPath<GameObject>(Root+"CrossFaceSurface.prefab")){
                var go=new GameObject("CrossFaceSurface");go.layer=10;var surface=go.AddComponent<CrossFaceSurface>();var box=surface.GetComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(28,18,.02f);PrefabUtility.SaveAsPrefabAsset(go,Root+"CrossFaceSurface.prefab");UnityEngine.Object.DestroyImmediate(go);
            }
            if(!AssetDatabase.LoadAssetAtPath<GameObject>(Root+"LevelExit.prefab")){
                var go=new GameObject("LevelExit");go.layer=13;var exit=go.AddComponent<LevelExit>();var box=exit.GetComponent<BoxCollider>();box.isTrigger=true;box.center=Vector3.up;box.size=new Vector3(1.2f,2,1);PrefabUtility.SaveAsPrefabAsset(go,Root+"LevelExit.prefab");UnityEngine.Object.DestroyImmediate(go);
            }
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Light Hungry/选中平台／添加喂食显现功能")]
        public static void AddReveal(){var g=Selection.activeGameObject;if(!g||EditorApplication.isPlaying)return;if(!g.GetComponent<RevealDevice>()){Undo.AddComponent<RevealDevice>(g);EditorSceneManager.MarkSceneDirty(g.scene);}}
        public static void VerifyAndBuild(){
            EnsureAssets();
            if(!LevelGraph.IsSource(new GameObject("Check source").AddComponent<BeastReceiver>()))throw new Exception("Beast graph input missing");
            var holder=GameObject.Find("Check source");UnityEngine.Object.DestroyImmediate(holder);
            foreach(var name in new[]{"CrossFaceSurface","LevelExit"}){
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+name+".prefab");if(!prefab||!prefab.GetComponent<BoxCollider>().isTrigger)throw new Exception("Missing trigger prefab: "+name);
            }
            var entries=EditorBuildSettings.scenes;string path="Assets/LightHungry/Scenes/_NovemberCheck_"+Guid.NewGuid().ToString("N")+".unity";
            try{
                WatercolorWorkshop.CreateEmpty(path);
                int backgrounds=0,floors=0;foreach(var root in EditorSceneManager.GetActiveScene().GetRootGameObjects()){if(root.name.StartsWith("Backdrop "))backgrounds++;if(root.name.StartsWith("Floor "))floors++;}
                if(backgrounds!=4||floors!=4||!UnityEngine.Object.FindObjectOfType<CameraViewController>().followPlayer)throw new Exception("Incomplete four-face template");
                var beast=new GameObject("Bound beast").AddComponent<BeastReceiver>();var platform=new GameObject("Bound platform");platform.AddComponent<BoxCollider>();var reveal=platform.AddComponent<RevealDevice>();
                var circuit=LevelGraph.Connect(beast,reveal);if(!circuit||circuit.inputs[0].source!=beast)throw new Exception("Beast graph connection failed");
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());EditorSceneManager.OpenScene(path);
                var restored=UnityEngine.Object.FindObjectOfType<DeviceCircuit>();if(!restored||!(restored.output is RevealDevice)||!(restored.inputs[0].source is BeastReceiver))throw new Exception("Graph reference did not survive save/reopen");
            }finally{EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorBuildSettings.scenes=entries;if(File.Exists(path))AssetDatabase.DeleteAsset(path);}
            Debug.Log("NOVEMBER_EDITOR_CHECKS_OK");BuildInteractionLab.ValidateAndBuild();
        }
    }
}
