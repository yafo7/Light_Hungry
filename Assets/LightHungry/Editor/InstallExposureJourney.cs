using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LightHungry.EditorTools {
    public static class InstallExposureJourney {
        public static void Install(){
            var scene=EditorSceneManager.OpenScene(LevelKit.Root+"Scenes/InteractionLab.unity");var session=Object.FindObjectOfType<LabSession>();
            if(!GameObject.Find("Exposure Journey")){
                var root=new GameObject("Exposure Journey").transform;
                var source=LevelKit.Put("Flashlight",new Vector3(-14.95f,-4.55f,0),root);source.name="Exposure start light";
                var source2=LevelKit.Put("Flashlight",new Vector3(-10,-3.2f,8),root);source2.name="Side goal light";var emitter=source2.GetComponent<LightEmitter>();emitter.useSpatialDirection=true;emitter.spatialDirection=Vector3.forward;
                var mirror=LevelKit.Put("SpaceMirror",new Vector3(-10,-3.2f,12),root).GetComponent<SpaceMirror>();mirror.name="Goal steering mirror";mirror.normals=new[]{new Vector3(0,-1,1),new Vector3(0,1,1)};mirror.orientation=1;mirror.Apply();
                var beast=LevelKit.Put("HungryBeast",new Vector3(-10,0,12),root).GetComponent<BeastReceiver>();beast.name="Journey goal beast";session.goal=beast;
                foreach(var c in root.GetComponentsInChildren<Component>()){EditorUtility.SetDirty(c);PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
            }
            if(!session.GetComponent<LightRouteReview>())session.gameObject.AddComponent<LightRouteReview>();
            session.startWithReflection=false;session.startWithSwallow=false;PrefabUtility.RecordPrefabInstancePropertyModifications(session);
            foreach(var p in Object.FindObjectsOfType<LightLaunchPad>()){p.available=false;EditorUtility.SetDirty(p);PrefabUtility.RecordPrefabInstancePropertyModifications(p);}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("EXPOSURE_JOURNEY_INSTALLED");
        }
        public static void InstallAndBuild(){Install();BuildInteractionLab.ValidateAndBuild();}
    }
}
