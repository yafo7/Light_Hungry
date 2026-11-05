using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed partial class JourneyUI : MonoBehaviour {
    static readonly Color Ink = new Color(.18f, .27f, .23f);
    static readonly Color Paper = new Color(.96f, .94f, .86f);
    static readonly Color Muted = new Color(.41f, .48f, .40f);
    static readonly Color Gold = new Color(.84f, .68f, .35f);
    GameSession session;
    Canvas canvas;
    RectTransform design, page, background;
    CanvasGroup fade;
    RawImage backdrop, preview;
    Text previewTitle, context;
    JourneyGraphic charge;
    JourneyGraphic shade;
    readonly List<JourneyGraphic> markers = new List<JourneyGraphic>();
    readonly List<RectTransform> motes = new List<RectTransform>();
    List<JourneyLevel> levels;
    int selected;
    bool fullscreen;
    LevelProgressBridge levelBridge;
    LightFormAbility ability;
    public Canvas Canvas => canvas;
    public JourneyLevel Selected => levels != null && selected >= 0 && selected < levels.Count ? levels[selected] : null;

    public void Initialize(GameSession owner) {
        session = owner;
        var root = new GameObject("Journey UI", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        root.AddComponent<GraphicRaycaster>();
        if (!FindObjectOfType<EventSystem>()) {
            var events = new GameObject("Journey EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            events.transform.SetParent(transform, false);
        }
        background = Rect(root.transform, "Background", 0, 0, 1600, 900);
        background.anchorMin = Vector2.zero; background.anchorMax = Vector2.one;
        background.offsetMin = background.offsetMax = Vector2.zero;
        backdrop = background.gameObject.AddComponent<RawImage>();
        backdrop.raycastTarget = false;
        design = Rect(root.transform, "Layout 1600x900", 0, 0, 1600, 900);
        design.anchorMin = design.anchorMax = new Vector2(.5f, .5f);
        design.pivot = new Vector2(.5f, .5f); design.anchoredPosition = Vector2.zero;
        // Children use a top-left origin in the reference composition.
        var anchor = Rect(design, "Composition", 0, 0, 1600, 900);
        design = anchor;
        var curtain = Rect(root.transform, "Transition", 0, 0, 1600, 900);
        curtain.anchorMin = Vector2.zero; curtain.anchorMax = Vector2.one;
        curtain.offsetMin = curtain.offsetMax = Vector2.zero;
        var cover = curtain.gameObject.AddComponent<Image>(); cover.color = new Color(.05f, .10f, .08f);
        cover.raycastTarget = true;
        fade = curtain.gameObject.AddComponent<CanvasGroup>(); fade.alpha = 0; fade.blocksRaycasts = false;
        fullscreen = PlayerPrefs.GetInt(GameSession.PrefsPrefix + "Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        if (PlayerPrefs.HasKey(GameSession.PrefsPrefix + "Fullscreen")) Screen.fullScreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    public void Show(JourneyScreen state) {
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
        if (page) { page.gameObject.SetActive(false); Destroy(page.gameObject); }
        if (shade) { shade.gameObject.SetActive(false); Destroy(shade.gameObject); shade = null; }
        motes.Clear(); markers.Clear(); context = null; charge = null;
        page = Rect(design, "Page " + state, 0, 0, 1600, 900);
        bool illustrated = state == JourneyScreen.Menu || state == JourneyScreen.Map || state == JourneyScreen.Settings && session.Current == null;
        background.gameObject.SetActive(illustrated);
        if (illustrated) {
            backdrop.texture = state == JourneyScreen.Map ? session.Catalog.mapBackground : session.Catalog.menuBackground;
            FitBackground();
        }
        switch (state) {
            case JourneyScreen.Menu: MainPage(); break;
            case JourneyScreen.Map: MapPage(); break;
            case JourneyScreen.Settings: SettingsPage(); break;
            case JourneyScreen.Playing: Hud(); break;
            case JourneyScreen.Paused: PausePage(); break;
            case JourneyScreen.Completed: CompletionPage(); break;
            case JourneyScreen.Opening: OpeningPage(); break;
        }
    }

    void FitBackground() {
        if (!backdrop.texture) return;
        float imageAspect = (float)backdrop.texture.width / backdrop.texture.height;
        float screenAspect = (float)Screen.width / Screen.height;
        backdrop.uvRect = imageAspect > screenAspect
            ? new Rect((1 - screenAspect / imageAspect) / 2, 0, screenAspect / imageAspect, 1)
            : new Rect(0, (1 - imageAspect / screenAspect) / 2, 1, imageAspect / screenAspect);
    }

    void MainPage() {
        var title = Label(page, "食 光", 118, 186, 500, 160, 88, Paper);
        title.verticalOverflow = VerticalWrapMode.Overflow;
        Action(page, "Main.Start", session.HasProgress ? "继续游戏" : "开始游戏", 126, 456, 320, session.StartJourney, true);
        Action(page, "Main.Map", "选择关卡", 126, 532, 320, session.ShowMap, false, true);
        Action(page, "Main.Settings", "设置", 126, 601, 320, session.ShowSettings, false, true);
        Action(page, "Main.Quit", "退出游戏", 126, 670, 320, QuitGame, false, true);
        for (int i = 0; i < 12; i++) {
            var mote = Shape(page, "Floating light", 820 + i * 47 % 650, 330 + i * 83 % 480, 3 + i % 3, 3 + i % 3, new Color(1, .94f, .72f, .3f), JourneyShape.Disc);
            motes.Add(mote.rectTransform);
        }
    }

    static void QuitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void MapPage() {
        levels = session.Catalog.Available;
        selected = Mathf.Clamp(levels.FindIndex(level => level.sceneName == session.LastScene), 0, Mathf.Max(0, levels.Count - 1));
        Shape(page, "Map heading wash", 66, 72, 370, 112, new Color(.98f, .96f, .88f, .88f), JourneyShape.Panel);
        Label(page, "选择关卡", 85, 88, 340, 72, 48, Ink);
        Action(page, "Map.Back", "返回", 1350, 72, 160, session.Back);
        var region = Rect(page, "Map nodes", 70, 250, 980, 480);
        var route = region.gameObject.AddComponent<JourneyRouteGraphic>();
        route.color = new Color(.36f, .48f, .34f, .65f); route.raycastTarget = false;
        route.points = levels.ConvertAll(level => level.mapPosition).ToArray();
        for (int i = 0; i < levels.Count; i++) {
            int index = i;
            JourneyLevel level = levels[i];
            float x = level.mapPosition.x * 980, y = level.mapPosition.y * 480;
            var halo = Shape(region, "Node halo", x - 34, y - 34, 68, 68, new Color(.98f, .95f, .79f, .8f), JourneyShape.Disc);
            var marker = Shape(region, "Map.Node." + level.id, x - 26, y - 26, 52, 52, Paper, JourneyShape.Disc);
            marker.raycastTarget = true;
            var button = marker.gameObject.AddComponent<Button>(); button.targetGraphic = marker;
            button.onClick.AddListener(() => { session.Audio.Click(); SelectLevel(index); });
            marker.gameObject.AddComponent<JourneyButtonMotion>();
            var outline = Shape(region, "Node border", x - 33, y - 33, 66, 66, new Color(.36f, .47f, .33f, .8f), JourneyShape.Ring);
            outline.thickness = 1.4f;
            Label(marker.transform, (i + 1).ToString("00"), 0, 0, 52, 52, 20, Ink, TextAnchor.MiddleCenter);
            if (session.IsComplete(level)) Shape(region, "Completed seed", x + 20, y - 29, 12, 12, Gold, JourneyShape.Disc);
            markers.Add(marker);
        }
        Shape(page, "Preview card", 1150, 246, 360, 408, new Color(.98f, .97f, .91f, .95f), JourneyShape.Panel);
        preview = Rect(page, "Level preview", 1170, 266, 320, 184).gameObject.AddComponent<RawImage>(); preview.raycastTarget = false;
        previewTitle = Label(page, "", 1170, 478, 320, 55, 31, Ink);
        Action(page, "Map.Enter", "开始", 1170, 566, 320, () => session.LoadLevel(Selected), true);
        SelectLevel(selected);
    }

    public void SelectLevel(int index) {
        if (levels == null || levels.Count == 0) return;
        selected = Mathf.Clamp(index, 0, levels.Count - 1);
        var level = levels[selected];
        preview.texture = level.preview;
        if (level.preview) {
            float aspect = (float)level.preview.width / level.preview.height;
            float desired = 320f / 184;
            preview.uvRect = aspect > desired ? new Rect((1 - desired / aspect) / 2, 0, desired / aspect, 1) : new Rect(0, (1 - aspect / desired) / 2, 1, aspect / desired);
        }
        previewTitle.text = level.title;
        for (int i = 0; i < markers.Count; i++) markers[i].color = i == selected ? new Color(.91f, .76f, .44f) : Paper;
    }

    void Shade() {
        shade = Shape(canvas.transform, "Shade", 0, 0, 1600, 900, new Color(.035f, .08f, .06f, .78f), JourneyShape.Panel);
        shade.radius = 0;
        shade.rectTransform.anchorMin = Vector2.zero; shade.rectTransform.anchorMax = Vector2.one;
        shade.rectTransform.offsetMin = shade.rectTransform.offsetMax = Vector2.zero;
        shade.transform.SetSiblingIndex(background.GetSiblingIndex() + 1);
    }
    void PausePage() {
        Shade();
        Label(page, "暂停", 510, 206, 580, 90, 43, Paper, TextAnchor.MiddleCenter);
        Action(page, "Pause.Resume", "继续游戏", 640, 360, 320, session.Resume, true);
        Action(page, "Pause.Restart", "重开本关", 640, 438, 320, session.Restart, false, true);
        Action(page, "Pause.Map", "选择关卡", 640, 506, 320, session.ShowMap, false, true);
        Action(page, "Pause.Settings", "设置", 640, 574, 320, session.ShowSettings, false, true);
        Action(page, "Pause.Menu", "返回主菜单", 640, 642, 320, session.MainMenu, false, true);
    }

    void CompletionPage() {
        Shade();
        Label(page, "关卡完成", 420, 230, 760, 80, 46, Paper, TextAnchor.MiddleCenter);
        var next = session.Current == null ? null : session.Catalog.Next(session.Current.sceneName);
        if (next != null) Action(page, "Complete.Next", "下一关", 575, 413, 450, () => session.LoadLevel(next), true);
        Action(page, "Complete.Map", "选择关卡", 575, 497, 450, session.ShowMap, next == null, next != null);
        Action(page, "Complete.Retry", "重玩", 575, 575, 450, session.Restart, false, true);
        Action(page, "Complete.Menu", "返回主菜单", 575, 645, 450, session.MainMenu, false, true);
    }

    void SettingsPage() {
        Shade();
        Shape(page, "Settings card", 420, 100, 760, 700, Paper, JourneyShape.Panel);
        Label(page, "声音与显示", 480, 150, 640, 68, 40, Ink);
        SliderControl("Settings.Music", "音乐", 486, 316, session.Audio.MusicVolume, session.Audio.SetMusic);
        SliderControl("Settings.Effects", "音效", 486, 414, session.Audio.EffectsVolume, session.Audio.SetEffects);
        Action(page, "Settings.Fullscreen", fullscreen ? "显示模式  ·  全屏" : "显示模式  ·  窗口", 486, 517, 628, () => {
            fullscreen = !fullscreen; Screen.fullScreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            PlayerPrefs.SetInt(GameSession.PrefsPrefix + "Fullscreen", fullscreen ? 1 : 0); Show(JourneyScreen.Settings);
        });
        Label(page, "A / D  移动     空格  跳跃\nE  交互     Esc  暂停", 486, 232, 630, 65, 18, Muted);
        Action(page, "Settings.Opening", "重看开场", 486, 603, 628, session.ReplayOpening);
        Action(page, "Settings.Back", "返回", 486, 710, 628, session.Back, true);
    }

    void SliderControl(string name, string title, float x, float y, float value, UnityEngine.Events.UnityAction<float> changed) {
        Label(page, title, x, y, 160, 32, 22, Ink);
        var percent = Label(page, Mathf.RoundToInt(value * 100) + "%", x + 540, y, 88, 32, 18, Muted, TextAnchor.MiddleRight);
        var root = Rect(page, name, x, y + 53, 628, 30);
        var hitArea = root.gameObject.AddComponent<Image>(); hitArea.color = Color.clear;
        var track = Shape(root, "Track", 0, 12, 628, 5, new Color(.70f, .75f, .64f), JourneyShape.Panel);
        var fillArea = Rect(root, "Fill area", 0, 12, 628, 5);
        var fill = Shape(fillArea, "Fill", 0, 0, 0, 0, Gold, JourneyShape.Panel);
        fill.radius = track.radius = 2;
        var handleArea = Rect(root, "Handle area", 9, 15, 610, 0);
        var handle = Shape(handleArea, "Handle", 0, 0, 20, 20, Ink, JourneyShape.Disc);
        handle.rectTransform.pivot = new Vector2(.5f, .5f);
        handle.raycastTarget = true;
        var slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
        slider.minValue = 0; slider.maxValue = 1; slider.value = value;
        slider.onValueChanged.AddListener(number => { percent.text = Mathf.RoundToInt(number * 100) + "%"; changed(number); });
    }

    void Hud() {
        levelBridge = FindObjectOfType<LevelProgressBridge>(); ability = FindObjectOfType<LightFormAbility>();
        Action(page, "HUD.Pause", "暂停", 1440, 24, 120, session.Pause);
        context = Label(page, "", 280, 813, 1040, 42, 18, Paper, TextAnchor.MiddleCenter);
        var shadow = context.gameObject.AddComponent<Shadow>(); shadow.effectColor = new Color(0, 0, 0, .8f); shadow.effectDistance = new Vector2(1, -1);
        charge = Shape(page, "Charge", 690, 795, 220, 3, Gold, JourneyShape.Panel);
    }

    void Update() {
        if (session.State == JourneyScreen.Menu) for (int i = 0; i < motes.Count; i++) {
            var mote = motes[i]; mote.anchoredPosition += Vector2.up * Time.unscaledDeltaTime * (4 + i % 5);
            if (mote.anchoredPosition.y > -250) mote.anchoredPosition -= Vector2.up * 500;
        }
        if (session.State != JourneyScreen.Playing) return;
        if (context) context.text = levelBridge ? levelBridge.Context : "";
        if (charge) { charge.gameObject.SetActive(ability && ability.Charge > .05f && !ability.IsLight); if (ability) charge.rectTransform.sizeDelta = new Vector2(220 * Mathf.Clamp01(ability.Charge / ability.chargeSeconds), 3); }
    }

    public IEnumerator Fade(float target, float seconds) {
        fade.blocksRaycasts = true;
        float start = fade.alpha;
        for (float elapsed = 0; elapsed < seconds; elapsed += Time.unscaledDeltaTime) {
            fade.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0, 1, elapsed / seconds)); yield return null;
        }
        fade.alpha = target; fade.blocksRaycasts = target > .01f;
    }

    RectTransform Rect(Transform parent, string name, float x, float y, float width, float height) {
        // Supply renderers before Graphic.OnEnable, including in Editor Play mode.
        var element = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer)); element.transform.SetParent(parent, false);
        var rect = element.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height); return rect;
    }
    JourneyGraphic Shape(Transform parent, string name, float x, float y, float w, float h, Color tint, JourneyShape kind) {
        var graphic = Rect(parent, name, x, y, w, h).gameObject.AddComponent<JourneyGraphic>(); graphic.shape = kind; graphic.color = tint; graphic.raycastTarget = false; return graphic;
    }
    Text Label(Transform parent, string value, float x, float y, float w, float h, int size, Color tint, TextAnchor align = TextAnchor.UpperLeft) {
        var text = Rect(parent, value.Length > 0 ? value : "Label", x, y, w, h).gameObject.AddComponent<Text>();
        text.font = session.Catalog.font; text.text = value; text.fontSize = size; text.color = tint; text.alignment = align;
        text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; text.raycastTarget = false; text.lineSpacing = 1.15f; return text;
    }
    void Rule(Transform parent, float x, float y, float width, Color tint) { Shape(parent, "Fine rule", x, y, width, 1.4f, tint, JourneyShape.Panel); }
    Button Action(Transform parent, string name, string caption, float x, float y, float width, UnityEngine.Events.UnityAction action, bool primary = false, bool onDark = false) {
        var graphic = Shape(parent, name, x, y, width, 58, primary ? Paper : onDark ? new Color(1, 1, 1, .015f) : new Color(.94f, .94f, .85f, .7f), JourneyShape.Panel);
        graphic.raycastTarget = true; graphic.radius = 7;
        var button = graphic.gameObject.AddComponent<Button>(); button.targetGraphic = graphic;
        ColorBlock colors = button.colors; colors.highlightedColor = new Color(1, .92f, .68f); colors.pressedColor = new Color(.76f, .82f, .65f); colors.fadeDuration = .16f; button.colors = colors;
        Label(graphic.transform, caption, 22, 8, width - 44, 42, primary ? 23 : 22, onDark && !primary ? Paper : Ink, TextAnchor.MiddleLeft);
        button.onClick.AddListener(() => { session.Audio.Click(); action(); }); graphic.gameObject.AddComponent<JourneyButtonMotion>(); return button;
    }
}
