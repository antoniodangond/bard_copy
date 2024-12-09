using UnityEngine;

public class GameManager_New : MonoBehaviour
{
    private GameStateDefault gameStateDefault = new GameStateDefault();
    private GameStatePaused gameStatePaused = new GameStatePaused();
    private GameStateInstrument gameStateInstrument = new GameStateInstrument();

    private GameState prevState;
    private GameState currentState;
    private IGameState currentStateController;

    public static bool IsPaused = false;

    void Start()
    {
        prevState = GameState.Default;
        currentState = GameState.Default;
        currentStateController = gameStateDefault;
        // TODO: call this?
        // currentStateController.EnterState();
    }

    void Update()
    {
        currentStateController.Update(this);
    }

    public void ChangeState(GameState newState)
    {
        switch (newState)
        {
            case GameState.Default:
                currentStateController = gameStateDefault;
                break;
            case GameState.Paused:
                currentStateController = gameStatePaused;
                break;
            case GameState.Instrument:
                currentStateController = gameStateInstrument;
                break;
        }
        prevState = currentState;
        currentState = newState;
        currentStateController.EnterState();
    }

    public void ChangeToPrevState()
    {
        ChangeState(prevState);
    }

    public static void SetIsPaused(bool isPaused)
    {
        IsPaused = isPaused;
    }
}
