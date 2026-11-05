using System;
using UnityEngine;

[Serializable]public struct DrainInput {public float move,winch;public bool jump,interact,skip,overview,restart;}
public class DrainLevelFlow : MonoBehaviour {
 public PlayerMotor player;public LightFormAbility ability;public DrainMechanism mechanism;public DrainCamera cameraRig;
 public Transform[] checkpoints;public Transform finish;public bool Testing;public DrainInput testInput;
 public bool Adjusting {get;private set;}public bool Completed {get;private set;}public int Checkpoint {get;private set;}public int Deaths {get;private set;}
 public float ActiveTime {get;private set;}float respawnAt=-10;int winch;GUIStyle title,small,hint;Font font;
 void Start(){Application.targetFrameRate=60;QualitySettings.vSyncCount=0;}
 public void Respawn(){Deaths++;Adjusting=false;cameraRig.FocusMechanism=false;mechanism.WinchInput=0;ability.ResetAt(checkpoints[Checkpoint].position);respawnAt=Time.time;}
 void Update(){
  if(GameSession.InputBlocked){player.Drive(0,false);if(ability)ability.Control(0,false);return;}

  var i=Testing?testInput:new DrainInput{move=Input.GetAxisRaw("Horizontal"),winch=Input.GetAxisRaw("Vertical"),jump=Input.GetKeyDown(KeyCode.Space),interact=Input.GetKeyDown(KeyCode.E),skip=Input.GetKeyDown(KeyCode.Space),overview=Input.GetKeyDown(KeyCode.Tab),restart=Input.GetKeyDown(KeyCode.R)};testInput=new DrainInput();
  if(i.restart&&!SceneFade.Busy){SceneFade.Load("ThirdDrain");return;}if(i.overview)cameraRig.Toggle();if(i.skip&&!cameraRig.IntroFinished){cameraRig.Skip();i.jump=false;}
  if(Completed&&i.interact&&!SceneFade.Busy&&UnityEngine.SceneManagement.SceneManager.GetSceneByName("FourthReservoir").name!="FourthReservoir"&&Application.CanStreamedLevelBeLoaded("FourthReservoir")){SceneFade.Load("FourthReservoir");return;}
  if(SceneFade.Busy||!cameraRig.IntroFinished||Completed||Time.time-respawnAt<.22f){player.Drive(0,false);ability.Control(0,false);mechanism.WinchInput=0;return;}
  ActiveTime+=Time.deltaTime;
  if(player.transform.position.y<-.55f||player.transform.position.x<-.5f||player.transform.position.x>78){Respawn();return;}
  if(ability.IsLight){Adjusting=false;cameraRig.FocusMechanism=false;mechanism.WinchInput=0;player.Drive(0,false);ability.Control(i.move,false,i.interact);return;}
  if(i.interact&&player.Grounded){
   if(Adjusting)Adjusting=false;
   else if(Near(mechanism.rootSwitch,1.4f))mechanism.Activate();
   else if(mechanism.Powered&&Near(mechanism.leftWinch,1.6f)){Adjusting=true;winch=0;}
   else if(mechanism.Powered&&Near(mechanism.rightWinch,1.6f)){Adjusting=true;winch=1;}
  }
  if(Adjusting&&(!player.Grounded||!Near(winch==0?mechanism.leftWinch:mechanism.rightWinch,2)||(!Testing&&(!GameSession.Instance&&Input.GetKeyDown(KeyCode.Escape)))))Adjusting=false;
  player.Drive(Adjusting?0:i.move,!Adjusting&&i.jump);ability.Control(Adjusting?0:i.move,!Adjusting);
  mechanism.WinchInput=Adjusting?i.winch:0;
  cameraRig.FocusMechanism=Adjusting;
  if(player.Grounded){for(int n=Checkpoint+1;n<checkpoints.Length;n++)if(Vector2.Distance(player.transform.position,checkpoints[n].position)<1.1f)Checkpoint=n;}
  if(mechanism.Awakened&&mechanism.Aligned&&player.Grounded&&Near(finish,1.1f)){Completed=true;player.Drive(0,false);mechanism.WinchInput=0;}
 }
 bool Near(Transform t,float radius){return t&&Vector2.Distance(player.transform.position,t.position)<radius;}
 public string Hint(){
  if(!cameraRig.IntroFinished)return "水声引你走入根须深处……   空格跳过开场";
  if(cameraRig.Overview)return "Tab 返回游玩镜头";
  if(ability.IsLight)return "A / D 沿光前后移动 · 松键停留 · E 安全恢复";
  if(Adjusting)return (mechanism.Middle?"配重与两岸齐平。":"镜片上升，配重就会下降。")+"  W / S 升降 · E 松开";
  if(!mechanism.Powered)return Near(mechanism.rootSwitch,2)?"E 轻触根结，唤醒地下的球茎。":"沿着苔阶，寻找球茎垂下的根结。";
  if(Near(mechanism.leftWinch,2)||Near(mechanism.rightWinch,2))return "E 握住绞盘 · W / S 升降镜片 · 松手后会停在原位";
  if(!mechanism.Awakened&&player.transform.position.x>31)return "对岸的绞盘也连着镜架。让光穿过高处的小孔。";
  if(player.transform.position.x<25)return "水面太宽。让配重停在两岸之间，再借它过去。";
  if(player.transform.position.x<48)return "沿低处向右，借错层台阶折返向上。";
  if(player.transform.position.x<71)return "水轮推动小踏板。等它靠近，站稳后再跳。";
  return mechanism.Awakened&&mechanism.Aligned?"最后几级石阶。光鳞正在等你。":"光路还没有稳定。回到绞盘，让光继续穿过墙孔。";
 }
 void Styles(){if(font)return;font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei UI","Microsoft YaHei","Arial"},24);title=new GUIStyle{font=font,fontSize=24,normal={textColor=new Color(.96f,.92f,.76f)}};small=new GUIStyle(title){fontSize=13};hint=new GUIStyle(title){fontSize=16,alignment=TextAnchor.MiddleCenter,wordWrap=true};}
 void Panel(Rect r){var old=GUI.color;GUI.color=new Color(.025f,.047f,.05f,.82f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
 void OnGUI(){if(GameSession.Instance)return;Styles();var old=GUI.matrix;float k=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.Scale(new Vector3(k,k,1));float w=Screen.width/k,h=Screen.height/k;
  Panel(new Rect(24,22,290,64));GUI.Label(new Rect(40,28,270,32),"03  排水暗渠",title);GUI.Label(new Rect(41,61,270,22),mechanism.Awakened?"光沿鳞片，重新流动起来":"把沉睡的光，送往水声深处",small);
  Panel(new Rect(w-357,24,333,28));GUI.Label(new Rect(w-343,29,320,22),"A / D 移动  空格跳跃  E 交互  Tab 全景  R 重来",small);
  if(Completed){Panel(new Rect(w/2-240,h/2-95,480,190));GUI.Label(new Rect(w/2-220,h/2-64,440,42),"一条光的河流，在它体内醒来。",hint);GUI.Label(new Rect(w/2-210,h/2-8,420,42),"第三关完成\n光已越过暗渠，继续流向花园深处。",hint);GUI.Label(new Rect(w/2-210,h/2+58,420,24),Application.CanStreamedLevelBeLoaded("FourthReservoir")?"E 前往第四关 · 蓄水浅池       R 重新游玩":"R 重新游玩",small);}
  else {Panel(new Rect(w/2-365,h-66,730,42));GUI.Label(new Rect(w/2-350,h-62,700,34),Hint(),hint);}
  if(Adjusting){Panel(new Rect(w/2-120,h-93,240,12));var c=GUI.color;GUI.color=new Color(1,.77f,.28f);GUI.DrawTexture(new Rect(w/2-117,h-90,234*mechanism.height,6),Texture2D.whiteTexture);GUI.color=new Color(.9f,.92f,.8f);GUI.DrawTexture(new Rect(w/2-2,h-94,4,14),Texture2D.whiteTexture);GUI.color=c;}
  if(Time.time-respawnAt<.4f){var c=GUI.color;GUI.color=new Color(.01f,.02f,.025f,.65f*(1-(Time.time-respawnAt)/.4f));GUI.DrawTexture(new Rect(0,0,w,h),Texture2D.whiteTexture);GUI.color=c;}
  GUI.matrix=old;
 }
}
