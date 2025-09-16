using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{
    [SerializeField] public List<string> upLines;
    [SerializeField] public List<string> downLines;
    [SerializeField] public List<string> leftLines;
    [SerializeField] public List<string> rightLines;

    // Combined list for single-direction lines
    [SerializeField] public List<string> universalLines = new List<string>();

    // Get lines for the specified direction
    public List<string> GetLines(FacingDirection direction)
    {
        List<string> result = direction switch
        {
            FacingDirection.Up => upLines,
            FacingDirection.Down => downLines,
            FacingDirection.Left => leftLines,
            FacingDirection.Right => rightLines,
            _ => universalLines
        };

        if (result == null || result.Count == 0)
        {
            Debug.LogWarning($"No dialogue lines found for direction {direction} in {name}.");
            return universalLines;
        }

        return result;
    }
}

