using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstToSecondValidation : MonoBehaviour {
 FirstLightFlow flow;int stage;float start,changed;string output;bool jumped;
 void Start(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-validationOutput");output=i>=0?a[i+1]:Application.persistentDataPath;Directory.CreateDirectory(output);start=changed=Time.time;}
 void Next(){stage++;changed=Time.time;}
 void Update(){if(Time.time-start>85){Done(false,"timeout stage="+stage);return;}if(SceneManager.GetActiveScene().name=="SecondLight"){if(!SceneFade.Busy){var p=FindObjectOfType<PotLevelFlow>();NativeFrameCapture.Save(output+"/first-to-second.png");Done(p&&p.ability&&p.player.visual,"Actual FirstLight to SecondLight load; global ability present");}return;}if(!flow){flow=FindObjectOfType<FirstLightFlow>();if(!flow)return;flow.Testing=true;}var input=new GardenInput();float x=flow.player.transform.position.x;
  switch(stage){case 0:input.skip=true;Next();break;case 1:input.move=1;if(x>19.1f&&!jumped){input.jump=true;jumped=true;}if(x>24)Next();break;case 2:input.move=x<37.4f?1:0;if(x>=37.4f){input.interact=true;Next();}break;case 3:input.move=flow.puzzle.yaw<-.2f?1:0;if(flow.puzzle.yaw>=-.2f)Next();break;case 4:if(flow.puzzle.Solved){input.interact=true;Next();}break;case 5:input.move=x<75.4f?1:0;if(x>=75.4f)Next();break;case 6:if(Time.time-changed>.5f){NativeFrameCapture.Save(output+"/first-light-preserved.png");input.interact=true;Next();}break;}
  flow.testInput=input;
 }
 void Done(bool ok,string detail){File.WriteAllText(output+"/first-to-second.json","{\"passed\":"+(ok?"true":"false")+",\"detail\":\""+detail+"\"}");enabled=false;Application.Quit(ok?0:1);}
}
