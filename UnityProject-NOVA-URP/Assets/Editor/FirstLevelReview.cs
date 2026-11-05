using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FirstLevelReview {
 public static void Open(){
  EditorSceneManager.OpenScene("Assets/Scenes/FirstLight.unity");
  EditorApplication.delayCall+=()=>{
   var player=GameObject.Find("WickSpirit_Player");Selection.activeGameObject=player;
   var scene=SceneView.lastActiveSceneView;if(scene){scene.in2DMode=true;scene.pivot=new Vector3(40,10,0);scene.rotation=Quaternion.identity;scene.size=12;scene.Repaint();}
   EditorApplication.ExecuteMenuItem("Window/General/Game");
   Debug.Log("FIRST_LIGHT_REVIEW_OPEN: Assets/Scenes/FirstLight.unity");
  };
 }
}
