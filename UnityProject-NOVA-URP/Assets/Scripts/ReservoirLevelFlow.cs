using System;
using UnityEngine;

[Serializable]public struct ReservoirInput {public float move,adjust;public bool jump,interact,skip,overview,restart;}
public class ReservoirLevelFlow : MonoBehaviour {
 public PlayerMotor player;public LightFormAbility ability;public ReservoirMechanism mechanism;public PotCamera cameraRig;
 public Transform[] checkpoints;public Transform finish;public bool Testing;public ReservoirInput testInput;
 public bool Adjusting {get;private set;}public bool PushingSeed {get;private set;}public bool Completed {get;private set;}
 public int Checkpoint {get;private set;}public int Deaths {get;private set;}public float ActiveTime {get;private set;}
 float respawnAt=-10,lastMessageUntil;string message;Font font;GUIStyle title,small,hint;
 void Start(){Application.targetFrameRate=60;QualitySettings.vSyncCount=0;}
 public void Respawn(){Deaths++;Adjusting=PushingSeed=false;mechanism.MirrorInput=mechanism.SeedPushInput=0;ability.ResetAt(checkpoints[Checkpoint].position);respawnAt=Time.time;}
 void Update(){
  var i=Testing?testInput:new ReservoirInput{move=Input.GetAxisRaw("Horizontal"),adjust=Input.GetAxisRaw("Vertical"),jump=Input.GetKeyDown(KeyCode.Space),interact=Input.GetKeyDown(KeyCode.E),skip=Input.GetKeyDown(KeyCode.Space),overview=Input.GetKeyDown(KeyCode.Tab),restart=Input.GetKeyDown(KeyCode.R)};testInput=new ReservoirInput();
  if(Completed&&i.interact&&Application.CanStreamedLevelBeLoaded("FifthPumpHouse")){SceneFade.Load("FifthPumpHouse");return;}
  if(i.restart&&!SceneFade.Busy){SceneFade.Load("FourthReservoir");return;}
  if(i.overview&&cameraRig)cameraRig.Toggle();if(i.skip&&cameraRig&&!cameraRig.IntroFinished){cameraRig.Skip();i.jump=false;}
  if(SceneFade.Busy||(cameraRig&&!cameraRig.IntroFinished)||Completed||Time.time-respawnAt<.22f){player.Drive(0,false);ability.Control(0,false);mechanism.MirrorInput=mechanism.SeedPushInput=0;return;}
  ActiveTime+=Time.deltaTime;
  if(player.transform.position.y<ReservoirMechanism.Px(0,625).y||player.transform.position.x<-.4f||player.transform.position.x>23.7f){Respawn();return;}
  if(ability.IsLight){Adjusting=PushingSeed=false;player.Drive(0,false);ability.Control(i.move,false,i.interact);mechanism.MirrorInput=mechanism.SeedPushInput=0;return;}
  if(i.interact&&player.Grounded){
   if(Adjusting)Adjusting=false;else if(PushingSeed)PushingSeed=false;
   else if(Near(mechanism.guideHandle,.82f)){mechanism.ToggleGuide();Say(mechanism.LastAction);}
   else if(Near(mechanism.seedPushPoint,.95f)&&!mechanism.SeedSettled&&!mechanism.SeedMoving){if(mechanism.CanPush){PushingSeed=true;Say("按 A 向左靠近并推动种荚。");}else Say("落料口被叶梗挡住了。先接好下面的叶槽。");}
   else if(Near(mechanism.mirrorHandle,.95f))Adjusting=true;
  }
  if(Adjusting&&(!player.Grounded||!Near(mechanism.mirrorHandle,1.2f)||(!Testing&&Input.GetKeyDown(KeyCode.Escape))))Adjusting=false;
  if(PushingSeed&&(mechanism.SeedMoving||mechanism.SeedSettled||!player.Grounded||!mechanism.GuideDown||(!Testing&&Input.GetKeyDown(KeyCode.Escape))))PushingSeed=false;
  float motorInput=Adjusting?0:i.move;
  if(PushingSeed&&i.move<0&&mechanism.seed){
   float dx=player.transform.position.x-mechanism.seed.position.x;
   // Walk up to the seed first; contact then limits both bodies to the same push speed.
   if(dx<=mechanism.seedContactDistance+.025f)motorInput=dx<.10f?0:i.move*ReservoirMechanism.SeedPushSpeed/Mathf.Max(.01f,player.speed);
  }
  player.Drive(motorInput,!Adjusting&&!PushingSeed&&i.jump);ability.Control(Adjusting?0:i.move,!Adjusting&&!PushingSeed);
  mechanism.MirrorInput=Adjusting?i.adjust:0;mechanism.SeedPushInput=PushingSeed?i.move:0;
  if(player.Grounded){float x=player.transform.position.x;
   if(Checkpoint<1&&x>=650/80f&&x<=1080/80f)Checkpoint=1;
   if(Checkpoint<2&&x>=1430/80f)Checkpoint=2;
  }
  if(mechanism.Awakened&&player.Grounded&&Near(finish,.65f)){Completed=true;player.Drive(0,false);mechanism.MirrorInput=mechanism.SeedPushInput=0;}
 }
 void Say(string value){message=value;lastMessageUntil=Time.time+2.4f;}
 bool Near(Transform p,float radius){return p&&Vector2.Distance(player.transform.position,p.position)<radius;}
 string Hint(){
  if(cameraRig&&!cameraRig.IntroFinished)return "日光越过池水，停在一片垂落的叶子前。  空格跳过开场";
  if(cameraRig&&cameraRig.Overview)return "Tab 返回游玩镜头";
  if(ability.IsLight)return "A / D 沿光前后移动 · 松键停留 · E 在岸上恢复实体";
  if(Adjusting)return "W / S 转动镜片 · E 松开；让光落在那枚灰暗的鳞片上。";
  if(PushingSeed)return mechanism.SeedContact?"种荚正在滚动……按住 A 推入叶槽 · E 松手":"按住 A 靠近种荚，再继续向左推 · E 松手";
  if(Time.time<lastMessageUntil)return message;
  if(Near(mechanism.guideHandle,1))return mechanism.GuideDown?"E 收起导向叶槽，让光继续前行。":mechanism.SeedSettled?"光路接通了。向左走到压板右侧，在光中停留三秒。":"E 放下导向叶槽，接住上方滚落的种荚。";
  if(!mechanism.SeedSettled&&Near(mechanism.seedPushPoint,1.1f))return "E 握住种荚，再按 A 向左推。";
  if(Near(mechanism.mirrorHandle,1.1f))return "E 调整镜片 · W / S 转动";
  if(Checkpoint==0)return "站在光里，静候三秒；化成光后，按 D 越过水面。";
  if(mechanism.SeedMoving)return "种荚顺着叶槽滑落，留下可以托住光的重量。";
  if(!mechanism.SeedSettled)return mechanism.GuideDown?"沿岛上的三级短台跳上去，在储槽右侧推动种荚。":mechanism.PlateHeld?"叶帘升起了。可你化成光以后，还能压住它吗？":"踏上中岛的压板，看看远端叶帘的变化。";
  if(mechanism.GuideDown)return "种荚压住了机关；运送它的叶槽，却挡住了光。";
  if(Checkpoint<2)return "在压板右侧的光中停留三秒，沿光穿过根幕窄孔。";
  if(!mechanism.Awakened)return "只有那枚灰暗的鳞片，还在等待光。";
  return "鳞片重新起伏。沿右岸走向泵房的入口。";
 }
 void Styles(){if(font)return;font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei UI","Microsoft YaHei","Arial"},24);title=new GUIStyle{font=font,fontSize=24,normal={textColor=new Color(.99f,.96f,.86f)}};small=new GUIStyle(title){fontSize=13};hint=new GUIStyle(title){fontSize=16,alignment=TextAnchor.MiddleCenter,wordWrap=true};}
 void Panel(Rect r){var c=GUI.color;GUI.color=new Color(.10f,.13f,.10f,.78f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=c;}
 void OnGUI(){if(!mechanism)return;Styles();var old=GUI.matrix;float k=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.Scale(new Vector3(k,k,1));float w=Screen.width/k,h=Screen.height/k;
  Panel(new Rect(24,22,270,65));GUI.Label(new Rect(39,28,250,32),"04  蓄水浅池",title);GUI.Label(new Rect(40,62,250,20),"留下重量，带走光",small);
  Panel(new Rect(w-365,24,341,28));GUI.Label(new Rect(w-353,29,330,22),"A / D 移动   空格跳跃   E 交互   Tab 全景   R 重来",small);
  if(Completed){Panel(new Rect(w/2-250,h/2-95,500,190));GUI.Label(new Rect(w/2-230,h/2-68,460,45),"它的呼吸，轻轻推开了水面的光。",hint);GUI.Label(new Rect(w/2-230,h/2-7,460,55),"第四关完成\n按 E 进入第五关：废弃泵房",hint);GUI.Label(new Rect(w/2-200,h/2+62,400,22),"R 重新游玩第四关",small);}
  else{Panel(new Rect(w/2-375,h-68,750,44));GUI.Label(new Rect(w/2-360,h-64,720,36),Hint(),hint);}
  if(Adjusting){Panel(new Rect(w/2-125,h-96,250,13));var c=GUI.color;GUI.color=new Color(1,.77f,.28f);GUI.DrawTexture(new Rect(w/2-121,h-92,242*Mathf.InverseLerp(-24,48,mechanism.MirrorAngle),5),Texture2D.whiteTexture);GUI.color=c;}
  if(Time.time-respawnAt<.35f){var c=GUI.color;GUI.color=new Color(.07f,.11f,.10f,.5f*(1-(Time.time-respawnAt)/.35f));GUI.DrawTexture(new Rect(0,0,w,h),Texture2D.whiteTexture);GUI.color=c;}GUI.matrix=old;
 }
}
