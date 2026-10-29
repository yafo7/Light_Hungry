using UnityEngine;
namespace LightHungry {
    public sealed class DeviceSwitch : Interactable {
        public SpaceDevice target;
        public LightEmitter lightTarget;
        public bool state;
        public Transform handle;
        public override void Interact(PlayerController player){state=!state;if(target)target.SetState(state);if(lightTarget)lightTarget.active=state;Apply();}
        void OnEnable(){Apply();}
        void Apply(){if(handle)handle.localRotation=Quaternion.Euler(0,0,state?-30:30);}
    }
}
