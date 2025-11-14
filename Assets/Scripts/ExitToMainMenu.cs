using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToMainMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    // Hook this to your button's OnClick event
    public void ExitToMenu()
    {
        // 1. Save game if PlayerProgress exists
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.SaveNow();
        }

        // 2. Reset timescale (Pause menu leaves it at 0)
        Time.timeScale = 1f;

        // 3. Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
