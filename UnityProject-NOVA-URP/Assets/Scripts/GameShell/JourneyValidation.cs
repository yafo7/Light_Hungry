using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class JourneyValidation : MonoBehaviour {
    readonly List<string> checks = new List<string>();
    static readonly List<string> errors = new List<string>();
    string folder;
    GameSession session;
    float started;
    bool finished;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CaptureStartupErrors() {
        errors.Clear();
        Application.logMessageReceived -= Log;
        if (Environment.GetCommandLineArgs().Contains("--journey-qa")) Application.logMessageReceived += Log;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() {
        if (!Environment.GetCommandLineArgs().Contains("--journey-qa")) return;
        var root = new GameObject("Journey acceptance"); DontDestroyOnLoad(root); root.AddComponent<JourneyValidation>();
    }
    void Check(bool value, string label) { string text = (value ? "PASS " : "FAIL ") + label; checks.Add(text); Debug.Log(text); }
    static void Log(string text, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(text + "\n" + stack); }
    void Update() { if (!finished && started > 0 && Time.realtimeSinceStartup - started > 150) { Check(false, "UI acceptance watchdog"); Finish(); } }
    void OnDestroy() { Application.logMessageReceived -= Log; }

    bool Press(string name) {
        var button = FindObjectsOfType<Button>().FirstOrDefault(item => item.name == name);
        Check(button && button.isActiveAndEnabled && button.interactable, name + " exists and is interactive");
        if (!button) return false;
        Check(button.GetComponent<CanvasRenderer>(), name + " has a renderer before any forced rendering");
        Canvas.ForceUpdateCanvases();
        var rect = button.GetComponent<RectTransform>();
        var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
        var input = new PointerEventData(EventSystem.current) { position = point, button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(input, hits);
        bool reachable = hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button;
        Check(reachable, name + " is the topmost graphic raycast target");
        if (!reachable || !button.interactable) throw new InvalidOperationException("Pointer cannot reach " + name);
        ExecuteEvents.Execute(button.gameObject, input, ExecuteEvents.pointerClickHandler);
        return true;
    }
    IEnumerator Settled() { float until = Time.realtimeSinceStartup + 15; while ((session.Transitioning || SceneFade.Busy) && Time.realtimeSinceStartup < until) yield return null; yield return null; }
    IEnumerator Capture(string name) {
        yield return null;
        var source = Camera.main;
        var snapshot = new GameObject("UI snapshot").AddComponent<Camera>(); snapshot.CopyFrom(source); snapshot.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation); snapshot.enabled = false;
        var canvas = session.UI.Canvas; var mode = canvas.renderMode; var oldCamera = canvas.worldCamera; var plane = canvas.planeDistance;
        int width = Mathf.Min(Screen.width, 1440), height = Mathf.RoundToInt((float)width * Screen.height / Screen.width);
        var oldTarget = RenderTexture.active; var target = RenderTexture.GetTemporary(width, height, 24);
        Texture2D image = null;
        bool captured = false;
        try {
            snapshot.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = snapshot; canvas.planeDistance = 1; Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(snapshot, new UniversalRenderPipeline.SingleCameraRequest { destination = target }); RenderTexture.active = target;
            image = new Texture2D(width, height, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply(); File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG());
            captured = true;
        } finally {
            canvas.renderMode = mode; canvas.worldCamera = oldCamera; canvas.planeDistance = plane;
            snapshot.targetTexture = null; RenderTexture.active = oldTarget; RenderTexture.ReleaseTemporary(target); if (image) Destroy(image); Destroy(snapshot.gameObject);
        }
        Check(captured, "Captured actual URP UI " + name);
    }
    IEnumerator Start() {
        started = Time.realtimeSinceStartup;
        folder = Path.GetFullPath(Application.dataPath + (Application.isEditor ? "/../../" : "/../../../") + "Validation/journey-ui"); Directory.CreateDirectory(folder);
        while (!GameSession.Instance) yield return null;
        session = GameSession.Instance;
        foreach (var level in session.Catalog.levels) PlayerPrefs.DeleteKey(GameSession.PrefsPrefix + "Complete." + level.id);
        PlayerPrefs.DeleteKey(GameSession.PrefsPrefix + "LastScene");
        session.UI.Show(session.State);
        yield return null;
        Check(session.State == JourneyScreen.Menu, "Build launches into the main menu");
        Check(FindObjectsOfType<Graphic>().All(item => item.GetComponent<CanvasRenderer>()), "All initial graphics have renderers before Canvas update and snapshots");
        Check(session.Catalog.Available.Count == 7, "All seven independent levels are registered");
        Check("食光寻径暂停关卡继续声音显示".All(letter => session.Catalog.font.HasCharacter(letter)), "Bundled font contains Chinese interface glyphs");
        Check(session.Catalog.music && FindObjectsOfType<AudioSource>().Any(item => item.clip == session.Catalog.music && item.isPlaying), "Original ambient music plays at menu");
        yield return Capture("01-main-menu");
        Check(FindObjectsOfType<Text>().First(item => item.text == "食 光").cachedTextGenerator.vertexCount > 0, "Large Chinese title generates visible glyph geometry");
        Check(FindObjectsOfType<Text>().Length == 5, "Main menu contains only title and four functional actions");
        Press("Main.Map"); yield return null;
        Check(session.State == JourneyScreen.Map && FindObjectsOfType<Button>().Count(item => item.name.StartsWith("Map.Node.")) == 7, "Map presents seven selectable nodes");
        Press("Map.Node.level07"); yield return null;
        Check(session.UI.Selected.sceneName == "SeventhTrellis", "Node selection updates the preview and destination");
        Check(FindObjectsOfType<Text>().Length == 11, "Map contains only heading, seven nodes, selected title and two actions");
        yield return Capture("02-chapter-map");
        Press("Map.Back"); yield return null; Press("Main.Settings"); yield return null;
        var music = FindObjectsOfType<Slider>().First(item => item.name == "Settings.Music"); music.value = .8f;
        Canvas.ForceUpdateCanvases();
        var musicRect = music.GetComponent<RectTransform>();
        var musicPoint = RectTransformUtility.WorldToScreenPoint(null, musicRect.TransformPoint(new Vector2(musicRect.rect.width * .31f, -musicRect.rect.height / 2)));
        var musicInput = new PointerEventData(EventSystem.current) { position = musicPoint, button = PointerEventData.InputButton.Left };
        var musicHits = new List<RaycastResult>(); EventSystem.current.RaycastAll(musicInput, musicHits);
        Check(musicHits.Any(hit => hit.gameObject.GetComponentInParent<Slider>() == music), "Music slider is reachable through graphic raycasting");
        ExecuteEvents.Execute(music.gameObject, musicInput, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(music.gameObject, musicInput, ExecuteEvents.pointerUpHandler);
        Check(Mathf.Abs(session.Audio.MusicVolume - .31f) < .01f && Mathf.Abs(PlayerPrefs.GetFloat(GameSession.PrefsPrefix + "Music") - .31f) < .01f, "Music setting updates and persists");
        yield return Capture("03-settings");
        Press("Settings.Opening"); yield return null;
        Check(session.State == JourneyScreen.Opening && session.OpeningPreview, "Settings can replay the opening without starting or resetting a level");
        Press("Opening.Skip"); yield return null;
        Check(session.State == JourneyScreen.Settings && string.IsNullOrEmpty(session.LastScene), "Leaving opening replay preserves progress and returns to settings");
        Press("Settings.Back"); yield return null; Press("Main.Start"); yield return Settled();
        Check(session.State == JourneyScreen.Opening && session.OpeningIndex == 0, "New journey opens the narrative before gameplay");
        Check(session.Catalog.opening.Count == 10, "Both user comic pages are registered as ten ordered panels");
        for (int beat = 0; beat < 10; beat++) {
            Check(session.State == JourneyScreen.Opening && session.OpeningIndex == beat, "Opening presents single ordered panel " + (beat + 1));
            var panel = FindObjectOfType<OpeningPanelGraphic>();
            Check(panel && panel.beat.image && panel.beat.polygon.All(point => point.x >= 0 && point.y >= 0 && point.x <= panel.beat.sourceSize.x && point.y <= panel.beat.sourceSize.y), "Panel original artwork coordinates and UV bounds are valid " + (beat + 1));
            yield return new WaitForSecondsRealtime(.5f);
            if (beat == 0 || beat == 3 || beat == 8 || beat == 9) yield return Capture("opening-" + (beat + 1).ToString("00"));
            Press("Opening.Next");
            float deadline = Time.realtimeSinceStartup + 4;
            while (session.State == JourneyScreen.Opening && session.OpeningIndex == beat && Time.realtimeSinceStartup < deadline) yield return null;
        }
        yield return Settled();
        Check(session.Current.sceneName == "FirstLight" && session.State == JourneyScreen.Playing, "Final opening panel transitions into first level");
        float originalDuration = session.Catalog.opening[0].seconds;
        session.Catalog.opening[0].seconds = 2;
        session.BeginOpening(); yield return new WaitForSecondsRealtime(2.8f);
        Check(session.State == JourneyScreen.Opening && session.OpeningIndex == 1, "Opening advances automatically using unscaled time");
        session.Catalog.opening[0].seconds = originalDuration;
        Press("Opening.Skip"); yield return Settled();
        Check(session.Current.sceneName == "FirstLight" && session.State == JourneyScreen.Playing, "Skip opening reliably enters first level");
        Check(session.Current.sceneName == "FirstLight" && session.State == JourneyScreen.Playing, "Start enters first playable level");
        Press("HUD.Pause"); yield return null;
        var player = FindObjectOfType<PlayerMotor>(); Vector3 position = player.transform.position;
        yield return new WaitForSecondsRealtime(.25f);
        Check(session.State == JourneyScreen.Paused && Time.timeScale == 0 && GameSession.InputBlocked, "Pause freezes world and blocks gameplay input");
        Check(Vector3.Distance(player.transform.position, position) < .001f, "Player remains fixed while paused");
        yield return Capture("04-paused");
        Press("Pause.Resume"); yield return null;
        Check(session.State == JourneyScreen.Playing && Time.timeScale == 1, "Resume restores simulation");
        for (int index = 0; index < 7; index++) {
            session.Pause(); session.ShowMap(); yield return null;
            Press("Map.Node.level" + (index + 1).ToString("00")); Press("Map.Enter"); yield return Settled();
            Check(session.Current == session.Catalog.Available[index] && FindObjectOfType<PlayerMotor>() && FindObjectOfType<LightFormAbility>(), "Independently opens " + session.Current.sceneName);
            Check(FindObjectsOfType<GameSession>().Length == 1 && FindObjectsOfType<EventSystem>().Length == 1, "Global UI remains singular after level " + (index + 1));
        }
        session.CompleteLevel(); yield return null;
        Check(session.State == JourneyScreen.Completed && session.IsComplete(session.Current), "Completion is recorded and opens common completion UI");
        Check(!FindObjectsOfType<Button>().Any(item => item.name == "Complete.Next"), "Last available level offers map and replay without loading an absent scene");
        yield return Capture("05-completed");
        Press("Complete.Map"); yield return null;
        Check(FindObjectsOfType<JourneyGraphic>().Any(item => item.name == "Completed seed"), "Map displays saved completion marker");
        session.MainMenu(); yield return Settled();
        Check(session.State == JourneyScreen.Menu && session.HasProgress, "Return to main menu retains journey progress");
        Press("Main.Start"); yield return Settled();
        Check(session.Current.sceneName == "SeventhTrellis", "Continue returns to last played level");
        session.Pause(); yield return null; Press("Pause.Restart"); yield return Settled();
        Check(session.State == JourneyScreen.Playing && !FindObjectOfType<SeventhTrellisFlow>().Completed, "Restart returns selected level to initial state");
        Screen.SetResolution(1024, 768, false); yield return new WaitForSecondsRealtime(.4f); session.ShowMap(); yield return null;
        Press("Map.Node.level01"); Check(session.UI.Selected.sceneName == "FirstLight", "Map stays interactive at 4:3 aspect ratio");
        yield return Capture("06-map-4x3");
        session.Resume(); session.Pause(); yield return null;
        var shade = FindObjectsOfType<JourneyGraphic>().First(item => item.name == "Shade").rectTransform;
        var corners = new Vector3[4]; shade.GetWorldCorners(corners);
        Check(corners[0].x <= .1f && corners[0].y <= .1f && corners[2].x >= Screen.width - .1f && corners[2].y >= Screen.height - .1f, "Pause shade covers all four screen edges at 4:3");
        yield return Capture("07-pause-4x3");
        Finish();
    }
    void Finish() {
        if (finished) return; finished = true;
        Check(errors.Count == 0, "No runtime errors in menu, map and all seven scene entries");
        File.WriteAllLines(Path.Combine(folder, "checks.txt"), checks); File.WriteAllLines(Path.Combine(folder, "errors.txt"), errors);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(checks.Any(item => item.StartsWith("FAIL")) ? 1 : 0);
#endif
    }
}
