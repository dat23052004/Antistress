using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    private const string SfxVolumeKey = "SFXVolume";

    [Header("Cue Library")]
    [SerializeField] private AudioCueLibrary audioCueLibrary;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource loopSfxSource;

    [Header("Settings")]
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    private readonly Dictionary<SfxCue, float> nextAllowedSfxTimes = new Dictionary<SfxCue, float>();
    private readonly HashSet<SfxCue> warnedMissingSfx = new HashSet<SfxCue>();
    private bool isInitialized;
    private bool warnedMissingLibrary;
    private SfxCue? activeLoopCue;
    private float currentLoopSfxMultiplier = 1f;

    protected override void Initialize()
    {
        InitializeAudio();
    }

    private void InitializeAudio()
    {
        if (isInitialized)
            return;

        SetupAudioSources();
        LoadAudioSettings();
        ApplyVolumeSettings();
        isInitialized = true;
    }

    private void OnValidate()
    {
        sfxVolume = Mathf.Clamp01(sfxVolume);

        if (Application.isPlaying)
            ApplyVolumeSettings();
    }

    private void SetupAudioSources()
    {
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        if (loopSfxSource == null)
            loopSfxSource = gameObject.AddComponent<AudioSource>();

        ConfigureSource(sfxSource, false);
        ConfigureSource(loopSfxSource, true);
    }

    private static void ConfigureSource(AudioSource source, bool loop)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
    }

    private void LoadAudioSettings()
    {
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
    }

    private void ApplyVolumeSettings()
    {
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        if (loopSfxSource != null)
            loopSfxSource.volume = sfxVolume * currentLoopSfxMultiplier;
    }

    public void SetSfxVolume(float value)
    {
        InitializeAudio();
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();
        ApplyVolumeSettings();
    }

    public void PlaySfx(SfxCue cue)
    {
        InitializeAudio();

        if (!TryGetPlayableSfx(cue, out SfxCueEntry entry, out AudioClip clip))
            return;

        float now = Time.unscaledTime;
        if (entry.cooldown > 0f &&
            nextAllowedSfxTimes.TryGetValue(cue, out float allowedTime) &&
            now < allowedTime)
        {
            return;
        }

        nextAllowedSfxTimes[cue] = now + Mathf.Max(0f, entry.cooldown);

        if (sfxSource == null)
            SetupAudioSources();

        sfxSource.pitch = Random.Range(
            Mathf.Min(entry.pitchMin, entry.pitchMax),
            Mathf.Max(entry.pitchMin, entry.pitchMax));
        sfxSource.PlayOneShot(clip, sfxVolume * entry.volumeMultiplier);
    }

    public void StartSfxLoop(SfxCue cue)
    {
        InitializeAudio();

        if (!TryGetPlayableSfx(cue, out SfxCueEntry entry, out AudioClip clip))
            return;

        if (loopSfxSource == null)
            SetupAudioSources();

        if (activeLoopCue == cue && loopSfxSource.clip == clip && loopSfxSource.isPlaying)
            return;

        activeLoopCue = cue;
        currentLoopSfxMultiplier = entry.volumeMultiplier;
        loopSfxSource.Stop();
        loopSfxSource.clip = clip;
        loopSfxSource.loop = true;
        loopSfxSource.pitch = Random.Range(
            Mathf.Min(entry.pitchMin, entry.pitchMax),
            Mathf.Max(entry.pitchMin, entry.pitchMax));
        loopSfxSource.volume = sfxVolume * entry.volumeMultiplier;
        loopSfxSource.Play();
    }

    public void StopSfxLoop(SfxCue cue)
    {
        if (activeLoopCue != cue || loopSfxSource == null)
            return;

        loopSfxSource.Stop();
        loopSfxSource.clip = null;
        activeLoopCue = null;
        currentLoopSfxMultiplier = 1f;
    }

    public void StopAllSfxLoops()
    {
        if (loopSfxSource != null)
        {
            loopSfxSource.Stop();
            loopSfxSource.clip = null;
        }

        activeLoopCue = null;
        currentLoopSfxMultiplier = 1f;
    }

    public void PlaySFX(AudioClip clip)
    {
        InitializeAudio();

        if (clip == null)
            return;

        if (sfxSource == null)
            SetupAudioSources();

        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayButtonClick() => PlaySfx(SfxCue.UiClick);

    private bool TryGetPlayableSfx(SfxCue cue, out SfxCueEntry entry, out AudioClip clip)
    {
        entry = null;
        clip = null;

        if (!EnsureCueLibrary() || !audioCueLibrary.TryGetSfx(cue, out entry))
        {
            WarnMissingSfx(cue);
            return false;
        }

        clip = PickClip(entry);
        if (clip != null)
            return true;

        WarnMissingSfx(cue);
        return false;
    }

    private AudioClip PickClip(SfxCueEntry entry)
    {
        if (entry == null || entry.clips == null || entry.clips.Length == 0)
            return null;

        int startIndex = Random.Range(0, entry.clips.Length);
        for (int i = 0; i < entry.clips.Length; i++)
        {
            AudioClip clip = entry.clips[(startIndex + i) % entry.clips.Length];
            if (clip != null)
                return clip;
        }

        return null;
    }

    private bool EnsureCueLibrary()
    {
        if (audioCueLibrary != null)
            return true;

        if (!warnedMissingLibrary)
        {
            warnedMissingLibrary = true;
            Debug.LogWarning("AudioManager is missing an AudioCueLibrary reference.");
        }

        return false;
    }

    private void WarnMissingSfx(SfxCue cue)
    {
        if (warnedMissingSfx.Add(cue))
            Debug.LogWarning($"AudioManager missing SFX cue: {cue}");
    }
}
