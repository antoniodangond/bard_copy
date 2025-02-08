using System.Collections;
using UnityEngine;

// Sound is a wrapper class for controlling an AudioSource
[System.Serializable]
public class Sound
{
    public AudioClip Clip;
    [Range (0f, 1f)]
    public float DefaultPitch = 1f;
    [Range (0f, 1f)]
    public float DefaultVolume = 1f;
    public bool ShouldLoop = false;
    public bool IsPlaying => Source.isPlaying;

    // Set source in Initialize method
    public AudioSource Source;

    public void Initialize(AudioSource audioSource)
    {
        // Set audio source and its properties
        Source = audioSource;
        Source.clip = Clip;
        Source.pitch = DefaultPitch;
        Source.volume = DefaultVolume;
        Source.loop = ShouldLoop;
    }

    public void Play(float pitch = 1f, float volume = 1f)
    {
        Source.pitch = pitch;
        Source.volume = volume;
        Source.Play();
    }

    public void Pause()
    {
        Source.Pause();
    }

    public void UnPause()
    {
        Source.UnPause();
    }

    public void SetVolume(float volume)
    {
        Source.volume = volume;
    }
}
