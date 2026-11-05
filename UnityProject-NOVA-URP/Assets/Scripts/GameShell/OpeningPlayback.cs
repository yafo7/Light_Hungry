using System.Collections;
using UnityEngine;

public sealed class OpeningPlayback : MonoBehaviour {
    GameSession session;
    CanvasGroup picture;
    float elapsed;
    bool advancing;
    public void Initialize(GameSession owner, CanvasGroup group) { session = owner; picture = group; picture.alpha = 0; }
    void Update() {
        if (!session || session.State != JourneyScreen.Opening || session.Transitioning) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { session.FinishOpening(); return; }
        elapsed += Time.unscaledDeltaTime;
        if (!advancing) picture.alpha = Mathf.Clamp01(elapsed / .45f);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightArrow) || elapsed >= Mathf.Max(2, session.Catalog.opening[session.OpeningIndex].seconds)) Advance();
    }
    public void Advance() { if (!advancing && elapsed > .15f && session.State == JourneyScreen.Opening) StartCoroutine(Change()); }
    IEnumerator Change() {
        advancing = true;
        float start = picture.alpha;
        for (float t = 0; t < .25f; t += Time.unscaledDeltaTime) { picture.alpha = start * (1 - t / .25f); yield return null; }
        session.AdvanceOpening();
    }
}
