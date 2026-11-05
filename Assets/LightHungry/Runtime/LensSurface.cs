using UnityEngine;
namespace LightHungry {
    public sealed class LensSurface:MonoBehaviour {
        [Tooltip("风格化定角偏折，0 保留直通；不模拟真实介质或焦点。") ]
        [Range(-80,80)] public float deflectionDegrees;
        public Vector3 Refract(Vector3 incoming,OpticalRoute route)=>Quaternion.AngleAxis(deflectionDegrees,OpticalRoute.Normal(route.view))*incoming;
        [Tooltip("光柱宽度倍率，不模拟焦点。")]
        [Range(.25f,2f)] public float widthMultiplier=.6f;
    }
}
