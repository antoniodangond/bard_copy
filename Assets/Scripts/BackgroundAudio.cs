using UnityEditor.UI;
using UnityEngine;

public class BackgroundAudio : AudioController
{
    public BackgroundAudioData AudioData;
    public AudioLowPassFilter LowPassFilter;

    void Awake() 
    {
        // Instantiate audio source
        InitializeSound(AudioData.BackgroundMusic);
        InitializeSound(AudioData.OverworldAmbience);
        if (LowPassFilter == null)
        {
            LowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        }
        // if (AudioData.RandomAmbienceBreaths.audioSource == null)
        // {
        AudioData.RandomAmbienceBreaths = gameObject.AddComponent<RandomAudioManager>();
        AudioData.RandomAmbienceBreaths.audioSource = gameObject.AddComponent<AudioSource>();
        AudioData.RandomAmbienceFrogs = gameObject.AddComponent<RandomAudioManager>();
        AudioData.RandomAmbienceFrogs.audioSource = gameObject.AddComponent<AudioSource>();


        // }

        // Subscribe to custom event
        CustomEvents.OnPlayerStateChange.AddListener(OnPlayerStateChange);
        CustomEvents.OnPause.AddListener(OnPause);
        CustomEvents.OnUnPause.AddListener(OnUnPause);
    }

    void OnDestroy()
    {
        // Remove listener on destroy to prevent memory leaks
        CustomEvents.OnPlayerStateChange.RemoveListener(OnPlayerStateChange);
    }

    public void PlayBackgroundMusic()
    {
        AudioData.BackgroundMusic.Play();
    }

    public void PlayOverworldAmbience()
    {

        AudioData.OverworldAmbience.Play();
    }

    public void PlayRandomBreaths()
    {
        AudioData.RandomAmbienceBreaths.audioSource.volume = .7f;
        AudioData.RandomAmbienceBreaths.StartRandomAudio(AudioData.AmbienceBreaths);
    }
    public void PlayRandomFrogs()
    {
        AudioData.RandomAmbienceFrogs.minDelay = .4f;
        AudioData.RandomAmbienceFrogs.maxDelay = 5f;
        AudioData.RandomAmbienceFrogs.audioSource.volume = .2f;
        AudioData.RandomAmbienceFrogs.StartRandomAudio(AudioData.AmbienceFrogs);
    }

    public void StopRandomBreaths()
    {
        AudioData.RandomAmbienceBreaths.StopRandomAudio();
    }

    public void StopRandomFrogs()
    {
        AudioData.RandomAmbienceFrogs.StopRandomAudio();
    }

    public void OnPlayerStateChange(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Default:
                AudioData.BackgroundMusic.SetVolume(AudioData.BackgroundMusic.DefaultVolume);
                break;
            case PlayerState.Instrument:
                AudioData.BackgroundMusic.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                break;
            case PlayerState.InstrumentMelody:
                AudioData.BackgroundMusic.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                break;
        }
    }

    // TODO: implement
    public override void OnPause(bool isPaused)
    {
        AudioData.BackgroundMusicPauseVolume = 0.1f;
        AudioData.RandomAmbienceFrogs.audioSource.volume = 0.1f;
        AudioData.RandomAmbienceBreaths.audioSource.volume = 0.1f;
        LowPassFilter.cutoffFrequency = 500f;

    }

    // TODO: implement
    public override void OnUnPause(bool isPaused)
    {
        AudioData.RandomAmbienceFrogs.audioSource.volume = 1f;
        AudioData.RandomAmbienceBreaths.audioSource.volume = 1f;
        AudioData.BackgroundMusicPauseVolume = 0.5f;
        LowPassFilter.cutoffFrequency = 20000f;
    }
}
