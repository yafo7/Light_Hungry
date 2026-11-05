using UnityEngine;

[RequireComponent(typeof(PlayerMotor),typeof(Rigidbody2D),typeof(CapsuleCollider2D))]
public class LightFormAbility : MonoBehaviour {
 public float chargeSeconds=3,coreRadius=.04f;public LayerMask obstacles=1<<6;
 // Level four uses a continuous ray across consecutive shores: deliberate movement
 // rearms a later three-second charge without forcing the entity to leave that ray.
 // Opt-in keeps existing scenes' rearm behavior unchanged.
 public bool rearmOnMovement;
 // Optional entity-only glass surfaces: support restoration, but transmit the light mote.
 public LayerMask restoreSurfaces;
 // Opt-in safe dismount: reform at the ray and fall onto a nearby support.
 // Existing levels keep their original instant safe-ground restoration.
 public bool shortDropRestore;public float shortDropDistance=2.2f;
 public int ShortDropCount {get;private set;}
 public bool IsLight {get;private set;}public float Charge {get;private set;}public float Along {get;private set;}
 public int TransformCount {get;private set;}public int RestoreCount {get;private set;}public bool Blocked {get;private set;}
 public LightPath CurrentPath {get;private set;}public bool InValidLight {get;private set;}
 PlayerMotor motor;Rigidbody2D body;CapsuleCollider2D capsule;CircleCollider2D core;Renderer chargeHalo,orb,orbHalo;TrailRenderer trail;
 Vector3 anchor;float input,originalGravity;bool allowCharge=true,reformRequested,rearm=true;ParticleSystem[] sparks;MaterialPropertyBlock block;
 void Awake(){motor=GetComponent<PlayerMotor>();body=GetComponent<Rigidbody2D>();capsule=GetComponent<CapsuleCollider2D>();originalGravity=body.gravityScale;core=gameObject.AddComponent<CircleCollider2D>();core.radius=coreRadius;core.enabled=false;block=new MaterialPropertyBlock();
  chargeHalo=SpiritEffects.Halo("Charge_GrowingHalo",transform,new Vector3(0,.5f,-.5f),new Vector2(1.3f,1.6f),new Color(1,.73f,.25f,0));
  orbHalo=SpiritEffects.Halo("LightMote_Halo",transform,new Vector3(0,0,-.5f),new Vector2(.52f,.52f),new Color(1,.75f,.25f,.85f));
  orb=SpiritEffects.Halo("LightMote_Core",transform,new Vector3(0,0,-.6f),new Vector2(.12f,.12f),new Color(1,.98f,.8f,1));orb.enabled=orbHalo.enabled=false;
  var go=new GameObject("LightMote_Trail");go.transform.SetParent(transform,false);go.transform.localPosition=new Vector3(0,0,-.5f);trail=go.AddComponent<TrailRenderer>();trail.time=.26f;trail.minVertexDistance=.025f;trail.startWidth=.075f;trail.endWidth=0;trail.sharedMaterial=new Material(Shader.Find("LightFeast/SoftLight"));trail.sharedMaterial.SetColor("_Color",new Color(1,.78f,.3f,.8f));trail.emitting=false;
  sparks=motor.visual?GetComponentsInChildren<ParticleSystem>():new ParticleSystem[0];
 }
 public void Control(float direction,bool canCharge=true,bool reform=false){input=Mathf.Clamp(direction,-1,1);allowCharge=canCharge;reformRequested|=reform;}
 public void ResetAt(Vector3 feet){anchor=feet;if(IsLight)Restore(feet);Charge=0;CurrentPath=null;rearm=true;reformRequested=false;input=0;motor.LightCharge=0;motor.WarpTo(feet);UpdateGlow();}
 void FixedUpdate(){
  if(IsLight){if(!CurrentPath||!CurrentPath.isActiveAndEnabled){Restore(anchor);return;}
   Blocked=false;float next=Mathf.Clamp(Along+input*CurrentPath.travelSpeed*Time.fixedDeltaTime,0,CurrentPath.Length);
   // Substeps preserve path corners; a physical circle sweep prevents tunnelling through solids.
   float d=Along;Vector2 p=body.position;while(Mathf.Abs(next-d)>.00001f){float step=Mathf.MoveTowards(d,next,.025f);if(!CurrentPath.BeforeTravel(d,step))break;Vector2 q=CurrentPath.At(step),v=q-p;if(v.sqrMagnitude>.0000001f){var hit=Physics2D.CircleCast(p,coreRadius,v.normalized,v.magnitude,obstacles);if(hit.collider){Blocked=true;break;}}p=q;d=step;}
   Along=d;body.MovePosition(p);
   if(reformRequested){if(CurrentPath.CanRestore(Along))TryRestoreNear(p);reformRequested=false;}
   if(IsLight&&input>0&&Along>=CurrentPath.Length-.015f&&CurrentPath.CanRestore(Along)){if(CurrentPath.safeEnd)TryRestoreAt(CurrentPath.safeEnd.position);else TryRestoreNear(p);}
   return;
  }
  reformRequested=false;LightPath path;float distance;Vector2 point;InValidLight=LightPath.FindAtBody(body.position,out path,out distance,out point);
  if(!InValidLight||(rearmOnMovement&&Mathf.Abs(input)>.05f))rearm=true;
  if(allowCharge&&rearm&&InValidLight&&motor.Grounded&&Mathf.Abs(input)<.05f&&Mathf.Abs(body.velocity.y)<.2f){Charge+=Time.fixedDeltaTime;if(Charge+0.0001f>=chargeSeconds)Enter(path,distance,point);}
  else Charge=0;
  UpdateGlow();
 }
 void UpdateGlow(){float t=Mathf.Clamp01(Charge/chargeSeconds);motor.LightCharge=t;chargeHalo.transform.localScale=new Vector3(1.3f+t*1.4f,1.6f+t*1.6f,1);block.SetColor("_Color",new Color(1,.77f,.3f,t*.72f));chargeHalo.SetPropertyBlock(block);}
 void Enter(LightPath path,float distance,Vector2 point){
  if(Physics2D.OverlapCircle(point,coreRadius,obstacles)){Charge=0;return;}
  anchor=transform.position;CurrentPath=path;Along=distance;TransformCount++;IsLight=true;motor.Drive(0,false);motor.ExternalMotion=true;body.velocity=Vector2.zero;body.gravityScale=0;body.bodyType=RigidbodyType2D.Kinematic;body.useFullKinematicContacts=true;capsule.enabled=false;core.enabled=true;body.position=point;motor.visual.gameObject.SetActive(false);foreach(var ps in sparks)ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);chargeHalo.enabled=false;orb.enabled=orbHalo.enabled=true;trail.Clear();trail.emitting=true;
 }
 public bool SafeBodyAt(Vector2 feet){return !Physics2D.OverlapCapsule(feet+capsule.offset,capsule.size,CapsuleDirection2D.Vertical,0,obstacles.value|restoreSurfaces.value);}
 bool TryRestoreAt(Vector2 feet){if(!SafeBodyAt(feet))return false;Restore(new Vector3(feet.x,feet.y,anchor.z));return true;}
 bool TryRestoreNear(Vector2 p){
  int mask=obstacles.value|restoreSurfaces.value;
  var hit=Physics2D.Raycast(p+Vector2.up*.4f,Vector2.down,shortDropRestore?shortDropDistance+.4f:2.2f,mask);
  if(!hit.collider||hit.normal.y<=.5f)return false;
  Vector2 target=hit.point+Vector2.up*.035f;
  if(!shortDropRestore)return TryRestoreAt(target);
  if(!SafeBodyAt(target)||p.y-target.y>shortDropDistance)return false;
  // If the light is inside the entity's standing envelope, restore onto that
  // same surface. Otherwise the whole appearing capsule and its swept corridor
  // must be free before enabling normal gravity; this never tunnels through walls.
  Vector2 start=p;
  if(start.y<=target.y+.08f)return TryRestoreAt(target);
  if(!SafeBodyAt(start))return false;
  var sweep=Physics2D.CapsuleCast(start+capsule.offset,capsule.size,CapsuleDirection2D.Vertical,0,Vector2.down,Mathf.Max(0,start.y-target.y-.06f),mask);
  if(sweep.collider)return false;
  Restore(new Vector3(start.x,start.y,anchor.z));ShortDropCount++;return true;
 }
 void Restore(Vector3 feet){IsLight=false;RestoreCount++;core.enabled=false;capsule.enabled=true;body.bodyType=RigidbodyType2D.Dynamic;body.gravityScale=originalGravity;body.velocity=Vector2.zero;body.position=feet;transform.position=new Vector3(feet.x,feet.y,anchor.z);motor.ExternalMotion=false;motor.Drive(0,false);motor.visual.gameObject.SetActive(true);foreach(var ps in sparks)ps.Play();orb.enabled=orbHalo.enabled=false;chargeHalo.enabled=true;trail.emitting=false;trail.Clear();Charge=0;rearm=false;CurrentPath=null;UpdateGlow();}
}
