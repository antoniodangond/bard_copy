using System.Collections;
using UnityEngine;

public class BackgroundAudio : AudioController
{
    public BackgroundAudioData AudioData;
    public AudioLowPassFilter LowPassFilter;
    public float FadeDuration;

    private Sound currentBackgroundMusic;

    void Awake()
    {
        // Instantiate audio sources
        InitializeSound(AudioData.BackgroundMusic);
        InitializeSound(AudioData.BackgroundMusicUnderworld);
        InitializeSound(AudioData.BackgroundMusicMausoleum);
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

    public void StartBackgroundMusic()
    {
        currentBackgroundMusic = AudioData.BackgroundMusic;
        AudioData.BackgroundMusic.Play();
        // Play alternate bg clips at 0 volume
        AudioData.BackgroundMusicUnderworld.Play(1f, 0f);
        AudioData.BackgroundMusicMausoleum.Play(1f, 0f);
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

    public IEnumerator ChangeBackgroundMusic(string region)
    {
        // Fade out the current background music
        // Change the backgroudn music clip
        Sound newSound;
        switch(region)
        {
            case "Underworld":
                newSound = AudioData.BackgroundMusicUnderworld;
                break;
            case "Mausoleum":
                newSound = AudioData.BackgroundMusicMausoleum;
                break;
            case "Beach":
                newSound = AudioData.BackgroundMusic;
                break;
            default:
                // Use for Overworld
                newSound = AudioData.BackgroundMusic;
                break;
        }
        // Fade in the new background music clip
        if (newSound != currentBackgroundMusic && newSound != null)
        {
            Debug.Log("playing new clip: " + newSound.Clip);
            yield return StartCoroutine(AudioFader.FadeOutCoroutine(currentBackgroundMusic.Source, FadeDuration));
            currentBackgroundMusic = newSound;
            yield return StartCoroutine(AudioFader.FadeInCoroutine(newSound.Source, FadeDuration, newSound.DefaultVolume));
        }
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
