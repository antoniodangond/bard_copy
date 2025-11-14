using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

#region SaveData DTOs

[Serializable]
public class SaveData // Data structure for saving/loading player progress
{
    // --- Meta / robustness ---
    public int version = 1;                 // Bump if you change schema
    public string currentScene;             // Scene at time of save
    public string lastSaveUtc;              // ISO-8601 timestamp
    public double totalPlaySeconds;         // Accumulated playtime (secs)

    // --- Progress payload ---
    public List<string> upgrades = new List<string>();
    public List<string> claimedRewards = new List<string>();
    public string[] collectedTablets = new string[5] { null, null, null, null, null };
    public int numTabletsCollected;

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

#endregion

public class PlayerProgress : MonoBehaviour // Singleton class to manage player progress and save/load functionality
{
    public static PlayerProgress Instance { get; private set; }

    // --- World-facing events (UI/listeners subscribe for refresh/apply-on-load) ---
    public event Action OnLoaded;
    public event Action OnSaved;

    // --- Core progress sets/state ---
    private HashSet<string> upgrades = new HashSet<string>();
    private HashSet<string> claimedRewards = new HashSet<string>();
    private string[] collectedTablets = new string[5] { null, null, null, null, null };
    private int numTabletsCollected;
    public int GetNumTabletsCollected() => numTabletsCollected;

    private Vector3 playerPosition;
    private int playerHealth;
    private HashSet<string> collectedCollectables = new HashSet<string>();
    private HashSet<string> defeatedEnemies = new HashSet<string>();
    private HashSet<string> removedObstacles = new HashSet<string>();
    private Dictionary<string, string> npcStatuses = new Dictionary<string, string>();
    private HashSet<string> savedSongs = new HashSet<string>();

    public bool HasSaveFile() => System.IO.File.Exists(GetSavePath());
    public void SaveNow() => Save();          // simple public wrapper
    public string GetSavedSceneName() => CurrentScene; // already tracked


    // --- Meta/state tracking ---
    public string CurrentScene { get; private set; }
    private double _sessionStartEpochSecs;    // Utc epoch seconds when this session began
    private double _accumulatedPlaySeconds;   // From saves prior to this session

    const string SaveFileName = "savefile.json";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sessionStartEpochSecs = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;

        // Load from disk (this sets CurrentScene from save, if any)
        Load();
        RaiseLoaded();

        // OPTIONAL: if there was no saved scene, fall back to current
        if (string.IsNullOrEmpty(CurrentScene))
        {
            CurrentScene = SceneManager.GetActiveScene().name;
        }
    }


    #region Tablet Collection

    public void CollectTablet(string itemPickup)
    {
        numTabletsCollected += 1;
        int indexToReplace = Array.IndexOf(collectedTablets, null);
        if (indexToReplace != -1)
        {
            collectedTablets[indexToReplace] = itemPickup;

            if (PlayerUIManager.Instance != null)
                PlayerUIManager.Instance.UpdateCollectedTabletsUI(numTabletsCollected, collectedTablets);

            // NEW: also update pause menu tablet text
            if (MenuManager.Instance != null)
                MenuManager.Instance.UpdateTabletsCountUI();
        }

        Save();
    }


    #endregion

    #region Upgrades / Rewards

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

    #endregion

    #region Enemies / Obstacles / Collectibles / NPCs

    // Enemies
    public bool IsEnemyDefeated(string enemyId) => defeatedEnemies.Contains(enemyId);
    public void MarkEnemyDefeated(string enemyId)
    {
        if (defeatedEnemies.Add(enemyId)) Save();
    }

    // Obstacles
    public bool IsObstacleRemoved(string obstacleId) => removedObstacles.Contains(obstacleId);
    public void MarkObstacleRemoved(string obstacleId)
    {
        if (removedObstacles.Add(obstacleId)) Save();
    }

    // Generic collectibles (non-tablets or for presence checks)
    public bool HasCollected(string collectibleId) => collectedCollectables.Contains(collectibleId);
    public void MarkCollected(string collectibleId)
    {
        if (collectedCollectables.Add(collectibleId)) Save();
    }

    // NPC Status
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

    #endregion

    #region Songs

    public bool HasSong(string songId) => savedSongs.Contains(songId);
    public void AddSong(string songId)
    {
        if (savedSongs.Add(songId)) Save();
    }
    public IEnumerable<string> GetSavedSongs() => savedSongs;

    #endregion

    #region Player State

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

    #endregion

    #region Save / Load / Apply

    // Call this instead of writing the file directly. If you add a SaveManager later,
    // it'll debounce/atomic-write for you (we cooperate if it exists).
    internal void Save()
    {
        // If a SaveManager exists, let it schedule/flush; else write immediately.
        var saveManager = FindObjectOfType<MonoBehaviour>() as object; // dummy to avoid hard dependency
        var json = BuildJson();

        try
        {
            // Write atomically even without a manager
            string path = GetSavePath();
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string tmp = path + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(path))
                File.Replace(tmp, path, path + ".bak", true);
            else
                File.Move(tmp, path);

            RaiseSaved();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] Failed: {e}");
        }
    }

    public void Load() // Loads progress from save file
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            // Initialize defaults
            numTabletsCollected = 0;
            playerPosition = Vector3.zero;
            playerHealth = 0;
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Meta
        CurrentScene = string.IsNullOrEmpty(data.currentScene) ? SceneManager.GetActiveScene().name : data.currentScene;
        _accumulatedPlaySeconds = data.totalPlaySeconds;

        // Payload
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
            foreach (var npc in data.npcStatuses) npcStatuses[npc.npcId] = npc.status;

        savedSongs = new HashSet<string>(data.savedSongs ?? new List<string>());
    }

    // Build JSON from current in-memory progress (used by Save)
    internal string BuildJson()
    {
        var data = new SaveData();

        // Meta
        data.version = 1;
        data.currentScene = SceneManager.GetActiveScene().name;
        data.lastSaveUtc = DateTime.UtcNow.ToString("o");
        var nowSecs = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        data.totalPlaySeconds = _accumulatedPlaySeconds + (nowSecs - _sessionStartEpochSecs);

        // Payload
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

        return JsonUtility.ToJson(data);
    }

    public void ApplyToWorld()
    {
        // Let listeners (binders/UI) respond to the loaded state
        OnLoaded?.Invoke();
    }

    public void DeleteSave() // Deletes the save file
    {
        string path = GetSavePath();
        if (File.Exists(path)) File.Delete(path);
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
        RaiseLoaded();
    }

    public string GetSavePath() // Returns the full path to the save file
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    #endregion

    #region UI-friendly getters

    public IReadOnlyCollection<string> GetUpgrades() => upgrades;
    public (int count, string[] tablets) GetTabletSummary() => (numTabletsCollected, collectedTablets);
    public IReadOnlyCollection<string> GetDefeatedEnemies() => defeatedEnemies;
    public IReadOnlyCollection<string> GetCollectibles() => collectedCollectables;
    public IReadOnlyDictionary<string, string> GetNpcStatuses() => npcStatuses;

    #endregion

    #region Event helpers

    public void RaiseLoaded() => OnLoaded?.Invoke();
    public void RaiseSaved() => OnSaved?.Invoke();

    #endregion

    // --- Development Utilities ---
#if UNITY_EDITOR
    [ContextMenu("DEV: Clear All Progress")]
    private void DevClearAll()
    {
        ClearAll();
    }
#endif
}
