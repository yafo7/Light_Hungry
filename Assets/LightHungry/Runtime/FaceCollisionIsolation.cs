using UnityEngine;
namespace LightHungry {
    [DefaultExecutionOrder(-600)]
    public sealed class FaceCollisionIsolation : MonoBehaviour {
        Collider[] bodies=new Collider[0],world=new Collider[0];float refresh;int view;
        public void Refresh(){refresh=0;FixedUpdate();}
        void FixedUpdate(){
            int current=CameraViewController.Active?CameraViewController.Active.CurrentView:1;
            if(Time.time<refresh&&current==view)return;view=current;refresh=Time.time+.25f;
            world=FindObjectsOfType<Collider>();var list=new System.Collections.Generic.List<Collider>();foreach(var c in world)if(c.attachedRigidbody)list.Add(c);bodies=list.ToArray();
            foreach(var body in bodies)foreach(var other in world){if(!body.enabled||!other.enabled||body==other||body.isTrigger||other.isTrigger||body.attachedRigidbody==other.attachedRigidbody)continue;bool overlap=(FaceMembership.OpticalOf(body)&FaceMembership.OpticalOf(other))!=0;Physics.IgnoreCollision(body,other,!overlap);}
        }
    }
}
