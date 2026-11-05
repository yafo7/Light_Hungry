using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Gameplay acceptance through the same input fields as a human player.
// Never assigns puzzle states, cart position, travel distance, or actor position.
public class SeventhLevelValidation : MonoBehaviour {
 [Serializable] class Item { public string name; public bool passed; }
 [Serializable] class Report {
  public bool completed,inputDriven=true;
  public List<Item> checks=new List<Item>(); public List<string> errors=new List<string>();
  public int jumps,deaths,plannedDeaths,unexpectedDeaths,transformations,restorations,crossings;
  public float seconds; public string action;
 }
 readonly Report r=new Report(); SeventhTrellisFlow f; string output;
 bool failed,finished; float started; int observedFace=-1;
 float X=>f.player.Body.position.x*50;
 float Y=>887-f.player.Body.position.y*50;
 Vector3 P(float x,float y)=>new Vector3(x/50,(887-y)/50,0);
 void Log(string s,string stack,LogType type){if((type==LogType.Error||type==LogType.Exception)&&r.errors.Count<40)r.errors.Add(s+"\n"+stack);}
 void OnDestroy(){Application.logMessageReceived-=Log;}
 void Check(string name,bool value){r.checks.Add(new Item{name=name,passed=value});if(!value){failed=true;Debug.LogWarning("L07 CHECK FAILED: "+name);}}
 void Drive(float move=0,bool jump=false,bool interact=false){if(f)f.testInput=new ReservoirInput{move=move,jump=jump,interact=interact};}

