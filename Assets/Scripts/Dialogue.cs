using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{
    [SerializeField] private List<string> upLines;
    [SerializeField] private List<string> downLines;
    [SerializeField] private List<string> leftLines;
    [SerializeField] private List<string> rightLines;

    // Get lines for the specified direction
    public List<string> GetLines(FacingDirection direction)
    {
        switch (direction)
        {
            case FacingDirection.Up: return upLines;
            case FacingDirection.Down: return downLines;
            case FacingDirection.Left: return leftLines;
            case FacingDirection.Right: return rightLines;
            default: return new List<string>();
        }
    }
}