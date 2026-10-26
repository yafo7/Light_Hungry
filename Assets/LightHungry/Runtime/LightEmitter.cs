using UnityEngine;
namespace LightHungry {
    public sealed class LightEmitter:MonoBehaviour {
        public bool active=true;
        public Vector2 direction=Vector2.right;
        [Range(.015f,.2f)] public float beamWidth=.055f;
        public Collider2D owner;
    }
}
