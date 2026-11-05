using UnityEngine;

public class DrainCamera : MonoBehaviour {
 public Camera view;public Transform player,frame;public float worldWidth=77.5f,worldTop=19.357f,worldBottom=-6.536f,playSize=5.2f;
 public bool IntroFinished {get;private set;}public bool Overview {get;private set;}public bool FocusMechanism;float began;
 void Start(){began=Time.time;}
 public void Skip(){IntroFinished=true;began=Time.time-6;}
 public void Toggle(){Skip();Overview=!Overview;}
 void LateUpdate(){
  float wide=Mathf.Max((worldTop-worldBottom)/2,worldWidth/(2*view.aspect));
  var all=new Vector3(worldWidth/2,(worldTop+worldBottom)/2,-40);
  float t=Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.time-began-1.3f)/3f));if(t>=1)IntroFinished=true;
  float size=FocusMechanism?9.7f:playSize,half=size*view.aspect;var follow=FocusMechanism?new Vector3(25.8f,9.65f,-40):new Vector3(Mathf.Clamp(player.position.x+1.5f,half,worldWidth-half),Mathf.Clamp(player.position.y+2.1f,worldBottom+size,worldTop-size),-40);
  view.orthographicSize=Overview?wide:IntroFinished?Mathf.Lerp(view.orthographicSize,size,1-Mathf.Exp(-Time.deltaTime*4)):Mathf.Lerp(wide,size,t);
  var target=Overview?all:Vector3.Lerp(all,follow,t);transform.position=IntroFinished&&!Overview?Vector3.Lerp(transform.position,target,1-Mathf.Exp(-5*Time.deltaTime)):target;
  if(frame){frame.gameObject.SetActive(!Overview&&t>.65f);frame.localPosition=new Vector3(0,0,2);frame.localScale=new Vector3(view.orthographicSize*view.aspect*2,view.orthographicSize*2,1);}
 }
}
