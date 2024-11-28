using System.Collections;
using UnityEngine;

public class BackgroundAudio : AudioController
{
    public static BackgroundAudioData AudioData;
    public AudioLowPassFilter LowPassFilter;
    public float FadeDuration;

    private AudioClip defaultBackgroundMusic;

    void Awake()
    {
        // Instantiate audio sources.
        // Even though we have multiple AudioSources for each of the different background music clips,
        // we'll just use the BackgroundMuisic Sound's AudioSource and change its AudioClip when we
        // want to change background music.
        InitializeSound(AudioData.BackgroundMusic);
        // Save the AudioClip as the default background music clip
        defaultBackgroundMusic = AudioData.BackgroundMusic.Clip;
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

    public IEnumerator ChangeBackgroundMusic(string tag)
    {
        // Fade out the current background music
        yield return StartCoroutine(AudioFader.FadeOutCoroutine(AudioData.BackgroundMusic.Source, FadeDuration));
        // Change the backgroudn music clip
        switch(tag)
        {
            case "Underworld":
                AudioData.BackgroundMusic.Clip = AudioData.BackgroundMusicUnderworld.Clip;
                break;
            case "Mausoleum":
                AudioData.BackgroundMusic.Clip = AudioData.BackgroundMusicMausoleum.Clip;
                break;
            case "Beach":
                AudioData.BackgroundMusic.Clip = defaultBackgroundMusic;
                break;
            default:
                // Use for Overworld
                AudioData.BackgroundMusic.Clip = defaultBackgroundMusic;
                break;
        }
        // Fade in the new background music clip
        yield return StartCoroutine(AudioFader.FadeInCoroutine(AudioData.BackgroundMusic.Source, FadeDuration, AudioData.BackgroundMusic.DefaultVolume));
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
