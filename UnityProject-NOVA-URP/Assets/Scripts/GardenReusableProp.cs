using UnityEngine;

// Independent art prefab metadata. Mirror head and base are separate objects so
// future levels can reuse the prefab without rotating its mounting hardware.
public class GardenReusableProp:MonoBehaviour {
 public string kind;
 public Transform movingPart;
 public void SetMirrorPlane(Vector3 incomingPoint,Vector3 opticalPoint,Vector3 outgoingPoint){
  if(!movingPart)return;
  Vector2 incoming=((Vector2)(opticalPoint-incomingPoint)).normalized;
  Vector2 outgoing=((Vector2)(outgoingPoint-opticalPoint)).normalized;
  Vector2 tangent=incoming+outgoing;
  if(tangent.sqrMagnitude<.0001f)tangent=Vector2.Perpendicular(incoming);
  // Source mirror-head sprite is upright; its long axis is local Y.
  movingPart.rotation=Quaternion.Euler(0,0,Mathf.Atan2(tangent.y,tangent.x)*Mathf.Rad2Deg-90);
 }
}
