using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace LightHungry.EditorTools {
 public static class RoomBackgrounds {
  static readonly string[] Files={"background","background_back","background_left","background_right"};
  public static void Import(){foreach(string file in Files){string path=WatercolorAssets.Folder+file+".png";var imp=(TextureImporter)AssetImporter.GetAtPath(path);if(!imp)throw new Exception("Missing background "+path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=100;imp.alphaIsTransparency=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=2048;imp.SaveAndReimport();}}
  public static void Assign(GameObject go,int face){var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(WatercolorAssets.Folder+Files[face-1]+".png");var r=go.GetComponentInChildren<SpriteRenderer>();if(!r)return;r.sprite=sprite;r.transform.localScale=Vector3.one*(32/sprite.bounds.size.x);EditorUtility.SetDirty(r);PrefabUtility.RecordPrefabInstancePropertyModifications(r);PrefabUtility.RecordPrefabInstancePropertyModifications(r.transform);}
  public static void UpdateExisting(){
   Import();var original=SceneManager.GetActiveScene();string path=LevelKit.Root+"Scenes/InteractionLab.unity";var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
   foreach(var root in scene.GetRootGameObjects()){
    foreach(var review in root.GetComponentsInChildren<LightRouteReview>(true)){review.automatic=false;EditorUtility.SetDirty(review);PrefabUtility.RecordPrefabInstancePropertyModifications(review);}
    if(root.name.StartsWith("Backdrop ")&&int.TryParse(root.name.Substring(9),out int face)&&face>=1&&face<=4)Assign(root,face);
   }
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);if(opened)EditorSceneManager.CloseScene(scene,true);if(original.isLoaded)SceneManager.SetActiveScene(original);
   AssetDatabase.SaveAssets();Debug.Log("ROOM_BACKGROUND_UPDATE_OK");
  }
 }
}
