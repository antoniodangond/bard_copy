using UnityEngine;

public class GameStateInstrument : IGameState
{
    public void EnterState()
    {
        Time.timeScale = 0f;
        GameManager_New.SetIsPaused(false);
        // TODO: pause audio
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
