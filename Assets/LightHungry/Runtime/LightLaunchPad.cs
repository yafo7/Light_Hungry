using UnityEngine;
namespace LightHungry {
    public sealed class LightLaunchPad : Interactable {
        public Vector3 localDirection=Vector3.right;
        public Vector3 lightOffset=Vector3.up*.95f;
        public bool available=true;
        public Vector3 Origin=>transform.TransformPoint(lightOffset);
        public Vector3 Direction=>transform.TransformDirection(localDirection).normalized;
        public override void Interact(PlayerController player){player.GetComponent<LightTravel>()?.Launch(this);}
        void OnDrawGizmos(){Gizmos.color=new Color(1,.8f,.25f);Gizmos.DrawWireSphere(Origin,.18f);Gizmos.DrawLine(Origin,Origin+Direction*1.4f);}
    }
}
