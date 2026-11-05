using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class JourneyLevel {
    public string id, sceneName, title, subtitle;
    [TextArea] public string description;
    public bool available = true;
    public Texture2D preview;
    public Vector2 mapPosition;
}

[CreateAssetMenu(menuName = "Light Hungry/关卡清单")]
public sealed class LevelCatalog : ScriptableObject {
    public Font font;
    public Texture2D menuBackground, mapBackground;
    public AudioClip music, hover, click, dissolve, restore, mechanism, crossing, complete;
    public List<JourneyLevel> levels = new List<JourneyLevel>();

    public JourneyLevel Find(string scene) => levels.Find(level => level.sceneName == scene);
    public List<JourneyLevel> Available => levels.FindAll(level => level.available && Application.CanStreamedLevelBeLoaded(level.sceneName));
    public JourneyLevel Next(string scene) {
        var list = Available;
        int index = list.FindIndex(level => level.sceneName == scene);
        return index >= 0 && index + 1 < list.Count ? list[index + 1] : null;
    }
}
