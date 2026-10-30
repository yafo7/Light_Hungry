using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace LightHungry.EditorTools {
    public static class LevelEditorChecksV2 {
        static readonly List<string> checks=new List<string>();
        static void Check(bool value,string message){if(!value)throw new Exception(message);checks.Add("PASS "+message);}
        public static void Run(){
            checks.Clear();Directory.CreateDirectory("Verification");string scratch=AssetDatabase.GenerateUniqueAssetPath(LevelKit.Root+"Scenes/EditorV2Check.unity");var buildSettings=EditorBuildSettings.scenes;
            string current=EditorSceneManager.GetActiveScene().path;
            try{
                LevelKit.NewLevel(scratch);
                Check(Object.FindObjectOfType<FaceVisibility>()&&Object.FindObjectOfType<LightRouteReview>(),"New levels include face isolation and completion review");
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(LevelKit.Folder+"DeviceSwitch.prefab");
                var a=LightHungryLevelEditor.PlacePrefab(prefab,new Vector3(1,2,3),4);a.name="Button A";
                Check(PrefabUtility.GetCorrespondingObjectFromSource(a)==prefab&&a.GetComponent<FaceMembership>().faces==RoomFaces.Right,"Palette placement reuses source prefab and stores selected face");
                Undo.PerformUndo();Check(!a,"One Undo removes whole placed instance");Undo.PerformRedo();
                a=GameObject.Find("DeviceSwitch");if(!a)a=Object.FindObjectOfType<DeviceSwitch>().gameObject;a.name="Button A";
                var light=LightHungryLevelEditor.PlacePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(LevelKit.Root+"Prefabs/Flashlight.prefab"),new Vector3(4,2,0),4).GetComponent<LightEmitter>();Check(light.useSpatialDirection&&Vector3.Dot(light.spatialDirection,Vector3.forward)>.99f,"Placing light on right face aligns actual optical direction");
                var b=LightHungryLevelEditor.PlacePrefab(prefab,new Vector3(2,2,3),4);b.name="Button B";
                var gate=LightHungryLevelEditor.PlacePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(LevelKit.Folder+"Gate.prefab"),new Vector3(3,2,3),4).GetComponent<SpaceDevice>();gate.name="Gate A";
                var sa=a.GetComponent<DeviceSwitch>();var sb=b.GetComponent<DeviceSwitch>();sa.target=gate;LevelGraph.Changed(sa);
                var circuit=LevelGraph.Connect(sb,gate);
                Check(circuit&&circuit.inputs.Count==2&&!sa.target,"Connecting second input migrates legacy link without competing writers");
                LevelGraph.Connect(sa,gate);Check(circuit.inputs.Count==2,"Duplicate connection does not duplicate input");
                var rail=LevelKit.Put("RailDevice",Vector3.zero).GetComponent<RailDevice>();var plate=LevelKit.Put("PressurePlate",Vector3.zero).GetComponent<PressurePlate>();
                Check(LevelGraph.ConnectDirectional(plate,rail,1)&&rail.leftPlate==plate,"Graph can directly edit original pulley left-button relation");
                Check(LevelGraph.Connect(sa,rail)==null&&rail.leftPlate==plate,"Combining inputs does not silently replace directional pulley controls");
                sa.state=true;sb.state=false;circuit.rule=CircuitRule.Any;Check(circuit.Evaluate(),"Any input rule accepts one active switch");
                circuit.rule=CircuitRule.All;Check(!circuit.Evaluate(),"All input rule rejects partial activation");sb.state=true;Check(circuit.Evaluate(),"All input rule accepts both active switches");
                circuit.inputs[1].invert=true;Check(!circuit.Evaluate(),"Input inversion affects evaluated output");circuit.inputs[1].invert=false;
                LevelGraph.Changed(circuit);LevelGraph.Changed(sa);LevelGraph.Changed(sb);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());EditorSceneManager.OpenScene(scratch);
                circuit=Object.FindObjectOfType<DeviceCircuit>();Check(circuit&&circuit.inputs.Count==2&&circuit.rule==CircuitRule.All&&circuit.output,"Graph references, rule and inputs survive scene save/reopen");
                Check(LevelGraph.Edges().FindAll(e=>e.circuit==circuit).Count==2,"Graph reconstructs from actual saved references");
                var edge=LevelGraph.Edges().Find(e=>e.circuit==circuit);LevelGraph.Disconnect(edge);Check(circuit.inputs.Count==1,"Deleting graph edge removes actual input");Undo.PerformUndo();Check(circuit.inputs.Count==2,"Undo restores deleted connection");
                var renderer=Object.FindObjectOfType<BeastReceiver>().visual;var member=renderer.GetComponentInParent<FaceMembership>();bool wasOff=renderer.forceRenderingOff;
                using(new FaceVisibility.RenderScope(RoomFaces.Right))Check(renderer.forceRenderingOff&&renderer.gameObject.activeInHierarchy,"Hidden face filters rendering without deactivating object");
                Check(renderer.forceRenderingOff==wasOff,"Per-camera filtering restores original renderer state");
                var window=EditorWindow.GetWindow<LightHungryLevelEditor>();window.ReloadPalette();window.Close();Check(true,"Editor panel opens with existing project prefab catalogue");
                File.WriteAllLines("Verification/editor-v2-results.txt",checks);Debug.Log("LEVEL_EDITOR_V2_CHECKS_OK "+checks.Count);
            }finally{Selection.activeObject=null;FaceScenePreview.Reset();EditorSceneManager.OpenScene(string.IsNullOrEmpty(current)?LevelKit.Root+"Scenes/InteractionLab.unity":current);EditorBuildSettings.scenes=buildSettings;AssetDatabase.DeleteAsset(scratch);Undo.ClearAll();}
        }
    }
}
