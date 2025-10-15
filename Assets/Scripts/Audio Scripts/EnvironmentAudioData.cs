using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentAudioData", menuName = "Scriptable Objects/EnvironmentAudioData")]
public class EnvironmentAudioData : ScriptableObject
{
     [Header("Grave")]
    public AudioClip[] gravePushes;

    [Header("Log")]
    public AudioClip[] LogDisintegrate;
}
