using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    public static class SpaceOptics {
        public static readonly int TravelMask=(1<<8)|(1<<10)|(1<<11)|(1<<12)|(1<<13);
        public static bool Cast(Vector3 origin,Vector3 direction,float distance,Collider previous,PlayerController player,bool travel,out RaycastHit nearest){
            nearest=default;float best=float.PositiveInfinity;
            foreach(var hit in Physics.RaycastAll(origin,direction,distance,travel?TravelMask:~0,QueryTriggerInteraction.Collide)){
                if(hit.collider==previous || (player && hit.collider.GetComponentInParent<PlayerController>()==player))continue;
                if(hit.collider.isTrigger && !hit.collider.GetComponentInParent<SpaceMirror>() && !hit.collider.GetComponentInParent<MirrorSurface>() && !hit.collider.GetComponentInParent<LightLanding>() && !hit.collider.GetComponentInParent<BeastReceiver>() && !hit.collider.GetComponentInParent<SpaceHazard>())continue;
                if(hit.distance<best){best=hit.distance;nearest=hit;}
            }
            return nearest.collider;
        }
        public static bool Reflect(Collider c,Vector3 incoming,out Vector3 direction,out int view){
            direction=incoming;view=0;
            var mirror=c.GetComponentInParent<SpaceMirror>();
            if(mirror){direction=Vector3.Reflect(incoming,mirror.Normal).normalized;view=mirror.travelView;return true;}
            var old=c.GetComponentInParent<MirrorSurface>();
            if(old){direction=Vector3.Reflect(incoming,old.face.transform.up.normalized).normalized;return true;}
            return false;
        }
        public static LightLanding Preview(LightLaunchPad pad,List<Vector3> points,out string problem,int limit=12,float range=80){
            points.Clear();points.Add(pad.Origin);Vector3 p=pad.Origin,d=pad.Direction;Collider previous=null;
            if(d.sqrMagnitude<.5f){problem="出发方向为零";return null;}
            for(int i=0;i<limit;i++){
                if(!Cast(p,d,range,previous,null,true,out var hit)){points.Add(p+d*range);problem="光路未到达落点";return null;}
                points.Add(hit.point);range-=hit.distance;
                var landing=hit.collider.GetComponentInParent<LightLanding>();
                if(landing){problem=landing.available?null:"落点未启用";return landing.available?landing:null;}
                if(!Reflect(hit.collider,d,out d,out _)){problem="被 "+hit.collider.name+" 阻挡";return null;}
                previous=hit.collider;p=hit.point+d*.04f;range-=.04f;
                if(range<=0)break;
            }
            problem="达到反射或距离上限";return null;
        }
    }
}
