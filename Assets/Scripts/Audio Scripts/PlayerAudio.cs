using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class NamedAudioClip
{
    public string name;
    public AudioClip clip;
}

public class PlayerAudio : AudioController
{
    [SerializeField] private NamedAudioClip[] playerSoundLibrary;
    public PlayerAudioData AudioData;
    public static PlayerAudio instance;
    public string currentTerrain;

    void Awake() {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // Destroy any duplicate instances to enforce singleton pattern
            Debug.LogWarning("Duplicate PlayerAudio instance detected. Destroying the new instance.");
            Destroy(gameObject);
            return;
        }
        // TODO: improve this
        // Instantiate audio sources

        InitializeSound(AudioData.NoteB);
        InitializeSound(AudioData.NoteC);
        InitializeSound(AudioData.NoteD);
        InitializeSound(AudioData.NoteE);
        InitializeSound(AudioData.Melody1);
        InitializeSound(AudioData.Melody2);

        AudioData.RandomFootsteps = gameObject.AddComponent<RandomAudioManager>();
        AudioData.RandomAttackNotes = gameObject.AddComponent<RandomAudioManager>();
        AudioData.RandomAttackChords = gameObject.AddComponent<RandomAudioManager>();
        AudioData.RandomHits = gameObject.AddComponent<RandomAudioManager>();
        AudioData.PlayerSoundsSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayFootstep()
    {
        AudioClip[] currentClips;

        switch(currentTerrain)
        {
            case "Grass":
                currentClips = AudioData.GrassFootsteps;
                break;
            case "Stone":
                currentClips = AudioData.StoneFootsteps;
                break;
            case "Sand":
                currentClips = AudioData.SandFootsteps;
                break;
            default:
                currentClips = AudioData.GrassFootsteps; // default
                break;
        }

        // Stone footsteps were too quiet so add a case where there is no voume modulation
        if (currentTerrain == "Stone")
        {
            AudioData.RandomFootsteps.PlayRandomAudioNoDelayWithFX(currentClips, 0.8f, 1.15f, false, AudioData.PlayerSoundsSource);
        }
        else
        {
            AudioData.RandomFootsteps.PlayRandomAudioNoDelayWithFX(currentClips, 0.8f, 1.15f, true, AudioData.PlayerSoundsSource);
        }

    }

    public void PlayNote(string noteName)
    {
        // Determine which Sound to play
        Sound sound;
        switch (noteName)
        {
            case MelodyData.NoteB:
                sound = AudioData.NoteB;
                break;
            case MelodyData.NoteC:
                sound = AudioData.NoteC;
                break;
            case MelodyData.NoteD:
                sound = AudioData.NoteD;
                break;
            case MelodyData.NoteE:
                sound = AudioData.NoteE;
                break;
            default:
                return;
        }
        // If a valid note name was provided, play the related Sound
        if (sound != null)
        {
            sound.Play();
        }
    }

    public void PlayMelody(string melody)
    {
        switch (melody)
        {
            case MelodyData.Melody1:
                AudioData.Melody1.Play();
                break;
            case MelodyData.Melody2:
                AudioData.Melody2.Play();
                break;
            default:
                break;
        }
    }

    public void PlayAttackNote(AudioSource audioSource)
    {
        if (AudioData.AttackNotes.Length == 0) {
            Debug.LogError("Player audio 'Attack Chords' length is 0");
            return;
        }
        AudioData.RandomAttackNotes.PlayRandomAudioNoDelayWithFX(AudioData.AttackNotes, 1, 1, true, audioSource);
    }
    public void PlayAttackChord(AudioSource audioSource)
    {
        if (AudioData.AttackChords.Length == 0) {
            Debug.LogError("Player audio 'Attack Chords' length is 0");
            return;
        }
        AudioData.RandomAttackChords.PlayRandomAudioNoDelayWithFX(AudioData.AttackChords, 1, 1, true, audioSource);
    }

    public void PlayHit(AudioSource audioSource)
    {
        if (AudioData.AttackChords.Length == 0) 
        {
            Debug.LogError("Player audio 'Hits' length is 0");
            return;
        }
        AudioData.RandomHits.PlayRandomAudioNoDelayWithFX(AudioData.Hits, 0.8f, 1.25f, true, audioSource);
    }
    
    public void PlayPlayerSound(string clipname, float vol_mult)
    {
        if (AudioData.PlayerSounds.Length == 0) 
        {
            Debug.LogError("Player audio 'Player Sounds' length is 0");
            return;
        }
        foreach (var sound in playerSoundLibrary)
        {
            if (sound.name == clipname)
            {
                AudioData.PlayerSoundsSource.PlayOneShot(sound.clip, vol_mult);
            }
            else
            {
                Debug.LogError($"sound {clipname} not found!");
            }
        }
    }

    // TODO: implement
    public override void OnPause(bool isPaused)
    {

    }

    // TODO: implement
    public override void OnUnPause(bool isPaused)
    {

    }
}
