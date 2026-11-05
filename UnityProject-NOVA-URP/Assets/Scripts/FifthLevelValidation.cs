using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FifthLevelValidation:MonoBehaviour {
 [Serializable]class CheckItem {public string name;public bool passed;}
 [Serializable]class Report {public bool completed,inputDriven=true;public List<CheckItem> checks=new List<CheckItem>();public List<string> errors=new List<string>();public int jumps,deaths,transformations;public string action;}
 Report r=new Report();FifthPumpFlow f;string output;bool fail;float begin;
 void Log(string s,string st,LogType t){if(t==LogType.Exception||t==LogType.Error)r.errors.Add(s);}
 void OnDestroy(){Application.logMessageReceived-=Log;}
 void Check(string s,bool p){r.checks.Add(new CheckItem{name=s,passed=p});if(!p){fail=true;Debug.LogWarning("L05 CHECK FAILED: "+s);}}
 float X=>f.player.Body.position.x*80;float Y=>1024-f.player.Body.position.y*80;
 void Input(float move=0,bool jump=false,bool interact=false,float adjust=0){f.testInput=new ReservoirInput{move=move,jump=jump,interact=interact,adjust=adjust};}
 IEnumerator Start(){
  Application.logMessageReceived+=Log;output=Path.GetFullPath(Application.dataPath+"/../../../Validation/level05/native-run");Directory.CreateDirectory(output);
  yield return null;f=FindObjectOfType<FifthPumpFlow>();while(!f){yield return null;f=FindObjectOfType<FifthPumpFlow>();}f.Testing=true;f.cameraRig.Skip();begin=Time.time;
  yield return new WaitForSeconds(.5f);
  Check("five scenes in build",Application.CanStreamedLevelBeLoaded("FirstLight")&&Application.CanStreamedLevelBeLoaded("FourthReservoir")&&Application.CanStreamedLevelBeLoaded("FifthPumpHouse"));
  Check("three initially folded steps",!f.StepsOpen&&!f.stepColliders[0].enabled);
  Check("one optical path",FindObjectsOfType<LightPath>().Length==1);
  Check("real rigged character",f.player.visual.GetComponentInChildren<SkinnedMeshRenderer>()!=null);
  Check("glass transmits light but supports entity",f.ability.restoreSurfaces.value==(1<<10)&&f.ability.obstacles.value==(1<<6));
  foreach(var t in f.steps){var m=t.GetComponent<MeshFilter>().sharedMesh;Check(t.name+" horizontal",Mathf.Abs(m.vertices[2].y-m.vertices[3].y)<.00001f&&t.rotation==Quaternion.identity);}
  yield return Capture("01-initial");
  yield return Charge();yield return Ride(FifthPumpFlow.A);if(fail){Finish();yield break;}
  Check("A reached by light",Y>745&&Y<755);
  yield return Walk(1180);yield return Tap();yield return Adjust(1);yield return Tap();Check("A aimed into B",f.aimA==1);
  yield return Walk(1118);yield return Charge();yield return Ride(FifthPumpFlow.Px(430,475),true);if(fail){Finish();yield break;}
  Check("B light-only slit crossed",X<575&&Y>532&&Y<548);
  yield return Tap();yield return new WaitForSeconds(1);Check("B unfolds and latches steps",f.StepsOpen&&f.stepColliders[0].enabled);yield return Capture("02-inspection-chamber");
  yield return Walk(420);yield return Charge();yield return Ride(FifthPumpFlow.A);yield return Walk(1180);yield return Tap();yield return Adjust(1);yield return Tap();
  Check("B light reclaimed for suspended mirror",f.aimA==2&&f.StepsOpen);yield return Walk(1280);
  yield return JumpTo(1365,620);yield return JumpTo(1270,521);yield return Walk(1330);yield return JumpTo(1370,444);yield return Walk(1328);yield return JumpTo(1225,385);
  if(fail){Finish();yield break;}Check("C reached by entity switchback jumps",Y>378&&Y<390);yield return Tap();yield return Adjust(1);yield return Tap();Check("C connects suspended mirror to D",f.aimC==1);yield return Capture("03-mirror-back-control");
  yield return DropTo(1370,444);yield return DropTo(1480,750);yield return Walk(1118);
  if(fail){Finish();yield break;}yield return Charge();yield return Ride(FifthPumpFlow.D,true);if(fail){Finish();yield break;}
  Check("D entered through glass as light",Y>252&&Y<268);yield return Walk(1320);yield return Tap();yield return Adjust(1);yield return Tap();yield return new WaitForSeconds(2.3f);
  Check("gray scale fed and roots opened",f.Awakened&&!f.exitBlock.enabled);yield return Capture("04-fed-scale");yield return Walk(1490);Check("level completed",f.Completed);Finish();
 }
 IEnumerator Tap(){Input(interact:true);yield return null;Input();yield return new WaitForSeconds(.15f);}
 IEnumerator Adjust(float value){Input(adjust:value);yield return null;Input();yield return new WaitForSeconds(.4f);}
 IEnumerator Walk(float x){r.action="walk "+x;float end=Time.time+10;while(Mathf.Abs(X-x)>4&&Time.time<end&&!fail&&!f.Completed){Input(Mathf.Sign(x-X));yield return null;}Input();yield return new WaitForSeconds(.15f);Check(r.action,f.Completed||Mathf.Abs(X-x)<12);}
 IEnumerator DropTo(float x,float y){r.action="drop "+x+","+y;float end=Time.time+5;while(Time.time<end){Input(Mathf.Abs(X-x)<4?0:Mathf.Sign(x-X));yield return null;if(f.player.Grounded&&Mathf.Abs(Y-y)<12&&Mathf.Abs(X-x)<12)break;}Input();yield return new WaitForSeconds(.12f);Check(r.action+" landed="+X+","+Y,Mathf.Abs(Y-y)<12);}
 IEnumerator Charge(){r.action="charge at "+X+","+Y;float end=Time.time+5;while(!f.ability.IsLight&&Time.time<end){Input();yield return null;}Check(r.action,f.ability.IsLight);}
 IEnumerator Ride(Vector3 target,bool forwardEnd=false){r.action="ride to "+target;float along;Vector2 pt;f.path.Closest(target,out along,out pt);float end=Time.time+15;while(f.ability.IsLight&&Time.time<end){if(forwardEnd&&f.ability.Along>f.path.Length-.03f)break;float d=along-f.ability.Along;if(!forwardEnd&&Mathf.Abs(d)<.05f)break;Input(forwardEnd?1:Mathf.Sign(d));yield return null;}Input();if(f.ability.IsLight)yield return Tap();yield return new WaitForSeconds(.3f);Check(r.action,!f.ability.IsLight);}
 IEnumerator JumpTo(float x,float y){r.action="jump "+x+","+y;float end=Time.time+4;while((!f.player.Grounded||f.player.Body.velocity.y>.1f)&&Time.time<end){Input();yield return null;}Input(Mathf.Sign(x-X),true);yield return null;yield return new WaitForSeconds(.08f);end=Time.time+4;while(Time.time<end){Input(Mathf.Abs(X-x)<4?0:Mathf.Sign(x-X));yield return null;if(f.player.Grounded&&f.player.Body.velocity.y<=.1f&&Mathf.Abs(Y-y)<12)break;}Input();yield return new WaitForSeconds(.12f);Check(r.action+" landed="+X+","+Y,Mathf.Abs(X-x)<35&&Mathf.Abs(Y-y)<12);}
 IEnumerator Capture(string n){f.cameraRig.Toggle();yield return new WaitForSeconds(.1f);NativeFrameCapture.Save(output+"/"+n+".png");yield return new WaitForSeconds(.2f);f.cameraRig.Toggle();}
 void Update(){if(begin>0&&Time.time-begin>150){Check("watchdog "+r.action,false);Finish();StopAllCoroutines();begin=0;}}
 void Finish(){r.completed=!fail&&f.Completed&&r.errors.Count==0;r.jumps=f.player.JumpCount;r.deaths=f.Deaths;r.transformations=f.ability.TransformCount;File.WriteAllText(output+"/playthrough.json",JsonUtility.ToJson(r,true));Application.Quit(r.completed?0:1);}
}
