using UnityEngine;
namespace LightHungry {
    public sealed class LightLanding : MonoBehaviour {
        public Transform spawn;
        [Range(1,4)] public int view=1;
        public bool available=true;
        public Vector3 Feet=>spawn?spawn.position:transform.position-Vector3.up*.95f;
        public bool CanMaterialize(PlayerController player){
            if(!available)return false;
            foreach(var c in Physics.OverlapBox(Feet+Vector3.up*.75f,new Vector3(.36f,.64f,.3f),Quaternion.identity,player.groundMask,QueryTriggerInteraction.Ignore))
                if(c.attachedRigidbody!=player.Body3D)return false;
            return Physics.Raycast(Feet+Vector3.up*.1f,Vector3.down,.35f,player.groundMask,QueryTriggerInteraction.Ignore);
        }
        void OnDrawGizmos(){Gizmos.color=Color.cyan;Gizmos.DrawWireSphere(transform.position,.38f);Gizmos.DrawLine(transform.position,Feet);}
    }
}
