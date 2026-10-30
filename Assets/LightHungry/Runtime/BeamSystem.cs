using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    [DefaultExecutionOrder(500)]
    public sealed class BeamSystem:MonoBehaviour {
        public LayerMask opticalMask;public int bounceLimit=12;public float maxDistance=60;
        readonly List<LineRenderer> lines=new List<LineRenderer>();int used;
        static Material beamMaterial;
        public Material beamStyle;
        void Awake(){if(beamStyle)beamMaterial=beamStyle;}
        public int SegmentsLastFrame {get;private set;}
        public readonly List<string> LastPaths=new List<string>();
        public readonly Dictionary<BeastReceiver,List<Vector3>> ReceivedPaths=new Dictionary<BeastReceiver,List<Vector3>>();
        public static void ConfigureLine(LineRenderer line,float width,Color color){
            if(!beamMaterial)beamMaterial=new Material(Shader.Find("LightHungry/WatercolorBeam")??Shader.Find("Sprites/Default"));
            line.sharedMaterial=beamMaterial;line.startWidth=line.endWidth=width;line.startColor=line.endColor=color;
            line.useWorldSpace=true;line.sortingOrder=25;line.numCapVertices=4;
        }
        void LateUpdate(){
            if(LabSession.Instance&&LabSession.Instance.GetComponent<LightRouteReview>()&&LabSession.Instance.GetComponent<LightRouteReview>().IsPlaying)return;
            Physics2D.SyncTransforms();used=0;LastPaths.Clear();
            ReceivedPaths.Clear();var exposures=FindObjectsOfType<LightExposure>();foreach(var exposure in exposures)exposure.BeginFrame();
            if(SpatialLabWorld.Active)Physics.SyncTransforms();
            var receivers=FindObjectsOfType<BeastReceiver>();foreach(var receiver in receivers)receiver.Lit=false;
            foreach(var source in FindObjectsOfType<LightEmitter>())if(source.active&&source.enabled){if(SpatialLabWorld.Active)TraceSpatial(source);else Trace(source);}
            foreach(var receiver in receivers)receiver.Tick(Time.deltaTime);
            foreach(var exposure in exposures)exposure.EndFrame(Time.deltaTime);
            for(int i=used;i<lines.Count;i++)lines[i].enabled=false;
            SegmentsLastFrame=used/2;
        }
        void TraceSpatial(LightEmitter source){
            Vector3 origin=source.transform.position;
            var owner=source.owner?source.owner.GetComponentInParent<PlayerController>():null;
            Vector3 dir=owner?owner.WorldAim:source.useSpatialDirection?source.spatialDirection.normalized:((Vector3)source.direction).normalized;
            var points=new List<Vector3>{origin};
            RoomFaces routeFace=FaceMembership.Of(source);
            Collider previous=null;float remaining=maxDistance,width=source.beamWidth;
            for(int bounce=0;bounce<bounceLimit&&remaining>.02f;bounce++){
                RaycastHit chosen=default;bool found=false;float nearest=float.PositiveInfinity;
                foreach(var hit in Physics.RaycastAll(origin,dir,remaining,opticalMask,QueryTriggerInteraction.Collide)){
                    if(hit.collider.GetComponentInParent<OpticalPassThrough>())continue;
                    if(hit.collider.isTrigger&&!hit.collider.GetComponentInParent<PlayerController>()&&!hit.collider.GetComponentInParent<MirrorSurface>()&&!hit.collider.GetComponentInParent<SpaceMirror>()&&!hit.collider.GetComponentInParent<BeastReceiver>()&&!hit.collider.GetComponentInParent<LensSurface>())continue;
                    var link=hit.collider.GetComponent<SpatialColliderLink>();
                    if(hit.collider==previous||(source.owner&&link&&link.original==source.owner))continue;
                    if(hit.distance<nearest){nearest=hit.distance;chosen=hit;found=true;}
                }
                Vector3 end=found?chosen.point:origin+dir*remaining;
                points.Add(end);
                LastPaths.Add(source.name+": "+origin+" -> "+end+" hit "+(found?chosen.collider.name:"none"));
                Draw(origin,end,width,routeFace);remaining-=Vector3.Distance(origin,end);
                if(!found)break;
                var player=chosen.collider.GetComponentInParent<PlayerController>();
                if(player){player.GetComponent<LightExposure>()?.Receive(source,dir,chosen.point);if(player.Absorb())break;if(player.Mode==PlayerController.AbilityMode.Reflect)dir=player.WorldAim;else break;}
                else if(chosen.collider.GetComponentInParent<SpaceMirror>() is SpaceMirror spatialMirror){var reflected=Vector3.Reflect(dir,spatialMirror.Normal).normalized;routeFace=FaceMembership.AfterReflection(dir,reflected,routeFace,spatialMirror.travelView);dir=reflected;}
                else if(chosen.collider.GetComponentInParent<MirrorSurface>() is MirrorSurface mirror){var next=Vector3.Reflect(dir,mirror.WorldNormal).normalized;routeFace=FaceMembership.AfterReflection(dir,next,routeFace,mirror.destinationView);dir=next;}
                else if(chosen.collider.GetComponentInParent<LensSurface>() is LensSurface lens)width=Mathf.Clamp(width*lens.widthMultiplier,.015f,.22f);
                else if(chosen.collider.GetComponentInParent<BeastReceiver>() is BeastReceiver receiver){receiver.Lit=true;if(!ReceivedPaths.ContainsKey(receiver))ReceivedPaths[receiver]=new List<Vector3>(points);break;}
                else break;
                previous=chosen.collider;origin=end+dir*.035f;remaining-=.035f;
            }
        }
        void Trace(LightEmitter source){
            Vector2 origin=source.transform.position,dir=source.direction.normalized;
            Collider2D previous=null;float remaining=maxDistance,width=source.beamWidth;
            for(int bounce=0;bounce<bounceLimit&&remaining>.02f;bounce++){
                RaycastHit2D chosen=default;
                foreach(var hit in Physics2D.RaycastAll(origin,dir,remaining,opticalMask)){
                    if(hit.collider==source.owner||hit.collider==previous)continue;
                    chosen=hit;break;
                }
                Vector2 end=chosen.collider?chosen.point:origin+dir*remaining;
                LastPaths.Add(source.name+": "+origin+" -> "+end+" hit "+(chosen.collider?chosen.collider.name:"none"));
                Draw(origin,end,width);remaining-=Vector2.Distance(origin,end);
                if(!chosen.collider)break;
                var player=chosen.collider.GetComponentInParent<PlayerController>();
                if(player){if(player.Absorb())break;if(player.Mode==PlayerController.AbilityMode.Reflect)dir=player.Aim;else break;}
                else if(chosen.collider.GetComponentInParent<MirrorSurface>() is MirrorSurface mirror)dir=Vector2.Reflect(dir,mirror.Normal).normalized;
                else if(chosen.collider.TryGetComponent<LensSurface>(out var lens))width=Mathf.Clamp(width*lens.widthMultiplier,.015f,.22f);
                else if(chosen.collider.TryGetComponent<BeastReceiver>(out var receiver)){receiver.Lit=true;break;}
                else break;
                previous=chosen.collider;origin=end+dir*.035f;remaining-=.035f;
            }
        }
        void Draw(Vector3 start,Vector3 end,float width,RoomFaces faces=RoomFaces.All){
            DrawLine(start,end,width*9f,new Color(.72f,.85f,.94f,.24f),faces);
            DrawLine(start,end,width*2f,new Color(1f,.99f,.90f,.88f),faces);
        }
        void DrawLine(Vector3 start,Vector3 end,float width,Color color,RoomFaces faces){
            if(used>=lines.Count){var child=new GameObject("Beam segment");child.transform.SetParent(transform,false);child.AddComponent<FaceMembership>();var line=child.AddComponent<LineRenderer>();lines.Add(line);}
            var renderer=lines[used++];renderer.GetComponent<FaceMembership>().faces=faces;renderer.enabled=true;ConfigureLine(renderer,width,color);renderer.positionCount=2;renderer.SetPosition(0,start);renderer.SetPosition(1,end);
        }
    }
}
