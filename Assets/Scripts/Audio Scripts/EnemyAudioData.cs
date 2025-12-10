using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAudioData", menuName = "Scriptable Objects/EnemyAudioData")]
public class EnemyAudioData : ScriptableObject
{
    [Header("Combat")]
    // public Sound[] Hits;
    public AudioClip[] Hits;
    public AudioClip[] OwlHits;
    public AudioClip[] PhantomHits;

    [Header("States")]
    // public Sound[] Aggro;
    public AudioClip[] Aggro;
    public AudioClip[] OwlAggro;
    public AudioClip[] PhantomAggro;

    [Header("Attack")]
    // public Sound[] Attacks;
    public AudioClip[] Attacks;
    public AudioClip[] OwlAttacks;
    public AudioClip[] PhantomAttacks;

    [Header("Idle")]
    public AudioClip[] OwlIdle_1;
    public AudioClip[] OwlIdle_2;

    [Header ("Audio Managers")]
    public RandomAudioManager RandomHits;
    public RandomAudioManager OwlRandomIdle;
    // public RandomAudioManager RandomAggro;
    // public RandomAudioManager RandomAttacks;
}
