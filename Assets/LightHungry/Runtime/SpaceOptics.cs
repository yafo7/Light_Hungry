using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    public static class SpaceOptics {
        public struct PreviewHit {public Component collider;public Vector3 point;public float distance;}
        public static bool CastPreview(Vector3 origin,Vector3 direction,float range,Component previous,OpticalRoute route,out PreviewHit nearest){
            nearest=default;float best=range;
            foreach(var hit in Physics.RaycastAll(origin,direction,range,TravelMask,QueryTriggerInteraction.Collide)){
                if(hit.collider==previous||!route.CanHit(hit.collider,direction)||hit.collider.GetComponentInParent<OpticalPassThrough>())continue;
                if(hit.collider.isTrigger&&!hit.collider.GetComponentInParent<MirrorSurface>()&&!hit.collider.GetComponentInParent<SpaceMirror>()&&!hit.collider.GetComponentInParent<BeastReceiver>())continue;
                if(hit.distance<best){best=hit.distance;nearest=new PreviewHit{collider=hit.collider,point=hit.point,distance=hit.distance};}
            }
            foreach(var c in Object.FindObjectsOfType<Collider2D>()){
                if(!c.enabled||c==previous||!route.CanHit(c,direction)||c.GetComponentInParent<OpticalPassThrough>())continue;
                if(c.isTrigger&&!c.GetComponentInParent<MirrorSurface>()&&!c.GetComponentInParent<BeastReceiver>())continue;
                Vector3 center=c.offset;Vector3 size;
                if(c is BoxCollider2D box)size=new Vector3(box.size.x,box.size.y,c.GetComponentInParent<MirrorSurface>()?.08f:.9f);
                else if(c is CircleCollider2D circle)size=new Vector3(circle.radius*2,circle.radius*2,circle.radius*2);else continue;
                Vector3 local=c.transform.InverseTransformPoint(origin)-center,heading=c.transform.InverseTransformVector(direction);float enter=0,exit=best;bool valid=true;
                for(int axis=0;axis<3;axis++){float half=size[axis]*.5f;if(Mathf.Abs(heading[axis])<.000001f){if(Mathf.Abs(local[axis])>half){valid=false;break;}continue;}float a=(-half-local[axis])/heading[axis],b=(half-local[axis])/heading[axis];if(a>b){float t=a;a=b;b=t;}enter=Mathf.Max(enter,a);exit=Mathf.Min(exit,b);if(enter>exit){valid=false;break;}}
                if(valid&&enter>.0001f&&enter<best){best=enter;nearest=new PreviewHit{collider=c,point=origin+direction*enter,distance=enter};}
            }
            return nearest.collider;
        }
        public static readonly int TravelMask=(1<<8)|(1<<10)|(1<<11)|(1<<12)|(1<<13);
        public static bool Cast(Vector3 origin,Vector3 direction,float distance,Collider previous,PlayerController player,bool travel,out RaycastHit nearest){
            return Cast(origin,direction,distance,previous,player,travel,OpticalRoute.From(player),out nearest);
        }
        public static bool Cast(Vector3 origin,Vector3 direction,float distance,Collider previous,PlayerController player,bool travel,OpticalRoute route,out RaycastHit nearest){
            nearest=default;float best=float.PositiveInfinity;
            foreach(var hit in Physics.RaycastAll(origin,direction,distance,travel?TravelMask:~0,QueryTriggerInteraction.Collide)){
                if(!route.CanHit(hit.collider,direction))continue;
                if(hit.collider.GetComponentInParent<OpticalPassThrough>())continue;
                if(hit.collider==previous || (player && hit.collider.GetComponentInParent<PlayerController>()==player))continue;
                if(hit.collider.isTrigger && !hit.collider.GetComponentInParent<SpaceMirror>() && !hit.collider.GetComponentInParent<MirrorSurface>() && !hit.collider.GetComponentInParent<LightLanding>() && !hit.collider.GetComponentInParent<BeastReceiver>() && !hit.collider.GetComponentInParent<SpaceHazard>())continue;
                if(hit.distance<best){best=hit.distance;nearest=hit;}
            }
            return nearest.collider;
        }
        [System.Obsolete("Use OpticalRoute.Reflect and preserve the route state across all segments.")]
        public static bool Reflect(Collider c,Vector3 incoming,out Vector3 direction,out int view){var route=OpticalRoute.From(c);return route.Reflect(c,incoming,out direction,out view);}
        public static LightLanding Preview(LightLaunchPad pad,List<Vector3> points,out string problem,int limit=12,float range=80){
            points.Clear();points.Add(pad.Origin);Vector3 p=pad.Origin,d=pad.Direction;Collider previous=null;var route=OpticalRoute.From(pad);
            if(d.sqrMagnitude<.5f){problem="出发方向为零";return null;}
            for(int i=0;i<limit;i++){
                if(!Cast(p,d,range,previous,null,true,route,out var hit)){points.Add(p+d*range);problem=route.inTransit?"跨面光需要接收用 DepthMirror 转回平面":"光路未到达落点";return null;}
                points.Add(hit.point);range-=hit.distance;
                var landing=hit.collider.GetComponentInParent<LightLanding>();
                if(landing){problem=landing.available?null:"落点未启用";return landing.available?landing:null;}
                if(!route.Reflect(hit.collider,d,out d,out _)){problem="被 "+hit.collider.name+" 阻挡";return null;}
                previous=hit.collider;p=hit.point+d*.04f;range-=.04f;
                if(range<=0)break;
            }
            problem="达到反射或距离上限";return null;
        }
    }
}
