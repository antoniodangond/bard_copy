using UnityEngine;
using UnityEngine.InputSystem;
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

        // 2. Unpause game so that when returning we are no longer in a pause state
        PlayerInputManager.Instance.isPaused = false;

        // 3. Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
