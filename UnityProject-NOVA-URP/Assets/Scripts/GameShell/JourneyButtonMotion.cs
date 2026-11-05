using UnityEngine;
using UnityEngine.EventSystems;

public sealed class JourneyButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler {
    bool highlighted;
    public void OnPointerEnter(PointerEventData data) { Highlight(true); }
    public void OnPointerExit(PointerEventData data) { Highlight(false); }
    public void OnSelect(BaseEventData data) { Highlight(true); }
    public void OnDeselect(BaseEventData data) { Highlight(false); }
    void Highlight(bool value) {
        if (value && !highlighted && GameSession.Instance) GameSession.Instance.Audio.Hover();
        highlighted = value;
    }
    void Update() { transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * (highlighted ? 1.025f : 1), 1 - Mathf.Exp(-12 * Time.unscaledDeltaTime)); }
}
