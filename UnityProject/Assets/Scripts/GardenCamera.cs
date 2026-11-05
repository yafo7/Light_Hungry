using UnityEngine;

public class GardenCamera : MonoBehaviour {
 public Transform player; public Camera view; public bool Overview {get;private set;} public bool IntroFinished {get;private set;}
 float start;public float gameplaySize=8;
 void Start(){start=Time.time;view.orthographic=true;}
 public void SkipIntro(){start=Time.time-5;IntroFinished=true;}
 public void ToggleOverview(){SkipIntro();Overview=!Overview;}
 void LateUpdate(){
  float wide=Mathf.Max(24,48/view.aspect);float t=Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.time-start-1.2f)/3));
  if(t>=1)IntroFinished=true;float size=Overview?wide:Mathf.Lerp(wide,gameplaySize,t);view.orthographicSize=size;
  float half=gameplaySize*view.aspect;float followX=Mathf.Clamp(player.position.x+4,half,96-half);
  Vector3 opening=new Vector3(48,Mathf.Min(24,48.1f-wide),-70);
  Vector3 target=Overview?opening:Vector3.Lerp(opening,new Vector3(followX,10.5f,-70),t);
  transform.position=IntroFinished&&!Overview?Vector3.Lerp(transform.position,target,1-Mathf.Exp(-Time.deltaTime*4)):target;
 }
}
