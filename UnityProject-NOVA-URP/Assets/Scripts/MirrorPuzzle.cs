using UnityEngine;

public class MirrorPuzzle : MonoBehaviour {
 public Transform mirror,source,target;public Collider mirrorCollider,targetCollider;public LineRenderer incoming,outgoing,core;
 public TailLife tail;public float yaw=-22,rotationSpeed=22,requiredExposure=2.2f;public LayerMask opticalMask;
 public bool Hitting {get;private set;}public bool Solved {get;private set;}public float Exposure {get;private set;}public int HitFrames {get;private set;}
 public Vector3 ReflectedDirection {get;private set;}Quaternion solution;bool initialized;
 void Start(){Initialize();}
 public void Initialize(){if(initialized)return;Vector3 a=(mirror.position-source.position).normalized,b=(target.position-mirror.position).normalized;solution=Quaternion.FromToRotation(Vector3.back,(a-b).normalized);initialized=true;ApplyRotation();}
 public void Turn(float direction){yaw=Mathf.Clamp(yaw+direction*rotationSpeed*Time.deltaTime,-38,38);ApplyRotation();}
 void ApplyRotation(){mirror.rotation=Quaternion.AngleAxis(yaw,Vector3.up)*solution;}
 public bool RayHits(float testYaw){Initialize();float old=yaw;yaw=testYaw;ApplyRotation();Physics.SyncTransforms();bool result=Cast(out _);yaw=old;ApplyRotation();Physics.SyncTransforms();return result;}
 bool Cast(out Vector3 end){
  Vector3 dir=(mirror.position-source.position).normalized;RaycastHit first;
  if(!Physics.Raycast(source.position,dir,out first,100,opticalMask)||first.collider!=mirrorCollider){end=mirror.position;return false;}
  ReflectedDirection=Vector3.Reflect(dir,first.normal).normalized;RaycastHit hit;
  if(Physics.Raycast(first.point+ReflectedDirection*.03f,ReflectedDirection,out hit,65,opticalMask)){end=hit.point;return hit.collider==targetCollider;}
  float length=40;if(ReflectedDirection.y<-.001f)length=Mathf.Min(length,(6.03f-first.point.y)/ReflectedDirection.y);
  end=first.point+ReflectedDirection*Mathf.Max(.1f,length);return false;
 }
 void Update(){
  ApplyRotation();Physics.SyncTransforms();Hitting=Cast(out Vector3 end);incoming.SetPosition(0,source.position);incoming.SetPosition(1,mirror.position);
  outgoing.SetPosition(0,mirror.position);outgoing.SetPosition(1,end);core.SetPosition(0,mirror.position);core.SetPosition(1,end);
  if(Hitting){HitFrames++;Exposure=Mathf.Min(requiredExposure,Exposure+Time.deltaTime);}else if(!Solved)Exposure=Mathf.Max(0,Exposure-Time.deltaTime*.7f);
  tail.Illuminate(Exposure/requiredExposure);if(!Solved&&Exposure>=requiredExposure){Solved=true;tail.Wake();}
 }
}
