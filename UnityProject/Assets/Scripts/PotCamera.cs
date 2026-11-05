using UnityEngine;

public class PotCamera : MonoBehaviour {
 public Transform player;public Camera view;public float canvasWidth=28.15873f,top=9.06349f,bottom=-5.01587f,playSize=4.35f;
 public bool IntroFinished {get;private set;}public bool Overview {get;private set;}float start;
 void Start(){start=Time.time;}public void Skip(){start=Time.time-5;IntroFinished=true;}
 public void Toggle(){Skip();Overview=!Overview;}
 void LateUpdate(){float wide=Mathf.Max((top-bottom)/2,canvasWidth/(2*view.aspect));Vector3 opening=new Vector3(canvasWidth/2,(top+bottom)/2,-40);
  float t=Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.time-start-1.1f)/2.6f));if(t>=1)IntroFinished=true;
  float half=playSize*view.aspect,x=half*2>=canvasWidth?canvasWidth/2:Mathf.Clamp(player.position.x+2.2f,half,canvasWidth-half);float y=Mathf.Clamp(player.position.y+1.4f,bottom+playSize,top-playSize);
  float size=Overview?wide:Mathf.Lerp(wide,playSize,t);Vector3 target=Overview?opening:Vector3.Lerp(opening,new Vector3(x,y,-40),t);view.orthographicSize=size;transform.position=IntroFinished&&!Overview?Vector3.Lerp(transform.position,target,1-Mathf.Exp(-Time.deltaTime*4)):target;
 }
}
