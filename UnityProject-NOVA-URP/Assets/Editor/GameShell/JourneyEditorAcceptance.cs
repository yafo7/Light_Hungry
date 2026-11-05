using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Runs the runtime UI acceptance in actual Editor Play mode, through domain reload.
[InitializeOnLoad]
public static class JourneyEditorAcceptance {
    const string Active = "Journey.EditorAcceptance.Active";
    const string Started = "Journey.EditorAcceptance.Started";
    static JourneyEditorAcceptance() {
        EditorApplication.playModeStateChanged += Changed;
        EditorApplication.update += Watch;
    }
    public static void Run() {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--journey-qa") < 0) throw new Exception("Pass --journey-qa");
        var folder = Path.GetFullPath(Application.dataPath + "/../../Validation/journey-ui");
        if (File.Exists(folder + "/checks.txt")) File.Delete(folder + "/checks.txt");
        EditorSceneManager.OpenScene("Assets/Scenes/GameMenu.unity");
        SessionState.SetBool(Active, true);
        SessionState.SetFloat(Started, (float)EditorApplication.timeSinceStartup);
        EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange change) {
        if (!SessionState.GetBool(Active, false) || change != PlayModeStateChange.EnteredEditMode) return;
        SessionState.SetBool(Active, false);
        var report = Path.GetFullPath(Application.dataPath + "/../../Validation/journey-ui/checks.txt");
        bool passed = File.Exists(report) && !File.ReadAllText(report).Contains("FAIL ");
        Debug.Log("JOURNEY_EDITOR_PLAY_" + (passed ? "PASS" : "FAIL"));
        EditorApplication.Exit(passed ? 0 : 1);
    }
    static void Watch() {
        if (SessionState.GetBool(Active, false) && EditorApplication.timeSinceStartup - SessionState.GetFloat(Started, 0) > 240) {
            SessionState.SetBool(Active, false);
            Debug.LogError("Editor Play acceptance timeout");
            EditorApplication.Exit(1);
        }
    }
}
