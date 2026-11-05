using UnityEngine;

public sealed partial class JourneyUI {
    void OpeningPage() {
        Shade(); shade.color = new Color(.065f, .08f, .07f, 1);
        var picture = Rect(page, "Opening composition", 0, 0, 1600, 900);
        var group = picture.gameObject.AddComponent<CanvasGroup>();
        var beat = session.Catalog.opening[session.OpeningIndex];
        var panel = Rect(picture, "Opening panel", 110, 80, 1380, 610).gameObject.AddComponent<OpeningPanelGraphic>();
        panel.beat = beat; panel.color = Color.white; panel.raycastTarget = false;
        Label(picture, beat.caption, 180, 731, 1240, 76, 26, Paper, TextAnchor.MiddleCenter);
        var playback = page.gameObject.AddComponent<OpeningPlayback>(); playback.Initialize(session, group);
        Action(page, "Opening.Skip", "跳过", 90, 820, 150, session.FinishOpening, false, true);
        Action(page, "Opening.Next", session.OpeningIndex + 1 == session.Catalog.opening.Count ? (session.OpeningPreview ? "返回" : "开始") : "继续", 1330, 820, 180, playback.Advance, false, true);
    }
}
