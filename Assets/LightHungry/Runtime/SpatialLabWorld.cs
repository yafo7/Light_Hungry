using System.Collections.Generic;
using UnityEngine;

namespace LightHungry {
    /// <summary>Opt-in spatial adapter for the authored InteractionLab, preserving all prefab sources.</summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class SpatialLabWorld : MonoBehaviour {
        public static SpatialLabWorld Active { get; private set; }
        public Material spriteMaterial, actorMaterial, solidMaterial, backdropMaterial;
        [Range(4, 24)] public float platformDepth = 12;
        readonly List<Renderer> backdrops = new List<Renderer>();
        readonly List<Vector3> backdropNormals = new List<Vector3>();
        PhysicMaterial frictionless;
        public int AdaptedColliderCount { get; private set; }

        void Awake() {
            Active = this;
            frictionless = new PhysicMaterial("Spatial player friction") {
                dynamicFriction = 0, staticFriction = 0, bounciness = 0,
                frictionCombine = PhysicMaterialCombine.Minimum
            };
            foreach (var body2D in FindObjectsOfType<Rigidbody2D>()) {
                body2D.simulated = false;
                // Unity forbids 2D and 3D rigidbodies on one GameObject. Keep the authored
                // prefab intact inside a spatial body wrapper, created only for this scene.
                var wrapper = new GameObject(body2D.name + " spatial physics");
                wrapper.layer = body2D.gameObject.layer;
                wrapper.transform.SetParent(body2D.transform.parent, false);
                wrapper.transform.position = body2D.transform.position;
                body2D.transform.SetParent(wrapper.transform, true);
                var body = wrapper.AddComponent<Rigidbody>();
                wrapper.AddComponent<SpatialBodyLink>().rail = body2D.GetComponent<RailDevice>();
                body.isKinematic = body2D.bodyType != RigidbodyType2D.Dynamic;
                body.useGravity = !body.isKinematic;
                body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = body.isKinematic ? CollisionDetectionMode.ContinuousSpeculative : CollisionDetectionMode.ContinuousDynamic;
                body.mass = body2D.mass;
            }
            var colliders = FindObjectsOfType<Collider2D>();
            foreach (var original in colliders) AdaptCollider(original);
            foreach (var renderer in FindObjectsOfType<SpriteRenderer>()) {
                if (renderer.gameObject.name == "Background") { renderer.enabled = false; continue; }
                if (spriteMaterial) renderer.sharedMaterial = spriteMaterial;
                if (actorMaterial && renderer.GetComponentInParent<PlayerController>()) renderer.sharedMaterial = actorMaterial;
                renderer.sortingOrder = 0;
                if (renderer.GetComponentInParent<PlayerController>() || renderer.GetComponentInParent<BeastReceiver>() || renderer.GetComponentInParent<AbilityPickup>()) {
                    if (!renderer.GetComponent<ViewFacingSprite>()) renderer.gameObject.AddComponent<ViewFacingSprite>();
                }
            }
            MakeBackdrop();
            foreach(var p in FindObjectsOfType<PlayerController>()){
                if(!p.GetComponent<PlayerCarry>())p.gameObject.AddComponent<PlayerCarry>();
                if(!p.GetComponent<LightTravel>())p.gameObject.AddComponent<LightTravel>();
                if(!p.GetComponent<LightExposure>())p.gameObject.AddComponent<LightExposure>();
                if(!p.GetComponent<ScreenBreakEffect>())p.gameObject.AddComponent<ScreenBreakEffect>();
            }
        }

        void AdaptCollider(Collider2D original) {
            if (!original.enabled) return;
            Collider result = null;
            var proxy = new GameObject("Spatial collider");
            proxy.layer = original.gameObject.layer;
            proxy.transform.SetParent(original.transform, false);
            proxy.AddComponent<SpatialColliderLink>().original = original;
            var box = original as BoxCollider2D;
            if (box) {
                var spatial = proxy.AddComponent<BoxCollider>();
                spatial.center = box.offset;
                float depth = DepthFor(box);
                spatial.size = new Vector3(box.size.x, box.size.y, depth);
                result = spatial;
                bool solidArt = !box.isTrigger && !box.GetComponentInParent<PlayerController>() && box.GetComponentInChildren<SpriteRenderer>();
                bool mirrorFace = box.GetComponentInParent<MirrorSurface>() && box.gameObject.name == "Optical face";
                if (solidArt || mirrorFace) {
                    var color = mirrorFace ? new Color(.60f,.78f,.78f) : box.GetComponent<PushableBox>() ? new Color(.39f,.28f,.18f) : new Color(.29f,.34f,.36f);
                    AddVolume(box.transform, spatial.center, spatial.size, color);
                    if (!mirrorFace) {
                        AddFrontAndBackArt(box, depth);
                        if (box.gameObject.layer == 8 && !box.GetComponent<RailDevice>() && box.size.y <= .6f) AddSideArt(box, depth);
                    }
                }
            } else if (original is CircleCollider2D circle) {
                var sphere = proxy.AddComponent<SphereCollider>();
                sphere.center = circle.offset;
                sphere.radius = circle.radius;
                result = sphere;
            }
            if (!result) { Debug.LogWarning("Spatial adapter does not support " + original.GetType().Name + " on " + original.name); return; }
            result.isTrigger = original.isTrigger;
            result.sharedMaterial = frictionless;
            original.enabled = false;
            AdaptedColliderCount++;
        }

