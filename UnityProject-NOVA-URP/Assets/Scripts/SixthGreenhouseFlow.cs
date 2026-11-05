using System.Collections.Generic;
using UnityEngine;

public class SixthGreenhouseFlow:MonoBehaviour {
 public PlayerMotor player;public LightFormAbility ability;public PotCamera cameraRig;public LightPath path;
 public Transform[] wheels,landings;public GardenReusableProp[] mirrors;
 public Transform tray,curtain,rootGate;public Collider2D trayCollider,curtainBlock,exitBlock;public Collider2D[] stepColliders;
 public LineRenderer[] trayRopes;public Renderer scaleSurface,scaleGlow;
 public int aimA,aimC,aimD,Checkpoint,Deaths,Adjusting=-1;
 public bool TrayHigh=true,CurtainOpen,Awakened,Completed,Testing;
 public ReservoirInput testInput;public float exposure;
 public bool TraySettled=>Mathf.Abs(trayBlend-(TrayHigh?1:0))<.001f;
 public bool UpperConnected=>aimA==1&&CurtainOpen&&aimC==1&&TrayHigh&&TraySettled;
 public float TrayBlend=>trayBlend;
 float trayBlend=1,cooldown;Font uiFont;MaterialPropertyBlock scaleProps;
 public const float PPU=50,CanvasHeight=887;
 public static Vector3 Px(float x,float y,float z=0)=>new Vector3(x/PPU,(CanvasHeight-y)/PPU,z);
 public static Vector3 S=>Px(80,340);public static Vector3 A=>Px(255,670);public static Vector3 B=>Px(1540,670);
 public static Vector3 C=>Px(420,225);public static Vector3 HighP=>Px(960,255);public static Vector3 LowP=>Px(960,720);
 public static Vector3 D=>Px(1450,225);public static Vector3 T=>Px(1660,105);
 public static Vector3 TrayLow=>Px(960,765);public static Vector3 TrayHighPosition=>Px(960,300);
 public float PixelX=>player.Body.position.x*PPU;public float PixelY=>CanvasHeight-player.Body.position.y*PPU;

