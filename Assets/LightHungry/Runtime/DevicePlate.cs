using System.Collections.Generic;
using UnityEngine;
namespace LightHungry {
    public sealed class DevicePlate : MonoBehaviour {
        public SpaceDevice target;
        public Transform top;
        public bool IsPressed {get;private set;}
        readonly HashSet<Collider> contacts=new HashSet<Collider>();
        Vector3 rest;
        void Awake(){rest=top?top.localPosition:Vector3.zero;}
        void OnTriggerEnter(Collider other){if(other.GetComponentInParent<PlayerController>()||other.GetComponentInParent<CarryableProp>()||other.GetComponentInParent<PushableBox>())contacts.Add(other);}
        void OnTriggerExit(Collider other){contacts.Remove(other);}
        void Update(){contacts.RemoveWhere(c=>!c||!c.enabled||!c.gameObject.activeInHierarchy);bool pressed=contacts.Count>0;if(pressed!=IsPressed){IsPressed=pressed;if(target)target.SetState(pressed);}if(top)top.localPosition=rest+Vector3.down*(pressed?.08f:0);}
        void OnDisable(){contacts.Clear();if(IsPressed&&target)target.SetState(false);IsPressed=false;}
    }
}
