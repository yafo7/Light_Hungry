using UnityEngine;

[DefaultExecutionOrder(-20)]
public class PlayerMotor : MonoBehaviour {
 public Transform visual,hairEmitter; public float speed=5f,jumpSpeed=9f; public LayerMask groundMask;
 public bool Grounded {get;private set;} public int JumpCount {get;private set;} public float MaximumY {get;private set;}
 public Rigidbody2D Body {get;private set;} public bool ExternalMotion;public float LightCharge;public bool movingSurfaceSupport;float axis,lastGround=-10,jumpQueued=-10,phase,facing=1;MaterialPropertyBlock props;
 public float MoveInput=>axis;
 public float Facing=>facing;
 Renderer[] renderers;bool rigged;
 void Awake(){Body=GetComponent<Rigidbody2D>();props=new MaterialPropertyBlock();renderers=visual.GetComponentsInChildren<Renderer>();rigged=visual.GetComponent<WickRigAnimation>()!=null;MaximumY=transform.position.y;if(hairEmitter)hairEmitter.gameObject.AddComponent<WickFlameTrail>().Initialize(this);}
 public void Drive(float horizontal,bool jump){axis=horizontal;if(jump)jumpQueued=Time.time;}
 public void WarpTo(Vector3 p){axis=0;jumpQueued=lastGround=-10;Grounded=false;Body.position=p;transform.position=p;Body.velocity=Vector2.zero;}
 void FixedUpdate(){
  if(ExternalMotion){Grounded=false;jumpQueued=-10;lastGround=-10;return;}
  var support=Physics2D.OverlapBox((Vector2)transform.position+new Vector2(0,0.03f),new Vector2(.32f,.1f),0,groundMask);Grounded=support!=null;
  if(Grounded)lastGround=Time.time;
  var vel=Body.velocity;vel.x=axis*speed;
  if(movingSurfaceSupport&&support){var surface=support.GetComponentInParent<DrainMovingSurface>();if(surface)vel.x+=surface.Velocity.x;}
  if(Time.time-jumpQueued<.14f&&Time.time-lastGround<.12f){vel.y=jumpSpeed;jumpQueued=-10;lastGround=-10;Grounded=false;JumpCount++;}
  Body.velocity=vel;MaximumY=Mathf.Max(MaximumY,transform.position.y);
 }
 void LateUpdate(){
  if(ExternalMotion)return;
  if(Mathf.Abs(axis)>.05f)facing=Mathf.Sign(axis);float m=Mathf.Clamp01(Mathf.Abs(Body.velocity.x)/speed);phase+=Time.deltaTime*10*m;
  // The supplied mesh's forward direction is +X (confirmed by four rendered views).
  visual.localRotation=Quaternion.Slerp(visual.localRotation,Quaternion.Euler(0,facing>0?22:158,Mathf.Sin(phase)*m*2),Time.deltaTime*10);
  if(hairEmitter)hairEmitter.localPosition=new Vector3(-facing*.20f,.85f,-.15f);
  visual.localPosition=new Vector3(0,Grounded?Mathf.Abs(Mathf.Sin(phase))*.025f*m:0,0);
  props.SetMatrix("_SpiritWorldToLocal",visual.worldToLocalMatrix);props.SetFloat("_Stride",phase);props.SetFloat("_Move",rigged?0:m);props.SetFloat("_LightCharge",LightCharge);foreach(var r in renderers)r.SetPropertyBlock(props);
 }
}
