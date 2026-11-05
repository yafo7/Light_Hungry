using System;
using System.IO;
using System.Collections;
using UnityEngine;
public class RigGameplayCapture:MonoBehaviour {
 IEnumerator Start(){
  var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-validationOutput");if(index<0)yield break;
  string dir=Path.Combine(args[index+1],"rig-motion");Directory.CreateDirectory(dir);DrainLevelFlow flow=null;
  while(!(flow=FindObjectOfType<DrainLevelFlow>())||flow.Checkpoint<1)yield return null;
  var go=new GameObject("QA_RigCloseCamera");var cam=go.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;cam.orthographicSize=1.45f;
  int previous=Time.captureFramerate;Time.captureFramerate=30;
  bool walk=false,air=false;var animator=flow.player.visual.GetComponentInChildren<Animator>();
  for(int frame=0;frame<300;frame++){
   yield return new WaitForEndOfFrame();if(!flow)break;
   walk|=animator.GetFloat("Move")>.5f&&flow.player.Grounded;air|=animator.GetCurrentAnimatorStateInfo(0).IsName("Airborne");
   cam.transform.position=flow.player.transform.position+new Vector3(.45f,.6f,-39);cam.transform.rotation=Quaternion.identity;
   if(frame%2==0)NativeFrameCapture.Save(Path.Combine(dir,(frame/2).ToString("D3")+".png"),cam);
  }
  Time.captureFramerate=previous;Destroy(go);File.WriteAllText(Path.Combine(dir,"states.json"),"{\"walkObserved\":"+walk.ToString().ToLower()+",\"airborneObserved\":"+air.ToString().ToLower()+",\"frames\":150,\"fps\":15}");
 }
}
