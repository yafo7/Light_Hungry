using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LightHungry.EditorTools {
 public static class WatercolorChecks {
  public static void VerifyAndBuild(){
   int checks=0;Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception("Watercolor editor check: "+label);checks++;Debug.Log("EDITOR_PASS "+label);};
   string original=EditorSceneManager.GetActiveScene().path;const string scratch="Assets/LightHungry/Scenes/__WatercolorQA.unity";
   try{
    RoomBackgrounds.Import();
    string template="Assets/LightHungry/Scenes/__TemplateQA.unity";var entries=EditorBuildSettings.scenes;
    try{WatercolorWorkshop.CreateEmpty(template);var roots=EditorSceneManager.GetActiveScene().GetRootGameObjects();check(roots.Count(g=>g.name.StartsWith("Floor "))==4,"New Scene contains four default floors");var backgrounds=roots.Where(g=>g.name.StartsWith("Backdrop ")).ToArray();check(backgrounds.Length==4&&backgrounds.Select(g=>g.GetComponentInChildren<SpriteRenderer>().sprite).Distinct().Count()==4,"New Scene contains four distinct face backgrounds");check(!UnityEngine.Object.FindObjectOfType<LightRouteReview>().automatic,"3D review retained but automatic playback disabled");check(NewLevelNameWindow.ScenePath("__FreshName")=="Assets/LightHungry/Scenes/__FreshName.unity","Name resolves directly into Scenes directory");bool duplicate=false;try{NewLevelNameWindow.ScenePath("__TemplateQA");}catch(ArgumentException){duplicate=true;}check(duplicate,"Duplicate name cannot overwrite Scene");}
    finally{EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);AssetDatabase.DeleteAsset(template);EditorBuildSettings.scenes=entries;}
    foreach(string name in WatercolorWorkshop.Palette){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(LevelKit.Root+"Prefabs/"+name+".prefab");check(prefab&&prefab.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.sprite),name+" has actual artwork");}
    check(WatercolorAssets.Frames("motion").Length==16&&WatercolorAssets.Frames("interaction").Length==16,"Complete player frame imports");
    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
    var prefabSwitch=AssetDatabase.LoadAssetAtPath<GameObject>(LevelKit.Root+"Prefabs/DeviceSwitch.prefab");var prefabGate=AssetDatabase.LoadAssetAtPath<GameObject>(LevelKit.Root+"Prefabs/Gate.prefab");
    var a=LightHungryLevelEditor.PlacePrefab(prefabSwitch,Vector3.zero,1).GetComponent<DeviceSwitch>();var b=LightHungryLevelEditor.PlacePrefab(prefabGate,Vector3.right*3,1).GetComponent<SpaceDevice>();
    check(PrefabUtility.IsPartOfPrefabInstance(a),"Palette placement keeps prefab connection");var circuit=LevelGraph.Connect(a,b);check(circuit&&circuit.inputs.Count==1,"Graph creates actual circuit reference");
    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scratch);EditorSceneManager.OpenScene(scratch);circuit=UnityEngine.Object.FindObjectOfType<DeviceCircuit>();check(circuit&&circuit.output&&circuit.inputs[0].source,"Graph references survive scene reload");
    var p=LightHungryLevelEditor.PlacePrefab(prefabSwitch,Vector3.up*3,4);int count=UnityEngine.Object.FindObjectsOfType<DeviceSwitch>().Length;Undo.PerformUndo();check(UnityEngine.Object.FindObjectsOfType<DeviceSwitch>().Length==count-1,"Card placement Undo removes instance");
    var window=EditorWindow.GetWindow<LightHungryLevelEditor>();window.ReloadPalette();window.Close();check(true,"Curated editor opens and closes");
   }finally{Selection.activeObject=null;EditorSceneManager.OpenScene(string.IsNullOrEmpty(original)?LevelKit.Root+"Scenes/InteractionLab.unity":original);AssetDatabase.DeleteAsset(scratch);Undo.ClearAll();}
   Debug.Log("WATERCOLOR_EDITOR_OK checks="+checks);BuildInteractionLab.ValidateAndBuild();
  }
 }
}
