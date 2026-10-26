using UnityEngine;
namespace LightHungry {
    public sealed class LensSurface:MonoBehaviour {
        [Tooltip("Prototype changes beam width only; no physical focal-point simulation.")]
        [Range(.25f,2f)] public float widthMultiplier=.6f;
    }
}
