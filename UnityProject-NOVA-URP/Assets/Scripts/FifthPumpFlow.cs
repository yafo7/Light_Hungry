using System;
using UnityEngine;

// One source, one ordered optical path. Detents are visible physical mirror stops.
public class FifthPumpFlow : MonoBehaviour {
 public PlayerMotor player; public LightFormAbility ability; public PotCamera cameraRig; public LightPath path;
 public Transform[] mirrors,wheels,landings,steps; public Collider2D[] stepColliders;
 public Transform rootGate; public Collider2D exitBlock; public Renderer scaleGlow;
 public int aimA,aimC,aimD; public bool StepsOpen,Awakened,Completed,Testing;
 public ReservoirInput testInput; public int Adjusting=-1,Deaths; public float exposure;
 public int Checkpoint; float inputCooldown,stepBlend; Font uiFont;
 public static Vector3 Px(float x,float y,float z=0)=>new Vector3(x/80,(1024-y)/80,z);
 public static Vector3 S=>Px(130,90); public static Vector3 M0=>Px(185,825);
 public static Vector3 A=>Px(1118,690); public static Vector3 Q=>Px(868,478);
 public static Vector3 D=>Px(1214,190); public static Vector3 T=>Px(1420,96);
 public Vector3[] OpticalNodes(){
  if(aimA==0)return new[]{S,M0,A,Px(970,620)};
  if(aimA==1)return new[]{S,M0,A,Px(350,450)};
  if(aimC==0)return new[]{S,M0,A,Q,Px(760,478)};
  return new[]{S,M0,A,Q,D,aimD==0?Px(1214,62):T};
 }
 // For each detent, mirror normal is the bisector satisfying the reflection law.
 public static float MirrorRotation(Vector3 previous,Vector3 mirror,Vector3 next){
  Vector2 n=((Vector2)(mirror-previous).normalized-(Vector2)(next-mirror).normalized).normalized;
  return Mathf.Atan2(n.y,n.x)*Mathf.Rad2Deg;
 }
 public void RefreshOptics(){
  path.nodes=OpticalNodes();path.safeEnd=aimA==1?landings[2]:(aimA==2&&aimC==1?landings[4]:null);
  var n=path.nodes;mirrors[0].rotation=Quaternion.Euler(0,0,MirrorRotation(S,M0,A));
  mirrors[1].rotation=Quaternion.Euler(0,0,MirrorRotation(M0,A,n[3]));
  mirrors[2].rotation=Quaternion.Euler(0,0,MirrorRotation(A,Q,aimC==0?Px(760,478):D));
  mirrors[3].rotation=Quaternion.Euler(0,0,MirrorRotation(Q,D,aimD==0?Px(1214,62):T));
 }
 void Start(){RefreshOptics();Application.targetFrameRate=60;}
 public void Respawn(){Deaths++;Adjusting=-1;ability.ResetAt(landings[Checkpoint].position);}
 void Update(){
  var i=Testing?testInput:new ReservoirInput{move=Input.GetAxisRaw("Horizontal"),adjust=Input.GetAxisRaw("Vertical"),jump=Input.GetKeyDown(KeyCode.Space),interact=Input.GetKeyDown(KeyCode.E),overview=Input.GetKeyDown(KeyCode.Tab),restart=Input.GetKeyDown(KeyCode.R)};testInput=new ReservoirInput();
  if(i.restart){SceneFade.Load("FifthPumpHouse");return;}
  if(i.overview)cameraRig.Toggle();if(i.jump&&!cameraRig.IntroFinished){cameraRig.Skip();i.jump=false;}
  stepBlend=Mathf.MoveTowards(stepBlend,StepsOpen?1:0,Time.deltaTime*1.3f);
  for(int j=0;j<steps.Length;j++){steps[j].localScale=new Vector3(Mathf.Max(.04f,stepBlend),1,1);stepColliders[j].enabled=stepBlend>=.99f;}
  if(scaleGlow)scaleGlow.enabled=Awakened||exposure>0;
  if(rootGate)rootGate.localScale=new Vector3(1,Awakened?.08f:1,1);exitBlock.enabled=!Awakened;
  if(aimA==2&&aimC==1&&aimD==1&&!Awakened){exposure+=Time.deltaTime;if(exposure>=2)Awakened=true;}else if(!Awakened)exposure=0;
  if(Completed&&i.interact&&!SceneFade.Busy&&Application.CanStreamedLevelBeLoaded("SixthGreenhouse")){SceneFade.Load("SixthGreenhouse");return;}
  if(Completed||SceneFade.Busy||!cameraRig.IntroFinished){player.Drive(0,false);ability.Control(0,false);return;}
  if(player.transform.position.y<Px(0,970).y){Respawn();return;}
  if(ability.IsLight){Adjusting=-1;player.Drive(0,false);ability.Control(i.move,false,i.interact);return;}
  if(player.Grounded){
   if(Near(landings[1],1.1f)&&Checkpoint==0)Checkpoint=1;
   if(Near(landings[4],1.7f)&&aimC==1)Checkpoint=4;
  }
  if(i.interact&&player.Grounded){
   if(Adjusting>=0)Adjusting=-1;
   else if(Near(wheels[1],1.15f)){StepsOpen=true;wheels[1].Rotate(0,0,55);}
   else for(int k=0;k<3;k++){int index=k==0?0:k+1;if(Near(wheels[index],1.15f)){Adjusting=index;break;}}
  }
  if(Adjusting>=0&&!Near(wheels[Adjusting],1.5f))Adjusting=-1;
  if(Adjusting>=0&&Mathf.Abs(i.adjust)>.5f&&Time.time>=inputCooldown){
   int d=i.adjust>0?1:-1;
   if(Adjusting==0)aimA=Mathf.Clamp(aimA+d,0,2);
   if(Adjusting==2)aimC=Mathf.Clamp(aimC+d,0,1);
   if(Adjusting==3)aimD=Mathf.Clamp(aimD+d,0,1);
   wheels[Adjusting].Rotate(0,0,d*45);inputCooldown=Time.time+.35f;RefreshOptics();
  }
  player.Drive(Adjusting>=0?0:i.move,Adjusting<0&&i.jump);ability.Control(i.move,Adjusting<0);
  if(Awakened&&player.Grounded&&player.transform.position.x>18.2f)Completed=true;
 }
 bool Near(Transform t,float r)=>Vector2.Distance(player.transform.position,t.position)<r;
 string Hint(){
  if(!cameraRig.IntroFinished)return "空格跳过开场 · 一束光，将旧泵房重新连接。";
  if(Completed)return Application.CanStreamedLevelBeLoaded("SixthGreenhouse")?"第五关完成 · 按 E 进入第六关温室苗床。":"第五关完成 · 温室的风从前方吹来。";
  if(ability.IsLight)return "A / D 沿光线前后移动 · E 在落脚面恢复实体";
  if(Adjusting>=0)return "W / S 调整镜架限位 · E 松手（实体才能操作机关）";
  if(!StepsOpen)return "先把 A 镜的光送进左侧检修舱；舱内手轮能展开检修踏板。";
  if(aimA!=2)return "检修踏板已锁止。回 A 收回舱内的光，转送中央悬镜。";
  if(aimC==0)return "沿右侧三级踏板折返向上，在 C 的镜背手轮接通高处光路。";
  if(Checkpoint<4)return "回到 A，在光中静止三秒，化光经过悬镜进入玻璃观察廊。";
  return "E 握住末镜手轮，W 调整，让光照到灰暗的侧腹光鳞。";
 }
 void OnGUI(){if(!uiFont)uiFont=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei UI","Microsoft YaHei","Arial"},20);var style=new GUIStyle(GUI.skin.box){font=uiFont,fontSize=18,wordWrap=true};GUI.Box(new Rect(20,16,420,38),"05  废弃泵房 · 一束光，多次改道",style);GUI.Box(new Rect(20,Screen.height-78,Screen.width-40,60),Hint()+"\nA/D 移动  空格跳跃  E 交互  Tab 全景  R 重来",style);}
}
