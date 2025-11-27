using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentAudioData", menuName = "Scriptable Objects/EnvironmentAudioData")]
public class EnvironmentAudioData : ScriptableObject
{
     [Header("Grave")]
    public AudioClip[] gravePushes;

    [Header("Objects Affected by Songs")]
    public AudioClip[] LogDisintegrate;
    public AudioClip[] VinesGrow;
    public AudioClip[] IceMelt;
}
