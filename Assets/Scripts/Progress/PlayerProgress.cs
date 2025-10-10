// Assets/Scripts/Progress/PlayerProgress.cs
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance { get; private set; }

    private HashSet<string> upgrades = new HashSet<string>();
    private HashSet<string> claimedRewards = new HashSet<string>();
    private HashSet<string> collectedTablets = new HashSet<string>();
    private int numTabletsCollected;

    void Awake()
    {
        numTabletsCollected = 0;
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void CollectTablet(string itemPickup)
    {
        numTabletsCollected += 1;
        collectedTablets.Add(itemPickup);
    }

    public bool HasUpgrade(UpgradeSO u) => u != null && upgrades.Contains(u.id);
    public void Unlock(UpgradeSO u)
    {
        if (u == null) return;
        if (upgrades.Add(u.id)) Save();
    }

    public bool IsRewardClaimed(string rewardId) => !string.IsNullOrEmpty(rewardId) && claimedRewards.Contains(rewardId);
    public void MarkRewardClaimed(string rewardId)
    {
        if (string.IsNullOrEmpty(rewardId)) return;
        if (claimedRewards.Add(rewardId)) Save();
    }

    // --- super simple persistence (swap with your SaveSystem later) ---
    const string UpgKey = "pp_upgrades";
    const string RwdKey = "pp_rewards";
    void Save()
    {
        PlayerPrefs.SetString(UpgKey, string.Join("|", upgrades));
        PlayerPrefs.SetString(RwdKey, string.Join("|", claimedRewards));
        PlayerPrefs.Save();
    }
    void Load()
    {
        upgrades.Clear(); claimedRewards.Clear();
        foreach (var s in PlayerPrefs.GetString(UpgKey, "").Split('|')) if (!string.IsNullOrEmpty(s)) upgrades.Add(s);
        foreach (var s in PlayerPrefs.GetString(RwdKey, "").Split('|')) if (!string.IsNullOrEmpty(s)) claimedRewards.Add(s);
    }

    public void ClearAll()
    {
        upgrades.Clear();
        claimedRewards.Clear();
        PlayerPrefs.DeleteKey(UpgKey);
        PlayerPrefs.DeleteKey(RwdKey);
        PlayerPrefs.Save();
        Debug.Log("[Progress] Cleared all upgrades/rewards.");
    }

#if UNITY_EDITOR
[ContextMenu("DEV: Clear All Upgrades")]
private void DevClearAll()
{
    ClearAll();
}
#endif


}
