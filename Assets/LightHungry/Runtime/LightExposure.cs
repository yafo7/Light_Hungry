using UnityEngine;
namespace LightHungry {
    [DefaultExecutionOrder(700)]
    public sealed class LightExposure:MonoBehaviour {
        public bool automatic=true;
        public float secondsRequired=2,travelSpeed=11,exitProtection=.8f;
        public float Progress {get;private set;}
        public Vector3 Incoming {get;private set;}
        public Vector3 Contact {get;private set;}
        public bool Illuminated {get;private set;}
        PlayerController player;LightEmitter source,frameSource;Vector3 frameDirection,framePoint;float protectedUntil;
        Transform[] stars;OpticalRoute frameRoute;
        void Awake(){player=GetComponent<PlayerController>();}
        public void BeginFrame(){frameSource=null;}
        public void Receive(LightEmitter emitter,Vector3 incoming,Vector3 point){Receive(emitter,incoming,point,OpticalRoute.From(this));}
        public void Receive(LightEmitter emitter,Vector3 incoming,Vector3 point,OpticalRoute route){if(!frameSource&&!route.inTransit){frameSource=emitter;frameDirection=incoming;framePoint=point;frameRoute=route;}}
        public void EndFrame(float dt){
            Illuminated=frameSource;var travel=GetComponent<LightTravel>();
            if(!automatic||!player.Body3D||travel.IsTravelling||Time.time<protectedUntil||(GetComponent<PlayerCarry>()&&GetComponent<PlayerCarry>().Held)||(CameraViewController.Active&&CameraViewController.Active.IsRotating)){Progress=0;source=null;return;}
            if(!frameSource){Progress=0;source=null;return;}
            if(source!=frameSource||Vector3.Dot(Incoming,frameDirection)<.99f)Progress=0;
            source=frameSource;Incoming=frameDirection;Contact=framePoint;Progress+=dt;
            if(Progress>=secondsRequired&&travel.LaunchFromLight(Contact+Incoming*.04f,Incoming,travelSpeed,frameRoute)){Progress=0;source=null;}
        }
        public void Protect(){Progress=0;source=null;protectedUntil=Time.time+exitProtection;}
        void LateUpdate(){
            if(!player||!player.visual)return;
            float t=automatic?Mathf.Clamp01(Progress/Mathf.Max(.01f,secondsRequired)):0;
            var color=player.visual.color;color.a=Mathf.Lerp(.78f,.20f,t);player.visual.color=color;
            if(stars==null&&t>.02f){stars=new Transform[12];for(int i=0;i<stars.Length;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Dissolving starlight";g.transform.SetParent(transform,false);var c=g.GetComponent<Collider>();c.enabled=false;Destroy(c);g.GetComponent<Renderer>().sharedMaterial=SpatialLabWorld.Active.actorMaterial;stars[i]=g.transform;}}
            if(stars!=null)for(int i=0;i<stars.Length;i++){stars[i].gameObject.SetActive(t>.02f);float phase=i*2.4f+Time.time*1.5f;stars[i].position=transform.position+Vector3.up*(.2f+i*.1f)+Camera.main.transform.right*Mathf.Sin(phase)*(.35f+t*.3f)+Camera.main.transform.forward*Mathf.Cos(phase)*.12f;stars[i].localScale=Vector3.one*(.025f+t*.06f);}
        }
    }
}
