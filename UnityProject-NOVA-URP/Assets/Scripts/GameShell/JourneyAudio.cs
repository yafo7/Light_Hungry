using UnityEngine;

public sealed class JourneyAudio : MonoBehaviour {
    AudioSource music, effects;
    GameSession session;
    public float MusicVolume { get; private set; }
    public float EffectsVolume { get; private set; }

    public void Initialize(GameSession owner) {
        session = owner;
        MusicVolume = PlayerPrefs.GetFloat(GameSession.PrefsPrefix + "Music", .45f);
        EffectsVolume = PlayerPrefs.GetFloat(GameSession.PrefsPrefix + "Effects", .6f);
        music = gameObject.AddComponent<AudioSource>();
        effects = gameObject.AddComponent<AudioSource>();
        music.clip = owner.Catalog.music;
        music.loop = true;
        music.spatialBlend = effects.spatialBlend = 0;
        music.ignoreListenerPause = effects.ignoreListenerPause = true;
        if (music.clip) music.Play();
        ApplyVolumes();
    }

    public void SetMusic(float value) { MusicVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(GameSession.PrefsPrefix + "Music", MusicVolume); }
    public void SetEffects(float value) { EffectsVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(GameSession.PrefsPrefix + "Effects", EffectsVolume); ApplyVolumes(); }
    void ApplyVolumes() { if (effects) effects.volume = EffectsVolume; }
    void Update() {
        if (!music) return;
        float duck = session.State == JourneyScreen.Playing ? .65f : session.State == JourneyScreen.Paused ? .4f : 1;
        music.volume = Mathf.Lerp(music.volume, MusicVolume * duck, 1 - Mathf.Exp(-3 * Time.unscaledDeltaTime));
    }
    public void Play(AudioClip clip) { if (clip && effects) effects.PlayOneShot(clip); }
    public void Click() => Play(session.Catalog.click);
    public void Hover() => Play(session.Catalog.hover);
    void OnApplicationQuit() { PlayerPrefs.Save(); }
}
