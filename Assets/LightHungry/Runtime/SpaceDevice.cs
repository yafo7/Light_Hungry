using UnityEngine;
namespace LightHungry {
    public enum DeviceAction { SetActive, Move, Mirror }
    public sealed class SpaceDevice : MonoBehaviour {
        public DeviceAction action;
        public Transform movingPart;
        public Vector3 localTravel=Vector3.up*3;
        public float speed=2;
        public Collider gate;
        public SpaceMirror mirror;
        public bool state;
        Vector3 start;
        void Awake(){start=movingPart?movingPart.localPosition:Vector3.zero;Apply();}
        public void SetState(bool value){state=value;Apply();}
        public void Toggle(){SetState(!state);}
        void Apply(){
            if(action==DeviceAction.SetActive){if(gate)gate.enabled=!state;if(movingPart)foreach(var r in movingPart.GetComponentsInChildren<Renderer>())r.enabled=!state;}
            if(action==DeviceAction.Mirror&&mirror){mirror.orientation=state?1:0;mirror.Apply();}
        }
        void FixedUpdate(){if(action==DeviceAction.Move&&movingPart){Vector3 target=start+(state?localTravel:Vector3.zero);var body=movingPart.GetComponent<Rigidbody>();Vector3 next=Vector3.MoveTowards(movingPart.position,movingPart.parent?movingPart.parent.TransformPoint(target):target,speed*Time.fixedDeltaTime);if(body)body.MovePosition(next);else movingPart.position=next;}}
    }
}
