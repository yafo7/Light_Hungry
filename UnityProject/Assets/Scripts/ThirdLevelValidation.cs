using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// This runner supplies ordinary input to the real motor; it never teleports during the playthrough.
public class ThirdLevelValidation : MonoBehaviour {
 [Serializable]class Check{public string name;public bool passed;public string detail;}
 [Serializable]class Sample{public float t,x,y,height;public bool grounded,light;public string action;}
 [Serializable]class Report{public bool completed;public string unityVersion,graphicsDevice;public float seconds;public int jumps,deaths;public List<Check> checks=new List<Check>();public List<string> errors=new List<string>();public List<Sample> trace=new List<Sample>();}
 Report report=new Report();DrainLevelFlow f;string output,action="start";float begin,traceAt;bool failed,finished;Vector3 frameLocal;
 void Log(string text,string stack,LogType kind){if(kind==LogType.Error||kind==LogType.Exception||kind==LogType.Assert)report.errors.Add(text);}
 void OnDestroy(){Application.logMessageReceived-=Log;}
 void Update(){if(f&&!finished&&Time.time>traceAt){traceAt=Time.time+.15f;report.trace.Add(new Sample{t=Time.time-begin,x=f.player.transform.position.x,y=f.player.transform.position.y,height=f.mechanism.height,grounded=f.player.Grounded,light=f.ability.IsLight,action=action});}}
 IEnumerator Start(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-validationOutput");output=i>=0?args[i+1]:Application.persistentDataPath+"/Level03Validation";Directory.CreateDirectory(output);begin=Time.time;report.unityVersion=Application.unityVersion;report.graphicsDevice=SystemInfo.graphicsDeviceName;Application.logMessageReceived+=Log;
  yield return Until(()=>SceneManager.GetActiveScene().name=="ThirdDrain",8,"load third scene");f=FindObjectOfType<DrainLevelFlow>();if(!f){CheckIt("full level flow",false,"DrainLevelFlow missing");Finish();yield break;}f.Testing=true;
  yield return null;CheckIt("full scene replaces arrival",FindObjectOfType<DrainArrivalFlow>()==null,"ThirdDrain_FullLevel loaded");CheckIt("native graphics",SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Direct3D11,SystemInfo.graphicsDeviceType.ToString());CheckIt("supplied 3D character",f.player.visual.GetComponentsInChildren<SkinnedMeshRenderer>().Any(m=>m.sharedMesh&&m.sharedMesh.vertexCount>10000&&m.bones.Length>=20),"Original WickSpirit prefab");CheckIt("global light ability",f.ability&&Mathf.Abs(f.ability.chargeSeconds-3)<.001f,"Shared LightFormAbility on third-level player");
  if(Array.IndexOf(args,"--capture-rig")>=0)gameObject.AddComponent<RigGameplayCapture>();
  yield return Capture("01-overview.png");yield return InputFor(new DrainInput{skip=true},.12f);yield return Until(()=>f.player.Grounded,3,"spawn on entry");yield return null;frameLocal=f.cameraRig.frame.localPosition;
  CheckIt("initial bulb off",!f.mechanism.Powered&&!f.mechanism.path.enabled,"No effective light before activation");
  float maxRise=f.player.jumpSpeed*f.player.jumpSpeed/(2*Mathf.Abs(Physics2D.gravity.y)*f.player.Body.gravityScale);float maxRun=f.player.speed*f.player.jumpSpeed*2/(Mathf.Abs(Physics2D.gravity.y)*f.player.Body.gravityScale);
  CheckIt("initial counterweight inaccessible",(424-f.mechanism.DeckPixelY)/28>maxRise,"Deck rise="+((424-f.mechanism.DeckPixelY)/28)+" max jump="+maxRise);
  CheckIt("gap exceeds unaided jump",(700-455)/28f>maxRun,"Gap="+(245/28f)+" flat range="+maxRun);
  CheckIt("no hidden floor below gap",Physics2D.OverlapBox(DrainMechanism.Px(592,520),new Vector2(7,.3f),0,1<<6)==null,"Open water between banks; rail has no collider");
  yield return Walk(105);yield return TapE();CheckIt("root wakes bulb",f.mechanism.Powered&&f.mechanism.ActivationCount==1&&f.mechanism.path.enabled,"E at reachable root knot");
  yield return Jump(190,357,"A entry step");yield return Jump(323,424,"A left bank");yield return Walk(411);yield return Until(()=>f.Checkpoint==1,2,"left checkpoint");
  action="deliberate unaided crossing";int deaths=f.Deaths;yield return Walk(444);f.testInput=new DrainInput{move=1,jump=true};yield return null;
  float fallUntil=Time.time+4;while(f.Deaths==deaths&&Time.time<fallUntil){f.testInput=new DrainInput{move=1};yield return null;}f.testInput=new DrainInput();yield return InputFor(new DrainInput(),.4f);
  CheckIt("failed jump respawns",f.Deaths==deaths+1&&f.Checkpoint==1,"Deaths="+f.Deaths+", checkpoint="+f.Checkpoint);CheckIt("respawn keeps bulb state",f.mechanism.Powered,"Failure retains activated source");
  yield return Walk(411);yield return TapE();CheckIt("left winch interact",f.Adjusting,"E grabs winch");yield return Height(.5f);CheckIt("winch camera shows full mechanism",f.cameraRig.FocusMechanism&&Camera.main.orthographicSize>8,"Local wide view while holding winch");yield return Capture("02a-winch-focus.png");
  if(Array.IndexOf(args,"--capture-water-motion")>=0)yield return CaptureWaterMotion();
  yield return TapE();yield return InputFor(new DrainInput(),.2f);
  CheckIt("middle joins banks",Mathf.Abs(f.mechanism.DeckPixelY-420)<2&&!f.mechanism.Aligned,"Deck="+f.mechanism.DeckPixelY+", mirror="+f.mechanism.MirrorPixelY);yield return Capture("02-middle-crossing.png");
  yield return Walk(444);yield return Jump(558,420,"counterweight landing");yield return Walk(650);yield return Jump(751,417,"A right bank");yield return Jump(831,355,"upper winch ledge");yield return Walk(910);yield return Until(()=>f.Checkpoint==2,2,"right checkpoint");
  yield return TapE();CheckIt("right winch controls same mechanism",f.Adjusting&&FindObjectsOfType<DrainMechanism>().Length==1,"One shared mirror and counterweight");yield return Height(1);yield return TapE();yield return InputFor(new DrainInput(),2.3f);
  CheckIt("high ray passes aperture",f.mechanism.Aligned,"Raycast reaches receiver through wall opening");CheckIt("scale awakens after exposure",f.mechanism.Awakened&&f.mechanism.Exposure>=2,"Exposure="+f.mechanism.Exposure);CheckIt("not completed from remote illumination",!f.Completed,"Player must still traverse B and C");
  f.testInput=new DrainInput{overview=true};yield return null;yield return InputFor(new DrainInput(),.1f);yield return Capture("03-high-light-path.png");f.testInput=new DrainInput{overview=true};yield return null;
  yield return Walk(1080);yield return Until(()=>f.player.Grounded&&Mathf.Abs(f.player.transform.position.y-DrainMechanism.Px(1080,519).y)<.25f,4,"B low lane landing");yield return Walk(1190);
  yield return Jump(1230,446,"B small step");yield return Jump(1300,426,"B right turn 2");yield return Jump(1237,391,"B back-left 3");yield return Jump(1148,375,"B back-left 4");yield return Capture("04-B-backtrack.png");yield return Jump(1231,307,"B upper-right 5");yield return Jump(1318,287,"B upper-right 6");yield return Jump(1415,278,"B upper rest");yield return Until(()=>f.Checkpoint==3,2,"upper checkpoint");
  CheckIt("frame follows camera only",Vector3.Distance(frameLocal,f.cameraRig.frame.localPosition)<.001f&&f.cameraRig.frame.parent==Camera.main.transform,"Root overlay local position constant after world scrolling");
  yield return Walk(1677);yield return Until(()=>f.player.Grounded&&f.Checkpoint==4,5,"C recovery bank");yield return Walk(1702);
  var ferry=FindObjectsOfType<DrainMovingSurface>().First(p=>p.ferry);action="wait for ferry";yield return Until(()=>ferry.Body.position.x<1830/28f&&ferry.Velocity.x<0,9,"ferry approaches left");
  yield return JumpMoving(ferry);yield return InputFor(new DrainInput(),.2f);var oldPlayer=f.player.Body.position;var oldDeck=ferry.Body.position;yield return InputFor(new DrainInput(),1.0f);
  float carryError=Mathf.Abs((f.player.Body.position.x-oldPlayer.x)-(ferry.Body.position.x-oldDeck.x));CheckIt("moving platform carries passenger",f.player.Grounded&&carryError<.25f,"Relative horizontal drift="+carryError);yield return Capture("05-riding-waterwheel-ferry.png");
  yield return Until(()=>ferry.Body.position.x>1912/28f&&ferry.Velocity.x>0,9,"ferry reaches far bank");yield return Jump(2007,430,"C final step one");yield return Jump(2072,390,"C final step two");yield return Jump(2142,350,"C final landing");yield return Until(()=>f.Completed,3,"complete at safe exit");yield return Capture("06-completed.png");
  CheckIt("complete full input-driven route",f.Completed&&!failed,"No warps during playthrough; checkpoint respawn from deliberate failure only");report.jumps=f.player.JumpCount;report.deaths=f.Deaths;
  f.testInput=new DrainInput{restart=true};var previous=f;yield return null;yield return Until(()=>FindObjectOfType<DrainLevelFlow>()!=previous&&!SceneFade.Busy,8,"restart full third scene");f=FindObjectOfType<DrainLevelFlow>();CheckIt("restart resets progress",f&&!f.Completed&&!f.mechanism.Powered&&f.Checkpoint==0,"R resets source, checkpoint and completion");Finish();
 }
 void CheckIt(string name,bool ok,string detail){report.checks.Add(new Check{name=name,passed=ok,detail=detail});if(!ok)failed=true;Debug.Log("L3_CHECK "+name+"="+ok+" "+detail);}
 IEnumerator Until(Func<bool> ready,float timeout,string label){action=label;float end=Time.time+timeout;while(!ready()&&Time.time<end&&!failed)yield return null;if(!ready()){CheckIt(label,false,"Timeout; "+Position());Finish();StopAllCoroutines();}}
 IEnumerator InputFor(DrainInput i,float seconds){float end=Time.time+seconds;while(Time.time<end&&!failed){if(f)f.testInput=i;yield return null;}}
 IEnumerator TapE(){f.testInput=new DrainInput{interact=true};yield return null;yield return InputFor(new DrainInput(),.08f);}
 IEnumerator Walk(float px){if(failed)yield break;action="walk to "+px;float target=px/28,end=Time.time+12;while(Mathf.Abs(f.player.Body.position.x-target)>.11f&&Time.time<end&&!failed){f.testInput=new DrainInput{move=Mathf.Sign(target-f.player.Body.position.x)};yield return null;}f.testInput=new DrainInput();if(Mathf.Abs(f.player.Body.position.x-target)>.25f){CheckIt(action,false,Position());Finish();StopAllCoroutines();}yield return InputFor(new DrainInput(),.12f);}
 IEnumerator Height(float target){action="winch height "+target;float end=Time.time+7;while(Mathf.Abs(f.mechanism.height-target)>.008f&&Time.time<end&&!failed){f.testInput=new DrainInput{winch=Mathf.Sign(target-f.mechanism.height)};yield return null;}f.testInput=new DrainInput();yield return InputFor(new DrainInput(),.15f);CheckIt(action,Mathf.Abs(f.mechanism.height-target)<.03f,"height="+f.mechanism.height);}
 IEnumerator Jump(float px,float top,string label){if(failed)yield break;yield return Until(()=>f.player.Grounded,3,"ground before "+label);action=label;int before=f.player.JumpCount,deaths=f.Deaths;float target=px/28,end=Time.time+3;f.testInput=new DrainInput{move=Mathf.Sign(target-f.player.Body.position.x),jump=true};yield return null;bool air=false;
  while(Time.time<end&&!failed){float delta=target-f.player.Body.position.x;f.testInput=new DrainInput{move=Mathf.Abs(delta)>.09f?Mathf.Sign(delta):0};if(!f.player.Grounded)air=true;if(air&&f.player.Grounded&&f.player.Body.velocity.y<1&&f.player.JumpCount>before)break;if(f.Deaths!=deaths)break;yield return null;}
  f.testInput=new DrainInput();float dy=Mathf.Abs(f.player.Body.position.y-DrainMechanism.Px(px,top).y);bool ok=air&&f.player.Grounded&&f.Deaths==deaths&&Mathf.Abs(f.player.Body.position.x-target)<.8f&&dy<.32f;CheckIt(label,ok,Position()+", expected top="+top+", deltaY="+dy);if(!ok){yield return Capture("FAIL-"+report.checks.Count+".png");Finish();StopAllCoroutines();}yield return InputFor(new DrainInput(),.1f);
 }
 IEnumerator JumpMoving(DrainMovingSurface ferry){action="board moving ferry";int before=f.player.JumpCount,deaths=f.Deaths;float end=Time.time+3;f.testInput=new DrainInput{move=1,jump=true};yield return null;bool air=false;
  while(Time.time<end&&!failed){float dx=ferry.Body.position.x-f.player.Body.position.x;f.testInput=new DrainInput{move=Mathf.Abs(dx)>.12f?Mathf.Sign(dx):0};if(!f.player.Grounded)air=true;if(air&&f.player.Grounded&&f.player.JumpCount>before)break;if(f.Deaths!=deaths)break;yield return null;}
  f.testInput=new DrainInput();bool ok=air&&f.player.Grounded&&f.Deaths==deaths&&Mathf.Abs(f.player.Body.position.x-ferry.Body.position.x)<1.4f;CheckIt("board moving ferry",ok,Position());if(!ok){yield return Capture("FAIL-ferry.png");Finish();StopAllCoroutines();}
 }
 string Position()=>f?"x="+(f.player.Body.position.x*28).ToString("F1")+", y="+(542-f.player.Body.position.y*28).ToString("F1")+", grounded="+f.player.Grounded+", deaths="+f.Deaths:"no flow";
 IEnumerator Capture(string file){yield return new WaitForEndOfFrame();NativeFrameCapture.Save(Path.Combine(output,file));}
 IEnumerator CaptureWaterMotion(){
  var cameraBehaviour=f.cameraRig;cameraBehaviour.enabled=false;f.testInput=new DrainInput();
  int previous=Time.captureFramerate;Time.captureFramerate=30;
  string dir=Path.Combine(output,"water-motion");Directory.CreateDirectory(dir);
  for(int frame=0;frame<90;frame++){yield return new WaitForEndOfFrame();NativeFrameCapture.Save(Path.Combine(dir,frame.ToString("D3")+".png"));}
  var view=Camera.main;Vector3 oldPosition=view.transform.position;float oldSize=view.orthographicSize;bool oldFrame=cameraBehaviour.frame.gameObject.activeSelf;
  view.transform.position=new Vector3(38.75f,6.41f,-40);view.orthographicSize=22;cameraBehaviour.frame.gameObject.SetActive(false);
  dir=Path.Combine(output,"water-overview");Directory.CreateDirectory(dir);
  for(int frame=0;frame<90;frame++){yield return new WaitForEndOfFrame();NativeFrameCapture.Save(Path.Combine(dir,frame.ToString("D3")+".png"));}
  view.transform.position=oldPosition;view.orthographicSize=oldSize;cameraBehaviour.frame.gameObject.SetActive(oldFrame);
  Time.captureFramerate=previous;cameraBehaviour.enabled=true;
 }
 void Finish(){if(finished)return;finished=true;report.seconds=Time.time-begin;report.completed=!failed&&report.errors.Count==0&&report.checks.Any(c=>c.name=="restart resets progress"&&c.passed);File.WriteAllText(output+"/playthrough.json",JsonUtility.ToJson(report,true));Application.Quit(report.completed?0:1);}
}

