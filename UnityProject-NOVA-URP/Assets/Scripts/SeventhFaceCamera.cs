using UnityEngine;

public class SeventhFaceCamera:MonoBehaviour {
 public Camera view;public Transform player;public SeventhTrellisFlow flow;
 public float canvasWidth=35.48f,height=17.74f,playSize=5.5f;
 public bool IntroFinished{get;private set;}public bool Overview{get;private set;}
 public float Veil{get;set;}float start;
 void Start(){start=Time.time;}
 public void Skip(){start=Time.time-5;IntroFinished=true;}
 public void Toggle(){Skip();Overview=!Overview;}
 void LateUpdate(){
  if(!view||!player)return;float wide=Mathf.Max(height/2,canvasWidth/(2*view.aspect));
  float t=Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.time-start-1.1f)/2.6f));if(t>=1)IntroFinished=true;
  Vector3 opening=new Vector3(canvasWidth/2,height/2,-40);float half=playSize*view.aspect;
  Vector2 follow=flow&&flow.PreviewBusy?flow.PreviewTarget:(Vector2)player.position+Vector2.right*1.2f;
  float x=half*2>=canvasWidth?canvasWidth/2:Mathf.Clamp(follow.x,half,canvasWidth-half),y=Mathf.Clamp(follow.y+1.4f,playSize,height-playSize);
  bool wideView=Overview||flow&&flow.PreviewBusy;view.orthographicSize=wideView?wide:Mathf.Lerp(wide,playSize,t);
  var target=wideView?opening:Vector3.Lerp(opening,new Vector3(x,y,-40),t);
  transform.position=IntroFinished&&!wideView?Vector3.Lerp(transform.position,target,1-Mathf.Exp(-Time.deltaTime*5)):target;
 }
 void OnGUI(){if(Veil<=0)return;var old=GUI.color;GUI.depth=-100;GUI.color=new Color(.1f,.14f,.12f,Veil);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=old;}
}
