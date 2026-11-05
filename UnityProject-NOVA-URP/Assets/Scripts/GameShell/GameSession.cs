using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum JourneyScreen { Menu, Map, Settings, Playing, Paused, Completed, Loading }

[DefaultExecutionOrder(-10000)]
public sealed class GameSession : MonoBehaviour {
    public static GameSession Instance { get; private set; }
    public static bool InputBlocked => Instance && (Instance.State != JourneyScreen.Playing || Instance.Transitioning);
    public LevelCatalog Catalog { get; private set; }
    public JourneyUI UI { get; private set; }
    public JourneyAudio Audio { get; private set; }
    public JourneyLevel Current { get; private set; }
    public JourneyScreen State { get; private set; }
    public static string PrefsPrefix => Array.Exists(Environment.GetCommandLineArgs(), argument => argument == "--journey-qa") ? "LH.QA." : "LH.";
    public string LastScene => PlayerPrefs.GetString(PrefsPrefix + "LastScene", "");
    public bool HasProgress => Catalog.Find(LastScene) != null;
    public bool Transitioning { get; private set; }
    JourneyScreen mapReturn, settingsReturn;
    LevelProgressBridge bridge;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap() {
        if (Instance || Array.Exists(Environment.GetCommandLineArgs(), argument => argument.StartsWith("--validate"))) return;
        var catalog = Resources.Load<LevelCatalog>("GameShell/LevelCatalog");
        if (!catalog) return;
        var root = new GameObject("Light Hungry 路 Journey");
        DontDestroyOnLoad(root);
        var session = root.AddComponent<GameSession>();
        session.Initialize(catalog);
    }

    void Initialize(LevelCatalog catalog) {
        Instance = this;
        Catalog = catalog;
        Audio = gameObject.AddComponent<JourneyAudio>();
        Audio.Initialize(this);
        UI = gameObject.AddComponent<JourneyUI>();
        UI.Initialize(this);
        SceneManager.sceneLoaded += SceneLoaded;
        AttachScene(SceneManager.GetActiveScene());
    }

    void SceneLoaded(Scene scene, LoadSceneMode mode) { AttachScene(scene); }
    void AttachScene(Scene scene) {
        Current = Catalog.Find(scene.name);
        bridge = null;
        if (Current != null) {
            bridge = new GameObject("Journey level bridge").AddComponent<LevelProgressBridge>();
            bridge.Initialize(this);
            PlayerPrefs.SetString(PrefsPrefix + "LastScene", scene.name);
        }
        if (!Transitioning) SetScreen(Current == null ? JourneyScreen.Menu : JourneyScreen.Playing);
    }

    void Update() {
        if (Transitioning || !Input.GetKeyDown(KeyCode.Escape)) return;
        if (State == JourneyScreen.Playing && !SceneFade.Busy) Pause();
        else if (State == JourneyScreen.Paused) Resume();
        else if (State == JourneyScreen.Map || State == JourneyScreen.Settings) Back();
    }

    public bool IsComplete(JourneyLevel level) => PlayerPrefs.GetInt(PrefsPrefix + "Complete." + level.id, 0) == 1;
    public void CompleteLevel() {
        if (State != JourneyScreen.Playing || Current == null) return;
        PlayerPrefs.SetInt(PrefsPrefix + "Complete." + Current.id, 1);
        PlayerPrefs.Save();
        Audio.Play(Catalog.complete);
        SetScreen(JourneyScreen.Completed);
    }
    public void Pause() { if (State == JourneyScreen.Playing && !SceneFade.Busy && (!bridge || !bridge.Busy)) SetScreen(JourneyScreen.Paused); }
    public void Resume() { SetScreen(JourneyScreen.Playing); }
    public void ShowMap() { mapReturn = State == JourneyScreen.Paused ? JourneyScreen.Paused : JourneyScreen.Menu; SetScreen(JourneyScreen.Map); }
    public void ShowSettings() { settingsReturn = State; SetScreen(JourneyScreen.Settings); }
    public void Back() { SetScreen(State == JourneyScreen.Settings ? settingsReturn : mapReturn); }
    public void StartJourney() {
        var level = Catalog.Find(LastScene);
        if (level == null || !level.available) level = Catalog.Available.Find(item => true);
        if (level != null) LoadLevel(level);
    }
    public void LoadLevel(JourneyLevel level) {
        if (level == null || !level.available || !Application.CanStreamedLevelBeLoaded(level.sceneName) || Transitioning || SceneFade.Busy) return;
        StartCoroutine(Load(level.sceneName));
    }
    public void Restart() { LoadLevel(Current); }
    public void MainMenu() { if (!Transitioning && !SceneFade.Busy) StartCoroutine(Load("GameMenu")); }
    IEnumerator Load(string sceneName) {
        Transitioning = true;
        SetScreen(JourneyScreen.Loading);
        yield return UI.Fade(1, .28f);
        var operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone) yield return null;
        yield return null;
        SetScreen(Current == null ? JourneyScreen.Menu : JourneyScreen.Playing);
        yield return UI.Fade(0, .35f);
        Transitioning = false;
    }
    void SetScreen(JourneyScreen value) {
        State = value;
        Time.timeScale = value == JourneyScreen.Playing || value == JourneyScreen.Menu ? 1 : 0;
        if (value != JourneyScreen.Playing && bridge) bridge.ClearInput();
        UI.Show(value);
    }
    void OnDestroy() {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= SceneLoaded;
        Instance = null;
        Time.timeScale = 1;
    }
}
