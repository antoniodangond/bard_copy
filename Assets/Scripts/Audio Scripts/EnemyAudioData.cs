using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAudioData", menuName = "Scriptable Objects/EnemyAudioData")]
public class EnemyAudioData : ScriptableObject
{
    [Header("Combat")]
    // public Sound[] Hits;
    public AudioClip[] Hits;

    [Header("States")]
    // public Sound[] Aggro;
    public AudioClip[] Aggro;

    [Header("Attack")]
    // public Sound[] Attacks;
    public AudioClip[] Attacks;

    [Header ("Audio Managers")]
    public RandomAudioManager RandomHits;
    public RandomAudioManager RandomAggro;
    public RandomAudioManager RandomAttacks;
}
