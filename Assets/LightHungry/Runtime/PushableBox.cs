using UnityEngine;
namespace LightHungry {
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PushableBox:Interactable {
        float drive,driveUntil;
        public void Drive(float velocity){drive=velocity;driveUntil=Time.time+.06f;}
        public override void Interact(PlayerController player){}
        void FixedUpdate(){
            var spatial=SpatialLabWorld.Active?GetComponentInParent<Rigidbody>():null;
            if(spatial){
                if(spatial.isKinematic)return;
                var right=CameraViewController.Active?CameraViewController.Active.ViewRight:Vector3.right;
                spatial.constraints=RigidbodyConstraints.FreezeRotation|(Mathf.Abs(right.x)>.5f?RigidbodyConstraints.FreezePositionZ:RigidbodyConstraints.FreezePositionX);
                float horizontal=Vector3.Dot(spatial.velocity,right);
                spatial.velocity=right*(Time.time<driveUntil?drive:Mathf.MoveTowards(horizontal,0,30*Time.fixedDeltaTime))+Vector3.up*spatial.velocity.y;
                spatial.AddForce(Physics.gravity*(GetComponent<Rigidbody2D>().gravityScale-1),ForceMode.Acceleration);
                return;
            }
            var body=GetComponent<Rigidbody2D>();body.velocity=new Vector2(Time.time<driveUntil?drive:Mathf.MoveTowards(body.velocity.x,0,30*Time.fixedDeltaTime),body.velocity.y);
        }
    }
}
