using System.Collections;
using UnityEngine;

public class SeventhTrellisFlow:MonoBehaviour {
 public PlayerMotor player;public LightFormAbility ability;public SeventhFaceLightPath path;public SeventhFaceCamera cameraRig;
 public GameObject[] faceRoots;public Transform[] frontSteps;public Collider2D[] frontStepColliders;
 public Transform bridge,screen,rootGate,cart;public Collider2D bridgeCollider,screenBlock,exitBlock;
 public Transform[] switches,cartWheels;public Renderer scaleSurface,scaleGlow;
 public bool Testing;public ReservoirInput testInput;
 public int ActiveFace{get;private set;}public int VisibleFace{get;private set;}
 public bool TransitionBusy{get;private set;}public bool PreviewBusy{get;private set;}
 public bool StepsOpen{get;private set;}public bool BridgeOpen{get;private set;}
 public bool Grabbing{get;private set;}public bool Awakened{get;private set;}public bool Completed{get;private set;}
 public int Crossings{get;private set;}public int Deaths{get;private set;}public int BackActivations{get;private set;}public int FrontActivations{get;private set;}
 public float CartX{get;private set;}=480;
 public int DockIndex=>Mathf.Abs(CartX-480)<.6f?0:Mathf.Abs(CartX-1455)<.6f?1:-1;
 public bool MechanismsSettled=>!PreviewBusy&&!TransitionBusy&&Mathf.Abs(stepBlend-(StepsOpen?1:0))<.001f&&Mathf.Abs(bridgeBlend-(BridgeOpen?1:0))<.001f;
 public float StepBlend=>stepBlend;public float BridgeBlend=>bridgeBlend;
 public Vector2 PreviewTarget{get;private set;}
 public GameObject frontRoot=>faceRoots[0];public GameObject backRoot=>faceRoots[1];
 public Collider2D backBridgeCollider=>bridgeCollider;public Collider2D backBlockerCollider=>screenBlock;
 public const float PPU=50,CanvasHeight=887;
 public static Vector3 Px(float x,float y,float z=0)=>new Vector3(x/PPU,(CanvasHeight-y)/PPU,z);
 public static Vector3 Source=>Px(80,300);public static Vector3 P=>Px(285,670);public static Vector3 R0=>Px(480,670);public static Vector3 R1=>Px(1455,670);public static Vector3 R0Stop=>Px(540,670);public static Vector3 Target=>Px(1650,320);
 public float PixelX=>player.Body.position.x*PPU;public float PixelY=>CanvasHeight-player.Body.position.y*PPU;
 float stepBlend,bridgeBlend,gripOffset,exposure;MaterialPropertyBlock scaleProps;Font uiFont;

