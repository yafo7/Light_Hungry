using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Native-player acceptance: all puzzle progress is driven through the same input
// structure used by the player. No mirror, curtain or tray state is assigned here.
public class SixthLevelValidation : MonoBehaviour {
 [Serializable] class CheckItem { public string name; public bool passed; }
 [Serializable] class Report {
  public bool completed,inputDriven=true;
  public List<CheckItem> checks=new List<CheckItem>();
  public List<string> errors=new List<string>();
  public int jumps,deaths,transformations,shortDrops;
  public float seconds;
  public string action;
 }
 readonly Report report=new Report();
 SixthGreenhouseFlow f;
 string output;
 bool failed,finished;
 float started;
 float X=>f.player.Body.position.x*50;
 float Y=>887-f.player.Body.position.y*50;
 Vector3 LastNode=>f.path.nodes[f.path.nodes.Length-1];

 void Log(string s,string stack,LogType type){
  if((type==LogType.Error||type==LogType.Exception)&&report.errors.Count<50)report.errors.Add(s+"\n"+stack);
 }
 void OnDestroy(){Application.logMessageReceived-=Log;}
 void Check(string name,bool pass){
  report.checks.Add(new CheckItem{name=name,passed=pass});
  if(!pass){failed=true;Debug.LogWarning("L06 CHECK FAILED: "+name);}
 }
 void Drive(float move=0,bool jump=false,bool interact=false,float adjust=0){
  if(f)f.testInput=new ReservoirInput{move=move,jump=jump,interact=interact,adjust=adjust};
 }

