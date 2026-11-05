using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeValidation : MonoBehaviour {
 [Serializable]class Check{public string name;public bool passed;public string detail;}
 [Serializable]class Report{public string unityVersion,graphicsDevice;public bool completed;public float seconds;public int jumpCount,hitFrames;public float minimumFPS;public List<Check> checks=new List<Check>();}
 FirstLightFlow flow;Report report=new Report();string output;int stage;float started,stageStarted,minFps=1000;bool jumpRequested,finished;
 void Start(){var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"--validate")<0){enabled=false;return;}flow=GetComponent<FirstLightFlow>();int index=Array.IndexOf(args,"-validationOutput");output=index>=0?args[index+1]:Application.persistentDataPath+"/Validation";Directory.CreateDirectory(output);started=stageStarted=Time.time;report.unityVersion=Application.unityVersion;report.graphicsDevice=SystemInfo.graphicsDeviceName;CheckIt("correct yaw hits tail",flow.puzzle.RayHits(0),"3D physics ray reflection");CheckIt("wrong yaw misses tail",!flow.puzzle.RayHits(-22),"Initial orientation must require player input");StartCoroutine(Capture("01-overview.png"));}
 void CheckIt(string name,bool passed,string detail){report.checks.Add(new Check{name=name,passed=passed,detail=detail});}
 void Next(){stage++;stageStarted=Time.time;}
 void Update(){if(!flow||finished)return;if(Time.time-started>80){CheckIt("playthrough timeout",false,"stage "+stage);Finish();return;}if(Time.time-started>4&&Time.unscaledDeltaTime>.0001f)minFps=Mathf.Min(minFps,1/Time.unscaledDeltaTime);GardenInput i=new GardenInput();float x=flow.player.transform.position.x;
  switch(stage){
   case 0: if(Time.time-stageStarted>1){i.skip=true;Next();}break;
   case 1: i.move=1;if(x>19.1f&&!jumpRequested){i.jump=true;jumpRequested=true;}if(x>24){CheckIt("jump crossed obstacle",flow.player.JumpCount>0&&flow.player.MaximumY>7.1f,"maxY="+flow.player.MaximumY);Next();StartCoroutine(Capture("02-platforming.png"));}break;
   case 2: i.move=x<37.4f?1:0;if(x>=37.4f){i.interact=true;Next();}break;
   case 3: if(!flow.Adjusting&&Time.time-stageStarted>.3f){CheckIt("mirror interaction",false,"Could not grip");Finish();break;}i.move=flow.puzzle.yaw<-.2f?1:0;if(flow.puzzle.yaw>=-.2f)Next();break;
   case 4: if(flow.puzzle.Solved){CheckIt("sustained light awakens tail",true,"hitFrames="+flow.puzzle.HitFrames);StartCoroutine(Capture("03-reflection.png"));Next();}break;
   case 5: if(Time.time-stageStarted>1){i.interact=true;Next();}break;
   case 6: i.move=x<75.4f?1:0;if(x>=75.4f){StartCoroutine(Capture("04-pot-entrance.png"));Next();}break;
   case 7: if(Time.time-stageStarted>1){i.interact=true;Next();}break;
   case 8: if(Time.time-stageStarted>.6f){CheckIt("pot transition trigger",flow.Completed,"Level 1 completion, Level 2 not built");StartCoroutine(Capture("05-complete.png"));Next();}break;
   case 9: if(Time.time-stageStarted>1.5f)Finish();break;
  }
  flow.testInput=i;
 }
 IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,name));}
 void Finish(){finished=true;report.completed=flow.Completed;report.seconds=Time.time-started;report.jumpCount=flow.player.JumpCount;report.hitFrames=flow.puzzle.HitFrames;report.minimumFPS=minFps;CheckIt("native graphics device",SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null,SystemInfo.graphicsDeviceType.ToString());File.WriteAllText(Path.Combine(output,"playthrough.json"),JsonUtility.ToJson(report,true));StartCoroutine(Quit());}
 IEnumerator Quit(){yield return new WaitForSeconds(.8f);Application.Quit();}
}
