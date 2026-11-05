using UnityEngine;

public sealed class LevelProgressBridge : MonoBehaviour {
    GameSession session;
    FirstLightFlow first; DrainLevelFlow third; ReservoirLevelFlow fourth;
    FifthPumpFlow fifth; SixthGreenhouseFlow sixth; SeventhTrellisFlow seventh;
    PlayerMotor player; LightFormAbility ability;
    int transformations, restorations, crossings;
    bool awakened;

    public void Initialize(GameSession owner) {
        session = owner;
        player = FindObjectOfType<PlayerMotor>();
        ability = FindObjectOfType<LightFormAbility>();
        first = FindObjectOfType<FirstLightFlow>();
        third = FindObjectOfType<DrainLevelFlow>();
        fourth = FindObjectOfType<ReservoirLevelFlow>();
        fifth = FindObjectOfType<FifthPumpFlow>();
        sixth = FindObjectOfType<SixthGreenhouseFlow>();
        seventh = FindObjectOfType<SeventhTrellisFlow>();
    }
    public bool Busy => seventh && (seventh.TransitionBusy || seventh.PreviewBusy);
    public void ClearInput() { if (player) player.SuspendInput(); if (ability) ability.SuspendInput(); }
    void LateUpdate() {
        if (session.State != JourneyScreen.Playing) return;
        if (ability) {
            if (ability.TransformCount > transformations) session.Audio.Play(session.Catalog.dissolve);
            if (ability.RestoreCount > restorations) session.Audio.Play(session.Catalog.restore);
            transformations = ability.TransformCount; restorations = ability.RestoreCount;
        }
        if (seventh && seventh.Crossings > crossings) { session.Audio.Play(session.Catalog.crossing); crossings = seventh.Crossings; }
        bool lit = first && first.puzzle.Solved || third && third.mechanism.Awakened || fourth && fourth.mechanism.Awakened || fifth && fifth.Awakened || sixth && sixth.Awakened || seventh && seventh.Awakened;
        if (lit && !awakened) { awakened = true; session.Audio.Play(session.Catalog.mechanism); }
        bool completed = first && first.Completed || third && third.Completed || fourth && fourth.Completed || fifth && fifth.Completed || sixth && sixth.Completed || seventh && seventh.Completed;
        if (completed) session.CompleteLevel();
    }
    public string Context {
        get {
            if (ability && ability.IsLight) return "A / D  移动     E  恢复";
            if (first && first.Adjusting) return "A / D  转动反光片     ·     E  松开";
            if (third && third.Adjusting) return "W / S  拉动绳索     ·     E  松开";
            if (fourth && fourth.Adjusting) return "W / S  调整反光片     ·     E  松开";
            if (seventh && seventh.Grabbing) return "A / D  推拉镜车     ·     E  松开";
            if (fifth && fifth.Adjusting >= 0 || sixth && sixth.Adjusting >= 0) return "W / S  调整镜子     E  松开";
            if (third) return ActionHint(third.Hint());
            if (fourth) return ActionHint(fourth.Hint());
            if (fifth) return ActionHint(fifth.Hint());
            if (sixth) return ActionHint(sixth.Hint());
            if (seventh) return ActionHint(seventh.Hint());
            if (first && player && first.puzzle.Solved && Mathf.Abs(player.transform.position.x - first.entrance.position.x) < 2) return "E  进入";
            if (first && player && Mathf.Abs(player.transform.position.x - first.grip.position.x) < 1.4f) return "E  握住园灯下的把手";
            return "";
        }
    }
    // Adapt legacy walkthrough text into brief action prompts without changing level rules.
    static string ActionHint(string hint) {
        if (string.IsNullOrEmpty(hint)) return "";
        if (hint.Contains("空格跳过")) return "空格  跳过";
        if (hint.StartsWith("Tab ")) return "Tab  返回";
        if (hint.StartsWith("E ")) return hint.Split('·', '；', '，', '。')[0].Trim();
        return "";
    }
}
