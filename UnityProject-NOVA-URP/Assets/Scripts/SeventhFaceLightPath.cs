using UnityEngine;

// ONE ordered optical route. Depth segments have real traversal distance but
// identical XY endpoints. Only the active face's local spans charge/render.
public class SeventhFaceLightPath:LightPath {
 public SeventhTrellisFlow flow;
 public LineRenderer[] frontSourceBeams=new LineRenderer[0],rearBeams=new LineRenderer[0],frontReturnBeams=new LineRenderer[0];
 public const float DepthLength=1f;
 public float SourceLength=>Vector2.Distance(SeventhTrellisFlow.Source,SeventhTrellisFlow.P);
 public float RearStart=>SourceLength+DepthLength;
 public float RearEnd=>RearStart+Vector2.Distance(SeventhTrellisFlow.P,CartPoint);
 public float ReturnStart=>RearEnd+DepthLength;
 public int ReturnDock=>flow?flow.DockIndex:0;
 public bool HasReturnWindow=>ReturnDock>=0;
 public Vector2 CartPoint=>SeventhTrellisFlow.Px(flow?flow.CartX:480,670);
 public Vector2 FrontEnd=>ReturnDock==1?SeventhTrellisFlow.Target:SeventhTrellisFlow.R0Stop;
 public override float Length=>HasReturnWindow?ReturnStart+Vector2.Distance(CartPoint,FrontEnd):RearEnd;
 public override Vector2 At(float distance){
  float d=Mathf.Clamp(distance,0,Length);
  if(d<=SourceLength)return Vector2.Lerp(SeventhTrellisFlow.Source,SeventhTrellisFlow.P,d/SourceLength);
  if(d<=RearStart)return SeventhTrellisFlow.P;
  if(d<=RearEnd)return Vector2.Lerp(SeventhTrellisFlow.P,CartPoint,Mathf.InverseLerp(RearStart,RearEnd,d));
  if(d<=ReturnStart)return CartPoint;
  return Vector2.Lerp(CartPoint,FrontEnd,Mathf.InverseLerp(ReturnStart,Length,d));
 }
 int FaceAt(float d){if(d<SourceLength+DepthLength*.5f)return 0;if(!HasReturnWindow||d<RearEnd+DepthLength*.5f)return 1;return 0;}
 public bool InDepthSegment(float d)=>d>SourceLength+.001f&&d<RearStart-.001f||HasReturnWindow&&d>RearEnd+.001f&&d<ReturnStart-.001f;
 public override bool BeforeTravel(float from,float to){
  if(!flow||flow.TransitionBusy||flow.PreviewBusy)return false;
  int desired=FaceAt(to);if(flow.ActiveFace!=desired){flow.RequestFace(desired);return false;}return true;
 }
 public override bool CanRestore(float along)=>flow&&!flow.TransitionBusy&&!flow.PreviewBusy&&!InDepthSegment(along);
 public override float Closest(Vector2 p,out float along,out Vector2 point){return ClosestOnFace(flow?flow.ActiveFace:0,p,out along,out point);}
 public float GetAlong(int face,Vector2 point){float a;Vector2 q;ClosestOnFace(face,point,out a,out q);return a;}
 public float ClosestOnFace(int face,Vector2 p,out float along,out Vector2 point){
  along=0;point=p;float best=float.MaxValue;
  if(face==0){SegmentClosest(p,SeventhTrellisFlow.Source,SeventhTrellisFlow.P,0,ref best,ref along,ref point);if(HasReturnWindow)SegmentClosest(p,CartPoint,FrontEnd,ReturnStart,ref best,ref along,ref point);}
  else SegmentClosest(p,SeventhTrellisFlow.P,CartPoint,RearStart,ref best,ref along,ref point);
  return best;
 }
 static void SegmentClosest(Vector2 p,Vector2 a,Vector2 b,float offset,ref float best,ref float along,ref Vector2 point){var v=b-a;float len=v.magnitude;float t=len>.0001f?Mathf.Clamp01(Vector2.Dot(p-a,v)/(len*len)):0;Vector2 q=a+v*t;float d=Vector2.Distance(p,q);if(d<best){best=d;along=offset+t*len;point=q;}}
 protected override void LateUpdate(){RefreshBeams();}
 public void RefreshBeams(){
  int face=flow?flow.VisibleFace:0;
  Draw(frontSourceBeams,face==0,SeventhTrellisFlow.Source,SeventhTrellisFlow.P);
  Draw(rearBeams,face==1,SeventhTrellisFlow.P,CartPoint);
  Draw(frontReturnBeams,face==0&&HasReturnWindow,CartPoint,FrontEnd);
  nodes=HasReturnWindow?new[]{SeventhTrellisFlow.Source,SeventhTrellisFlow.P,(Vector3)CartPoint,(Vector3)FrontEnd}:new[]{SeventhTrellisFlow.Source,SeventhTrellisFlow.P,(Vector3)CartPoint};
 }
 static void Draw(LineRenderer[] renderers,bool visible,Vector2 a,Vector2 b){if(renderers==null)return;foreach(var r in renderers){if(!r)continue;r.enabled=visible;r.positionCount=2;r.SetPosition(0,new Vector3(a.x,a.y,-2.2f));r.SetPosition(1,new Vector3(b.x,b.y,-2.2f));}}
}
