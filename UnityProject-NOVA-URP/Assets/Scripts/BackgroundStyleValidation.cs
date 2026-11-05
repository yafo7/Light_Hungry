using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

// Visual-only review: freezes each scene and compares the same camera and time.
// It does not run a playthrough or claim gameplay acceptance from image captures.
public class BackgroundStyleValidation : MonoBehaviour {
    [Serializable] class Check { public string name, detail; public bool passed; }
    [Serializable] class ViewRecord { public string scene, view, before, after, foregroundBefore, foregroundAfter, collidersBefore, collidersAfter; public bool unchanged; }
    [Serializable] class Report { public bool completed; public string graphicsDevice; public List<Check> checks = new List<Check>(); public List<ViewRecord> views = new List<ViewRecord>(); public List<string> errors = new List<string>(); }
    struct View {
        public string name; public Vector3 position; public float size; public bool rootFrame;
        public View(string n, float x, float y, float z, float s, bool roots = false) { name = n; position = new Vector3(x, y, z); size = s; rootFrame = roots; }
    }
    static bool started;
    readonly Report report = new Report();
    string output;
    float began;
    bool finished;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetBootstrap() { started = false; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Bootstrap() {
        if (started || Array.IndexOf(Environment.GetCommandLineArgs(), "--review-backgrounds") < 0) return;
        started = true;
        var g = new GameObject("BackgroundStyle_VisualReview"); DontDestroyOnLoad(g); g.AddComponent<BackgroundStyleValidation>();
    }
    void OnDestroy() { Application.logMessageReceived -= Log; Time.timeScale = 1; }
    void Log(string value, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(value); }
    void Update() { if (!finished && began > 0 && Time.realtimeSinceStartup - began > 120) { CheckIt("visual-review watchdog", false, "120-second realtime limit"); Finish(); StopAllCoroutines(); } }

    IEnumerator Start() {
        var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, "-validationOutput");
        output = i >= 0 && i + 1 < args.Length ? args[i + 1] : Application.persistentDataPath + "/BackgroundStyleReview";
        Directory.CreateDirectory(output); began = Time.realtimeSinceStartup;
        Application.logMessageReceived += Log; report.graphicsDevice = SystemInfo.graphicsDeviceName;
        Application.runInBackground = true;
        CheckIt("native graphics", SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null, SystemInfo.graphicsDeviceType.ToString());
        CheckIt("all four scenes available", new[] { "FirstLight", "SecondLight", "ThirdDrain", "FourthReservoir" }.All(Application.CanStreamedLevelBeLoaded), "Background update retains four-level build");
        string[] scenes = { "FirstLight", "SecondLight", "ThirdDrain" };
        string[] objects = { "GardenWall", "PotInterior_Background", "Drain_Background" };
        for (int level = 0; level < scenes.Length; level++) {
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync(scenes[level]);
            yield return null; yield return null;
            // Give the existing rig and NOVA attachment a moment to initialize.
            yield return new WaitForSeconds(.25f);
            var scene = SceneManager.GetActiveScene();
            var renderers = InScene<Renderer>(scene);
            var backgrounds = renderers.Where(r => r.name == objects[level]).ToArray();
            if (!CheckIt(scenes[level] + " exact background target", backgrounds.Length == 1, objects[level])) { Finish(); yield break; }
            var background = backgrounds[0];
            var material = background.sharedMaterial;
            bool styled = material && material.shader && material.shader.name == "LightFeast/WatercolorBackdrop" && material.HasProperty("_StyleBlend") && material.HasProperty("_OriginalTex");
            if (!CheckIt(scenes[level] + " dedicated background shader", styled, material && material.shader ? material.shader.name : "missing")) { Finish(); yield break; }
            CheckIt(scenes[level] + " distinct original and styled textures", material.mainTexture && material.GetTexture("_OriginalTex") && material.mainTexture != material.GetTexture("_OriginalTex"), "Both source versions available for exact blend toggle");
            CheckIt(scenes[level] + " only one styled renderer", renderers.Count(r => r.sharedMaterials.Any(m => m && m.shader && m.shader.name == "LightFeast/WatercolorBackdrop")) == 1, "Platforms, water, roots and interactables retain their materials");
            CheckIt(scenes[level] + " NOVA character particles retained", InScene<ParticleSystemRenderer>(scene).Any(r => r.sharedMaterial && r.sharedMaterial.shader && r.sharedMaterial.shader.name == "Nova/Particles/UberUnlit"), "Existing head flame shader remains present");
            float blend = material.GetFloat("_StyleBlend");
            CheckIt(scenes[level] + " style blend enabled", blend > 0 && blend <= 1, "Blend=" + blend);
            // Only a runtime material instance is toggled; serialized scene assets stay untouched.
            var instance = new Material(material); background.sharedMaterial = instance;
            foreach (var behaviour in InScene<MonoBehaviour>(scene)) behaviour.enabled = false;
            foreach (var animator in InScene<Animator>(scene)) animator.enabled = false;
            foreach (var ps in InScene<ParticleSystem>(scene)) ps.Pause(false);
            Time.timeScale = 0;
            var camera = Camera.main;
            if (!CheckIt(scenes[level] + " scene camera", camera != null, "Orthographic review camera")) { Finish(); yield break; }
            var rootFrame = InScene<Transform>(scene).FirstOrDefault(t => t.name == "06_CameraFixed_RootFrame");
            foreach (var view in Views(level, camera.aspect)) {
                camera.transform.SetPositionAndRotation(view.position, Quaternion.identity);
                camera.orthographic = true; camera.orthographicSize = view.size;
                if (rootFrame) {
                    rootFrame.gameObject.SetActive(view.rootFrame);
                    rootFrame.localPosition = new Vector3(0, 0, 2);
                    rootFrame.localScale = new Vector3(view.size * camera.aspect * 2, view.size * 2, 1);
                }
                string prefix = (level + 1).ToString("00") + "-" + view.name;
                var record = new ViewRecord { scene = scene.name, view = view.name, before = prefix + "-before.png", after = prefix + "-after.png" };
                record.foregroundBefore = RendererSnapshot(scene, background);
                record.collidersBefore = ColliderSnapshot(scene);
                instance.SetFloat("_StyleBlend", 0);
                yield return Capture(record.before, camera);
                instance.SetFloat("_StyleBlend", blend);
                yield return Capture(record.after, camera);
                record.foregroundAfter = RendererSnapshot(scene, background);
                record.collidersAfter = ColliderSnapshot(scene);
                record.unchanged = record.foregroundBefore == record.foregroundAfter && record.collidersBefore == record.collidersAfter;
                report.views.Add(record);
                CheckIt(prefix + " foreground and collision unchanged", record.unchanged, "Same camera, frozen time, same non-background materials/geometry");
            }
            background.sharedMaterial = material; Destroy(instance);
        }
        Finish();
    }

