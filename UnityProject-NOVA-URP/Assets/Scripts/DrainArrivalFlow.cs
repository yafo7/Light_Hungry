using UnityEngine;

public class DrainArrivalFlow : MonoBehaviour {
 public PlayerMotor player;public LightFormAbility ability;GUIStyle title,small;Font font;
 void Update(){bool free=!SceneFade.Busy;float move=free?Input.GetAxisRaw("Horizontal"):0;player.Drive(!ability.IsLight?move:0,free&&Input.GetKeyDown(KeyCode.Space));ability.Control(move,free,Input.GetKeyDown(KeyCode.E));if(free&&Input.GetKeyDown(KeyCode.R))SceneFade.Load("SecondLight");}
 void OnGUI(){if(!font){font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei UI","Microsoft YaHei","Arial"},24);title=new GUIStyle{font=font,fontSize=24,normal={textColor=new Color(.83f,.88f,.79f)}};small=new GUIStyle(title){fontSize=14};}var c=GUI.color;GUI.color=new Color(.02f,.04f,.035f,.8f);GUI.DrawTexture(new Rect(24,22,350,90),Texture2D.whiteTexture);GUI.color=c;GUI.Label(new Rect(42,35,330,32),"03  排水暗渠",title);GUI.Label(new Rect(43,75,320,30),"入口预览  ·  A / D 行走  ·  R 返回陶盆",small);}
}
