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
    public bool IsPlaying => source.isPlaying;

    // Set source in Initialize method
    private AudioSource source;

    public void Initialize(AudioSource audioSource)
    {
        // Set audio source and its properties
        source = audioSource;
        source.clip = Clip;
        source.pitch = DefaultPitch;
        source.volume = DefaultVolume;
        source.loop = ShouldLoop;
    }

    public void Play(float pitch = 1f, float volume = 1f)
    {
        source.pitch = pitch;
        source.volume = volume;
        source.Play();
    }

    public void Pause()
    {
        source.Pause();
    }

    public void UnPause()
    {
        source.UnPause();
    }

    public void SetVolume(float volume)
    {
        source.volume = volume;
    }
}
