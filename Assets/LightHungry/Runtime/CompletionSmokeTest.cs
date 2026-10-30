using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace LightHungry {
 public sealed class CompletionSmokeTest:MonoBehaviour {
  readonly List<string> checks=new List<string>();int events;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-completionQA"))new GameObject("Completion QA").AddComponent<CompletionSmokeTest>().StartCoroutine("Run");}
  void Check(bool ok,string name){string line=(ok?"PASS ":"FAIL ")+name;checks.Add(line);Debug.Log(line);}
  void CaptureNotice(){
   var canvas=FindObjectsOfType<Canvas>().First(c=>c.name=="Level complete notice");var camera=Camera.main;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();
   var rt=new RenderTexture(1280,720,24);var old=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(Path.GetFullPath("Verification/completion-notice.png"),texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;canvas.renderMode=RenderMode.ScreenSpaceOverlay;rt.Release();Destroy(rt);Destroy(texture);
  }
  IEnumerator Run(){
   yield return new WaitForSeconds(.2f);var session=LabSession.Instance;session.GetComponent<LightRouteReview>().automatic=false;session.player.automationInput=true;session.player.GetComponent<LightExposure>().automatic=false;
   foreach(var source in FindObjectsOfType<LightEmitter>())source.active=false;foreach(var old in FindObjectsOfType<BeastReceiver>())old.isGoal=false;
   session.goal=null;session.LevelCompleted+=()=>events++;yield return null;Check(!session.Completed&&!session.CompletionNoticeVisible,"No target does not complete empty level");
   var g=new GameObject("QA target beast");g.layer=12;g.transform.position=new Vector3(1.5f,2,0);var beast=g.AddComponent<BeastReceiver>();beast.requiredSeconds=.7f;g.AddComponent<SphereCollider>().radius=.45f;session.goal=beast;
   yield return new WaitForSeconds(.1f);Check(!session.Completed,"Hungry target does not complete level");
   var other=new GameObject("QA other beast").AddComponent<BeastReceiver>();other.Lit=true;other.Tick(10);yield return null;Check(!session.Completed,"A satisfied unselected beast does not complete selected goal");
   session.goal=null;other.isGoal=false;
   var light=new GameObject("QA feeding beam").AddComponent<LightEmitter>();light.transform.position=new Vector3(-1,2,0);light.direction=Vector2.right;light.beamWidth=.4f;yield return new WaitForSeconds(.3f);Check(beast.Progress>0&&!session.Completed,"Real beam feeds beast before completion threshold");
   yield return new WaitForSeconds(.65f);Check(beast.Satisfied&&session.Completed&&session.CompletionNoticeVisible,"Fed goal completes level and displays notice");
   yield return new WaitForSeconds(.35f);var label=FindObjectsOfType<Text>().FirstOrDefault(t=>t.text=="关卡完成");Check(label&&label.font&&"关卡完成".All(c=>label.font.HasCharacter(c)),"Completion notice contains rendered Chinese glyphs");
   Check(FindObjectsOfType<Canvas>().Count(c=>c.name=="Level complete notice")==1&&events==1,"Exactly one completion notification and event");Check(Time.timeScale==1&&!session.GetComponent<LightRouteReview>().IsPlaying,"Completion leaves gameplay running and 3D review disabled");
   Directory.CreateDirectory("Verification");CaptureNotice();yield return new WaitForEndOfFrame();yield return new WaitForSeconds(.1f);light.active=false;beast.Clear();yield return new WaitForSeconds(.1f);Check(session.Completed&&events==1,"Leaving light or clearing receiver cannot retrigger completion");
   string result=Path.GetFullPath("Verification/completion-qa.txt");
   var operation=SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);DontDestroyOnLoad(gameObject);while(!operation.isDone)yield return null;yield return null;Check(!LabSession.Instance.Completed&&!LabSession.Instance.CompletionNoticeVisible,"Scene reload resets completion notice");
   File.WriteAllLines(result,checks);Application.Quit(checks.Any(c=>c.StartsWith("FAIL"))?1:0);
  }
 }
}
