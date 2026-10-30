using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LightHungry {
 public sealed class LightFixSmokeTest:MonoBehaviour {
  readonly List<string> results=new List<string>();string folder;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-lightFixQA"))new GameObject("Light fix QA").AddComponent<LightFixSmokeTest>().StartCoroutine("Run");}
  void Check(bool ok,string label){results.Add((ok?"PASS ":"FAIL ")+label);Debug.Log(results.Last());}
  void Capture(Camera camera,string name){var rt=new RenderTexture(1400,900,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1400,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1400,900),0,0);t.Apply();File.WriteAllBytes(Path.Combine(folder,name),t.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;rt.Release();Destroy(rt);Destroy(t);}
  IEnumerator Run(){
   folder=Path.GetFullPath("Verification");Directory.CreateDirectory(folder);yield return new WaitForSeconds(.5f);
   var player=FindObjectOfType<PlayerController>();foreach(var e in FindObjectsOfType<LightExposure>())e.automatic=false;foreach(var l in FindObjectsOfType<LightEmitter>())l.active=false;player.automationInput=true;
   var presentation=player.GetComponent<PlayerPresentation>();var frames=presentation.idle.Concat(presentation.walk).Concat(presentation.jump).ToArray();
   var group=new GameObject("Repaired frame inspection");group.AddComponent<FaceMembership>().faces=RoomFaces.All;
   var camera=new GameObject("QA camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5.6f;camera.backgroundColor=new Color(.2f,.27f,.32f);camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.position=new Vector3(0,100.5f,-20);
   for(int i=0;i<frames.Length;i++){var g=new GameObject(frames[i].name);g.transform.SetParent(group.transform);g.transform.position=new Vector3((i%4-1.5f)*2.1f,103.2f-(i/4)*2.3f,0);var r=g.AddComponent<SpriteRenderer>();r.sprite=frames[i];r.sharedMaterial=player.visual.sharedMaterial;g.transform.localScale=Vector3.one*1.7f;Check(frames[i].texture.name==frames[i].name+"_texture","Isolated frame excludes adjacent poses: "+frames[i].name);}
   Capture(camera,"repaired-motion-frames.png");Destroy(group);Destroy(camera.gameObject);
   var light=new GameObject("QA light column").AddComponent<LightEmitter>();light.directionFromTransform=true;light.transform.position=new Vector3(-12,2,0);light.beamWidth=.8f;light.maxLength=7;yield return null;yield return new WaitForEndOfFrame();
   var columns=FindObjectsOfType<BeamColumn>();Check(columns.Any(c=>Mathf.Abs(c.transform.localScale.x-.8f)<.01f&&Mathf.Abs(c.transform.localScale.z-7)<.1f),"Column mesh uses configured diameter and length");
   light.transform.rotation=Quaternion.Euler(0,0,-25);light.beamWidth=1.4f;yield return null;yield return new WaitForEndOfFrame();Check(FindObjectsOfType<BeamColumn>().Any(c=>Mathf.Abs(c.transform.localScale.x-1.4f)<.01f&&Vector3.Dot(c.transform.forward,light.transform.right)>.99f),"Rotating source and changing width updates beam");
   CameraViewController.Active.EndTravelView(1);Capture(Camera.main,"light-column-preview.png");light.transform.rotation=Quaternion.identity;light.transform.position=new Vector3(-12,-3.9f,0);light.maxLength=3;light.beamWidth=1.2f;player.SetSpatialPosition(new Vector3(-10,-4.95f,.65f));player.GetComponent<LightExposure>().automatic=true;yield return new WaitForSeconds(.5f);Check(player.GetComponent<LightExposure>().Progress>.2f,"Wide light column illuminates body away from central ray");
   light.active=false;yield return null;yield return new WaitForEndOfFrame();Check(!FindObjectsOfType<BeamColumn>().Any(c=>c.gameObject.activeInHierarchy),"Disabling source hides all pooled columns");File.WriteAllLines(Path.Combine(folder,"light-fix-qa.txt"),results);Application.Quit(results.Any(x=>x.StartsWith("FAIL"))?1:0);
  }
 }
}
