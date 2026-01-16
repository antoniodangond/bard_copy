using System.Collections;
using UnityEngine;

public class BackgroundAudio : AudioController
{

    public BackgroundAudioData AudioData;
    public AudioLowPassFilter LowPassFilter;
    // Assign in inspector
    public AudioSource breathsAudioSource;
    public AudioSource frogsAudioSource;
    public AudioSource loudBirdsAudioSource;
    public AudioSource quietBirdsAudioSource;
    public AudioSource statuePiecePickupStingSource;
    private AudioClip statuePiecePickupSting;
    public AudioSource gravePiecePickupSting;
    public float FadeDuration;

    private Sound currentBackgroundMusic;
    private Sound currentAmbience;

    void Awake()
    {

        // Instantiate audio sources and clips
        InitializeSound(AudioData.BackgroundMusic);
        InitializeSound(AudioData.BackgroundMusicUnderworld);
        InitializeSound(AudioData.BackgroundMusicMausoleum);
        InitializeSound(AudioData.BackgroundMusicForest);
        InitializeSound(AudioData.BackgroundMusicMountain);
        InitializeSound(AudioData.BackgroundMusicDungeonA);
        InitializeSound(AudioData.BackgroundMusicDungeonB);
        InitializeSound(AudioData.BackgroundMusicDungeonC);
        InitializeSound(AudioData.BackgroundMusicBeach);
        InitializeSound(AudioData.BackgroundMusicMainMenu);
        InitializeSound(AudioData.BackgroundMusicBadEnd);
        InitializeSound(AudioData.BackgroundMusicGoodEnd);
        InitializeSound(AudioData.OverworldAmbience);
        InitializeSound(AudioData.UnderworldAmbience);
        InitializeSound(AudioData.BeachAmbience);
        
        statuePiecePickupSting = statuePiecePickupStingSource.clip;
        // Old approach, may have been getting unloaded at initialization in the build
        // breathsAudioSource = gameObject.transform.GetChild(2).GetChild(0).GetComponent<AudioSource>();
        // frogsAudioSource = gameObject.transform.GetChild(2).GetChild(1).GetComponent<AudioSource>();
        // loudBirdsAudioSource = gameObject.transform.GetChild(2).GetChild(2).GetComponent<AudioSource>();
        // quietBirdsAudioSource = gameObject.transform.GetChild(2).GetChild(3).GetComponent<AudioSource>();
        if (LowPassFilter == null)
        {
            LowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        }
        // Instantiate Random Audio Managers and their audio source
        AudioData.RandomAmbienceBreaths = gameObject.AddComponent<RandomAudioManager>();
        AudioData.RandomAmbienceFrogs = gameObject.AddComponent<RandomAudioManager>();
        AudioData.RandomAmbienceLoudBirds = gameObject.AddComponent<RandomAudioManager>();
        AudioData.RandomAmbienceQuietBirds = gameObject.AddComponent<RandomAudioManager>();
        

        // currentBackgroundMusic = AudioData.BackgroundMusicMainMenu;

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
        currentBackgroundMusic = AudioData.BackgroundMusicMainMenu;
        AudioData.BackgroundMusicMainMenu.Play();
        // Play alternate bg clips at 0 volume
        AudioData.BackgroundMusicUnderworld.Play(1f, 0f);
        AudioData.BackgroundMusic.Play(1f, 0f);
        AudioData.BackgroundMusicMausoleum.Play(1f, 0f);
        AudioData.BackgroundMusicForest.Play(1f, 0f);
        AudioData.BackgroundMusicBeach.Play(1f, 0f);
        AudioData.BackgroundMusicMountain.Play(1f, 0f);
        AudioData.BackgroundMusicDungeonA.Play(1f, 0f);
        AudioData.BackgroundMusicDungeonB.Play(1f, 0f);
        AudioData.BackgroundMusicDungeonC.Play(1f, 0f);
    }
    
    public void StartAmbience()
    {
        currentAmbience = AudioData.OverworldAmbience;
        AudioData.OverworldAmbience.Play();
        // Play alternate bg clips at 0 volume
        AudioData.UnderworldAmbience.Play(1f, 0f);
        AudioData.BeachAmbience.Play(1f, 0f);
    }

   //public void PlayOverworldAmbience()
   // {
   //     AudioData.OverworldAmbience.Play();
   // }

