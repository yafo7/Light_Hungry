using System.IO;
using UnityEditor;
using UnityEngine;

public static class ModelPreview {
 public static void Render(){
  string output=Path.GetFullPath(Application.dataPath+"/../../Validation/model-views");Directory.CreateDirectory(output);
  var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WickSpirit.prefab");var model=Object.Instantiate(source);model.transform.position=new Vector3(140,0,0);
  foreach(var t in model.GetComponentsInChildren<Transform>())t.gameObject.layer=10;
  var obj=new GameObject("ModelInspectionCamera");var cam=obj.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=.65f;cam.transform.position=new Vector3(140,.5f,-3);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.2f,.23f,.22f);cam.cullingMask=1<<10;
  var rt=new RenderTexture(640,640,24,RenderTextureFormat.ARGB32);cam.targetTexture=rt;
  foreach(int angle in new[]{0,90,180,270}){model.transform.rotation=Quaternion.Euler(0,angle,0);cam.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(640,640,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,640,640),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(output,"yaw-"+angle+".png"),texture.EncodeToPNG());Object.DestroyImmediate(texture);RenderTexture.active=previous;}
  cam.targetTexture=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(obj);Object.DestroyImmediate(model);
 }
}