    static View[] Views(int level, float aspect) {
        if (level == 0) {
            float wide = Mathf.Max(24, 48 / aspect);
            return new[] { new View("overview", 48, Mathf.Min(24, 48.1f - wide), -70, wide), new View("play-mirror", 43, 10.5f, -70, 10.5f) };
        }
        if (level == 1) return new[] { new View("overview", 14.079365f, 2.02381f, -40, Mathf.Max(7.03968f, 28.15873f / (2 * aspect))), new View("play-dew-rubble", 16.5f, 2, -40, 4.9f) };
        return new[] {
            new View("overview", 38.75f, 6.4105f, -40, Mathf.Max(12.9465f, 77.5f / (2 * aspect))),
            new View("play-A-pulleys", 25.8f, 9.65f, -40, 9.7f, true),
            new View("play-B-platforms", 43.5f, 6.4f, -40, 8.5f, true),
            new View("play-C-ferry", 66, 6.4f, -40, 6.4f, true)
        };
    }
    IEnumerator Capture(string filename, Camera camera) {
        string path = Path.Combine(output, filename);
        if (File.Exists(path)) File.Delete(path);
        yield return new WaitForEndOfFrame(); NativeFrameCapture.Save(path, camera);
        float until = Time.realtimeSinceStartup + 8;
        while (!File.Exists(path) && Time.realtimeSinceStartup < until) yield return null;
        CheckIt(filename + " written", File.Exists(path) && new FileInfo(path).Length > 10000, "Native URP render callback");
        yield return null;
    }
    static T[] InScene<T>(Scene scene) where T : Component { return scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray(); }
    static string RendererSnapshot(Scene scene, Renderer exclude) {
        var rows = new List<string>();
        foreach (var renderer in InScene<Renderer>(scene)) {
            if (renderer == exclude) continue;
            string row = PathOf(renderer.transform) + "|" + renderer.enabled + "|" + TransformKey(renderer.transform);
            foreach (var m in renderer.sharedMaterials) {
                row += "|" + (m ? m.GetInstanceID() + ":" + m.name + ":" + m.shader.name + ":" + m.renderQueue : "null");
                if (m) {
                    if (m.HasProperty("_Color")) row += ":" + m.GetColor("_Color").ToString("R");
                    foreach (string p in m.GetTexturePropertyNames()) { var t = m.GetTexture(p); row += ":" + p + "=" + (t ? t.GetInstanceID().ToString() : "null"); }
                }
            }
            var mesh = renderer.GetComponent<MeshFilter>(); if (mesh && mesh.sharedMesh) row += "|mesh=" + mesh.sharedMesh.GetInstanceID() + ":" + mesh.sharedMesh.vertexCount;
            rows.Add(row);
        }
        rows.Sort(StringComparer.Ordinal); return Hash(string.Join("\n", rows));
    }
    static string ColliderSnapshot(Scene scene) {
        var rows = new List<string>();
        foreach (var c in InScene<Collider2D>(scene)) {
            string row = PathOf(c.transform) + "|" + c.GetType().Name + "|" + TransformKey(c.transform) + "|" + c.enabled + "|" + c.isTrigger + "|" + c.offset.ToString("R") + "|" + c.gameObject.layer;
            if (c is BoxCollider2D) row += "|" + ((BoxCollider2D)c).size.ToString("R");
            else if (c is CapsuleCollider2D) row += "|" + ((CapsuleCollider2D)c).size.ToString("R");
            else if (c is CircleCollider2D) row += "|" + ((CircleCollider2D)c).radius.ToString("R", CultureInfo.InvariantCulture);
            else if (c is PolygonCollider2D) { var p = (PolygonCollider2D)c; for (int n = 0; n < p.pathCount; n++) row += "|" + string.Join(";", p.GetPath(n).Select(v => v.ToString("R"))); }
            rows.Add(row);
        }
        foreach (var c in InScene<Collider>(scene)) rows.Add(PathOf(c.transform) + "|" + c.GetType().Name + "|" + TransformKey(c.transform) + "|" + c.enabled + "|" + c.isTrigger + "|" + c.bounds.ToString("R"));
        rows.Sort(StringComparer.Ordinal); return Hash(string.Join("\n", rows));
    }
    static string TransformKey(Transform t) { return t.localPosition.ToString("R") + ":" + t.localRotation.ToString("R") + ":" + t.localScale.ToString("R") + ":" + t.gameObject.activeInHierarchy; }
    static string PathOf(Transform t) { return t.parent ? PathOf(t.parent) + "/" + t.name : t.name; }
    static string Hash(string value) { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant(); }
    bool CheckIt(string name, bool passed, string detail) { report.checks.Add(new Check { name = name, passed = passed, detail = detail }); Debug.Log("BACKGROUND_REVIEW " + name + "=" + passed + " " + detail); return passed; }
    void Finish() {
        if (finished) return; finished = true; Time.timeScale = 1;
        report.completed = report.errors.Count == 0 && report.checks.Count > 0 && report.checks.All(c => c.passed) && report.views.Count == 8;
        File.WriteAllText(Path.Combine(output, "review.json"), JsonUtility.ToJson(report, true));
        Application.Quit(report.completed ? 0 : 1);
    }
}
