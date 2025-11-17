using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueGame : MonoBehaviour
{
    public void Continue()
    {
        if (PlayerProgress.Instance == null)
        {
            Debug.LogError("[Continue] PlayerProgress.Instance is null.");
            return;
        }

        if (!PlayerProgress.Instance.HasSaveFile())
        {
            Debug.LogWarning("[Continue] No save file found. Continue disabled.");
            return;
        }

        // Force a fresh load from disk and notify listeners
        PlayerProgress.Instance.ReloadFromDisk();

        string sceneToLoad = PlayerProgress.Instance.GetSavedSceneName();

        Debug.Log($"[Continue] GetSavedSceneName() returned '{sceneToLoad}'");

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[Continue] Saved scene name is null or empty.");
            return;
        }

        // Make sure this scene name is exactly the one in Build Settings
        Debug.Log($"[Continue] Loading saved scene '{sceneToLoad}'...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }
}
