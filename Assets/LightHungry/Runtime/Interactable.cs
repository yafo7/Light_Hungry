using UnityEngine;
namespace LightHungry {
    public abstract class Interactable : MonoBehaviour {
        public abstract void Interact(PlayerController player);
        public virtual bool Continuous => false;
        public virtual void Hold(PlayerController player,float deltaTime) {}
        public virtual void Release(PlayerController player) {}
    }
}
