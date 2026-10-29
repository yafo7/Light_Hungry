using UnityEngine;
namespace LightHungry { public sealed class SpaceHazard:MonoBehaviour {
    void OnTriggerEnter(Collider other){if(other.GetComponentInParent<PlayerController>())LabSession.Instance?.Restart();}
} }
