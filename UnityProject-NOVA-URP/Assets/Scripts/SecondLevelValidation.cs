using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Native-player exercise. Inputs use the exact same motor and ability as normal gameplay.
public class SecondLevelValidation : MonoBehaviour {
 [Serializable]class Check{public string name;public bool passed;public string detail;}
 [Serializable]class Report{public string unityVersion,graphicsDevice;public float seconds;public bool completed;public List<Check> checks=new List<Check>();public List<string> errors=new List<string>();}
 Report report=new Report();PotLevelFlow flow;string output;float begin,stageAt,mark,nextJump;int stage;bool finished,earlyChargeChecked,slitCaptured;Vector2 stopped;
 void Start(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-validationOutput");output=i>=0?a[i+1]:Application.persistentDataPath+"/Level02Validation";Directory.CreateDirectory(output);begin=stageAt=Time.time;report.unityVersion=Application.unityVersion;report.graphicsDevice=SystemInfo.graphicsDeviceName;Application.logMessageReceived+=Log;}
 void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(text);}
 void OnDestroy(){Application.logMessageReceived-=Log;}
 void CheckIt(string n,bool ok,string detail){report.checks.Add(new Check{name=n,passed=ok,detail=detail});Debug.Log("L2_CHECK "+n+"="+ok+" "+detail);}
 void Next(){stage++;stageAt=Time.time;}
 void Update(){if(finished)return;if(Time.time-begin>95){CheckIt("timeout",false,"stage="+stage+", player="+(flow?flow.player.transform.position.ToString():"null"));Finish();return;}
  if(stage==0){if(SceneManager.GetActiveScene().name!="SecondLight")return;flow=FindObjectOfType<PotLevelFlow>();if(!flow)return;flow.Testing=true;CheckIt("native graphics",SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null,SystemInfo.graphicsDeviceType.ToString());CheckIt("real supplied 3D model",flow.player.visual.GetComponentsInChildren<SkinnedMeshRenderer>().Any(f=>f.sharedMesh&&f.sharedMesh.vertexCount>10000&&f.bones.Length>=20),"Player uses imported mesh");CheckIt("no beast in level 2",FindObjectsOfType<TailLife>().Length==0,"Dew and obstacle tutorial only");StartCoroutine(Capture("01-overview.png"));Next();return;}
  if(stage==15&&SceneManager.GetActiveScene().name!="SecondLight"){CheckIt("running and jumping cannot bypass rubble",false,"Reached exit without using light form");Finish();return;}
  if(stage==12){if(SceneManager.GetActiveScene().name=="ThirdDrain"&&!SceneFade.Busy){var arrival=FindObjectOfType<DrainArrivalFlow>();var full=FindObjectOfType<DrainLevelFlow>();var motor=full?full.player:arrival?arrival.player:null;CheckIt("shadow contact loads third scene",full||arrival,"SceneManager active scene="+SceneManager.GetActiveScene().name);CheckIt("third arrival has walkable floor",motor&&motor.Body.bodyType==RigidbodyType2D.Dynamic,"Arrival scene is an actual scene, not an end card");StartCoroutine(Capture("07-third-drain-arrival.png"));Next();}return;}
  if(stage==13){if(Time.time-stageAt>.8f){SceneFade.Load("SecondLight");Next();}return;}
  if(stage==14){if(SceneManager.GetActiveScene().name=="SecondLight"&&!SceneFade.Busy){flow=FindObjectOfType<PotLevelFlow>();CheckIt("reload resets form and passage",flow&&!flow.ability.IsLight&&flow.ability.Charge==0&&!flow.exit.Activated,"Clean per-scene state after return");Finish();}return;}
  if(!flow)return;var input=new GardenInput();float x=flow.player.transform.position.x;
  switch(stage){
   case 1: if(Time.time-stageAt>1){input.skip=true;Next();}break;
   case 2: input.move=x<15.75f?1:0;if(x>=15.75f){CheckIt("passage not premature",!flow.exit.Activated,"Entering the charge area must not exit");ProbeSlit();stage=15;stageAt=Time.time;nextJump=Time.time;}break;
   case 3: if(flow.ability.Charge>1.05f){CheckIt("partial charge retains body",!flow.ability.IsLight&&flow.player.GetComponent<CapsuleCollider2D>().enabled,"charge="+flow.ability.Charge);StartCoroutine(Capture("02-charging.png"));Next();}break;
   case 4: input.move=-1;if(Time.time-stageAt>.65f){CheckIt("movement interrupts charge",flow.ability.Charge<.03f&&!flow.ability.IsLight,"charge="+flow.ability.Charge);Next();}break;
   case 5: input.move=x<15.75f?1:0;if(x>=15.75f)Next();break;
   case 6: if(flow.ability.Charge>=2.7f&&!earlyChargeChecked){earlyChargeChecked=true;CheckIt("no transform before three seconds",!flow.ability.IsLight,"charge="+flow.ability.Charge);}
    if(flow.ability.IsLight){CheckIt("three second transform",flow.ability.Charge>=2.999f&&flow.ability.TransformCount==1,"charge="+flow.ability.Charge);CheckIt("mote swaps physical collider",!flow.player.GetComponent<CapsuleCollider2D>().enabled&&flow.player.GetComponent<CircleCollider2D>().enabled&&flow.player.Body.bodyType==RigidbodyType2D.Kinematic,"Small circle + kinematic body");StartCoroutine(Capture("03-light-mote.png"));mark=flow.ability.Along;Next();}break;
   case 7: input.move=1;if(Time.time-stageAt>.65f){CheckIt("forward along ray",flow.ability.Along>mark+.8f,"delta="+(flow.ability.Along-mark));mark=flow.ability.Along;Next();}break;
   case 8: input.move=-1;if(Time.time-stageAt>.35f){CheckIt("reverse along same ray",flow.ability.Along<mark-.5f,"delta="+(flow.ability.Along-mark));Next();}break;
   case 9: if(Time.time-stageAt<.15f)stopped=flow.player.Body.position;if(Time.time-stageAt>.55f){CheckIt("released input stops mote",Vector2.Distance(stopped,flow.player.Body.position)<.015f,"drift="+Vector2.Distance(stopped,flow.player.Body.position));StartCoroutine(Capture("04-mote-paused.png"));Next();}break;
   case 10: input.move=1;if(flow.ability.IsLight&&x>18.8f&&x<19.1f&&!slitCaptured){slitCaptured=true;StartCoroutine(Capture("05-through-stone-slit.png"));}if(!flow.ability.IsLight&&flow.ability.RestoreCount>0){CheckIt("cross slit and reform safely",x>21.5f&&flow.player.GetComponent<CapsuleCollider2D>().enabled,"x="+x);StartCoroutine(Capture("06-after-slit-shadow.png"));Next();}break;
   case 11: input.move=1;if(flow.exit.Activated){CheckIt("contact-only gate",!input.interact&&flow.exit.destination=="ThirdDrain","No E input; trigger contacted by body");Next();}break;
   case 15: input.move=1;if(Time.time>=nextJump){input.jump=true;nextJump=Time.time+.85f;}if(Time.time-stageAt>6){CheckIt("running and jumping cannot bypass rubble",!flow.exit.Activated&&!flow.ability.IsLight,"Six second attempt; final x="+x+", y="+flow.player.transform.position.y);StartCoroutine(Capture("02a-body-blocked.png"));stage=16;stageAt=Time.time;}break;
   case 16: input.move=-1;if(x<=15.75f){stage=3;stageAt=Time.time;input.move=0;}break;
  }
  flow.testInput=input;
 }
 void ProbeSlit(){var path=FindObjectOfType<LightPath>();Vector2 a=Px(1095,573.631f),b=Px(1310,685.216f),d=b-a;var light=Physics2D.CircleCast(a,.04f,d.normalized,d.magnitude,1<<6);var body=Physics2D.CapsuleCast(a,new Vector2(.42f,.94f),CapsuleDirection2D.Vertical,0,d.normalized,d.magnitude,1<<6);CheckIt("physical slit passes light core",!light.collider,light.collider?light.collider.name:"Circle sweep clear");CheckIt("physical slit blocks full body",body.collider!=null,body.collider?body.collider.name:"No hit");CheckIt("shared optical path",path&&path.nodes.Length==4&&path.beams.Length==2,"Visible rays and movement use one path");}
 static Vector2 Px(float x,float y){return new Vector2(x/63,(571-y)/63);}
 IEnumerator Capture(string name){yield return new WaitForEndOfFrame();NativeFrameCapture.Save(Path.Combine(output,name));}
 void Finish(){if(finished)return;finished=true;report.seconds=Time.time-begin;report.completed=stage==14&&report.errors.Count==0&&report.checks.All(c=>c.passed);File.WriteAllText(Path.Combine(output,"playthrough.json"),JsonUtility.ToJson(report,true));StartCoroutine(Quit());}
 IEnumerator Quit(){yield return new WaitForSeconds(.8f);Application.Quit(report.completed?0:1);}
}

