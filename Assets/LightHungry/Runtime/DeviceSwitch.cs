using UnityEngine;
namespace LightHungry {
    public sealed class DeviceSwitch : Interactable {
        public SpaceDevice target;
        public LightEmitter lightTarget;
        public bool state;
        public Transform handle;
        public SpriteRenderer visual;public Sprite offSprite,onSprite;
        public float activeSeconds;
        float expires;
        public override void Interact(PlayerController player){state=!state;if(state&&activeSeconds>0)expires=Time.time+activeSeconds;Apply();}
        void Update(){if(state&&activeSeconds>0&&Time.time>=expires){state=false;Apply();}}
        void OnEnable(){Apply();}
        void Apply(){if(handle)handle.localRotation=Quaternion.Euler(0,0,state?-30:30);if(visual)visual.sprite=state?onSprite:offSprite;if(target)target.SetState(state);if(lightTarget)lightTarget.active=state;}
    }
}
