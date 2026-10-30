using UnityEngine;

namespace LightHungry {
    /// <summary>Relays 3D trigger contacts to the existing authored interaction component.</summary>
    public sealed class SpatialColliderLink : MonoBehaviour {
        public Collider2D original;
        void OnTriggerEnter(Collider other) {
            if (!original) return;
            var plate = original.GetComponent<PressurePlate>();
            if (plate && plate.isActiveAndEnabled) plate.OnTriggerEnter(other);
            var hazard = original.GetComponent<Hazard>();
            if (hazard && hazard.isActiveAndEnabled) hazard.OnTriggerEnter(other);
            var pickup = original.GetComponent<AbilityPickup>();
            if (pickup && pickup.isActiveAndEnabled) pickup.OnTriggerEnter(other);
            var lift=original.GetComponent<AutomaticLift>();if(lift)lift.OnTriggerEnter(other);
        }
        void OnTriggerExit(Collider other) {
            if (!original) return;
            var plate = original.GetComponent<PressurePlate>();
            if (plate && plate.isActiveAndEnabled) plate.OnTriggerExit(other);
            var lift=original.GetComponent<AutomaticLift>();if(lift)lift.OnTriggerExit(other);
        }
    }
}