 IEnumerator Start(){
  output=Path.GetFullPath(Application.dataPath+"/../../../Validation/level06/native-run");
  Directory.CreateDirectory(output);
  Application.logMessageReceived+=Log;
  started=Time.realtimeSinceStartup;
  float findUntil=Time.realtimeSinceStartup+30;
  while(!f&&Time.realtimeSinceStartup<findUntil){yield return null;f=FindObjectOfType<SixthGreenhouseFlow>();}
  Check("SixthGreenhouse scene and flow loaded",f!=null);
  if(failed){Finish();yield break;}
  f.Testing=true;f.cameraRig.Skip();
  yield return new WaitForSeconds(.5f);
  InspectScene();
  yield return Walk(255);
  yield return Capture("01-initial");
  if(failed){Finish();yield break;}

  // First failed hypothesis: the upper route is still blocked by G.
  yield return SetAim(0,1);
  Check("closed G truncates the attempted upper route",!f.CurtainOpen&&LastNode.x<SixthGreenhouseFlow.C.x-.2f);
  yield return Charge();
  yield return TravelToBlockedEnd();
  Check("closed-curtain endpoint does not grant an entity landing",f.ability.IsLight&&!f.CurtainOpen);
  yield return Tap();
  Check("E without a nearby safe floor preserves light form",f.ability.IsLight);
  yield return RideAndRestore(SixthGreenhouseFlow.A,710);
  yield return SetAim(0,0);
  Check("returned from G and restored lower beam",f.aimA==0&&!f.CurtainOpen);
  if(failed){Finish();yield break;}

  // Reach B and lower the tray. It remains a fixed low landing while in use.
  yield return Charge();
  yield return RideAndRestore(SixthGreenhouseFlow.B,710);
  yield return Tap();
  yield return WaitTray(false);
  Check("B lowered tray without opening the curtain",!f.TrayHigh&&!f.CurtainOpen);
  yield return Charge();
  yield return Travel(SixthGreenhouseFlow.Px(900,670));
  yield return RestoreWithVisibleFall(765);
  Check("mid-route landing is on low P at K",Mathf.Abs(X-900)<14&&Mathf.Abs(Y-765)<10);
  yield return Tap();
  Check("fixed K opens and latches curtain",f.CurtainOpen);
  Check("K leaves the original lower beam intact",f.aimA==0&&Vector2.Distance(LastNode,SixthGreenhouseFlow.B)<.3f);
  yield return Capture("02-low-tray-latch");
  if(failed){Finish();yield break;}

  // Actual physical jumps, followed by a full three-second recharge in the beam.
  yield return Walk(1035);
  yield return JumpTo(1120,735);
  yield return JumpTo(1194,710);
  yield return Charge();
  Check("entity step route rejoined the same light path",f.ability.IsLight&&f.CurtainOpen&&!f.TrayHigh);
  if(failed){Finish();yield break;}

  // Deliberately leave P low, visit C, and discover the missing relay condition.
  // This is a valid player choice, not a synthetic state injected by this test.
  yield return RideAndRestore(SixthGreenhouseFlow.A,710);
  yield return SetAim(0,1);
  yield return Charge();
  yield return RideAndRestore(SixthGreenhouseFlow.C,275);
  yield return SetAim(1,1);
  Check("curtain stays open but low tray cannot connect D",f.CurtainOpen&&!f.TrayHigh&&LastNode.x<SixthGreenhouseFlow.D.x-.5f);
  yield return Charge();
  yield return TravelToBlockedEnd();
  yield return Tap();
  Check("missing relay ends at an opaque wall with no recovery floor",f.ability.IsLight&&!f.TrayHigh);
  yield return Capture("03-low-tray-wrong-combination");
  yield return RideAndRestore(SixthGreenhouseFlow.C,275);
  yield return Charge();
  yield return RideAndRestore(SixthGreenhouseFlow.A,710);
  yield return SetAim(0,0);
  if(failed){Finish();yield break;}

  // Repair the failed hypothesis through the legitimate return route to B.
  yield return Charge();
  yield return RideAndRestore(SixthGreenhouseFlow.B,710);
  yield return Tap();
  yield return WaitTray(true);
  Check("raising P preserves the latched curtain and lower return light",f.TrayHigh&&f.CurtainOpen&&f.aimA==0);
  yield return Capture("04-raised-tray-return-route");
  yield return Charge();
  yield return RideAndRestore(SixthGreenhouseFlow.A,710);
  yield return SetAim(0,1);
  yield return Charge();
  yield return RideAndRestore(SixthGreenhouseFlow.C,275);
  Check("previously calibrated C is preserved after the round trip",f.aimC==1);
  Check("single upper route now includes the raised relay",ContainsNode(SixthGreenhouseFlow.HighP)&&ContainsNode(SixthGreenhouseFlow.D));
  yield return Capture("05-upper-route-connected");
  if(failed){Finish();yield break;}

  // The high tray is genuinely usable: restore, land and recharge there as well.
  yield return Charge();
  yield return RideAndRestore(SixthGreenhouseFlow.HighP,300);
  Check("raised P is a real support, not a forbidden restoration zone",f.player.Grounded&&!f.ability.IsLight&&Mathf.Abs(Y-300)<10);
  yield return Charge();
  yield return RideAndRestore(SixthGreenhouseFlow.D,275);
  yield return SetAim(2,1);
  yield return new WaitForSeconds(2.5f);
  Check("target scale fed and exit collision opened",f.Awakened&&!f.exitBlock.enabled);
  yield return Capture("06-fed-scale");
  yield return Walk(1730);
  Check("level six completed from the upper exit",f.Completed&&Y<290);
  Check("no unplanned deaths during full return-route playthrough",f.Deaths==0);
  Finish();
 }

 void InspectScene(){
  Check("all six existing scenes present in player",
   Application.CanStreamedLevelBeLoaded("FirstLight")&&Application.CanStreamedLevelBeLoaded("SecondLight")&&
   Application.CanStreamedLevelBeLoaded("ThirdDrain")&&Application.CanStreamedLevelBeLoaded("FourthReservoir")&&
   Application.CanStreamedLevelBeLoaded("FifthPumpHouse")&&Application.CanStreamedLevelBeLoaded("SixthGreenhouse"));
  Check("exactly one active optical path",FindObjectsOfType<LightPath>().Length==1);
  Check("real rigged light spirit",f.player.visual.GetComponentInChildren<SkinnedMeshRenderer>()!=null);
  Check("initial state has raised tray, closed G and lower light",f.TrayHigh&&!f.CurtainOpen&&f.aimA==0);
  Check("transmitting glass supports entity but is excluded from light obstacles",
   (f.ability.restoreSurfaces.value&(1<<10))!=0&&(f.ability.obstacles.value&(1<<10))==0);
  Check("short-drop restoration enabled for level six",f.ability.shortDropRestore);
  Check("two fixed re-entry steps",f.stepColliders!=null&&f.stepColliders.Length==2);
  if(f.stepColliders!=null){
   foreach(var c in f.stepColliders){
    Check(c.name+" horizontal",Mathf.Abs(Mathf.DeltaAngle(c.transform.eulerAngles.z,0))<.01f);
    Check(c.name+" clear of the tray's vertical sweep",c.bounds.min.x>f.trayCollider.bounds.max.x+.01f);
   }
  }
  int reusable=0,mirrors=0;
  foreach(var component in FindObjectsOfType<MonoBehaviour>()){
   if(component.GetType().Name!="GardenReusableProp")continue;
   reusable++;
   var field=component.GetType().GetField("kind");
   if(field!=null){string value=Convert.ToString(field.GetValue(component));if(value.IndexOf("mirror",StringComparison.OrdinalIgnoreCase)>=0)mirrors++;}
  }
  Check("scene contains reusable garden prop instances",reusable>=6);
  Check("four independently reusable mirror instances",mirrors>=4);
  float stepRight=f.stepColliders[1].bounds.max.x*50;
  float theoreticalJump=f.player.speed*(2*f.player.jumpSpeed/(Mathf.Abs(Physics2D.gravity.y)*f.player.Body.gravityScale))*50;
  Check("last step to B exceeds maximum level-ground jump",1450-stepRight>theoreticalJump*1.1f);
 }

