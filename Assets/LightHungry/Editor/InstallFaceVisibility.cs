using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LightHungry.EditorTools {
    public static class InstallFaceVisibility {
        public static void Install(){
            var scene=EditorSceneManager.OpenScene(LevelKit.Root+"Scenes/InteractionLab.unity");
            foreach(var root in scene.GetRootGameObjects()){
                if(root.GetComponent<Camera>()||root.GetComponent<SpatialLabWorld>()||root.GetComponent<BeamSystem>()||root.GetComponent<LabSession>())continue;
                Mark(root,root.GetComponent<PlayerController>()?RoomFaces.All:RoomFaces.Front);
            }
            var workshop=GameObject.Find("Light Travel Workshop");
            if(workshop)foreach(Transform child in workshop.transform)Mark(child.gameObject,child.name=="Workshop depth mirror"?RoomFaces.Front|RoomFaces.Right:child.position.z>=6?RoomFaces.Right:RoomFaces.Front);
            var mirror=GameObject.Find("Workshop depth mirror");if(mirror){var membership=mirror.GetComponent<FaceMembership>();membership.faces=RoomFaces.Front|RoomFaces.Right;LevelGraph.Changed(membership);}
            var journey=GameObject.Find("Exposure Journey");if(journey)foreach(Transform child in journey.transform)Mark(child.gameObject,child.name=="Exposure start light"?RoomFaces.Front:RoomFaces.Right);
            var camera=Object.FindObjectOfType<CameraViewController>();if(camera&&!camera.GetComponent<FaceVisibility>())camera.gameObject.AddComponent<FaceVisibility>();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("FACE_VISIBILITY_INSTALLED_INCREMENTALLY");
        }
        static void Mark(GameObject g,RoomFaces faces){if(g.GetComponent<FaceMembership>())return;var member=g.AddComponent<FaceMembership>();member.faces=faces;LevelGraph.Changed(member);}
        public static void InstallAndBuild(){Install();LevelEditorChecksV2.Run();BuildInteractionLab.ValidateAndBuild();}
    }
}
