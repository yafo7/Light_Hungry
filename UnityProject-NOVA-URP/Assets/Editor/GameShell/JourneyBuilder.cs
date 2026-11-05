using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class JourneyBuilder {
    const string Folder = "Assets/Resources/GameShell/";
    static readonly string[] Scenes = { "FirstLight", "SecondLight", "ThirdDrain", "FourthReservoir", "FifthPumpHouse", "SixthGreenhouse", "SeventhTrellis" };
    static readonly string[] Titles = { "门槛苔地", "倒扣陶盆", "排水暗渠", "蓄水浅池", "废弃泵房", "温室苗床", "叶幕走廊" };
    static readonly string[] Subtitles = { "第一缕光", "露珠之间", "地下的回声", "水面上的路", "旧日的机关", "往返换位", "世界的另一面" };
    static readonly string[] Descriptions = {
        "越过花园的门槛，转动园灯，让一缕阳光唤醒沉睡的尾鳞。",
        "在露珠留下的光中停留，化作光核，穿过陶盆下的细小石缝。",
        "沿着地下水声前行，用配重、镜架与水轮，重新连接暗渠中的光。",
        "在浅池两岸寻找落脚处，让反光片与种荚为你搭起前行的路。",
        "走入废弃的泵房，用同一束光，唤醒层层相连的旧机关。",
        "让苗床与叶幕改变位置，在光与实体之间，找到往返的时机。",
        "跟随同一束光穿过镜窗，让藤架的两面，为彼此打开一条路。"
    };

    [MenuItem("Light Hungry/界面与流程/安装或更新游戏界面")]
    public static void Install() {
        AssetDatabase.Refresh();
        foreach (string path in new[] { Folder + "menu-background.png", Folder + "map-background.png" }) {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) throw new Exception("Missing UI artwork: " + path);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false; importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(Folder + "LevelCatalog.asset");
        bool fresh = !catalog;
        if (fresh) { catalog = ScriptableObject.CreateInstance<LevelCatalog>(); AssetDatabase.CreateAsset(catalog, Folder + "LevelCatalog.asset"); }
        catalog.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSerifCJKsc-Regular.otf");
        if (!catalog.font) throw new Exception("Chinese font is required");
        catalog.menuBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "menu-background.png");
        catalog.mapBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "map-background.png");
        catalog.music = Clip("greenhouse-loop"); catalog.hover = Clip("hover"); catalog.click = Clip("click");
        catalog.dissolve = Clip("dissolve"); catalog.restore = Clip("restore"); catalog.mechanism = Clip("mechanism");
        catalog.crossing = Clip("crossing"); catalog.complete = Clip("complete");
        string[] pictures = { "BackgroundWatercolor/level01-background.png", "BackgroundWatercolor/level02-background.png", "BackgroundWatercolor/level03-background.png", "Level04/clean-scene.png", "Level05/background.png", "Level06/background.png", "Level07/background-front.png" };
        Vector2[] positions = { new Vector2(.06f, .61f), new Vector2(.23f, .36f), new Vector2(.36f, .75f), new Vector2(.50f, .52f), new Vector2(.60f, .18f), new Vector2(.78f, .40f), new Vector2(.93f, .17f) };
        for (int i = 0; i < Scenes.Length; i++) {
            if (catalog.levels.Any(level => level.sceneName == Scenes[i])) continue;
            catalog.levels.Add(new JourneyLevel { id = "level" + (i + 1).ToString("00"), sceneName = Scenes[i], title = Titles[i], subtitle = Subtitles[i], description = Descriptions[i], mapPosition = positions[i], preview = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/" + pictures[i]) });
        }
        EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        const string menuPath = "Assets/Scenes/GameMenu.unity";
        if (!File.Exists(menuPath)) {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera").AddComponent<Camera>(); camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.08f, .15f, .10f); camera.gameObject.AddComponent<AudioListener>();
            EditorSceneManager.SaveScene(scene, menuPath);
            if (setup.Length > 0 && setup.All(entry => !string.IsNullOrEmpty(entry.path))) EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
        var entries = EditorBuildSettings.scenes.Where(entry => entry.path != menuPath).ToList();
        entries.Insert(0, new EditorBuildSettingsScene(menuPath, true));
        foreach (var level in catalog.levels.Where(level => level.available)) {
            string path = "Assets/Scenes/" + level.sceneName + ".unity";
            if (File.Exists(path) && entries.All(entry => entry.path != path)) entries.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = entries.ToArray();
        Debug.Log("JOURNEY_INSTALL_OK levels=" + catalog.levels.Count);
    }

    static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + name + ".wav");

    [MenuItem("Light Hungry/界面与流程/构建完整 Windows 游戏")]
    public static void Build() {
        Install();
        var directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Builds/LightHungry-Complete"));
        Directory.CreateDirectory(directory);
        var result = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes.Where(entry => entry.enabled).Select(entry => entry.path).ToArray(), Path.Combine(directory, "LightHungry.exe"), BuildTarget.StandaloneWindows64, BuildOptions.None);
        if (result.summary.result != BuildResult.Succeeded) throw new Exception("Journey build failed " + result.summary.result);
        File.Copy("Assets/Fonts/NotoSerif-OFL.txt", Path.Combine(directory, "NotoSerif-OFL.txt"), true);
        Debug.Log("JOURNEY_BUILD_OK " + directory);
    }
}
