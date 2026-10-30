using UnityEngine;

namespace LightHungry {
    /// <summary>Only a visual child faces the camera; the physical root never rotates.</summary>
    [DefaultExecutionOrder(800)]
    public sealed class ViewFacingSprite : MonoBehaviour {
        Vector3 offset;
        Quaternion rotation;
        void Awake() { offset = transform.localPosition; rotation = transform.localRotation; }
        void LateUpdate() {
            var view = CameraViewController.Active;
            if (!view) return;
            var yaw = Quaternion.Euler(0, view.VisualYaw, 0);
            if(transform.parent)transform.localPosition = Quaternion.Inverse(transform.parent.rotation) * yaw * offset;
            transform.rotation = yaw * rotation;
        }
    }
}
