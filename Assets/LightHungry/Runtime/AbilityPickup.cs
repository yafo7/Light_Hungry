using UnityEngine;
namespace LightHungry {
    public sealed class AbilityPickup:MonoBehaviour {
        public bool unlockReflection=true;
        void OnTriggerEnter2D(Collider2D other){var p=other.GetComponentInParent<PlayerController>();if(!p)return;if(unlockReflection)p.reflectUnlocked=true;else p.swallowUnlocked=true;gameObject.SetActive(false);}
    }
}