        float DepthFor(BoxCollider2D box) {
            if (box.GetComponentInParent<PlayerController>()) return .68f;
            if (box.GetComponent<PushableBox>()) return box.size.x;
            if (box.GetComponent<RailDevice>()) return 2.1f;
            if (box.GetComponentInParent<MirrorSurface>()) return .35f;
            if (box.gameObject.layer == 8) return platformDepth;
            return .8f;
        }

        void AddVolume(Transform parent, Vector3 center, Vector3 size, Color color) {
            var volume = GameObject.CreatePrimitive(PrimitiveType.Cube);
            volume.name = "Spatial volume";
            volume.transform.SetParent(parent, false);
            volume.transform.localPosition = center;
            volume.transform.localScale = size;
            var redundant = volume.GetComponent<Collider>();
            redundant.enabled = false;
            Destroy(redundant);
            var renderer = volume.GetComponent<MeshRenderer>();
            if (solidMaterial) renderer.sharedMaterial = solidMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_Color", color);
            renderer.SetPropertyBlock(properties);
        }

        void AddFrontAndBackArt(BoxCollider2D box, float depth) {
            foreach (var front in box.GetComponentsInChildren<SpriteRenderer>()) {
                if (front.GetComponentInParent<MirrorSurface>()) continue;
                // Artwork remains attached to the same authored object, on the surface of its volume.
                var p = front.transform.position;
                front.transform.position = new Vector3(p.x, p.y, box.transform.position.z - depth * box.transform.lossyScale.z * .5f - .015f);
                var back = new GameObject(front.name + " reverse").AddComponent<SpriteRenderer>();
                back.transform.SetParent(front.transform.parent, false);
                back.transform.localPosition = front.transform.localPosition;
                back.transform.position += Vector3.forward * (depth * box.transform.lossyScale.z + .03f);
                back.transform.localRotation = front.transform.localRotation;
                back.transform.localScale = front.transform.localScale;
                back.sprite = front.sprite;
                back.color = front.color;
                back.flipX = true;
            }
        }

        void AddSideArt(BoxCollider2D box, float depth) {
            var template = box.GetComponentInChildren<SpriteRenderer>();
            if (!template) return;
            int count = Mathf.CeilToInt(depth / 1.72f);
            float unit = depth / count;
            foreach (int side in new[] {-1,1}) {
                for (int i = 0; i < count; i++) {
                    var renderer = new GameObject("Platform side tile").AddComponent<SpriteRenderer>();
                    renderer.transform.SetParent(box.transform, false);
                    renderer.transform.localPosition = new Vector3(box.offset.x + side * (box.size.x * .5f + .015f),box.offset.y - box.size.y * .5f,-depth * .5f + unit * (i + .5f));
                    renderer.transform.localRotation = Quaternion.Euler(0,90,0);
                    renderer.transform.localScale = new Vector3(unit / 1.72f,1,1);
                    renderer.sprite = template.sprite;
                    renderer.color = template.color;
                }
            }
        }

        void MakeBackdrop() {
            if (!backdropMaterial) return;
            for (int i = 0; i < 4; i++) {
                var outward = Quaternion.Euler(0, i * 90, 0) * Vector3.forward;
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = "Room backdrop " + i;
                go.transform.SetParent(transform, false);
                go.transform.position = outward * 17;
                go.transform.rotation = Quaternion.LookRotation(outward);
                go.transform.localScale = new Vector3(34,18,1);
                var collider = go.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
                var renderer = go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = backdropMaterial;
                backdrops.Add(renderer);
                backdropNormals.Add(-outward);
            }
        }

        void LateUpdate() {
            if(LabSession.Instance&&LabSession.Instance.GetComponent<LightRouteReview>()&&LabSession.Instance.GetComponent<LightRouteReview>().IsPlaying)return;
            if (!Camera.main) return;
            for (int i = 0; i < backdrops.Count; i++)
                backdrops[i].enabled = Vector3.Dot(Camera.main.transform.forward, backdropNormals[i]) < -.001f;
        }

        void OnDestroy() {
            if (Active == this) Active = null;
            if (frictionless) Destroy(frictionless);
        }
    }
}