 bool ContainsNode(Vector3 point){foreach(var node in f.path.nodes)if(Vector2.Distance(node,point)<.2f)return true;return false;}
 IEnumerator Tap(){if(failed)yield break;Drive(interact:true);yield return null;Drive();yield return new WaitForSeconds(.16f);}
 IEnumerator Adjust(float amount){if(failed)yield break;Drive(adjust:amount);yield return null;Drive();yield return new WaitForSeconds(.41f);}
 IEnumerator SetAim(int which,int target){
  if(failed)yield break;
  report.action="adjust mirror "+which+" to "+target;
  yield return Tap();
  int guard=0;
  while(Aim(which)!=target&&guard++<5&&!failed){yield return Adjust(Mathf.Sign(target-Aim(which)));}
  yield return Tap();
  Check(report.action,Aim(which)==target&&f.Adjusting<0);
 }
 int Aim(int which)=>which==0?f.aimA:which==1?f.aimC:f.aimD;
 IEnumerator Walk(float x){
  if(failed)yield break;
  report.action="walk to "+x;float end=Time.time+12;
  while(Mathf.Abs(X-x)>3&&Time.time<end&&!f.Completed){Drive(Mathf.Sign(x-X));yield return null;}
  Drive();yield return new WaitForSeconds(.14f);
  Check(report.action+" at "+X.ToString("F1"),f.Completed||Mathf.Abs(X-x)<10);
 }
 IEnumerator Charge(){
  if(failed)yield break;
  // A deliberate small movement rearms the shared ability after a restoration.
  float x=X;yield return Walk(x+10);yield return Walk(x);
  if(failed)yield break;
  report.action="charge at "+X.ToString("F1")+","+Y.ToString("F1");
  float end=Time.time+5,start=Time.time,alreadyCharged=f.ability.Charge;
  while(!f.ability.IsLight&&Time.time<end){Drive();yield return null;}
  Check(report.action,f.ability.IsLight);
  Check("recharge obeyed the three-second rule",!f.ability.IsLight||Time.time-start+alreadyCharged>=f.ability.chargeSeconds-.15f);
 }
 IEnumerator Travel(Vector3 target){
  if(failed)yield break;
  report.action="light travel to "+target;
  float desired;Vector2 projected;f.path.Closest(target,out desired,out projected);
  float end=Time.time+18;
  while(f.ability.IsLight&&Time.time<end){
   float delta=desired-f.ability.Along;
   if(Mathf.Abs(delta)<.075f)break;
   Drive(Mathf.Sign(delta));yield return null;
  }
  Drive();yield return new WaitForFixedUpdate();
  Check(report.action,f.ability.IsLight?Mathf.Abs(f.ability.Along-desired)<.15f:Mathf.Abs(X-target.x*50)<15);
 }
 IEnumerator RideAndRestore(Vector3 point,float floorY){
  if(failed)yield break;
  yield return Travel(point);if(failed)yield break;
  if(f.ability.IsLight)yield return Tap();
  float end=Time.time+2;
  while((f.ability.IsLight||!f.player.Grounded)&&Time.time<end){Drive();yield return null;}
  Check("restored onto floor "+floorY+" at "+X.ToString("F1")+","+Y.ToString("F1"),
   !f.ability.IsLight&&f.player.Grounded&&Mathf.Abs(Y-floorY)<10);
 }
 IEnumerator TravelToBlockedEnd(){
  if(failed)yield break;
  report.action="probe blocked light endpoint";
  float end=Time.time+18,stalled=0,last=f.ability.Along;
  while(f.ability.IsLight&&Time.time<end){
   if(f.ability.Blocked||f.path.Length-f.ability.Along<.10f)break;
   Drive(1);yield return null;
   stalled=Mathf.Abs(f.ability.Along-last)<.0001f?stalled+Time.deltaTime:0;last=f.ability.Along;
   if(stalled>.7f)break;
  }
  Drive();yield return null;
  Check(report.action,f.ability.IsLight&&(f.ability.Blocked||f.path.Length-f.ability.Along<.25f||stalled>.7f));
 }
 IEnumerator RestoreWithVisibleFall(float floorY){
  if(failed)yield break;
  report.action="mid-route E restoration and gravity fall";
  int previous=f.ability.ShortDropCount;
  Drive(interact:true);yield return null;Drive();
  float end=Time.time+1;
  while(f.ability.IsLight&&Time.time<end)yield return null;
  Check("E starts entity above its eventual support, not at the floor",
   !f.ability.IsLight&&Y<floorY-18&&!f.player.Grounded&&f.player.Body.bodyType==RigidbodyType2D.Dynamic);
  bool observedDescent=f.player.Body.velocity.y<-.05f;
  end=Time.time+2;
  while(!f.player.Grounded&&Time.time<end){Drive();yield return null;observedDescent|=f.player.Body.velocity.y<-.05f;}
  Check("real gravity carries entity down to low tray",observedDescent&&f.player.Grounded&&Mathf.Abs(Y-floorY)<10);
  Check("shared ability recorded one safe short drop",f.ability.ShortDropCount==previous+1);
 }
 IEnumerator WaitTray(bool high){
  if(failed)yield break;
  report.action=high?"raise nursery tray":"lower nursery tray";
  float end=Time.time+5;
  while(!f.TraySettled&&Time.time<end){Drive();yield return null;}
  Drive();yield return new WaitForSeconds(.14f);
  Check(report.action,f.TraySettled&&f.TrayHigh==high);
 }
 IEnumerator JumpTo(float x,float floorY){
  if(failed)yield break;
  report.action="jump to "+x+","+floorY;
  float end=Time.time+3;
  while(!f.player.Grounded&&Time.time<end){Drive();yield return null;}
  int count=f.player.JumpCount;
  Drive(Mathf.Sign(x-X),jump:true);yield return null;
  yield return new WaitForSeconds(.07f);end=Time.time+4;
  while(Time.time<end){
   Drive(Mathf.Abs(X-x)<3?0:Mathf.Sign(x-X));yield return null;
   if(f.player.Grounded&&f.player.Body.velocity.y<=.1f&&Mathf.Abs(Y-floorY)<10)break;
  }
  Drive();yield return new WaitForSeconds(.12f);
  Check(report.action+" landed at "+X.ToString("F1")+","+Y.ToString("F1"),
   f.player.JumpCount>count&&Mathf.Abs(X-x)<20&&Mathf.Abs(Y-floorY)<10);
 }
 IEnumerator Capture(string name){
  if(failed)yield break;
  bool overview=f.cameraRig.Overview;if(!overview)f.cameraRig.Toggle();
  yield return new WaitForSeconds(.12f);yield return new WaitForEndOfFrame();
  NativeFrameCapture.Save(output+"/"+name+".png");
  yield return new WaitForSeconds(.12f);if(!overview)f.cameraRig.Toggle();
 }
 void Update(){
  if(!finished&&started>0&&Time.realtimeSinceStartup-started>260){
   Check("watchdog while "+report.action,false);StopAllCoroutines();Finish();
  }
 }
 void Finish(){
  if(finished)return;finished=true;Drive();
  report.completed=!failed&&f&&f.Completed&&report.errors.Count==0;
  report.seconds=Time.realtimeSinceStartup-started;
  if(f){report.jumps=f.player.JumpCount;report.deaths=f.Deaths;report.transformations=f.ability.TransformCount;report.shortDrops=f.ability.ShortDropCount;}
  File.WriteAllText(output+"/playthrough.json",JsonUtility.ToJson(report,true));
  Application.Quit(report.completed?0:1);
 }
}
