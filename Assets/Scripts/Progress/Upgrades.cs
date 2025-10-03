// Assets/Scripts/Progress/Upgrades.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Progress/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public string id;          // e.g., "dash", "aoe"
    public string displayName; // "Icarus Dash", "Echo Burst", etc.
    public Sprite icon;        // optional
}