 IEnumerator Start(){
  output=Path.GetFullPath(Application.dataPath+"/../../../Validation/level07/native-run");Directory.CreateDirectory(output);
  Application.logMessageReceived+=Log;started=Time.realtimeSinceStartup;
  float deadline=Time.realtimeSinceStartup+30;
  while(!f&&Time.realtimeSinceStartup<deadline){yield return null;f=FindObjectOfType<SeventhTrellisFlow>();}
  Check("SeventhTrellis runtime loaded",f!=null);if(failed){Finish();yield break;}
  f.Testing=true;f.cameraRig.Skip();yield return new WaitForSeconds(.5f);observedFace=f.ActiveFace;
  Inspect();yield return Capture("01-front-initial");
  yield return Walk(285);yield return Tap();
  Check("entity E at a mirror window cannot change faces",f.ActiveFace==0&&!f.ability.IsLight&&f.Crossings==0);
  yield return Charge();
  yield return Ride(1,420);yield return Restore(710);
  Check("first depth crossing reached rear in light form",f.ActiveFace==1&&f.Crossings==1);
  Check("rear arrival has not opened either mechanism",!f.StepsOpen&&!f.BridgeOpen);
  yield return Capture("02-back-initial");if(failed){Finish();yield break;}

  // Enter the rear jumping route as an entity, never by a high beam.
  yield return Walk(450);yield return JumpTo(510,650);
  yield return Walk(600);yield return JumpTo(680,590);
  yield return Walk(780);yield return JumpTo(860,530);
  yield return Walk(960);yield return JumpTo(1040,470);yield return Walk(1080);
  int faceBefore=f.ActiveFace;yield return Tap();yield return Stable();
  Check("K-back opens front shelves only",f.StepsOpen&&!f.BridgeOpen);
  Check("mechanism preview returns to the entity's rear face",f.ActiveFace==faceBefore&&!f.ability.IsLight);
  yield return Capture("03-back-lever-opened-front-steps");if(failed){Finish();yield break;}

  // Return down the physical shelves rather than teleporting back to the rail.
  yield return Walk(1030);yield return DropTo(960,530);
  yield return Walk(850);yield return DropTo(780,590);
  yield return Walk(670);yield return DropTo(600,650);
  yield return Walk(490);yield return DropTo(450,710);

  // A legitimate wrong hypothesis: push before the front bridge switch was used.
  yield return Tap();Check("E grips the single mirror cart",f.Grabbing);
  yield return PushTo(930);float blockedX=f.CartX;
  yield return Hold(1,1.0f);
  Check("closed bridge clamps the cart before the gap",!f.BridgeOpen&&f.CartX<=931&&Mathf.Abs(f.CartX-blockedX)<2);
  Check("pushing cancels light charging",!f.ability.IsLight&&f.ability.Charge<.1f);
  yield return PushTo(480);yield return Tap();
  Check("cart can return and dock at R0 without unlocking the bridge",!f.Grabbing&&f.DockIndex==0&&!f.BridgeOpen);
  yield return Walk(430);yield return Charge();yield return Ride(0,500);yield return Restore(710);
  Check("R0 light returns to front with open steps retained",f.ActiveFace==0&&f.StepsOpen&&f.Crossings==2);
  yield return Capture("04-front-steps-unfolded");yield return Capture("10-front-play-camera",false);if(failed){Finish();yield break;}

  // The low R0 light ends locally; all four ascending jumps are physically driven.
  yield return Walk(560);yield return JumpTo(615,650);
  yield return Walk(710);yield return JumpTo(790,590);
  yield return Walk(880);yield return JumpTo(960,530);
  yield return Walk(1050);yield return JumpTo(1130,470);yield return Walk(1170);
  yield return Tap();yield return Stable();
  Check("K-front opens rear rail bridge and preserves front steps",f.BridgeOpen&&f.StepsOpen);
  Check("front switch does not move the return cart",f.DockIndex==0&&Mathf.Abs(f.CartX-480)<2);
  yield return Capture("05-front-switch-opened-back-bridge");if(failed){Finish();yield break;}

  // Deliberately fall through a real gap. States and the legitimate return light persist.
  yield return PlannedFall();
  Check("both cross-face switches persist after the planned fall",f.StepsOpen&&f.BridgeOpen);
  Check("respawn is a fixed front safe bank with light available",f.ActiveFace==0&&X<580&&Mathf.Abs(Y-710)<12);
  yield return Walk(500);yield return Charge();yield return Ride(1,430);yield return Restore(710);
  Check("reverse travel through R0 returns to rear",f.ActiveFace==1&&f.Crossings==3);
  yield return Capture("06-back-bridge-open");if(failed){Finish();yield break;}

  // Stop halfway: solid trellis backing cannot become an arbitrary front portal.
  yield return Walk(450);yield return Tap();Check("cart can be re-gripped after returning",f.Grabbing);
  yield return PushTo(1100);yield return Tap();
  Check("mid-rail position has no return window",f.DockIndex==-1&&!f.path.HasReturnWindow);
  yield return Capture("07-cart-mid-bridge");
  yield return Walk(f.CartX-35);yield return Charge();yield return ProbeMiddleEnd();
  Check("mid-rail light does not cross to front",f.ActiveFace==1&&f.Crossings==3);
  if(f.ability.IsLight)yield return Restore(710);else yield return WaitGround(710);
  Check("mid-rail exploration retains a usable rear landing",!f.ability.IsLight&&f.player.Grounded&&f.BridgeOpen);
  yield return Tap();Check("mid-rail cart remains operable",f.Grabbing);
  yield return PushTo(1455);yield return Tap();
  Check("same cart docks at R1 with a new front return window",f.DockIndex==1&&f.path.HasReturnWindow&&f.CartX>1450);
  yield return Capture("08-cart-at-R1");yield return Capture("11-back-play-camera",false);if(failed){Finish();yield break;}

  yield return Walk(1420);yield return Charge();yield return Ride(0,1475);yield return Restore(710);
  Check("fourth crossing reaches front behind the permanent wall",f.ActiveFace==0&&X>1380&&f.Crossings==4);
  yield return new WaitForSeconds(2.3f);
  Check("R1 beam feeds the target scale",f.Awakened);
  yield return Capture("09-front-fed-scale");yield return Walk(1725);
  Check("level seven completed at the front exit",f.Completed&&f.ActiveFace==0);
  Check("only the explicitly planned fall occurred",f.Deaths==r.plannedDeaths&&r.plannedDeaths==1);
  Check("all required jumps were executed",f.player.JumpCount>=8);
  Finish();
 }

 void Inspect(){
  bool scenes=true;foreach(string n in new[]{"FirstLight","SecondLight","ThirdDrain","FourthReservoir","FifthPumpHouse","SixthGreenhouse","SeventhTrellis"})scenes&=Application.CanStreamedLevelBeLoaded(n);
  Check("all seven scenes included in player",scenes);
  Check("one shared optical path across both faces",FindObjectsOfType<LightPath>().Length==1&&LightPath.Active.Count==1);
  Check("initial state has front steps folded and rear bridge shut",f.ActiveFace==0&&!f.StepsOpen&&!f.BridgeOpen&&f.DockIndex==0);
  Check("three front treads initially have no entity collision",f.frontStepColliders.Length==3&&!f.frontStepColliders[0].enabled&&!f.frontStepColliders[1].enabled&&!f.frontStepColliders[2].enabled);
  Check("rear bridge initially absent while the rear leaf blocker is closed",!f.bridgeCollider.enabled&&f.screenBlock.enabled);
  Check("global light charge remains three seconds",Mathf.Abs(f.ability.chargeSeconds-3)<.01f);
  Check("original rigged spirit used",f.player.visual.GetComponentInChildren<SkinnedMeshRenderer>(true)!=null);
  bool nova=false;foreach(var renderer in f.player.GetComponentsInChildren<ParticleSystemRenderer>(true))foreach(var material in renderer.sharedMaterials)if(material&&material.shader&&material.shader.name=="Nova/Particles/UberUnlit")nova=true;
  Check("head particles retain runtime NOVA shader",nova);
  Check("K-front is beyond a single ground jump",(710-470)/50f>f.player.jumpSpeed*f.player.jumpSpeed/(2*Mathf.Abs(Physics2D.gravity.y)*f.player.Body.gravityScale));
  InspectFaceCollision();
 }

