using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{
    [SerializeField] private List<string> upLines;
    [SerializeField] private List<string> downLines;
    [SerializeField] private List<string> leftLines;
    [SerializeField] private List<string> rightLines;

    // Combined list for single-direction lines
    [SerializeField] private List<string> universalLines = new List<string>();

    // Get lines for the specified direction
    public List<string> GetLines(FacingDirection direction)
    {
        switch (direction)
        {
            case FacingDirection.Up: return upLines.Count > 0 ? upLines : universalLines;
            case FacingDirection.Down: return downLines.Count > 0 ? downLines : universalLines;
            case FacingDirection.Left: return leftLines.Count > 0 ? leftLines : universalLines;
            case FacingDirection.Right: return rightLines.Count > 0 ? rightLines : universalLines;
            default: return universalLines; // Default fallback
        }
    }
}