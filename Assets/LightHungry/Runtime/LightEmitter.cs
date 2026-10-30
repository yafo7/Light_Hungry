using UnityEngine;
namespace LightHungry {
    public sealed class LightEmitter:MonoBehaviour {
        public bool active=true;
        public Vector2 direction=Vector2.right;
        public bool useSpatialDirection;
        public Vector3 spatialDirection=Vector3.right;
        [Range(.05f,4f)] public float beamWidth=.65f;
        public bool directionFromTransform;
        public Color beamColor=new Color(.42f,.76f,.90f,.78f);
        [Min(.1f)] public float maxLength=60;
        public Material columnMaterial;
        public Vector3 WorldDirection=>directionFromTransform?transform.right:useSpatialDirection?spatialDirection.normalized:((Vector3)direction).normalized;
        public Collider2D owner;
    }
}
