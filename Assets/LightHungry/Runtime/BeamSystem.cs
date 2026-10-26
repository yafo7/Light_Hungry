using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    [DefaultExecutionOrder(500)]
    public sealed class BeamSystem:MonoBehaviour {
        public LayerMask opticalMask;public int bounceLimit=12;public float maxDistance=60;
        readonly List<LineRenderer> lines=new List<LineRenderer>();int used;
        static Material beamMaterial;
        public int SegmentsLastFrame {get;private set;}
        public readonly List<string> LastPaths=new List<string>();
        public static void ConfigureLine(LineRenderer line,float width,Color color){
            if(!beamMaterial)beamMaterial=new Material(Shader.Find("Sprites/Default"));
            line.sharedMaterial=beamMaterial;line.startWidth=line.endWidth=width;line.startColor=line.endColor=color;
            line.useWorldSpace=true;line.sortingOrder=25;line.numCapVertices=4;
        }
        void LateUpdate(){
            Physics2D.SyncTransforms();used=0;LastPaths.Clear();
            var receivers=FindObjectsOfType<BeastReceiver>();foreach(var receiver in receivers)receiver.Lit=false;
            foreach(var source in FindObjectsOfType<LightEmitter>())if(source.active&&source.enabled)Trace(source);
            foreach(var receiver in receivers)receiver.Tick(Time.deltaTime);
            for(int i=used;i<lines.Count;i++)lines[i].enabled=false;
            SegmentsLastFrame=used/2;
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
        void Draw(Vector2 start,Vector2 end,float width){
            DrawLine(start,end,width*3.5f,new Color(1f,.65f,.22f,.14f));
            DrawLine(start,end,width,new Color(1f,.9f,.56f,1f));
        }
        void DrawLine(Vector2 start,Vector2 end,float width,Color color){
            if(used>=lines.Count){var child=new GameObject("Beam segment");child.transform.SetParent(transform,false);var line=child.AddComponent<LineRenderer>();lines.Add(line);}
            var renderer=lines[used++];renderer.enabled=true;ConfigureLine(renderer,width,color);renderer.positionCount=2;renderer.SetPosition(0,start);renderer.SetPosition(1,end);
        }
    }
}
