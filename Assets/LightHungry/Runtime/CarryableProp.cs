using UnityEngine;
namespace LightHungry {
    public sealed class CarryableProp : Interactable {
        public bool Held {get;internal set;}
        public float floorOffset;
        public override void Interact(PlayerController player){player.GetComponent<PlayerCarry>()?.Toggle(this);}
    }
}
