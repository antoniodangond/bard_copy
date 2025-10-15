using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData // Data structure for saving/loading player progress
{
    public List<string> upgrades = new List<string>();
    public List<string> claimedRewards = new List<string>();
    public string[] collectedTablets = new string[5] { null, null, null, null, null };
    public int numTabletsCollected;

    // Expanded fields for full save system:
    public float[] playerPosition = new float[3]; // x, y, z
    public int playerHealth;
    public List<string> collectedCollectables = new List<string>();
    public List<string> defeatedEnemies = new List<string>();
    public List<string> removedObstacles = new List<string>();
    public List<NPCStatusData> npcStatuses = new List<NPCStatusData>();
    public List<string> savedSongs = new List<string>();
}

[Serializable]
public class NPCStatusData
{
    public string npcId;
    public string status;
}

public class PlayerProgress : MonoBehaviour // Singleton class to manage player progress and save/load functionality
{
    public static PlayerProgress Instance { get; private set; }

    private HashSet<string> upgrades = new HashSet<string>();
    private HashSet<string> claimedRewards = new HashSet<string>();
    private string[] collectedTablets = new string[5] { null, null, null, null, null };
    private int numTabletsCollected;

    

    // Expanded fields for full save system:
    private Vector3 playerPosition;
    private int playerHealth;
    private HashSet<string> collectedCollectables = new HashSet<string>();
    private HashSet<string> defeatedEnemies = new HashSet<string>();
    private HashSet<string> removedObstacles = new HashSet<string>();
    private Dictionary<string, string> npcStatuses = new Dictionary<string, string>();
    private HashSet<string> savedSongs = new HashSet<string>();

    const string SaveFileName = "savefile.json";

    void Awake()
    {
        numTabletsCollected = 0;
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // --- Tablet Collection ---
    public void CollectTablet(string itemPickup)
    {
        numTabletsCollected += 1;
        int indexToReplace = Array.IndexOf(collectedTablets, null);
        if (indexToReplace != -1) { collectedTablets[indexToReplace] = itemPickup; }
        PlayerUIManager.Instance.UpdateCollectedTabletsUI(numTabletsCollected, collectedTablets);
        Save();
    }

    // --- Upgrades ---
    public bool HasUpgrade(UpgradeSO u) => u != null && upgrades.Contains(u.id);
    public void Unlock(UpgradeSO u)
    {
        if (u == null) return;
        if (upgrades.Add(u.id)) Save();
    }

    // --- Rewards ---
    public bool IsRewardClaimed(string rewardId) => !string.IsNullOrEmpty(rewardId) && claimedRewards.Contains(rewardId);
    public void MarkRewardClaimed(string rewardId)
    {
        if (string.IsNullOrEmpty(rewardId)) return;
        if (claimedRewards.Add(rewardId)) Save();
    }


    // --- Enemies ---
    public bool IsEnemyDefeated(string enemyId) => defeatedEnemies.Contains(enemyId);
    public void MarkEnemyDefeated(string enemyId)
    {
        if (defeatedEnemies.Add(enemyId)) Save();
    }

    // --- Obstacles ---
    public bool IsObstacleRemoved(string obstacleId) => removedObstacles.Contains(obstacleId);
    public void MarkObstacleRemoved(string obstacleId)
    {
        if (removedObstacles.Add(obstacleId)) Save();
    }

    // --- NPC Status ---
    public string GetNPCStatus(string npcId)
    {
        if (npcStatuses.TryGetValue(npcId, out var status)) return status;
        return null;
    }
    public void SetNPCStatus(string npcId, string status)
    {
        npcStatuses[npcId] = status;
        Save();
    }

    // --- Songs ---
    public bool HasSong(string songId) => savedSongs.Contains(songId);
    public void AddSong(string songId)
    {
        if (savedSongs.Add(songId)) Save();
    }
    public IEnumerable<string> GetSavedSongs()
    {
        return savedSongs;
    }

    // --- Player State ---
    public void SetPlayerPosition(Vector3 pos)
    {
        playerPosition = pos;
        Save();
    }
    public Vector3 GetPlayerPosition() => playerPosition;

    public void SetPlayerHealth(int health)
    {
        playerHealth = health;
        Save();
    }
    public int GetPlayerHealth() => playerHealth;

    // --- Save/Load/Delete ---
    void Save() // Saves progress to file
    {
        SaveData data = new SaveData();
        data.upgrades = new List<string>(upgrades);
        data.claimedRewards = new List<string>(claimedRewards);
        data.collectedTablets = collectedTablets;
        data.numTabletsCollected = numTabletsCollected;

        data.playerPosition = new float[] { playerPosition.x, playerPosition.y, playerPosition.z };
        data.playerHealth = playerHealth;
        data.collectedCollectables = new List<string>(collectedCollectables);
        data.defeatedEnemies = new List<string>(defeatedEnemies);
        data.removedObstacles = new List<string>(removedObstacles);

        data.npcStatuses = new List<NPCStatusData>();
        foreach (var kvp in npcStatuses)
            data.npcStatuses.Add(new NPCStatusData { npcId = kvp.Key, status = kvp.Value });

        data.savedSongs = new List<string>(savedSongs);

        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(GetSavePath(), json);
    }

    void Load() // Loads progress from save file
    {
        string path = GetSavePath();
        if (!System.IO.File.Exists(path)) return;

        string json = System.IO.File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        upgrades = new HashSet<string>(data.upgrades ?? new List<string>());
        claimedRewards = new HashSet<string>(data.claimedRewards ?? new List<string>());
        collectedTablets = data.collectedTablets ?? new string[5] { null, null, null, null, null };
        numTabletsCollected = data.numTabletsCollected;

        if (data.playerPosition != null && data.playerPosition.Length == 3)
            playerPosition = new Vector3(data.playerPosition[0], data.playerPosition[1], data.playerPosition[2]);
        playerHealth = data.playerHealth;

        collectedCollectables = new HashSet<string>(data.collectedCollectables ?? new List<string>());
        defeatedEnemies = new HashSet<string>(data.defeatedEnemies ?? new List<string>());
        removedObstacles = new HashSet<string>(data.removedObstacles ?? new List<string>());

        npcStatuses = new Dictionary<string, string>();
        if (data.npcStatuses != null)
        {
            foreach (var npc in data.npcStatuses)
                npcStatuses[npc.npcId] = npc.status;
        }

        savedSongs = new HashSet<string>(data.savedSongs ?? new List<string>());
    }

    string GetSavePath() // Returns the full path to the save file
    {
        return System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    public void DeleteSave() // Deletes the save file
    {
        string path = GetSavePath();
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }

    public void ClearAll() // Resets all progress and deletes save file
    {
        upgrades.Clear();
        claimedRewards.Clear();
        collectedTablets = new string[5] { null, null, null, null, null };
        numTabletsCollected = 0;
        playerPosition = Vector3.zero;
        playerHealth = 0;
        collectedCollectables.Clear();
        defeatedEnemies.Clear();
        removedObstacles.Clear();
        npcStatuses.Clear();
        savedSongs.Clear();
        DeleteSave();
        Debug.Log("[Progress] Cleared all progress.");
    }

// --- Development Utilities ---
#if UNITY_EDITOR
    [ContextMenu("DEV: Clear All Upgrades")]
    private void DevClearAll()
    {
        ClearAll();
    }
#endif
}