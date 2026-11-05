using UnityEngine;
namespace LightHungry {
    public struct BeamPathSegment {
        public LightEmitter source;public Vector3 start,end;public float width;public OpticalRoute route;
        public Vector3 Direction=>(end-start).normalized;
        // Closest points of two finite segments; handles fast photons crossing a narrow light beam.
        public bool IntersectSweep(Vector3 from,Vector3 to,out float fraction){
            Vector3 u=to-from,v=end-start,w=from-start;float a=Vector3.Dot(u,u),b=Vector3.Dot(u,v),c=Vector3.Dot(v,v),d=Vector3.Dot(u,w),e=Vector3.Dot(v,w);fraction=0;
            if(c<.000001f)return false;
            float denominator=a*c-b*b,s=denominator>.000001f?Mathf.Clamp01((b*e-c*d)/denominator):0;
            float t=(b*s+e)/c;
            if(t<0){t=0;s=a>.000001f?Mathf.Clamp01(-d/a):0;}else if(t>1){t=1;s=a>.000001f?Mathf.Clamp01((b-d)/a):0;}
            fraction=s;return (w+s*u-t*v).sqrMagnitude<=Mathf.Pow(Mathf.Max(.02f,width*.5f),2);
        }
    }
}
