using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Tooltip("Seconds to debounce rapid save calls.")]
    public float saveDebounceSeconds = 0.5f;

    [Tooltip("Minutes between autosaves (0 to disable).")]
    public float autosaveMinutes = 3f;

    private float _pendingSaveAt = -1f;
    private bool _saveScheduled;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Attempt load on boot
        PlayerProgress.Instance.Load();              // make Load() public
        PlayerProgress.Instance.RaiseLoaded();
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (autosaveMinutes > 0) StartCoroutine(AutoSaveLoop());
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) FlushSaveNow(); // mobile safety
    }
    void OnApplicationQuit()
    {
        FlushSaveNow();
    }

    public void RequestSave()
    {
        _pendingSaveAt = Time.unscaledTime + saveDebounceSeconds;
        if (!_saveScheduled) StartCoroutine(SaveDebounced());
    }

    IEnumerator SaveDebounced()
    {
        _saveScheduled = true;
        while (Time.unscaledTime < _pendingSaveAt)
            yield return null;

        FlushSaveNow();
        _saveScheduled = false;
    }

    public void FlushSaveNow()
    {
        try
        {
            // Atomic-ish write: write tmp, then replace
            string path = PlayerProgress.Instance.GetSavePath();
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string tmp = path + ".tmp";
            File.WriteAllText(tmp, PlayerProgress.Instance.BuildJson());
            if (File.Exists(path))
                File.Replace(tmp, path, path + ".bak", true);
            else
                File.Move(tmp, path);

            PlayerProgress.Instance.RaiseSaved();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] Failed: {e}");
        }
    }

    IEnumerator AutoSaveLoop()
    {
        var wait = new WaitForSecondsRealtime(Mathf.Max(30f, autosaveMinutes * 60f));
        while (true)
        {
            yield return wait;
            RequestSave();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Optionally restore player position only in intended scenes:
        // e.g., if (scene.name == PlayerProgress.Instance.CurrentScene)
        PlayerProgress.Instance.ApplyToWorld(); // world listeners react here
    }
}
