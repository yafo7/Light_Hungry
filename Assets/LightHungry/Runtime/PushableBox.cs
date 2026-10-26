using UnityEngine;
namespace LightHungry {
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PushableBox:Interactable {
        float drive,driveUntil;
        public void Drive(float velocity){drive=velocity;driveUntil=Time.time+.06f;}
        public override void Interact(PlayerController player){}
        void FixedUpdate(){var body=GetComponent<Rigidbody2D>();body.velocity=new Vector2(Time.time<driveUntil?drive:Mathf.MoveTowards(body.velocity.x,0,30*Time.fixedDeltaTime),body.velocity.y);}
    }
}
