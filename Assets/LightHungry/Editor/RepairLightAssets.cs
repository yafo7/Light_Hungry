using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace LightHungry.EditorTools {
 public static class RepairLightAssets {
  public static void Install(){
   MotionFrameRepair.Repair();if(!AssetDatabase.IsValidFolder("Assets/LightHungry/Resources"))AssetDatabase.CreateFolder("Assets/LightHungry","Resources");const string matPath="Assets/LightHungry/Resources/LightColumn.mat";if(!AssetDatabase.LoadAssetAtPath<Material>(matPath)&&AssetDatabase.LoadAssetAtPath<Material>("Assets/LightHungry/Materials/LightColumn.mat"))AssetDatabase.MoveAsset("Assets/LightHungry/Materials/LightColumn.mat",matPath);var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(!mat){mat=new Material(Shader.Find("LightHungry/LightColumn"));AssetDatabase.CreateAsset(mat,matPath);}
   const string prefabPath="Assets/LightHungry/Prefabs/LightBeam.prefab";
   if(!AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)){var g=new GameObject("LightBeam");g.AddComponent<FaceMembership>();var light=g.AddComponent<LightEmitter>();light.directionFromTransform=true;light.beamWidth=.65f;light.maxLength=30;light.columnMaterial=mat;g.AddComponent<LightColumnPreview>();PrefabUtility.SaveAsPrefabAsset(g,prefabPath);UnityEngine.Object.DestroyImmediate(g);}
   var prefab=PrefabUtility.LoadPrefabContents(prefabPath);prefab.GetComponent<LightEmitter>().beamColor=new Color(.42f,.76f,.90f,.78f);PrefabUtility.SaveAsPrefabAsset(prefab,prefabPath);PrefabUtility.UnloadPrefabContents(prefab);
   AssetDatabase.SaveAssets();Debug.Log("LIGHT_ASSET_REPAIR_OK");
  }
  public static void VerifyAndBuild(){Install();var frames=WatercolorAssets.Frames("motion");if(frames.Length!=16||Array.Exists(frames,s=>!AssetDatabase.GetAssetPath(s.texture).EndsWith(".lhframes")))throw new Exception("Motion silhouettes were not isolated");var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightHungry/Prefabs/LightBeam.prefab");if(!prefab||!prefab.GetComponent<LightEmitter>().directionFromTransform||!prefab.GetComponent<LightColumnPreview>())throw new Exception("Light prefab is incomplete");Debug.Log("LIGHT_FIX_EDITOR_CHECKS_OK");BuildInteractionLab.ValidateAndBuild();}
 }
}
