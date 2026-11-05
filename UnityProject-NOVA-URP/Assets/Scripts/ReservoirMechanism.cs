using UnityEngine;

// Level-local mechanism. The shared three-second charge rule and manual light travel remain in effect.
[DefaultExecutionOrder(-100)]
public class ReservoirMechanism : MonoBehaviour {
 public const float PixelsPerUnit=80f,CanvasHeight=846f;
 public static Vector3 Px(float x,float y,float z=0){return new Vector3(x/PixelsPerUnit,(CanvasHeight-y)/PixelsPerUnit,z);}
 public PlayerMotor player;public LightFormAbility ability;
 public LightPath entryPath,crossingPath;
 public Transform guide,seed,shutter,plate,mirror;
 public Transform guideHandle,seedPushPoint,mirrorHandle,feedingPoint,entryLanding,rightLanding;
 public Collider2D feedingTarget;public LayerMask opticalMask=1<<11;
 // Beam arrays may contain a soft halo and a narrow core. They all follow actual effective light.
 public LineRenderer[] mainBeams=new LineRenderer[0],feedingBeams=new LineRenderer[0];
 public LineRenderer shutterRope;public Transform shutterRopeTop;public Vector3 shutterRopeOffset=Vector3.zero;public Transform[] pulleys=new Transform[0];
 public Renderer scaleArt,scaleGlow;public ParticleSystem awakening;
 public Vector3[] seedRoute;public float seedTravelSeconds=2.6f;
 public float guideDownAngle=165f,shutterTravel=1.05f,requiredExposure=2f;
 public float initialMirrorAngle=-12f,mirrorVisualAngleAtZero=0f,mirrorTurnSpeed=30f;
 public float seedContactDistance=.68f;
 public bool GuideDown {get;private set;}public bool SeedSettled {get;private set;}
 public bool SeedMoving {get;private set;}public bool PlateHeld {get;private set;}
 public bool GateOpen {get;private set;}public bool StableLight=>SeedSettled&&!GuideDown;
 public bool Aligned {get;private set;}public bool Awakened {get;private set;}
 public float Exposure {get;private set;}public float MirrorAngle {get;private set;}
 public float SeedPushProgress {get;private set;}public int GuideToggleCount {get;private set;}
 public int SeedDeliveryCount {get;private set;}public int WeightLossCount {get;private set;}
 public float MirrorInput {get;set;}public float SeedPushInput {get;set;}
 public string LastAction {get;private set;}
 Vector3 guideStart,seedStart,shutterStart,plateStart,scaleSize;Quaternion guideRotation,mirrorRotation;
 Vector3[] rollRoute;float seedTime,gateVisual;bool initialized,previousHeld;MaterialPropertyBlock block;
 public Vector2 ReflectedDirection {get;private set;}
 public Vector3 BeamEnd {get;private set;}
 public float TargetMirrorAngle=>.5f*Mathf.Atan2(feedingPoint.position.y-Px(1490,468).y,feedingPoint.position.x-Px(1490,468).x)*Mathf.Rad2Deg;

