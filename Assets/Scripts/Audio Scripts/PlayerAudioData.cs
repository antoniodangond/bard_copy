using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAudioData", menuName = "Scriptable Objects/PlayerAudioData")]
public class PlayerAudioData : ScriptableObject
{
    [Header("Footsteps")]
    public Sound[] Footsteps;
    public AudioClip[] GrassFootsteps;
    public AudioClip[] StoneFootsteps;
    public AudioClip[] SandFootsteps;
    [Range (0f, 1f)]
    public float MaxFootstepPitchVariation;
    [Range (0f, 1f)]
    public float MaxFootstepVolumeVariation;

    [Header("Instrument")]
    public Sound NoteB;
    public Sound NoteC;
    public Sound NoteD;
    public Sound NoteE;
    public Sound Melody1;
    public Sound Melody2;
    // Time between last note of Melody trigger and Melody being played
    [Range (0f, 1f)]
    public float TimeBeforeMelody;
    // Time before player is moved from InstrumentMelody state to Default state
    [Range (0f, 10f)]
    public float MelodyCooldownTime;

    [Header("Combat")]
    // public Sound[] AttackChords;
    public AudioClip[] AttackChords;
    // public Sound[] Hits;
    public AudioClip[] Hits;

    [Header ("Audio Managers")]
    public RandomAudioManager RandomFootsteps;
    public RandomAudioManager RandomAttackChords;
    public RandomAudioManager RandomHits;
}
