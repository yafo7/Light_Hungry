using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Updates only the three named background renderers. Never rebuilds gameplay scenes.
public static class BackgroundStyleReview {
    const string Art = "Assets/Art/BackgroundWatercolor/";
    const string Mats = "Assets/Materials/BackgroundWatercolor/";
    static readonly string[] SceneNames = { "FirstLight", "SecondLight", "ThirdDrain" };
    static readonly string[] ObjectNames = { "GardenWall", "PotInterior_Background", "Drain_Background" };
    static string Work { get { return Environment.GetEnvironmentVariable("LIGHTFEAST_WORKSPACE") ?? Path.GetFullPath(Application.dataPath + "/../.."); } }
    static string Evidence { get { return Work + "/Validation/background-watercolor-v01"; } }
    [Serializable] class SceneAudit {
        public string scene, renderer, originalTexture, styledTexture;
        public string otherComponentsBefore, otherComponentsAfter, collidersBefore, collidersAfter, foregroundAssetsBefore, foregroundAssetsAfter;
        public int colliderCount, rendererCount;
        public bool onlyBackgroundChanged;
    }
    [Serializable] class Audit { public bool passed; public SceneAudit[] scenes; }

    // Called from each legacy builder immediately after its background is created.
    // If the optional style assets are absent, the original renderer stays usable.
    public static bool ApplyBackdrop(Renderer target, int level) {
        if (!target || level < 1 || level > 3) throw new ArgumentException("A valid background renderer and level 1..3 are required.");
        if (target.name != ObjectNames[level - 1]) throw new InvalidOperationException("Refusing background style on unexpected object: " + target.name);
        string texturePath = Art + "level0" + level + "-background.png";
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        var shader = Shader.Find("LightFeast/WatercolorBackdrop");
        if (!texture || !shader) { Debug.LogWarning("BACKGROUND_STYLE_NOT_AVAILABLE: " + target.name + " retains original material"); return false; }
        Directory.CreateDirectory(Mats);
        var old = target.sharedMaterial;
        if (!old) throw new InvalidOperationException("Background material missing: " + target.name);
        Texture original = old.HasProperty("_OriginalTex") && old.GetTexture("_OriginalTex") ? old.GetTexture("_OriginalTex") : old.mainTexture;
        if (!original) throw new InvalidOperationException("Original background texture missing: " + target.name);
        Color color = old.HasProperty("_Color") ? old.GetColor("_Color") : Color.white;
        float cutoff = old.HasProperty("_Cutoff") ? old.GetFloat("_Cutoff") : .025f;
        int queue = old.renderQueue;
        string materialPath = Mats + target.name + "_Watercolor.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (!material) { material = new Material(shader) { name = target.name + "_Watercolor" }; AssetDatabase.CreateAsset(material, materialPath); }
        material.shader = shader;
        material.SetTexture("_MainTex", texture);
        material.SetTexture("_OriginalTex", original);
        material.SetColor("_Color", color);
        material.SetFloat("_Cutoff", cutoff);
        material.SetFloat("_StyleBlend", new[] { .85f, .70f, .72f }[level - 1]);
        material.SetFloat("_Softness", .65f);
        material.renderQueue = queue;
        target.sharedMaterial = material;
        EditorUtility.SetDirty(material);
        EditorUtility.SetDirty(target);
        return true;
    }

