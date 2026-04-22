using System;
using System.Collections.Generic;
using UnityEngine;

public enum SfxCue
{
    UiClick,
    UiBack,
    UiPopupOpen,
    UiPopupClose,
    RandomSelect,
    ToyButtonDown,
    ToyButtonUp,
    ToySwitchToggle,
    KeyboardDown,
    KeyboardUp,
    WoodenCollision,
    LilyTouch,
    LilyDrag,
    LilySplash,
    LilySpawn,
    PallinaCollision,
    RevealSwipe,
    GessoToolSelect,
    GessoChalkLoop,
    GessoEraseLoop,
}

[Serializable]
public class SfxCueEntry
{
    public SfxCue cue;
    public AudioClip[] clips;
    [Range(0f, 2f)] public float volumeMultiplier = 1f;
    [Range(0.1f, 3f)] public float pitchMin = 1f;
    [Range(0.1f, 3f)] public float pitchMax = 1f;
    [Min(0f)] public float cooldown = 0.05f;
    public bool loop;
}

[CreateAssetMenu(fileName = "AudioCueLibrary", menuName = "ScriptableObjects/AudioCueLibrary")]
public sealed class AudioCueLibrary : ScriptableObject
{
    public List<SfxCueEntry> sfxEntries = new List<SfxCueEntry>();

    public bool TryGetSfx(SfxCue cue, out SfxCueEntry entry)
    {
        if (sfxEntries != null)
        {
            for (int i = 0; i < sfxEntries.Count; i++)
            {
                if (sfxEntries[i] != null && sfxEntries[i].cue == cue)
                {
                    entry = sfxEntries[i];
                    return true;
                }
            }
        }

        entry = null;
        return false;
    }
}
