using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip[] gameMusic;
    public AudioClip[] toyAmbient;

    [Header("SFX Clips")]
    public AudioClip buttonClickSFX;
    public AudioClip transitionSound;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    protected override void Initialize()
    {
        SetupAudioSources();

        LoadAudioSettings();
    }



    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }

        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
        }
    }

    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }



    public void PlayGameMusic(int gameIndex)
    {
        if (gameIndex < gameMusic.Length && gameMusic[gameIndex] != null)
            PlayMusic(gameMusic[gameIndex]);
    }

    public void PlayToyAmbient(int toyIndex)
    {
        if (toyIndex < toyAmbient.Length && toyAmbient[toyIndex] != null)
            PlayAmbient(toyAmbient[toyIndex]);
    }
    private void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    private void PlayAmbient(AudioClip audioClip)
    {
        ambientSource.clip = audioClip;
        ambientSource.volume = musicVolume;
        ambientSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
    public void PlayTransitionSound() => PlaySFX(transitionSound);
}