 void InspectFaceCollision(){
  if(f.TransitionBusy||f.PreviewBusy)return;
  bool inactiveClear=true;foreach(var c in f.faceRoots[1-f.ActiveFace].GetComponentsInChildren<Collider2D>(true))inactiveClear&=!c.isActiveAndEnabled;
  Check("inactive face supplies no physics collision",inactiveClear);
  Check("current face root remains active",f.faceRoots[f.ActiveFace].activeInHierarchy);
 }

 IEnumerator Stable(){if(failed)yield break;float until=Time.time+6;while((f.TransitionBusy||f.PreviewBusy||!f.MechanismsSettled)&&Time.time<until){Drive();yield return null;}Check("camera and mechanisms settled",!f.TransitionBusy&&!f.PreviewBusy&&f.MechanismsSettled);}
 IEnumerator Tap(){if(failed)yield break;Drive(interact:true);yield return null;Drive();yield return new WaitForSeconds(.18f);}
 IEnumerator Hold(float direction,float seconds){if(failed)yield break;float until=Time.time+seconds;while(Time.time<until){Drive(direction);yield return null;}Drive();}
 IEnumerator Walk(float target){
  if(failed)yield break;r.action="walk to "+target;float until=Time.time+12;
  while(Mathf.Abs(X-target)>3&&Time.time<until&&!f.Completed){Drive(Mathf.Sign(target-X));yield return null;}
  Drive();yield return new WaitForSeconds(.13f);Check(r.action+" at "+X.ToString("F1"),f.Completed||Mathf.Abs(X-target)<10);
 }
 IEnumerator Charge(){
  if(failed)yield break;float startX=X;yield return Walk(startX-8);yield return Walk(startX);if(failed)yield break;
  r.action="charge at "+X.ToString("F1")+","+Y.ToString("F1");float until=Time.time+5,start=Time.time,previous=f.ability.Charge;
  while(!f.ability.IsLight&&Time.time<until){Drive();yield return null;}
  Check(r.action,f.ability.IsLight);Check("charge duration obeys three-second rule",!f.ability.IsLight||Time.time-start+previous>=2.85f);
 }
 IEnumerator Ride(int face,float x){
  if(failed)yield break;r.action="light ride to face "+face+" x="+x;
  float goal=f.path.GetAlong(face,P(x,670)),until=Time.time+20;
  bool remainedLight=f.ability.IsLight;
  while(Time.time<until){
   remainedLight&=f.ability.IsLight;if(!f.ability.IsLight)break;
   if(f.ActiveFace==face&&!f.TransitionBusy&&Mathf.Abs(f.ability.Along-goal)<.065f)break;
   Drive(Mathf.Sign(goal-f.ability.Along));yield return null;
  }
  Drive();yield return new WaitForFixedUpdate();
  Check(r.action,remainedLight&&f.ability.IsLight&&f.ActiveFace==face&&!f.TransitionBusy&&Mathf.Abs(f.ability.Along-goal)<.16f);
  InspectFaceCollision();
 }
 IEnumerator Restore(float floor){if(failed)yield break;if(f.ability.IsLight)yield return Tap();yield return WaitGround(floor);}
 IEnumerator WaitGround(float floor){
  if(failed)yield break;float until=Time.time+2;
  while((f.ability.IsLight||!f.player.Grounded)&&Time.time<until){Drive();yield return null;}
  Check("restore on floor "+floor+" at "+X.ToString("F1")+","+Y.ToString("F1"),!f.ability.IsLight&&f.player.Grounded&&Mathf.Abs(Y-floor)<12);
 }
 IEnumerator JumpTo(float x,float floor){
  if(failed)yield break;r.action="jump to "+x+","+floor;float until=Time.time+2;
  while(!f.player.Grounded&&Time.time<until){Drive();yield return null;}
  int count=f.player.JumpCount;Drive(Mathf.Sign(x-X),true);yield return null;yield return new WaitForSeconds(.07f);until=Time.time+4;
  while(Time.time<until){Drive(Mathf.Abs(X-x)<3?0:Mathf.Sign(x-X));yield return null;if(f.player.Grounded&&f.player.Body.velocity.y<=.1f&&Mathf.Abs(Y-floor)<10&&Mathf.Abs(X-x)<15)break;}
  Drive();yield return new WaitForSeconds(.13f);Check(r.action+" landed="+X.ToString("F1")+","+Y.ToString("F1"),f.player.JumpCount>count&&f.player.Grounded&&Mathf.Abs(X-x)<20&&Mathf.Abs(Y-floor)<12);
 }
 IEnumerator DropTo(float x,float floor){
  if(failed)yield break;r.action="drop to "+x+","+floor;float until=Time.time+5;
  while(Time.time<until){Drive(Mathf.Abs(X-x)<3?0:Mathf.Sign(x-X));yield return null;if(f.player.Grounded&&Mathf.Abs(Y-floor)<10&&Mathf.Abs(X-x)<10)break;}
  Drive();yield return new WaitForSeconds(.13f);Check(r.action+" landed="+X.ToString("F1")+","+Y.ToString("F1"),f.player.Grounded&&Mathf.Abs(X-x)<15&&Mathf.Abs(Y-floor)<12);
 }
 IEnumerator PushTo(float x){
  if(failed)yield break;r.action="push cart to "+x;float until=Time.time+14;
  while(Mathf.Abs(f.CartX-x)>2&&Time.time<until&&f.Grabbing){Drive(Mathf.Sign(x-f.CartX));yield return null;}
  Drive();yield return new WaitForSeconds(.12f);Check(r.action,f.Grabbing&&Mathf.Abs(f.CartX-x)<5);
 }
 IEnumerator PlannedFall(){
  if(failed)yield break;r.action="planned fall in the gap between front third shelf and K-front";
  int deaths=f.Deaths;float until=Time.time+6;bool falling=false;
  while(Time.time<until&&f.Deaths==deaths){Drive(Mathf.Abs(X-1090)<2?0:Mathf.Sign(1090-X));yield return null;falling|=!f.player.Grounded&&f.player.Body.velocity.y<-.5f&&Y>540;}
  Drive();if(f.Deaths==deaths+1)r.plannedDeaths++;
  Check("intentional gap fall reaches ordinary respawn",falling&&f.Deaths==deaths+1);
  yield return WaitGround(710);
 }
 IEnumerator ProbeMiddleEnd(){
  if(failed)yield break;r.action="follow mid-rail beam into opaque-backed cart endpoint";float until=Time.time+4;
  while(f.ability.IsLight&&Time.time<until){Drive(1);yield return null;if(f.ability.Blocked||f.path.Length-f.ability.Along<.06f)break;}
  Drive();yield return new WaitForSeconds(.15f);
  Check("solid-backed middle position has no depth continuation",f.DockIndex==-1&&!f.path.HasReturnWindow&&f.ActiveFace==1);
 }
 IEnumerator Capture(string name,bool fullOverview=true){
  if(failed)yield break;bool previousOverview=f.cameraRig.Overview;if(previousOverview!=fullOverview)f.cameraRig.Toggle();
  // Follow framing eases from the preceding overview; allow the camera to settle.
  yield return new WaitForSeconds(fullOverview?.14f:.9f);yield return new WaitForEndOfFrame();NativeFrameCapture.Save(output+"/"+name+".png");
  yield return new WaitForSeconds(.14f);if(f.cameraRig.Overview!=previousOverview)f.cameraRig.Toggle();
 }
 void Update(){
  if(f&&!finished&&observedFace>=0&&observedFace!=f.ActiveFace){Check("face crossing preserves light form",f.ability.IsLight);observedFace=f.ActiveFace;}
  if(!finished&&started>0&&Time.realtimeSinceStartup-started>350){Check("watchdog: "+r.action,false);StopAllCoroutines();Finish();}
 }
 void Finish(){
  if(finished)return;finished=true;Drive();r.completed=!failed&&f&&f.Completed&&r.errors.Count==0;r.seconds=Time.realtimeSinceStartup-started;
  if(f){r.jumps=f.player.JumpCount;r.deaths=f.Deaths;r.unexpectedDeaths=Mathf.Max(0,r.deaths-r.plannedDeaths);r.transformations=f.ability.TransformCount;r.restorations=f.ability.RestoreCount;r.crossings=f.Crossings;}
  File.WriteAllText(output+"/playthrough.json",JsonUtility.ToJson(r,true));Application.Quit(r.completed?0:1);
 }
}