    public void PlayRandomBreaths()
    {
        breathsAudioSource.volume = .7f;
        AudioData.RandomAmbienceBreaths.StartRandomAudioWithDelay(AudioData.AmbienceBreaths, breathsAudioSource);
    }
    public void PlayRandomFrogs()
    {
        AudioData.RandomAmbienceFrogs.minDelay = .4f;
        AudioData.RandomAmbienceFrogs.maxDelay = 5f;
        frogsAudioSource.volume = .35f;
        AudioData.RandomAmbienceFrogs.StartRandomAudioWithDelay(AudioData.AmbienceFrogs, frogsAudioSource);
    }
    public void PlayRandomLoudBirds()
    {
        AudioData.RandomAmbienceLoudBirds.minDelay = .7f;
        AudioData.RandomAmbienceLoudBirds.maxDelay = 5f;
        loudBirdsAudioSource.volume = .3f;
        AudioData.RandomAmbienceLoudBirds.StartRandomAudioWithDelay(AudioData.AmbienceLoudBirds, loudBirdsAudioSource);
    }
    public void PlayRandomQuietBirds()
    {
        AudioData.RandomAmbienceQuietBirds.minDelay = .4f;
        AudioData.RandomAmbienceQuietBirds.maxDelay = 4f;
        quietBirdsAudioSource.volume = .5f;
        AudioData.RandomAmbienceQuietBirds.StartRandomAudioWithDelay(AudioData.AmbienceQuietBirds, quietBirdsAudioSource);
    }

    public void StopRandomBreaths()
    {
        AudioData.RandomAmbienceBreaths.StopRandomAudio();
    }

    public void StopRandomFrogs()
    {
        AudioData.RandomAmbienceFrogs.StopRandomAudio();
    }
    public void StopRandomLoudBirds()
    {
        AudioData.RandomAmbienceLoudBirds.StopRandomAudio();
    }
    public void StopRandomQuietBirds()
    {
        AudioData.RandomAmbienceQuietBirds.StopRandomAudio();
    }

    public void StopAllAmbienceOneShots()
    {
        // Breaths are in all regions
        // StopRandomBreaths();
        StopRandomFrogs();
        StopRandomLoudBirds();
        StopRandomQuietBirds();


    }

    public IEnumerator ChangeBackgroundMusic(string region)
    {
        // Fade out the current background music
        // Change the backgroudn music clip
        Sound newSound;
        Sound newAmbience;
        switch(region)
        {
            case "Underworld":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicUnderworld;
                newAmbience = AudioData.UnderworldAmbience;
                PlayerAudio.instance.currentTerrain = "Stone";
                // PlayRandomBreaths();
                break;
            case "MainMenu":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicMainMenu;
                newAmbience = AudioData.OverworldAmbience;
                PlayerAudio.instance.currentTerrain = "Grass";
                // PlayRandomBreaths();
                PlayRandomFrogs();
                break;
            case "Mausoleum":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicMausoleum;
                newAmbience = AudioData.UnderworldAmbience;
                PlayerAudio.instance.currentTerrain = "Stone";
                break;
            case "Beach":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicBeach;
                newAmbience = AudioData.BeachAmbience;
                PlayerAudio.instance.currentTerrain = "Sand";
                PlayRandomLoudBirds();
                PlayRandomQuietBirds();
                break;
            case "Forest":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicForest;
                newAmbience = AudioData.OverworldAmbience;
                PlayerAudio.instance.currentTerrain = "Grass";
                // PlayRandomBreaths();
                break;
            case "Mountain":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicMountain;
                newAmbience = AudioData.OverworldAmbience;
                PlayerAudio.instance.currentTerrain = "Stone";
                // PlayRandomBreaths();
                break;
            case "DungeonA":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicDungeonA;
                newAmbience = AudioData.OverworldAmbience;
                PlayerAudio.instance.currentTerrain = "Stone";
                // PlayRandomBreaths();
                break;
            case "DungeonB":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicDungeonA;
                newAmbience = AudioData.OverworldAmbience;
                PlayerAudio.instance.currentTerrain = "Stone";
                // PlayRandomBreaths();
                break;
            case "DungeonC":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicDungeonA;
                newAmbience = AudioData.OverworldAmbience;
                PlayerAudio.instance.currentTerrain = "Stone";
                // PlayRandomBreaths();
                break;
            case "BadEnding":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicBadEnd;
                newAmbience = null;
                // PlayRandomBreaths();
                break;
            case "GoodEnding":
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusicGoodEnd;
                newAmbience = null;
                // PlayRandomBreaths();
                break;
            default:
                // Use for Overworld
                StopAllAmbienceOneShots();
                newSound = AudioData.BackgroundMusic;
                newAmbience = AudioData.OverworldAmbience;
                PlayerAudio.instance.currentTerrain = "Grass";
                PlayRandomBreaths();
                PlayRandomFrogs();
                break;
        }

            // Only crossfade if the new track is different
            if (newSound != currentBackgroundMusic && newSound != null)
            {
                // Start crossfade for background music and ambience simultaneously
                StartCoroutine(AudioFader.CrossfadeCoroutine(currentBackgroundMusic.Source, newSound.Source, FadeDuration, newSound.DefaultVolume));
                if (newAmbience != null) {StartCoroutine(AudioFader.CrossfadeCoroutine(currentAmbience.Source, newAmbience.Source, FadeDuration, newAmbience.DefaultVolume));}

                // Update references
                currentBackgroundMusic = newSound;
                currentAmbience = newAmbience;
            }
            else if (newAmbience != currentAmbience && currentAmbience != null)
            {
                // Only crossfade ambience if it changed
                StartCoroutine(AudioFader.CrossfadeCoroutine(currentAmbience.Source, newAmbience.Source, FadeDuration, newAmbience.DefaultVolume));
                currentAmbience = newAmbience;
            }

            yield return null; // Keep the coroutine structure
        }
    

