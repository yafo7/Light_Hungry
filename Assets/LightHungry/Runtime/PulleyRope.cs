using UnityEngine;
namespace LightHungry {
    public sealed class PulleyRope:Interactable {
        public Transform load,wheel,ropeEnd;
        public float height=3,speed=1.8f,fallSpeed=1.2f;
        public float Progress {get;private set;}
        Vector3 start;float heldUntil;
        public override bool Continuous=>true;
        void Awake(){if(load)start=load.localPosition;}
        public override void Interact(PlayerController player){}
        public override void Hold(PlayerController player,float deltaTime){heldUntil=Time.time+.08f;}
        public override void Release(PlayerController player){heldUntil=0;}
        void FixedUpdate(){if(!load)return;float previous=Progress;Progress=Mathf.MoveTowards(Progress,Time.time<heldUntil?height:0,(Time.time<heldUntil?speed:fallSpeed)*Time.fixedDeltaTime);load.localPosition=start+Vector3.up*Progress;if(wheel)wheel.Rotate(0,0,(Progress-previous)*-100);if(ropeEnd)ropeEnd.localScale=new Vector3(1,Mathf.Max(.2f,height-Progress),1);}
    }
}
