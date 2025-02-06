using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAudio : AudioController
{
    public PlayerAudioData AudioData;
    public static PlayerAudio instance;
    public string currentTerrain;

    // Store last played clip indexes to avoid playing it twice in a row
    private int lastWalkingClipIndex = 0;
    private int lastAttackChordClipIndex = 0;
    private int lastHitClipIndex = 0;


    // TODO: refactor to use less audio sources and just change clips
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

        foreach (Sound sound in AudioData.Footsteps)
        {
            InitializeSound(sound);
        }
        foreach (Sound sound in AudioData.AttackChords)
        {
            InitializeSound(sound);
        }
        foreach (Sound sound in AudioData.Hits)
        {
            InitializeSound(sound);
        }
        InitializeSound(AudioData.NoteB);
        InitializeSound(AudioData.NoteC);
        InitializeSound(AudioData.NoteD);
        InitializeSound(AudioData.NoteE);
        InitializeSound(AudioData.Melody1);
        InitializeSound(AudioData.Melody2);

        AudioData.RandomFootsteps = gameObject.AddComponent<RandomAudioManager>();
        AudioData.RandomFootsteps.audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayWalkingAudio(Vector2 movement)
    {
        // Don't attempt to play walking audio if not moving
        // or if a walking clip is currently playing
        if(
            movement.sqrMagnitude == 0 ||
            AudioData.Footsteps[lastWalkingClipIndex].IsPlaying
        )
        {
            return;
        }

        // Prevent playing the same clip twice in a row
        int clipIndex = lastWalkingClipIndex;
        while (clipIndex == lastWalkingClipIndex)
        {
            clipIndex = Random.Range(0, AudioData.Footsteps.Length);
        }
        Sound sound = AudioData.Footsteps[clipIndex];
        // Randomize pitch in either direction of the default pitch
        float pitch = Random.Range(sound.DefaultPitch - AudioData.MaxFootstepPitchVariation, sound.DefaultPitch + AudioData.MaxFootstepPitchVariation);
        // Randomize volume, but don't exceed the default volume
        float volume = Random.Range(sound.DefaultVolume - AudioData.MaxFootstepVolumeVariation, sound.DefaultVolume);
        // Play the sound with new pitch and volume
        AudioData.Footsteps[clipIndex].Play(pitch, volume);
        // Set lastWalkingClipIndex to the index just used
        lastWalkingClipIndex = clipIndex;
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
            AudioData.RandomFootsteps.PlayRandomAudioNoDelayWithFX(currentClips, 0.8f, 1.15f, false);
        }
        else
        {
            AudioData.RandomFootsteps.PlayRandomAudioNoDelayWithFX(currentClips, 0.8f, 1.15f, true);
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

    public void PlayAttackChord()
    {
        // Prevent playing the same clip twice in a row
        int clipIndex = lastAttackChordClipIndex;
        while (clipIndex == lastAttackChordClipIndex)
        {
            clipIndex = Random.Range(0, AudioData.AttackChords.Length);
        }
        // Play the sound with default pitch and volume
        AudioData.AttackChords[clipIndex].Play();
        // Set lastAttackChordClipIndex to the index just used
        lastAttackChordClipIndex = clipIndex;
    }

    public void PlayHit()
    {
        // Prevent playing the same clip twice in a row
        int clipIndex = lastHitClipIndex;
        while (clipIndex == lastHitClipIndex)
        {
            clipIndex = Random.Range(0, AudioData.Hits.Length);
        }
        // Play the sound with default pitch and volume
        AudioData.Hits[clipIndex].Play();
        // Set lastHitClipIndex to the index just used
        lastHitClipIndex = clipIndex;
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