 void Start(){scaleProps=new MaterialPropertyBlock();Application.targetFrameRate=60;ActiveFace=0;SetVisibleFace(0);ApplyMechanisms();RefreshOptics();}
 public void SetVisibleFace(int face){
  VisibleFace=Mathf.Clamp(face,0,1);if(faceRoots!=null)for(int i=0;i<faceRoots.Length;i++)if(faceRoots[i])faceRoots[i].SetActive(i==VisibleFace);
  Physics2D.SyncTransforms();if(path)path.RefreshBeams();
 }
 public void RefreshOptics(){if(cart){var p=cart.position;cart.position=new Vector3(CartX/PPU,P.y,p.z);}if(path){path.flow=this;path.RefreshBeams();}}
 public void RequestFace(int face){if(!TransitionBusy&&!PreviewBusy&&face!=ActiveFace&&ability&&ability.IsLight)StartCoroutine(CrossFace(face));}
 IEnumerator CrossFace(int nextFace){
  TransitionBusy=true;player.Drive(0,false);ability.Control(0,false);
  yield return FadeTo(.96f,.16f);ActiveFace=nextFace;SetVisibleFace(nextFace);
  yield return FadeTo(0,.22f);Crossings++;TransitionBusy=false;
 }
 IEnumerator FadeTo(float value,float duration){float initial=cameraRig?cameraRig.Veil:0;for(float t=0;t<duration;t+=Time.unscaledDeltaTime){if(cameraRig)cameraRig.Veil=Mathf.Lerp(initial,value,Mathf.Clamp01(t/duration));yield return null;}if(cameraRig)cameraRig.Veil=value;}
 IEnumerator PreviewOtherFace(int face,Vector2 focus){
  PreviewBusy=true;PreviewTarget=focus;player.Drive(0,false);ability.Control(0,false);
  var body=player.Body;var position=body.position;var velocity=body.velocity;bool simulated=body.simulated;
  body.velocity=Vector2.zero;body.simulated=false;player.gameObject.SetActive(false);
  yield return FadeTo(.8f,.16f);SetVisibleFace(face);yield return FadeTo(0,.18f);
  yield return new WaitForSeconds(1.15f);
  yield return FadeTo(.8f,.16f);SetVisibleFace(ActiveFace);body.position=position;player.transform.position=new Vector3(position.x,position.y,player.transform.position.z);
  player.gameObject.SetActive(true);body.simulated=simulated;body.velocity=velocity;
  yield return FadeTo(0,.18f);PreviewBusy=false;
 }
 void ApplyMechanisms(){
  if(frontSteps!=null)foreach(var t in frontSteps)if(t){t.gameObject.SetActive(StepsOpen);t.localScale=new Vector3(t.localScale.x,Mathf.Lerp(.04f,1,stepBlend),t.localScale.z);}
  if(frontStepColliders!=null)foreach(var c in frontStepColliders)if(c)c.enabled=StepsOpen&&stepBlend>.999f;
  if(bridge){bridge.gameObject.SetActive(BridgeOpen);bridge.localScale=new Vector3(bridge.localScale.x,Mathf.Lerp(.04f,1,bridgeBlend),bridge.localScale.z);}
  if(bridgeCollider)bridgeCollider.enabled=BridgeOpen&&bridgeBlend>.999f;
  if(screen)screen.localScale=new Vector3(screen.localScale.x,Mathf.Lerp(1,.04f,bridgeBlend),screen.localScale.z);
  if(screenBlock)screenBlock.enabled=!(BridgeOpen&&bridgeBlend>.999f);
  if(rootGate)rootGate.localScale=new Vector3(rootGate.localScale.x,Awakened?.04f:1,rootGate.localScale.z);
  if(exitBlock)exitBlock.enabled=!Awakened;
  if(scaleGlow)scaleGlow.enabled=Awakened||exposure>.1f;
  if(scaleSurface){if(scaleProps==null)scaleProps=new MaterialPropertyBlock();scaleSurface.GetPropertyBlock(scaleProps);scaleProps.SetColor("_Color",Awakened?new Color(1,.82f,.35f):new Color(.57f,.61f,.62f));scaleSurface.SetPropertyBlock(scaleProps);}
 }
 void AnimateMechanisms(){stepBlend=Mathf.MoveTowards(stepBlend,StepsOpen?1:0,Time.deltaTime*1.1f);bridgeBlend=Mathf.MoveTowards(bridgeBlend,BridgeOpen?1:0,Time.deltaTime*1.1f);ApplyMechanisms();}
 bool Near(Transform t,float distance=1.3f)=>t&&Vector2.Distance(player.Body.position,t.position)<distance;
 void BeginGrip(){
  Grabbing=true;gripOffset=Mathf.Clamp(PixelX-CartX,-65,65);player.ExternalMotion=true;player.Drive(0,false);ability.Control(0,false);
  player.Body.velocity=Vector2.zero;player.Body.bodyType=RigidbodyType2D.Kinematic;
 }
 void EndGrip(){
  if(!Grabbing)return;Grabbing=false;float previous=CartX;
  if(Mathf.Abs(CartX-480)<12)CartX=480;else if(BridgeOpen&&Mathf.Abs(CartX-1455)<12)CartX=1455;
  var p=player.Body.position;p.x+=(CartX-previous)/PPU;player.Body.position=p;player.transform.position=new Vector3(p.x,p.y,player.transform.position.z);
  player.Body.bodyType=RigidbodyType2D.Dynamic;player.ExternalMotion=false;player.Drive(0,false);player.Body.velocity=Vector2.zero;RefreshOptics();
 }
 void PushCart(float direction){
  float previous=CartX;CartX=Mathf.Clamp(CartX+direction*120*Time.deltaTime,480,BridgeOpen?1455:930);
  var feet=Px(CartX+gripOffset,709,player.transform.position.z);player.Body.position=feet;player.transform.position=feet;player.Body.velocity=Vector2.zero;
  if(Mathf.Abs(direction)>.05f&&player.visual)player.visual.localRotation=Quaternion.Euler(0,direction>0?22:158,0);
  if(cartWheels!=null)foreach(var w in cartWheels)if(w)w.Rotate(0,0,-(CartX-previous)*5);
  RefreshOptics();
 }
 public void Respawn(){
  EndGrip();Deaths++;ability.ResetAt(Px(ActiveFace==0?285:340,709,-1));SetVisibleFace(ActiveFace);RefreshOptics();
 }
 void Update(){
  if(GameSession.InputBlocked){player.Drive(0,false);if(ability)ability.Control(0,false);return;}

  var i=Testing?testInput:new ReservoirInput{move=Input.GetAxisRaw("Horizontal"),jump=Input.GetKeyDown(KeyCode.Space),interact=Input.GetKeyDown(KeyCode.E),overview=Input.GetKeyDown(KeyCode.Tab),restart=Input.GetKeyDown(KeyCode.R)};testInput=new ReservoirInput();
  if(i.restart){SceneFade.Load("SeventhTrellis");return;}
  AnimateMechanisms();if(i.overview&&!TransitionBusy&&!PreviewBusy)cameraRig.Toggle();
  if(i.jump&&!cameraRig.IntroFinished){cameraRig.Skip();i.jump=false;}
  if(!Awakened){if(StepsOpen&&BridgeOpen&&DockIndex==1&&MechanismsSettled){exposure+=Time.deltaTime;if(exposure>=2)Awakened=true;}else exposure=0;}
  if(Completed||TransitionBusy||PreviewBusy||SceneFade.Busy||!cameraRig.IntroFinished){player.Drive(0,false);ability.Control(0,false);return;}
  if(PixelY>884){Respawn();return;}
  if(ability.IsLight){player.Drive(0,false);ability.Control(i.move,false,i.interact);return;}
  if(Grabbing){if(i.interact){EndGrip();return;}PushCart(i.move);ability.Control(0,false);return;}
  if(i.interact&&player.Grounded&&MechanismsSettled){
   if(ActiveFace==1&&!StepsOpen&&Near(switches[1])){StepsOpen=true;BackActivations++;StartCoroutine(PreviewOtherFace(0,Px(850,530)));return;}
   if(ActiveFace==0&&StepsOpen&&!BridgeOpen&&Near(switches[0])){BridgeOpen=true;FrontActivations++;StartCoroutine(PreviewOtherFace(1,Px(1170,710)));return;}
   if(ActiveFace==1&&Mathf.Abs(PixelX-CartX)<90&&Mathf.Abs(PixelY-710)<12){BeginGrip();return;}
  }
  bool free=MechanismsSettled;player.Drive(free?i.move:0,free&&i.jump);ability.Control(i.move,free);
  if(ActiveFace==0&&Awakened&&player.Grounded&&PixelX>1710&&Mathf.Abs(PixelY-710)<12)Completed=true;
 }
 public string Hint(){
  if(!cameraRig.IntroFinished)return "沿用一束光，在藤架前后寻找彼此相连的路。空格跳过全景。";
  if(Completed)return "第七关完成 · 通往雨槽屋脊的叶幕已舒展。";
  if(TransitionBusy)return "沿镜窗穿过藤架纵深……";
  if(PreviewBusy)return ActiveFace==1?"背面轴轮转动，正面的育苗踏阶正在展开。":"正面轴轮转动，背面的轨桥与挡叶正在打开。";
  if(ability.IsLight)return "A / D 沿同一光路前后移动 · 穿过镜窗切换两面 · E 在安全地面恢复实体";
  if(Grabbing)return BridgeOpen?"A / D 推拉镜车 · E 松手 · 对齐镜窗的停靠槽":"A / D 推拉镜车 · E 松手 · 断轨上的挡块阻止镜车继续前进";
  if(ActiveFace==1&&Near(cart,1.8f))return "E 握住镜车 · 它的停靠位置决定光从哪一扇镜窗返回正面。";
  if(ActiveFace==1&&!StepsOpen&&Near(switches[1]))return "E 转动背面轴轮；藤绳通向另一面的育苗架。";
  if(ActiveFace==0&&StepsOpen&&!BridgeOpen&&Near(switches[0]))return "E 转动正面轴轮；连杆穿过叶墙，连接背面的断轨。";
  return ActiveFace==0?"正面 · 叶幕走廊。实体攀上搁板，光核沿镜窗穿行。":"背面 · 藤架检修侧。观察轨道、镜窗与穿架的机关连杆。";
 }
 void OnGUI(){if(GameSession.Instance)return;
  if(!uiFont)uiFont=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei UI","Microsoft YaHei","Arial"},20);
  var style=new GUIStyle(GUI.skin.box){font=uiFont,fontSize=18,wordWrap=true};
  GUI.Box(new Rect(20,16,500,38),"07  叶幕走廊 · "+(VisibleFace==0?"正面":"背面")+" · 两面互相开路",style);
  GUI.Box(new Rect(20,Screen.height-78,Screen.width-40,60),Hint()+"\nA/D 移动  空格跳跃  E 交互  Tab 全景  R 重来",style);
 }
}
