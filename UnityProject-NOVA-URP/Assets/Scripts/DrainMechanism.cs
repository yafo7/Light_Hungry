using UnityEngine;

[DefaultExecutionOrder(-150)]
public class DrainMechanism : MonoBehaviour {
 public const float PixelsPerH=28,WaterPixel=542;
 public Transform mirror,source,rootSwitch,leftWinch,rightWinch,feedingPoint,wheel;
 public DrainMovingSurface counterweight;public LightPath path;public Collider2D opticalTarget;
 public Renderer bulbArt,scaleArt,bulbGlow,scaleGlow;public LineRenderer[] ropes;public ParticleSystem awakening;
 public LayerMask opticalMask;public float height,requiredExposure=2;
 public bool Powered {get;private set;}public bool Awakened {get;private set;}public float Exposure {get;private set;}
 public bool Aligned {get;private set;}public bool Middle=>height>.455f&&height<.545f;
 public float MirrorPixelY=>Mathf.Lerp(425,215,height);public float DeckPixelY=>740-MirrorPixelY;
 public int ActivationCount {get;private set;}public float WinchInput {get;set;}
 MaterialPropertyBlock props;Vector3 scaleBase;float scaleTime;
 public static Vector3 Px(float x,float y,float z=0){return new Vector3(x/PixelsPerH,(WaterPixel-y)/PixelsPerH,z);}
 void Awake(){props=new MaterialPropertyBlock();scaleBase=scaleArt.transform.localScale;path.enabled=false;foreach(var b in path.beams)b.enabled=false;}
 public void Activate(){if(Powered)return;Powered=true;ActivationCount++;path.enabled=true;foreach(var b in path.beams)b.enabled=true;}
 void FixedUpdate(){
  if(Powered&&Mathf.Abs(WinchInput)>.01f)height=Mathf.Clamp01(height+WinchInput*.23f*Time.fixedDeltaTime);
  // The middle detent is set by actual deck alignment; it is not a second independent device.
  if(Mathf.Abs(WinchInput)<.01f&&Mathf.Abs(height-.5f)<.025f)height=.5f;
  mirror.position=Px(736,MirrorPixelY,-.3f);counterweight.Target=Px(592,DeckPixelY,.5f);
  for(int i=0;i<ropes.Length;i++){float x=i==0?535:i==1?649:710;ropes[i].SetPositions(new[]{Px(x,112,-.6f),Px(x,i<2?DeckPixelY-20:MirrorPixelY-20,-.6f)});}
  Aligned=false;if(!Powered)return;
  Vector2 origin=mirror.position;var hit=Physics2D.Raycast(origin+Vector2.right*.12f,Vector2.right,60,opticalMask);
  Vector2 end=hit.collider?hit.point:origin+Vector2.right*60;
  path.nodes=new[]{source.position,mirror.position,new Vector3(end.x,end.y,-2.2f)};
  Aligned=hit.collider==opticalTarget;if(Aligned)Exposure=Mathf.Min(requiredExposure,Exposure+Time.fixedDeltaTime);else if(!Awakened)Exposure=0;
  if(!Awakened&&Exposure>=requiredExposure){Awakened=true;scaleTime=Time.time;if(awakening)awakening.Play();}
 }
 void LateUpdate(){
  Tint(bulbArt,Powered?Color.white:new Color(.25f,.31f,.34f,1));Tint(bulbGlow,new Color(1,.7f,.23f,Powered?.3f:0));
  props.Clear();props.SetFloat("_Exposure",Awakened?1:Exposure/requiredExposure);scaleArt.SetPropertyBlock(props);
  Tint(scaleGlow,new Color(1,.72f,.24f,Awakened?.52f+Mathf.Sin(Time.time*2)*.08f:Exposure/requiredExposure*.5f));
  if(Awakened)scaleArt.transform.localScale=scaleBase*(1+Mathf.Sin((Time.time-scaleTime)*1.9f)*.009f);
  if(wheel)wheel.Rotate(0,0,-Time.deltaTime*32,Space.Self);
 }
 void Tint(Renderer r,Color color){if(!r)return;props.Clear();props.SetColor("_Color",color);r.SetPropertyBlock(props);}
}
