using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueGame : MonoBehaviour
{
    public void Continue()
    {
        // Must exist or there is nothing to continue
        if (PlayerProgress.Instance == null || !PlayerProgress.Instance.HasSaveFile())
        {
            Debug.LogWarning("[Continue] No save file found. Continue disabled.");
            return;
        }

        // Ensure the save is loaded (your PlayerProgress already loads on Awake)
        string sceneToLoad = PlayerProgress.Instance.GetSavedSceneName();

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[Continue] Saved scene name is null or empty.");
            return;
        }

        Debug.Log($"[Continue] Loading saved scene '{sceneToLoad}'...");
        Time.timeScale = 1f; // ensure game unpaused
        SceneManager.LoadScene(sceneToLoad);
    }
}
