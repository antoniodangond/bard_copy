using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAudioData", menuName = "Scriptable Objects/EnemyAudioData")]
public class EnemyAudioData : ScriptableObject
{
    [Header("Combat")]
    public Sound[] Hits;

    [Header("States")]
    public Sound[] Aggro;

    [Header("Attack")]
    public Sound[] Attacks;
}
