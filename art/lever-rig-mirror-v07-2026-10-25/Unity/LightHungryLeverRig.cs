using UnityEngine;

// Rigid 2D skeleton: the handle artwork is weighted entirely to one Transform bone.
// The base stays on the root. No animated scale, translation, or sprite replacement.
public sealed class LightHungryLeverRig : MonoBehaviour
{
    public Transform handleBone;
    [Min(0.01f)] public float duration = 0.64f;
    public bool startsRight;
    float angle, from, target, elapsed;
    bool moving, desiredRight;

    void Awake() { SetImmediate(startsRight); }

    public void Toggle() { SetRight(!desiredRight); }
    public void SetRight(bool right)
    {
        desiredRight = right;
        from = angle;
        target = right ? 43f : -43f;
        elapsed = 0f;
        moving = !Mathf.Approximately(from, target);
    }
    public void SetImmediate(bool right)
    {
        desiredRight = right;
        angle = target = right ? 43f : -43f;
        moving = false;
        Apply();
    }
    void Update()
    {
        if (!moving) return;
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
        float eased = t*t*t*(t*(t*6f-15f)+10f);
        angle = Mathf.LerpUnclamped(from, target, eased);
        if (t >= 1f) { angle = target; moving = false; }
        Apply();
    }
    void Apply()
    {
        if (handleBone) handleBone.localRotation = Quaternion.Euler(0f, 0f, -angle);
    }
}
