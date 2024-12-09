using UnityEngine;

public class GameStateDefault : IGameState
{
    public void EnterState()
    {
        Time.timeScale = 1f;
        GameManager_New.SetIsPaused(false);
        // TODO: unpause player animations
        // TODO: unpause audio
    }

    public void Update(GameManager_New gameManager)
    {
        // TODO: improve PlayerInputManager
        if (PlayerInputManager.MenuOpened)
        {
            gameManager.ChangeState(GameState.Paused);
            // TODO: open pause menu
            return;
        }
        // TODO: check for instrument press
    }
}
