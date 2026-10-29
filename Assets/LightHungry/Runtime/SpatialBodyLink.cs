using UnityEngine;
namespace LightHungry {
    public sealed class SpatialBodyLink : MonoBehaviour {
        public RailDevice rail;
        void OnCollisionStay(Collision collision) { if (rail) rail.CarrySpatialCollision(collision); }
    }
}
