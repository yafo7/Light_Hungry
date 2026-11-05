using UnityEngine;
using UnityEngine.SceneManagement;

public class PotLevelFlow : MonoBehaviour {
 public PlayerMotor player;public LightFormAbility ability;public PotCamera cameraRig;public ScenePassage exit;
 public bool Testing;public GardenInput testInput;GUIStyle title,small,hint;Font font;Vector3 spawn;
 void Start(){Application.targetFrameRate=60;QualitySettings.vSyncCount=0;spawn=player.transform.position;}
 void Update(){var i=Testing?testInput:new GardenInput{move=Input.GetAxisRaw("Horizontal"),jump=Input.GetKeyDown(KeyCode.Space),interact=Input.GetKeyDown(KeyCode.E),overview=Input.GetKeyDown(KeyCode.Tab),skip=Input.GetKeyDown(KeyCode.Space)};testInput=new GardenInput();
  if(!Testing&&Input.GetKeyDown(KeyCode.R)){SceneFade.Load("SecondLight");return;}
  if(SceneFade.Busy){player.Drive(0,false);ability.Control(0,false);return;}
  if(i.overview)cameraRig.Toggle();if(i.skip&&!cameraRig.IntroFinished){cameraRig.Skip();i.jump=false;}
  bool ready=cameraRig.IntroFinished;player.Drive(ready&&!ability.IsLight?i.move:0,ready&&!ability.IsLight&&i.jump);ability.Control(ready?i.move:0,ready,i.interact);
  if(player.transform.position.y<-8&&!ability.IsLight){player.Body.position=spawn;player.Body.velocity=Vector2.zero;}
 }
 void Styles(){if(font)return;font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei UI","Microsoft YaHei","Arial"},24);title=new GUIStyle{font=font,fontSize=24,normal={textColor=new Color(.98f,.94f,.8f)}};small=new GUIStyle(title){fontSize=13};hint=new GUIStyle(title){fontSize=17,alignment=TextAnchor.MiddleCenter};}
 void Panel(Rect r){var c=GUI.color;GUI.color=new Color(.045f,.06f,.047f,.76f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=c;}
 void OnGUI(){Styles();float k=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.Scale(new Vector3(k,k,1));float w=Screen.width/k,h=Screen.height/k;Panel(new Rect(24,22,295,66));GUI.Label(new Rect(40,28,270,32),"02  倒扣陶盆",title);GUI.Label(new Rect(41,61,265,20),ability.RestoreCount>0?"水声，在阴影另一边":"一滴露水，留住了穿行的光",small);
  Panel(new Rect(w-340,24,316,28));GUI.Label(new Rect(w-325,29,300,22),"A / D 移动    空格 跳跃    Tab 全景",small);
  string s=!cameraRig.IntroFinished?"阳光从盆底的小孔落下……   空格 跳过开场":ability.IsLight?"A / D 沿光线前后移动   ·   松键停留   ·   E 在安全地面恢复":ability.Charge>0?"停在光中，让身体慢慢亮起来……   "+Mathf.Min(3,ability.Charge).ToString("0.0")+" / 3 秒":ability.RestoreCount>0?"走入右下方的阴影，去听更深处的水声。":player.transform.position.x<13?"走近叶片上的露珠，寻找它留下的光。":"石缝太窄了。试着在光中静止三秒。";
  if(ability.Blocked)s="光被遮住了，试着沿原路退回。";Panel(new Rect(w/2-355,h-68,710,40));GUI.Label(new Rect(w/2-345,h-64,690,30),s,hint);
  if(ability.Charge>0&&!ability.IsLight){var c=GUI.color;GUI.color=new Color(1,.8f,.35f);GUI.DrawTexture(new Rect(w/2-110,h-78,220*Mathf.Clamp01(ability.Charge/3),4),Texture2D.whiteTexture);GUI.color=c;}
 }
}
