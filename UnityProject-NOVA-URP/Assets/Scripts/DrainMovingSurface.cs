using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Rigidbody2D))]
public class DrainMovingSurface : MonoBehaviour {
 public bool ferry;public Vector2 start,end;public float period=7;
 public Vector2 Velocity {get;private set;}public Rigidbody2D Body {get;private set;}
 public Vector2 Target {get;set;}float phase;
 void Awake(){Body=GetComponent<Rigidbody2D>();Target=Body.position;}
 void FixedUpdate(){
  if(ferry){phase+=Time.fixedDeltaTime;float t=.5f-.5f*Mathf.Cos(phase*Mathf.PI*2/period);Target=Vector2.Lerp(start,end,t);}
  Velocity=(Target-Body.position)/Time.fixedDeltaTime;Body.MovePosition(Target);
 }
}
