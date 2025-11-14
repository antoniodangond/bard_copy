using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Progress/Upgrade Registry")]
public class UpgradeRegistry : ScriptableObject
{
    [SerializeField] private List<UpgradeSO> upgrades = new List<UpgradeSO>();
    private Dictionary<string, UpgradeSO> _byId;

    void OnEnable()
    {
        _byId = new Dictionary<string, UpgradeSO>();
        foreach (var u in upgrades)
        {
            if (u == null || string.IsNullOrEmpty(u.id)) continue;
            _byId[u.id] = u;
        }
    }

    public bool TryGet(string id, out UpgradeSO so)
    {
        if (_byId == null) OnEnable();
        return _byId.TryGetValue(id, out so);
    }

    // Optional global access (so you can just call UpgradeRegistry.Instance)
    private static UpgradeRegistry _instance;
    public static UpgradeRegistry Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<UpgradeRegistry>("UpgradeRegistry");
            return _instance;
        }
    }
}