 void Awake(){Initialize();}
 void Initialize(){if(initialized)return;initialized=true;block=new MaterialPropertyBlock();MirrorAngle=initialMirrorAngle;
  if(guide){guideStart=guide.localPosition;guideRotation=guide.localRotation;}if(seed)seedStart=seed.position;
  if(shutter)shutterStart=shutter.localPosition;if(plate)plateStart=plate.localPosition;
  if(mirror)mirrorRotation=mirror.localRotation;if(scaleArt)scaleSize=scaleArt.transform.localScale;
  if(entryPath){entryPath.nodes=new[]{Px(270,468),Px(710,468)};entryPath.safeEnd=entryLanding;entryPath.chargeRadius=.43f;entryPath.enabled=true;}
  if(crossingPath){crossingPath.nodes=new[]{Px(710,468),Px(1090,468)};crossingPath.safeEnd=null;crossingPath.chargeRadius=.43f;crossingPath.enabled=true;}
 }
 public bool ToggleGuide(){if(SeedMoving){LastAction="种荚正在滑落，等它落稳。";return false;}GuideDown=!GuideDown;GuideToggleCount++;LastAction=GuideDown?"叶槽接好了，却挡住了光。":"叶槽收起，光能继续向前。";return true;}
 public bool CanPush=>GuideDown&&!SeedSettled&&!SeedMoving;
 public bool SeedContact=>CanPush&&player&&seed&&ability&&!ability.IsLight&&player.Grounded&&player.transform.position.x-seed.position.x>=.10f&&player.transform.position.x-seed.position.x<=seedContactDistance&&Mathf.Abs(player.transform.position.y-Px(990,237).y)<.22f;
 public const float SeedPushSpeed=.375f;
 void FixedUpdate(){
  if(!initialized)Initialize();
  UpdateSeed();
  bool standing=player&&ability&&!ability.IsLight&&player.Grounded&&player.transform.position.x>=840/80f&&player.transform.position.x<=1015/80f&&Mathf.Abs(player.transform.position.y-Px(950,558).y)<.17f;
  PlateHeld=SeedSettled||standing;GateOpen=PlateHeld;
  if(previousHeld&&!PlateHeld)WeightLossCount++;previousHeld=PlateHeld;
  if(Mathf.Abs(MirrorInput)>.01f)MirrorAngle=Mathf.Clamp(MirrorAngle+MirrorInput*mirrorTurnSpeed*Time.fixedDeltaTime,-24,48);
  if(mirror)mirror.localRotation=mirrorRotation*Quaternion.Euler(0,0,MirrorAngle-mirrorVisualAngleAtZero);
  UpdateLight();
 }
 void UpdateSeed(){
  if(!seed)return;
  if(SeedMoving){seedTime+=Time.fixedDeltaTime;float t=Mathf.Clamp01(seedTime/Mathf.Max(.1f,seedTravelSeconds));seed.position=AlongRoute(rollRoute,t);seed.Rotate(0,0,Time.fixedDeltaTime*160,Space.Self);
   if(t>=1){SeedMoving=false;SeedSettled=true;SeedDeliveryCount++;LastAction="种荚留下了重量。叶槽还挡着光。";}return;}
  if(!CanPush||SeedPushInput>=-.05f||!player||!player.Grounded||ability.IsLight)return;
  // A held input only moves the seed when the entity is actually beside it on the upper shelf.
  if(!SeedContact)return;
  LastAction="种荚在你的掌下缓缓滚动。";
  SeedPushProgress=Mathf.Clamp01(SeedPushProgress+(-SeedPushInput)*Time.fixedDeltaTime*.75f);
  seed.position=seedStart+Vector3.left*(SeedPushProgress*.50f);
  seed.Rotate(0,0,Time.fixedDeltaTime*100,Space.Self);
  if(SeedPushProgress>=1){SeedMoving=true;seedTime=0;
   var route=seedRoute!=null&&seedRoute.Length>0?seedRoute:new[]{Px(850,300,-.8f),Px(830,430,-.8f),Px(810,480,-.8f),Px(775,510,-.8f)};
   rollRoute=new Vector3[route.Length+1];rollRoute[0]=seed.position;for(int i=0;i<route.Length;i++)rollRoute[i+1]=route[i];LastAction="种荚沿叶槽滚向压板。";
  }
 }
 static Vector3 AlongRoute(Vector3[] route,float t){float total=0;for(int i=1;i<route.Length;i++)total+=Vector3.Distance(route[i-1],route[i]);float d=t*total;
  for(int i=1;i<route.Length;i++){float n=Vector3.Distance(route[i-1],route[i]);if(d<=n)return Vector3.Lerp(route[i-1],route[i],n>.0001f?d/n:0);d-=n;}return route[route.Length-1];}
 void UpdateLight(){
  Vector3 start=Px(270,468,-2.2f);float endX=GuideDown?830:(GateOpen?1490:1090);BeamEnd=Px(endX,468,-2.2f);
  foreach(var b in mainBeams)SetLine(b,start,BeamEnd,true);
  if(crossingPath){
   crossingPath.enabled=true;crossingPath.nodes=new[]{Px(710,468),BeamEnd};
   crossingPath.safeEnd=GateOpen&&!GuideDown?rightLanding:null;
   // If a previously valid downstream segment disappears, recover to the dry island.
   // Normal trial transformation occurs upstream and remains in light, freely reversible.
   if(ability&&ability.IsLight&&ability.CurrentPath==crossingPath&&player.transform.position.x>BeamEnd.x+.04f&&entryLanding)ability.ResetAt(entryLanding.position);
  }
  Aligned=false;Vector2 origin=Px(1490,468);float angle=MirrorAngle*Mathf.Deg2Rad;
  Vector2 normal=new Vector2(-Mathf.Sin(angle),Mathf.Cos(angle));ReflectedDirection=Vector2.Reflect(Vector2.right,normal).normalized;
  Vector2 end=origin+ReflectedDirection*8;bool powered=GateOpen&&!GuideDown;
  if(powered){var hit=Physics2D.Raycast(origin+ReflectedDirection*.05f,ReflectedDirection,8,opticalMask);if(hit.collider){end=hit.point;Aligned=hit.collider==feedingTarget;}}
  foreach(var b in feedingBeams)SetLine(b,new Vector3(origin.x,origin.y,-2.2f),new Vector3(end.x,end.y,-2.2f),powered);
  if(Aligned)Exposure=Mathf.Min(requiredExposure,Exposure+Time.fixedDeltaTime);else if(!Awakened)Exposure=0;
  if(!Awakened&&Exposure>=requiredExposure){Awakened=true;LastAction="一枚暗淡的鳞片，重新有了呼吸。";if(awakening)awakening.Play();}
 }
 static void SetLine(LineRenderer b,Vector3 a,Vector3 c,bool active){if(!b)return;b.enabled=active;b.positionCount=2;b.SetPosition(0,a);b.SetPosition(1,c);}
 void LateUpdate(){
  if(!initialized)return;float t=1-Mathf.Exp(-Time.deltaTime*9);
  if(guide){guide.localPosition=guideStart;guide.localRotation=Quaternion.Slerp(guide.localRotation,guideRotation*Quaternion.Euler(0,0,GuideDown?guideDownAngle:0),t);}
  float oldGate=gateVisual;gateVisual=Mathf.Lerp(gateVisual,GateOpen?1:0,t);
  if(shutter)shutter.localPosition=shutterStart+Vector3.down*shutterTravel*(1-gateVisual);
  if(plate)plate.localPosition=Vector3.Lerp(plate.localPosition,plateStart+Vector3.down*(PlateHeld?.035f:0),t);
  foreach(var p in pulleys)if(p)p.Rotate(0,0,(gateVisual-oldGate)*130,Space.Self);
  if(shutterRope&&shutter){shutterRope.positionCount=2;shutterRope.SetPosition(0,shutterRopeTop?shutterRopeTop.position:Px(1135,115,-.5f));shutterRope.SetPosition(1,shutter.position+shutterRopeOffset);}
  if(scaleArt){block.Clear();block.SetFloat("_Exposure",Awakened?1:Exposure/requiredExposure);scaleArt.SetPropertyBlock(block);if(Awakened)scaleArt.transform.localScale=scaleSize*(1+Mathf.Sin(Time.time*1.8f)*.008f);}
  if(scaleGlow){block.Clear();block.SetColor("_Color",new Color(1,.78f,.33f,Awakened?.35f+Mathf.Sin(Time.time*2)*.035f:Exposure/requiredExposure*.3f));scaleGlow.SetPropertyBlock(block);}
 }
}
