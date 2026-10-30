using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    [DefaultExecutionOrder(500)]
    public sealed class BeamSystem:MonoBehaviour {
        public LayerMask opticalMask;public int bounceLimit=12;public float maxDistance=60;
        readonly List<LineRenderer> lines=new List<LineRenderer>();int used;
        static Material beamMaterial,columnFallback;
        readonly List<BeamColumn> columns=new List<BeamColumn>();int columnsUsed;
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
            Physics2D.SyncTransforms();used=0;columnsUsed=0;LastPaths.Clear();
            ReceivedPaths.Clear();var exposures=FindObjectsOfType<LightExposure>();foreach(var exposure in exposures)exposure.BeginFrame();
            if(SpatialLabWorld.Active)Physics.SyncTransforms();
            var receivers=FindObjectsOfType<BeastReceiver>();foreach(var receiver in receivers)receiver.Lit=false;
            foreach(var source in FindObjectsOfType<LightEmitter>())if(source.active&&source.enabled){if(SpatialLabWorld.Active)TraceSpatial(source);else Trace(source);}
            foreach(var receiver in receivers)receiver.Tick(Time.deltaTime);
            foreach(var exposure in exposures)exposure.EndFrame(Time.deltaTime);
            for(int i=used;i<lines.Count;i++)lines[i].enabled=false;
            for(int i=columnsUsed;i<columns.Count;i++)columns[i].gameObject.SetActive(false);
            SegmentsLastFrame=columnsUsed;
        }
        void TraceSpatial(LightEmitter source){
            Vector3 origin=source.transform.position;
            var owner=source.owner?source.owner.GetComponentInParent<PlayerController>():null;
            Vector3 dir=owner?owner.WorldAim:source.WorldDirection;
            var points=new List<Vector3>{origin};
            var route=OpticalRoute.From(source);
            Collider previous=null;float remaining=Mathf.Min(maxDistance,source.maxLength),width=source.beamWidth;
            for(int bounce=0;bounce<bounceLimit&&remaining>.02f;bounce++){
                RaycastHit chosen=default;bool found=false;float nearest=float.PositiveInfinity;
                foreach(var hit in Physics.RaycastAll(origin,dir,remaining,opticalMask,QueryTriggerInteraction.Collide)){
                    if(!route.CanHit(hit.collider,dir)||hit.collider.GetComponentInParent<OpticalPassThrough>())continue;
                    if(hit.collider.isTrigger&&!hit.collider.GetComponentInParent<PlayerController>()&&!hit.collider.GetComponentInParent<MirrorSurface>()&&!hit.collider.GetComponentInParent<SpaceMirror>()&&!hit.collider.GetComponentInParent<BeastReceiver>()&&!hit.collider.GetComponentInParent<LensSurface>())continue;
                    var link=hit.collider.GetComponent<SpatialColliderLink>();
                    if(hit.collider==previous||(source.owner&&link&&link.original==source.owner))continue;
                    if(hit.distance<nearest){nearest=hit.distance;chosen=hit;found=true;}
                }
                Vector3 end=found?chosen.point:origin+dir*remaining;
                points.Add(end);
                LastPaths.Add(source.name+": "+origin+" -> "+end+" hit "+(found?chosen.collider.name:"none"));
                DrawColumn(source,origin,end,width,route);remaining-=Vector3.Distance(origin,end);
                if(!found)break;
                var player=chosen.collider.GetComponentInParent<PlayerController>();
                if(player){player.GetComponent<LightExposure>()?.Receive(source,dir,chosen.point,route);if(player.Absorb())break;if(player.Mode==PlayerController.AbilityMode.Reflect)dir=player.WorldAim;else break;}
                else if(chosen.collider.GetComponentInParent<SpaceMirror>()||chosen.collider.GetComponentInParent<MirrorSurface>()){
                    if(!route.Reflect(chosen.collider,dir,out var next,out _))break;dir=next;
                }
                else if(chosen.collider.GetComponentInParent<LensSurface>() is LensSurface lens)width=Mathf.Clamp(width*lens.widthMultiplier,.015f,.22f);
                else if(chosen.collider.GetComponentInParent<BeastReceiver>() is BeastReceiver receiver){receiver.Lit=true;if(!ReceivedPaths.ContainsKey(receiver))ReceivedPaths[receiver]=new List<Vector3>(points);break;}
                else break;
                previous=chosen.collider;origin=end+dir*.035f;remaining-=.035f;
            }
        }
        void Trace(LightEmitter source){
            Vector2 origin=source.transform.position,dir=((Vector2)source.WorldDirection).normalized;
            var route=OpticalRoute.From(source);Collider2D previous=null;float remaining=Mathf.Min(maxDistance,source.maxLength),width=source.beamWidth;
            for(int bounce=0;bounce<bounceLimit&&remaining>.02f;bounce++){
                RaycastHit2D chosen=default;
                foreach(var hit in Physics2D.RaycastAll(origin,dir,remaining,opticalMask)){
                    if(hit.collider==source.owner||hit.collider==previous||!route.CanHit(hit.collider,dir)||hit.collider.GetComponentInParent<OpticalPassThrough>())continue;
                    chosen=hit;break;
                }
                Vector2 end=chosen.collider?chosen.point:origin+dir*remaining;
                LastPaths.Add(source.name+": "+origin+" -> "+end+" hit "+(chosen.collider?chosen.collider.name:"none"));
                DrawColumn(source,origin,end,width,route);remaining-=Vector2.Distance(origin,end);
                if(!chosen.collider)break;
                var player=chosen.collider.GetComponentInParent<PlayerController>();
                if(player){if(player.Absorb())break;if(player.Mode==PlayerController.AbilityMode.Reflect)dir=player.Aim;else break;}
                else if(chosen.collider.GetComponentInParent<MirrorSurface>()){if(!route.Reflect(chosen.collider,dir,out var next,out _)||Mathf.Abs(next.z)>.001f)break;dir=(Vector2)next;}
                else if(chosen.collider.TryGetComponent<LensSurface>(out var lens))width=Mathf.Clamp(width*lens.widthMultiplier,.015f,.22f);
                else if(chosen.collider.TryGetComponent<BeastReceiver>(out var receiver)){receiver.Lit=true;break;}
                else break;
                previous=chosen.collider;origin=end+dir*.035f;remaining-=.035f;
            }
        }
        void DrawColumn(LightEmitter source,Vector3 start,Vector3 end,float width,OpticalRoute route){
            if(!columnFallback){columnFallback=Resources.Load<Material>("LightColumn");if(!columnFallback)columnFallback=new Material(Shader.Find("LightHungry/LightColumn"));}
            if(columnsUsed>=columns.Count){var go=new GameObject("Beam column");go.transform.SetParent(transform,false);columns.Add(go.AddComponent<BeamColumn>());}
            var column=columns[columnsUsed++];column.gameObject.SetActive(true);column.Set(start,end,Mathf.Max(.01f,width),source.beamColor,source.columnMaterial?source.columnMaterial:columnFallback,route.DisplayFaces);
            if(Application.isPlaying&&SpatialLabWorld.Active)ReceiveColumn(source,start,end,width,route);
        }
        void ReceiveColumn(LightEmitter source,Vector3 start,Vector3 end,float width,OpticalRoute route){
            if(route.inTransit)return;
            var direction=(end-start).normalized;float length=Vector3.Distance(start,end);if(length<.01f)return;
            foreach(var hit in Physics.OverlapCapsule(start,end,width*.5f,(1<<9)|(1<<12),QueryTriggerInteraction.Collide)){
                if(!route.CanHit(hit,direction))continue;
                Vector3 target=hit.ClosestPoint(start+direction*Mathf.Clamp(Vector3.Dot(hit.bounds.center-start,direction),0,length));
                if(Vector3.Dot(target-start,direction)<0||Vector3.Dot(target-start,direction)>length+.01f)continue;
                bool blocked=false;var offset=Vector3.ProjectOnPlane(target-start,direction);float along=Vector3.Dot(target-start,direction);
                foreach(var obstruction in Physics.RaycastAll(start+offset,direction,along,SpaceOptics.TravelMask,QueryTriggerInteraction.Ignore))if(route.CanHit(obstruction.collider,direction)&&!obstruction.collider.GetComponentInParent<OpticalPassThrough>()){blocked=true;break;}
                if(blocked)continue;var player=hit.GetComponentInParent<PlayerController>();if(player)player.GetComponent<LightExposure>()?.Receive(source,direction,target,route);
                var receiver=hit.GetComponentInParent<BeastReceiver>();if(receiver)receiver.Lit=true;
            }
        }
    }
}