 public Vector3[] IntendedNodes(){
  if(aimA==0)return new[]{S,A,B};
  if(aimC!=1)return new[]{S,A,C,Px(1700,aimC==0?455:100)};
  if(!TrayHigh||!TraySettled)return new[]{S,A,C,Px(1700,225+(1700-420)*30f/540)};
  return new[]{S,A,C,HighP,D,aimD==0?Px(1530,80):T};
 }
 public Vector3[] OpticalNodes(){
  var desired=IntendedNodes();var result=new List<Vector3>{desired[0]};
  for(int j=1;j<desired.Length;j++){
   Vector2 from=desired[j-1],v=(Vector2)desired[j]-from;
   var hit=Physics2D.Raycast(from,v.normalized,v.magnitude,1<<6);
   if(hit.collider){result.Add(hit.point-v.normalized*.07f);break;}
   result.Add(desired[j]);
  }
  return result.ToArray();
 }
 public void RefreshOptics(){
  if(!path)return;Physics2D.SyncTransforms();path.nodes=OpticalNodes();path.safeEnd=null;
  if(mirrors==null||mirrors.Length<4)return;
  mirrors[0].SetMirrorPlane(S,A,aimA==0?B:C);
  mirrors[1].SetMirrorPlane(A,C,aimC==1?HighP:Px(1700,aimC==0?455:100));
  mirrors[2].SetMirrorPlane(C,HighP,D);
  mirrors[3].SetMirrorPlane(HighP,D,aimD==0?Px(1530,80):T);
 }
 void Start(){scaleProps=new MaterialPropertyBlock();RefreshOptics();Application.targetFrameRate=60;}
 void AnimateMechanisms(){
  float old=trayBlend;trayBlend=Mathf.MoveTowards(trayBlend,TrayHigh?1:0,Time.deltaTime*.42f);
  if(tray){tray.position=Vector3.Lerp(TrayLow,TrayHighPosition,trayBlend);}
  if(curtain){curtain.localScale=new Vector3(1,CurtainOpen?.04f:1,1);curtainBlock.enabled=!CurtainOpen;}
  if(trayRopes!=null&&trayRopes.Length==2){
   for(int j=0;j<2;j++){float x=j==0?862:1058;trayRopes[j].SetPosition(0,Px(x,80,-.5f));trayRopes[j].SetPosition(1,new Vector3(x/PPU,tray.position.y,-.5f));}
  }
  if(rootGate)rootGate.localScale=new Vector3(1,Awakened?.05f:1,1);
  if(exitBlock)exitBlock.enabled=!Awakened;
  if(scaleGlow)scaleGlow.enabled=exposure>0||Awakened;
  if(scaleSurface){scaleSurface.GetPropertyBlock(scaleProps);scaleProps.SetColor("_Color",Awakened?new Color(1,.82f,.35f):new Color(.57f,.61f,.62f));scaleSurface.SetPropertyBlock(scaleProps);}
  if(old!=trayBlend)RefreshOptics();
 }
 public void Respawn(){
  Deaths++;Adjusting=-1;
  // A always retains source light; never respawn on a moved tray or dark B.
  if(Checkpoint==1&&aimA!=0)Checkpoint=0;
  if(Checkpoint>=2&&!(aimA==1&&CurtainOpen))Checkpoint=0;
  ability.ResetAt(landings[Checkpoint].position);
 }
 bool Near(Transform t,float r=1.25f)=>t&&Vector2.Distance(player.transform.position,t.position)<r;
 void Update(){
  var i=Testing?testInput:new ReservoirInput{move=Input.GetAxisRaw("Horizontal"),adjust=Input.GetAxisRaw("Vertical"),jump=Input.GetKeyDown(KeyCode.Space),interact=Input.GetKeyDown(KeyCode.E),overview=Input.GetKeyDown(KeyCode.Tab),restart=Input.GetKeyDown(KeyCode.R)};testInput=new ReservoirInput();
  if(i.restart){SceneFade.Load("SixthGreenhouse");return;}
  if(i.overview)cameraRig.Toggle();if(i.jump&&!cameraRig.IntroFinished){cameraRig.Skip();i.jump=false;}
  AnimateMechanisms();
  var n=path.nodes;bool targetLit=n!=null&&n.Length>1&&Vector2.Distance(n[n.Length-1],T)<.12f;
  if(UpperConnected&&aimD==1&&targetLit&&!Awakened){exposure+=Time.deltaTime;if(exposure>=2)Awakened=true;}else if(!Awakened)exposure=0;
  if(Completed&&i.interact&&!SceneFade.Busy&&Application.CanStreamedLevelBeLoaded("SeventhTrellis")){SceneFade.Load("SeventhTrellis");return;}
  if(Completed||SceneFade.Busy||!cameraRig.IntroFinished){player.Drive(0,false);ability.Control(0,false);return;}
  if(PixelY>884){Respawn();return;}
  if(ability.IsLight){Adjusting=-1;player.Drive(0,false);ability.Control(i.move,false,i.interact);return;}
  if(player.Grounded){
   if(PixelY>700&&PixelY<720){if(PixelX<580)Checkpoint=0;else if(PixelX>1450&&aimA==0)Checkpoint=1;}
   if(PixelY>265&&PixelY<285){if(PixelX<580&&aimA==1&&CurtainOpen)Checkpoint=2;else if(PixelX>1325&&UpperConnected)Checkpoint=3;}
  }
  if(i.interact&&player.Grounded&&TraySettled){
   if(Adjusting>=0)Adjusting=-1;
   else if(Near(wheels[1])){TrayHigh=!TrayHigh;}
   else if(!TrayHigh&&PixelX>=850&&PixelX<=1070&&Mathf.Abs(PixelY-765)<12&&Near(wheels[2])){CurtainOpen=true;curtainBlock.enabled=false;RefreshOptics();}
   else {foreach(int k in new[]{0,3,4})if(Near(wheels[k])){Adjusting=k;break;}}
  }
  if(Adjusting>=0&&!Near(wheels[Adjusting],1.6f))Adjusting=-1;
  if(Adjusting>=0&&Mathf.Abs(i.adjust)>.5f&&Time.time>=cooldown){
   int d=i.adjust>0?1:-1;
   if(Adjusting==0)aimA=Mathf.Clamp(aimA+d,0,1);else if(Adjusting==3)aimC=Mathf.Clamp(aimC+d,0,2);else if(Adjusting==4)aimD=Mathf.Clamp(aimD+d,0,1);
   cooldown=Time.time+.35f;RefreshOptics();
  }
  bool free=Adjusting<0&&TraySettled;player.Drive(free?i.move:0,free&&i.jump);ability.Control(i.move,free);
  if(Awakened&&player.Grounded&&PixelY<300&&PixelX>1710)Completed=true;
 }
 string Hint(){
  if(!cameraRig.IntroFinished)return "温室苗床 · 先借光落脚，再让托盘接光。空格跳过开场";
  if(Completed)return Application.CanStreamedLevelBeLoaded("SeventhTrellis")?"第六关完成 · 按 E 进入第七关叶幕走廊。":"第六关完成 · 叶幕后的两面走廊等待下一次探索。";
  if(ability.IsLight)return "A / D 沿光前后移动 · E 在安全落脚面上方恢复实体并短落";
  if(!TraySettled)return "托盘正在换位，请等升降卷轴锁止。";
  if(Adjusting>=0)return "W / S 调整镜向；观察光斑与窄缝 · E 松手";
  if(!CurtainOpen)return "沟壁上的帘绳扣够不到。右岸卷轴可以把育苗托盘降下来。";
  if(!TrayHigh)return "遮光帘已锁开。走右侧回光阶返回卷轴，让托盘换成高位接光。";
  if(aimA==0)return "托盘镜已归位。沿原光返回左岸，把 A 镜转向上层。";
  if(aimC!=1)return "在 C 平台调镜；只有穿过遮光板窄缝，光才能交给高位托盘。";
  return "沿上层光路抵达 D，调整末镜，让灰暗肋鳞重新发亮。";
 }
 void OnGUI(){if(!uiFont)uiFont=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei UI","Microsoft YaHei","Arial"},20);var style=new GUIStyle(GUI.skin.box){font=uiFont,fontSize=18,wordWrap=true};GUI.Box(new Rect(20,16,430,38),"06  温室苗床 · 途中落脚，往返换位",style);GUI.Box(new Rect(20,Screen.height-78,Screen.width-40,60),Hint()+"\nA/D 移动  空格跳跃  E 交互  Tab 全景  R 重来",style);}
}
