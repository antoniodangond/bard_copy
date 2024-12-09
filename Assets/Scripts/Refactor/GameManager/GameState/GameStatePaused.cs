using UnityEngine;

public class GameStatePaused : IGameState
{
    public void EnterState()
    {
        Time.timeScale = 0f;
        GameManager_New.SetIsPaused(true);
        // TODO: pause player animations
        // TODO: pause audio
    }

    // TODO: prevent player from receiving input while paused?
    // or add a "paused" state to the player?
    public void Update(GameManager_New gameManager)
    {
        // TODO: improve PlayerInputManager
        if (PlayerInputManager.MenuClosed)
        {
            gameManager.ChangeToPrevState();
            // TODO: close pause menu
            return;
        }
    }
}
