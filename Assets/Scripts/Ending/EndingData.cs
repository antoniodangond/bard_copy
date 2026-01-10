using UnityEngine;

[CreateAssetMenu (fileName = "endData", menuName = "Scriptable Objects/EndingData")]
public class EndingData : ScriptableObject
{
    public Sprite[] badEndingSprites;
    public Sprite[] goodEndingSprites;
}
