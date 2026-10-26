using UnityEngine;
namespace LightHungry {
    public abstract class Interactable : MonoBehaviour {
        public abstract void Interact(PlayerController player);
    }
}
