using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    [DefaultExecutionOrder(900)]
    public sealed class LightRouteReview:MonoBehaviour {
        public bool automatic=false;
        public float duration=7;
        public bool IsPlaying {get;private set;}
        public bool Played {get;private set;}
        public int RoutePointCount=>route.Count;
        public float PlaybackProgress {get;private set;}
        readonly List<Vector3> route=new List<Vector3>();readonly List<Renderer> hidden=new List<Renderer>();
        GameObject model,dot;LineRenderer line;Camera camera;Vector3 cameraPosition;Quaternion cameraRotation;float size,time,total,oldScale;
        LabSession session;
        Bounds modelBounds;
        void Start(){session=GetComponent<LabSession>();}
        void LateUpdate(){
            if(!IsPlaying){if(automatic&&!Played&&session&&session.goal&&session.goal.Satisfied&&session.beams.ReceivedPaths.TryGetValue(session.goal,out var points))Play(points);return;}
            time+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(time/duration);PlaybackProgress=t;
            Bounds bounds=modelBounds;Vector3 center=bounds.center;
            Quaternion target=Quaternion.Euler(30,40,0);float blend=Mathf.SmoothStep(0,1,Mathf.Clamp01(time/1.1f));
            camera.transform.SetPositionAndRotation(Vector3.Lerp(cameraPosition,center+target*(Vector3.back*40),blend),Quaternion.Slerp(cameraRotation,target,blend));camera.orthographicSize=Mathf.Lerp(size,Mathf.Max(10,bounds.size.magnitude*.45f),blend);
            float length=total*Mathf.Clamp01((time-1.1f)/Mathf.Max(.1f,duration-2));var visible=new List<Vector3>{route[0]};Vector3 head=route[0];
            for(int i=1;i<route.Count;i++){float segment=Vector3.Distance(route[i-1],route[i]);if(length<segment){head=Vector3.Lerp(route[i-1],route[i],segment>.001f?length/segment:1);visible.Add(head);break;}visible.Add(route[i]);head=route[i];length-=segment;}
            line.positionCount=visible.Count;line.SetPositions(visible.ToArray());dot.transform.position=head;
            if(t>=1)StopReview();
        }
        public bool Play(IList<Vector3> points){
            if(IsPlaying||points==null||points.Count<2)return false;
            route.Clear();route.AddRange(points);total=0;for(int i=1;i<route.Count;i++)total+=Vector3.Distance(route[i-1],route[i]);
            modelBounds=new Bounds(route[0],Vector3.zero);foreach(var point in route)modelBounds.Encapsulate(point);
            session=session?session:GetComponent<LabSession>();session.player.GetComponent<LightTravel>()?.Cancel();session.player.PrepareViewChange();
            camera=Camera.main;cameraPosition=camera.transform.position;cameraRotation=camera.transform.rotation;size=camera.orthographicSize;oldScale=Time.timeScale;Time.timeScale=0;
            model=new GameObject("Completed light route model");
            // Capture actual collision volumes rather than depending on final character/background art.
            var colliders=FindObjectsOfType<Collider>();
            foreach(var c in colliders){if(!c.enabled||c.GetComponentInParent<PlayerController>())continue;if(c.isTrigger&&!c.GetComponentInParent<SpaceMirror>()&&!c.GetComponentInParent<MirrorSurface>()&&!c.GetComponentInParent<BeastReceiver>())continue;if(c.bounds.size.y>15)continue;
                modelBounds.Encapsulate(c.bounds);
                var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Route model volume";g.transform.SetParent(model.transform);g.transform.position=c.bounds.center;g.transform.localScale=c.bounds.size;g.GetComponent<Collider>().enabled=false;Destroy(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=SpatialLabWorld.Active.solidMaterial;
                var tint=new MaterialPropertyBlock();tint.SetColor("_Color",c.isTrigger?new Color(.3f,.8f,.8f):new Color(.2f,.3f,.36f));g.GetComponent<Renderer>().SetPropertyBlock(tint);
            }
            foreach(var r in FindObjectsOfType<Renderer>())if(!r.transform.IsChildOf(model.transform)&&r.enabled){hidden.Add(r);r.enabled=false;}
            line=new GameObject("Successful light route").AddComponent<LineRenderer>();line.transform.SetParent(model.transform);BeamSystem.ConfigureLine(line,.12f,new Color(1,.85f,.3f));
            dot=GameObject.CreatePrimitive(PrimitiveType.Sphere);dot.name="Route replay light";dot.transform.SetParent(model.transform);dot.transform.localScale=Vector3.one*.3f;dot.GetComponent<Collider>().enabled=false;Destroy(dot.GetComponent<Collider>());dot.GetComponent<Renderer>().sharedMaterial=SpatialLabWorld.Active.actorMaterial;
            time=0;PlaybackProgress=0;IsPlaying=true;Played=true;return true;
        }
        public void StopReview(){if(!IsPlaying)return;IsPlaying=false;foreach(var r in hidden)if(r)r.enabled=true;hidden.Clear();if(model)Destroy(model);if(camera){camera.transform.SetPositionAndRotation(cameraPosition,cameraRotation);camera.orthographicSize=size;}Time.timeScale=session&&session.paused?0:oldScale;}
        void OnDestroy(){StopReview();}
    }
}
