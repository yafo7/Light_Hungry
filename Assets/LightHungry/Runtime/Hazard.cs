using UnityEngine;
namespace LightHungry {
    public sealed class Hazard:MonoBehaviour {
        void OnTriggerEnter2D(Collider2D other){if(other.GetComponentInParent<PlayerController>())LabSession.Instance?.Restart();}
    }
}
