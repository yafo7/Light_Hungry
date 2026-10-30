using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    public sealed class AutomaticLift:MonoBehaviour {
        public Transform platform;
        public float height=3,speed=1.5f;
        public bool Occupied=>occupants.Count>0;
        readonly HashSet<PlayerController> occupants=new HashSet<PlayerController>();
        Vector3 start;
        void Awake(){if(platform)start=platform.localPosition;}
        public void OnTriggerEnter(Collider c){var p=c.GetComponentInParent<PlayerController>();if(p)occupants.Add(p);}
        public void OnTriggerExit(Collider c){var p=c.GetComponentInParent<PlayerController>();if(p)occupants.Remove(p);}
        void FixedUpdate(){if(!platform)return;occupants.RemoveWhere(p=>!p);var next=Vector3.MoveTowards(platform.localPosition,start+(Occupied?Vector3.up*height:Vector3.zero),speed*Time.fixedDeltaTime);var delta=platform.parent?platform.parent.TransformVector(next-platform.localPosition):next-platform.localPosition;platform.localPosition=next;foreach(var p in occupants)if(p.Body3D&&!p.Body3D.isKinematic)p.SetSpatialPosition(p.Body3D.position+delta);}
    }
}