    [MenuItem("食光兽/前三关后景/应用淡水彩后景（不重建关卡）")]
    public static void ApplyAll() {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit play mode before styling backgrounds.");
        EditorSceneManager.SaveOpenScenes();
        var setup = EditorSceneManager.GetSceneManagerSetup();
        Directory.CreateDirectory(Evidence + "/before");
        Directory.CreateDirectory(Art); Directory.CreateDirectory(Mats);
        for (int i = 1; i <= 3; i++) {
            string name = "level0" + i + "-background.png";
            string source = Work + "/art/backgrounds-pale-watercolor-v01-2026-11-04/" + name;
            if (!File.Exists(source)) throw new FileNotFoundException("Generated background image missing", source);
            File.Copy(source, Art + name, true);
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        for (int i = 1; i <= 3; i++) {
            var importer = (TextureImporter)AssetImporter.GetAtPath(Art + "level0" + i + "-background.png");
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096; importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
        var audits = new List<SceneAudit>();
        try {
            for (int i = 0; i < 3; i++) {
                string scenePath = "Assets/Scenes/" + SceneNames[i] + ".unity";
                string backup = Evidence + "/before/" + SceneNames[i] + ".unity";
                if (!File.Exists(backup)) File.Copy(scenePath, backup);
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var renderers = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Renderer>(true)).ToArray();
                var matches = renderers.Where(r => r.name == ObjectNames[i]).ToArray();
                if (matches.Length != 1) throw new InvalidOperationException("Expected exactly one " + ObjectNames[i] + ", got " + matches.Length);
                var target = matches[0];
                string originalPath = AssetDatabase.GetAssetPath(target.sharedMaterial.HasProperty("_OriginalTex") ? target.sharedMaterial.GetTexture("_OriginalTex") : target.sharedMaterial.mainTexture);
                string otherBefore = ComponentsSnapshot(scene, target, false);
                string colliderBefore = ComponentsSnapshot(scene, target, true);
                string assetsBefore = ForegroundAssets(renderers, target);
                File.WriteAllText(Evidence + "/before/" + SceneNames[i] + "-components.txt", otherBefore);
                if (!ApplyBackdrop(target, i + 1)) throw new InvalidOperationException("Backdrop style did not apply to " + target.name);
                string otherAfter = ComponentsSnapshot(scene, target, false);
                string colliderAfter = ComponentsSnapshot(scene, target, true);
                string assetsAfter = ForegroundAssets(renderers, target);
                var audit = new SceneAudit {
                    scene = SceneNames[i], renderer = target.name, originalTexture = originalPath,
                    styledTexture = AssetDatabase.GetAssetPath(target.sharedMaterial.mainTexture),
                    otherComponentsBefore = Hash(otherBefore), otherComponentsAfter = Hash(otherAfter),
                    collidersBefore = Hash(colliderBefore), collidersAfter = Hash(colliderAfter),
                    foregroundAssetsBefore = Hash(assetsBefore), foregroundAssetsAfter = Hash(assetsAfter),
                    colliderCount = scene.GetRootGameObjects().Sum(g => g.GetComponentsInChildren<Collider2D>(true).Length + g.GetComponentsInChildren<Collider>(true).Length), rendererCount = renderers.Length,
                    onlyBackgroundChanged = otherBefore == otherAfter && colliderBefore == colliderAfter && assetsBefore == assetsAfter
                };
                audits.Add(audit);
                if (!audit.onlyBackgroundChanged) throw new InvalidOperationException("Non-background data changed while styling " + scene.name);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            // Retain prior ordering and all extra scenes; add any missing confirmed level.
            var settings = EditorBuildSettings.scenes.ToList();
            foreach (string name in new[] { "FirstLight", "SecondLight", "ThirdDrain", "FourthReservoir" }) {
                string path = "Assets/Scenes/" + name + ".unity";
                if (!File.Exists(path)) throw new FileNotFoundException("Confirmed level missing", path);
                int index = settings.FindIndex(s => s.path == path);
                if (index >= 0) settings[index] = new EditorBuildSettingsScene(path, true);
                else settings.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = settings.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("BACKGROUND_WATERCOLOR_APPLIED: three backgrounds only; gameplay and four scenes preserved");
        } finally {
            File.WriteAllText(Evidence + "/assignment-audit.json", JsonUtility.ToJson(new Audit { passed = audits.Count == 3 && audits.All(a => a.onlyBackgroundChanged), scenes = audits.ToArray() }, true));
            if (setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
    }

    [MenuItem("食光兽/前三关后景/构建四关淡水彩试玩")]
    public static void BuildAll() { ApplyAll(); BuildPlayer(); }
    public static void BuildPlayer() {
        string dir = Work + "/Builds/Windows-Watercolor-NOVA"; Directory.CreateDirectory(dir); Directory.CreateDirectory(Evidence);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = dir + "/LightFeast.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
        });
        File.WriteAllText(Evidence + "/build-result.json", "{\"result\":\"" + report.summary.result + "\",\"errors\":" + report.summary.totalErrors + ",\"warnings\":" + report.summary.totalWarnings + ",\"bytes\":" + report.summary.totalSize + "}");
        if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Background review build failed");
        Debug.Log("BACKGROUND_WATERCOLOR_BUILD_SUCCEEDED");
    }
    static string ComponentsSnapshot(Scene scene, Renderer target, bool collidersOnly) {
        var rows = new List<string>();
        foreach (var root in scene.GetRootGameObjects()) foreach (var c in root.GetComponentsInChildren<Component>(true)) {
            if (!c || c == target) continue;
            if (collidersOnly && !(c is Collider2D) && !(c is Collider)) continue;
            rows.Add(PathOf(c.transform) + "/" + c.GetType().FullName + "\n" + EditorJsonUtility.ToJson(c));
        }
        rows.Sort(StringComparer.Ordinal); return string.Join("\n", rows);
    }
    static string ForegroundAssets(Renderer[] renderers, Renderer target) {
        var paths = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var r in renderers) if (r != target) foreach (var m in r.sharedMaterials) if (m) {
            string p = AssetDatabase.GetAssetPath(m); if (File.Exists(p)) paths.Add(p);
            foreach (string property in m.GetTexturePropertyNames()) { p = AssetDatabase.GetAssetPath(m.GetTexture(property)); if (File.Exists(p)) paths.Add(p); }
        }
        return string.Join("\n", paths.Select(p => p + ":" + FileHash(p)));
    }
    static string PathOf(Transform t) { return t.parent ? PathOf(t.parent) + "/" + t.name : t.name; }
    static string FileHash(string path) { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant(); }
    static string Hash(string value) { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant(); }
}
