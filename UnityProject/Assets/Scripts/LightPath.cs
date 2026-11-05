using System.Collections.Generic;
using UnityEngine;

// One shared polyline is used for visible rays, charging and constrained travel.
public class LightPath : MonoBehaviour {
 public static readonly List<LightPath> Active=new List<LightPath>();
 public Vector3[] nodes;public LineRenderer[] beams=new LineRenderer[0];public MirrorPuzzle mirrorSource;
 public Transform safeEnd;public float travelSpeed=3.8f,chargeRadius=.24f;
 public float Length {get {float n=0;if(nodes!=null)for(int i=1;i<nodes.Length;i++)n+=Vector2.Distance(nodes[i-1],nodes[i]);return n;}}
 void OnEnable(){if(!Active.Contains(this))Active.Add(this);}void OnDisable(){Active.Remove(this);}
 void LateUpdate(){
  if(mirrorSource&&mirrorSource.outgoing.positionCount>=2)nodes=new[]{mirrorSource.incoming.GetPosition(0),mirrorSource.outgoing.GetPosition(0),mirrorSource.outgoing.GetPosition(1)};
  if(nodes==null)return;foreach(var b in beams){if(!b)continue;b.positionCount=nodes.Length;var p=new Vector3[nodes.Length];for(int i=0;i<p.Length;i++)p[i]=new Vector3(nodes[i].x,nodes[i].y,-2.2f);b.SetPositions(p);}
 }
 public Vector2 At(float distance){if(nodes==null||nodes.Length==0)return transform.position;for(int i=1;i<nodes.Length;i++){Vector2 a=nodes[i-1],b=nodes[i];float len=Vector2.Distance(a,b);if(distance<=len)return Vector2.Lerp(a,b,len>0?distance/len:0);distance-=len;}return nodes[nodes.Length-1];}
 public float Closest(Vector2 p,out float along,out Vector2 point){float best=float.MaxValue,offset=0;along=0;point=p;if(nodes==null)return best;
  for(int i=1;i<nodes.Length;i++){Vector2 a=nodes[i-1],d=(Vector2)nodes[i]-a;float len=d.magnitude,t=len>0?Mathf.Clamp01(Vector2.Dot(p-a,d)/(len*len)):0;Vector2 q=a+d*t;float dist=Vector2.Distance(p,q);if(dist<best){best=dist;along=offset+t*len;point=q;}offset+=len;}return best;
 }
 public static bool FindAtBody(Vector2 feet,out LightPath path,out float distance,out Vector2 point){path=null;distance=0;point=feet;float best=float.MaxValue;
  foreach(var p in Active){if(!p||!p.isActiveAndEnabled)continue;for(int i=0;i<3;i++){float along;Vector2 q;float d=p.Closest(feet+Vector2.up*(.18f+i*.32f),out along,out q);if(d<p.chargeRadius&&d<best){best=d;path=p;distance=along;point=q;}}}return path;
 }
}
