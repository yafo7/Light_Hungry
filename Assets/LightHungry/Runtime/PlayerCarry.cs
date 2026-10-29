using UnityEngine;
namespace LightHungry {
    public sealed class PlayerCarry : MonoBehaviour {
        public CarryableProp Held {get;private set;}
        PlayerController player;
        Transform originalParent;
        Quaternion originalRotation;
        Collider[] colliders;
        bool[] enabledStates;
        Rigidbody body;bool wasKinematic;bool gravity;
        void Awake(){player=GetComponent<PlayerController>();}
        public bool Toggle(CarryableProp prop){return Held?Drop():PickUp(prop);}
        public bool PickUp(CarryableProp prop){
            if(!prop||Held||prop.Held||!player.Body3D||player.GetComponent<LightTravel>().IsTravelling||Vector3.Distance(prop.transform.position,transform.position)>2.5f)return false;
            Held=prop;prop.Held=true;originalParent=prop.transform.parent;originalRotation=prop.transform.rotation;
            colliders=prop.GetComponentsInChildren<Collider>();enabledStates=new bool[colliders.Length];for(int i=0;i<colliders.Length;i++){enabledStates[i]=colliders[i].enabled;colliders[i].enabled=false;}
            body=prop.GetComponent<Rigidbody>();if(body){wasKinematic=body.isKinematic;gravity=body.useGravity;body.velocity=Vector3.zero;body.isKinematic=true;}
            prop.transform.SetParent(transform,true);return true;
        }
        void LateUpdate(){if(Held)Held.transform.position=transform.position+Vector3.up*1.9f;}
        public bool Drop(){
            if(!Held)return false;
            Vector3 candidate=transform.position+player.ViewRight*(player.visual.flipX?-1.35f:1.35f);
            if(!Physics.Raycast(candidate+Vector3.up*1.4f,Vector3.down,out var floor,3,player.groundMask,QueryTriggerInteraction.Ignore))return false;
            candidate=floor.point+Vector3.up*(.03f+Held.floorOffset);
            var check=Physics.OverlapBox(candidate+Vector3.up*.65f,new Vector3(.6f,.57f,.6f),Quaternion.identity,player.groundMask,QueryTriggerInteraction.Ignore);
            foreach(var c in check)if(c.attachedRigidbody!=player.Body3D)return false;
            Held.transform.SetParent(originalParent,true);Held.transform.SetPositionAndRotation(candidate,originalRotation);
            for(int i=0;i<colliders.Length;i++)if(colliders[i])colliders[i].enabled=enabledStates[i];
            if(body){body.isKinematic=wasKinematic;body.useGravity=gravity;body.constraints=RigidbodyConstraints.FreezeRotation|(Mathf.Abs(player.ViewRight.x)>.5f?RigidbodyConstraints.FreezePositionZ:RigidbodyConstraints.FreezePositionX);}
            Held.Held=false;Held=null;Physics.SyncTransforms();return true;
        }
    }
}
