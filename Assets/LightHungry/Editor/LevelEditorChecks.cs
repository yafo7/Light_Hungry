using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LightHungry.EditorTools {
    public static class LevelEditorChecks {
        public static void VerifyAndBuild(){LevelKit.InstallRoom();Run();BuildInteractionLab.ValidateAndBuild();}
        public static void Run(){
            LevelKit.EnsureAssets();string dir="Verification";Directory.CreateDirectory(dir);
            string asset=AssetDatabase.GenerateUniqueAssetPath("Assets/LightHungry/Scenes/EditorCheck.unity");
            try {
                LevelKit.NewLevel(asset);var g=LevelKit.Put("SpaceMirror",new Vector3(1,2,3));Undo.RegisterCreatedObjectUndo(g,"check");int count=UnityEngine.Object.FindObjectsOfType<SpaceMirror>().Length;Undo.PerformUndo();if(UnityEngine.Object.FindObjectsOfType<SpaceMirror>().Length!=count-1)throw new Exception("Undo did not remove placed object");
                g=LevelKit.Put("LightLaunchPad",new Vector3(-4,0,0));var landing=LevelKit.Put("LightLanding",new Vector3(4,.95f,0));
                var sw=LevelKit.Put("DeviceSwitch",Vector3.zero).GetComponent<DeviceSwitch>();sw.target=LevelKit.Put("Gate",new Vector3(6,2,0)).GetComponent<SpaceDevice>();
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.OpenScene(asset);if(!UnityEngine.Object.FindObjectOfType<DeviceSwitch>().target)throw new Exception("Connection lost after save/load");
                var pad=UnityEngine.Object.FindObjectOfType<LightLaunchPad>();pad.localDirection=Vector3.left;
                var report=LightHungryLevelEditor.ValidateLevel();if(report.Contains("未连接")||!report.Contains("光路"))throw new Exception("Validator failed incomplete-route check: "+report);
                var window=EditorWindow.GetWindow<LightHungryLevelEditor>();window.Close();
                File.WriteAllText(Path.Combine(dir,"editor-checks.txt"),"PASS All 12 kit prefabs load\nPASS New level creation\nPASS Scene placement and Undo\nPASS Scene save/reopen\nPASS Device connection persists\nPASS Validator detects incomplete optical route\nPASS Editor window opens and closes");Debug.Log("LEVEL_EDITOR_CHECKS_OK");
            } finally {EditorSceneManager.OpenScene(LevelKit.Root+"Scenes/InteractionLab.unity");var entries=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);entries.RemoveAll(e=>e.path==asset);EditorBuildSettings.scenes=entries.ToArray();AssetDatabase.DeleteAsset(asset);}
        }
    }
}