    public void OnPlayerStateChange(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Default:
                // AudioData.BackgroundMusic.SetVolume(AudioData.BackgroundMusic.DefaultVolume);
                currentBackgroundMusic.SetVolume(currentBackgroundMusic.DefaultVolume);
                break;
            case PlayerState.Instrument:
                switch (currentBackgroundMusic.Clip.name)
                {
                    case "mus_overworldtheme_loop_v2":
                        AudioData.BackgroundMusic.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                        break;
                    case "mus_mainmenu_v1":
                        AudioData.BackgroundMusicMainMenu.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                        break;    
                    case "mus_underworldtheme_loop_v2":
                        AudioData.BackgroundMusicUnderworld.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                        break;
                    case "mus_mausoleum_v2":
                        AudioData.BackgroundMusicMausoleum.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                        break;
                    case "mus_beach_v2":
                        AudioData.BackgroundMusicBeach.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                        break;
                    case "mus_forest_v1":
                        AudioData.BackgroundMusicForest.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                        break;
                    case "mus_mountain_v1":
                        AudioData.BackgroundMusicMountain.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                        break;
                    case "mus_dungeon_v1":
                        AudioData.BackgroundMusicDungeonA.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                        AudioData.BackgroundMusicDungeonB.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                        AudioData.BackgroundMusicDungeonC.SetVolume(AudioData.BackgroundMusicInstrumentVolume);
                        break;
                    default:
                        break;
                }
                // currentBackgroundMusic.SetVolume(currentBackgroundMusic)
                break;
            case PlayerState.InstrumentMelody:
                switch (currentBackgroundMusic.Clip.name)
                {
                    case "mus_overworldtheme_loop_v2":
                        AudioData.BackgroundMusic.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                        break;
                    case "mus_mainmenu_v1":
                        AudioData.BackgroundMusicMainMenu.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                        break;    
                    case "mus_mausoleum_v2":
                        AudioData.BackgroundMusicMausoleum.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                        break;
                    case "mus_underworldtheme_loop_v2":
                        AudioData.BackgroundMusicUnderworld.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                        break;
                    case "mus_beach_v2":
                        AudioData.BackgroundMusicBeach.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                        break;
                    case "mus_forest_v1":
                        AudioData.BackgroundMusicForest.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                        break;
                    case "mus_mountain_v1":
                        AudioData.BackgroundMusicMountain.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                        break;
                    case "mus_dungeon_v1":
                        AudioData.BackgroundMusicDungeonA.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                        AudioData.BackgroundMusicDungeonB.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                        AudioData.BackgroundMusicDungeonC.SetVolume(AudioData.BackgroundMusicInstrumentMelodyVolume);
                        break;
                    default:
                        break;
                }
                break;
        }
    }

    // TODO: implement
    public override void OnPause(bool isPaused)
    {
        AudioData.BackgroundMusicPauseVolume = 0.1f;
        frogsAudioSource.volume = 0.1f;
        breathsAudioSource.volume = 0.1f;
        loudBirdsAudioSource.volume = 0.1f;
        quietBirdsAudioSource.volume = 0.1f;
        LowPassFilter.cutoffFrequency = 500f;

    }

    // TODO: implement
    public override void OnUnPause(bool isPaused)
    {
        frogsAudioSource.volume = 1f;
        breathsAudioSource.volume = 1f;
        loudBirdsAudioSource.volume = 1f;
        quietBirdsAudioSource.volume = 1f;
        AudioData.BackgroundMusicPauseVolume = 0.5f;
        LowPassFilter.cutoffFrequency = 20000f;
    }

    public void PlayStatuePiecePickupSting()
    {
        StartCoroutine(StatuePickupRoutine(1f));
    }

    private IEnumerator StatuePickupRoutine(float fadeInTime)
    {
        // Set current bg music to 0 so we can hear sting
        currentBackgroundMusic.Source.volume = 0;
        // play sting and wait for it to be almost finished before fading in music again
        statuePiecePickupStingSource.PlayOneShot(statuePiecePickupSting);
        yield return new WaitForSeconds(statuePiecePickupSting.length - 1f);

        // fadeInTime in background music again
        float targetVolume = 0.65f;
        for (float elapsedTime = 0f; elapsedTime < fadeInTime; elapsedTime += Time.deltaTime)
        {
            float t = elapsedTime/fadeInTime;

            currentBackgroundMusic.Source.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }
        // currentBackgroundMusic.SetVolume(currentBackgroundMusic.DefaultVolume);
        
    }
}
