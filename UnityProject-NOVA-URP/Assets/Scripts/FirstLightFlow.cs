using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]public struct GardenInput {public float move;public bool jump,interact,overview,skip;}
public class FirstLightFlow : MonoBehaviour {
 public PlayerMotor player;public GardenCamera cameraRig;public MirrorPuzzle puzzle;public Transform grip,entrance;public LightFormAbility ability;
 public bool Adjusting {get;private set;}public bool Completed {get;private set;}public bool Testing;public GardenInput testInput;
 public bool HasTouchedMirror {get;private set;}float solvedAt=-1,completedAt;GUIStyle titleStyle,smallStyle,hintStyle,bodyStyle;Font font;AudioSource sound;bool announced;
 void Awake(){Application.targetFrameRate=60;QualitySettings.vSyncCount=0;sound=gameObject.AddComponent<AudioSource>();sound.volume=.22f;Testing=Array.IndexOf(Environment.GetCommandLineArgs(),"--validate")>=0;}
 void Update(){
  if(GameSession.InputBlocked){player.Drive(0,false);if(ability)ability.Control(0,false);return;}

  GardenInput i=Testing?testInput:new GardenInput{move=Input.GetAxisRaw("Horizontal"),jump=Input.GetKeyDown(KeyCode.Space),interact=Input.GetKeyDown(KeyCode.E),overview=Input.GetKeyDown(KeyCode.Tab),skip=Input.GetKeyDown(KeyCode.Space)};
  if(Testing)testInput=new GardenInput();
  if(!Testing&&Input.GetKeyDown(KeyCode.R)){SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);return;}
  if(i.overview)cameraRig.ToggleOverview();if(i.skip&&!cameraRig.IntroFinished){cameraRig.SkipIntro();i.jump=false;}
  if(!cameraRig.IntroFinished||Completed||SceneFade.Busy){player.Drive(0,false);if(ability)ability.Control(0,false);return;}
  if(ability&&ability.IsLight){ability.Control(i.move,false,i.interact);player.Drive(0,false);return;}
  if(i.interact){
   if(Adjusting)Adjusting=false;
   else if(Mathf.Abs(player.transform.position.x-grip.position.x)<1.4f){Adjusting=true;HasTouchedMirror=true;PlayNote(440,.08f);}
   else if(puzzle.Solved&&Mathf.Abs(player.transform.position.x-entrance.position.x)<1.35f){Completed=true;completedAt=Time.time;PlayNote(660,.5f);if(!GameSession.Instance&&Array.IndexOf(Environment.GetCommandLineArgs(),"--validate")<0)SceneFade.Load("SecondLight");}
  }
  if(!Testing&&(!GameSession.Instance&&Input.GetKeyDown(KeyCode.Escape)))Adjusting=false;
  if(Adjusting){player.Drive(0,false);puzzle.Turn(i.move);}else player.Drive(i.move,i.jump);
  if(ability)ability.Control(Adjusting?0:i.move,!Adjusting);
  if(puzzle.Solved&&!announced){announced=true;solvedAt=Time.time;PlayNote(523.25f,1.1f);}
 }
 void PlayNote(float frequency,float duration){if(GameSession.Instance)sound.volume=.22f*GameSession.Instance.Audio.EffectsVolume;int n=(int)(44100*duration);float[] a=new float[n];for(int i=0;i<n;i++){float t=i/44100f;a[i]=Mathf.Sin(t*frequency*Mathf.PI*2)*Mathf.Exp(-t*5)*Mathf.Min(1,t*80)*.28f+Mathf.Sin(t*frequency*2*Mathf.PI*2)*Mathf.Exp(-t*9)*.06f;}var clip=AudioClip.Create("Glass light",n,1,44100,false);clip.SetData(a,0);sound.PlayOneShot(clip);Destroy(clip,duration+1);}
 void Styles(){if(font)return;font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei UI","Microsoft YaHei","Arial"},24);
  titleStyle=new GUIStyle{font=font,fontSize=24,normal={textColor=new Color(.98f,.92f,.76f)},alignment=TextAnchor.MiddleLeft};
  smallStyle=new GUIStyle(titleStyle){fontSize=13};hintStyle=new GUIStyle(titleStyle){fontSize=17,alignment=TextAnchor.MiddleCenter};bodyStyle=new GUIStyle(hintStyle){fontSize=14,wordWrap=true};}
 void Panel(Rect r,float opacity=.7f){Color c=GUI.color;GUI.color=new Color(.055f,.075f,.065f,opacity);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=c;}
 void OnGUI(){if(GameSession.Instance)return;Styles();float k=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.Scale(new Vector3(k,k,1));float w=Screen.width/k,h=Screen.height/k;
  Panel(new Rect(24,22,272,66),.68f);GUI.Label(new Rect(40,28,252,31),"01  门槛苔地",titleStyle);GUI.Label(new Rect(41,60,240,20),puzzle.Solved?"光正在石阶下呼吸":"把一缕阳光，送到沉睡之处",smallStyle);
  Panel(new Rect(w-325,24,301,28),.55f);GUI.Label(new Rect(w-311,28,285,22),"A / D 移动    空格 跳跃    Tab 全景",smallStyle);
  string hint="";
  if(Completed){Panel(new Rect(w/2-230,h/2-98,460,196),.9f);GUI.Label(new Rect(w/2-210,h/2-76,420,36),"第一缕光，醒来了。",hintStyle);GUI.Label(new Rect(w/2-210,h/2-26,420,54),"微弱的呼吸沿尾鳞向远处传去。\n下一站：倒扣陶盆",bodyStyle);GUI.Label(new Rect(w/2-210,h/2+55,420,24),"第一关原型完成  ·  按 R 重新游玩",bodyStyle);return;}
  if(!cameraRig.IntroFinished)hint="阳光从破墙落下……   空格 跳过开场";
  else if(cameraRig.Overview)hint="Tab 回到游玩镜头";
  else if(Adjusting)hint=puzzle.Solved?"尾鳞苏醒了  ·  E 松开把手":"A / D 转动反光片    ·    让光停留在尾尖    ·    E 松开";
  else if(puzzle.Solved&&Time.time-solvedAt<5)hint="它在呼吸。沿着尾尖，去陶盆下看看。";
  else if(Mathf.Abs(player.transform.position.x-grip.position.x)<1.4f)hint="E 握住园灯下的小把手";
  else if(Mathf.Abs(player.transform.position.x-entrance.position.x)<2)hint=puzzle.Solved?"E 进入陶盆缺口":"缺口里很暗，石阶下似乎还有微弱的呼吸。";
  else if(player.transform.position.x<23)hint="A / D 向前走    ·    空格 越过碎石";
  if(hint!=""){Panel(new Rect(w/2-330,h-70,660,42),.78f);GUI.Label(new Rect(w/2-320,h-65,640,32),hint,hintStyle);}
  if(Adjusting&&!puzzle.Solved){Panel(new Rect(w/2-65,h-84,130,4),.6f);Color c=GUI.color;GUI.color=new Color(1,.78f,.33f);GUI.DrawTexture(new Rect(w/2-65,h-84,130*puzzle.Exposure/puzzle.requiredExposure,4),Texture2D.whiteTexture);GUI.color=c;}
 }
}
