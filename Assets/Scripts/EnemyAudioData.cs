using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAudioData", menuName = "Scriptable Objects/EnemyAudioData")]
public class EnemyAudioData : ScriptableObject
{
    [Header("Combat")]
    public Sound[] Hits;
}
