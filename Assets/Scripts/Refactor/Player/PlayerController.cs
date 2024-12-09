using UnityEngine;

public class PlayerController_New : MonoBehaviour{
    public Rigidbody2D rb;
    public PlayerConfig playerConfig;

    private PlayerMovement_New playerMovement;
    private PlayerAudio_New playerAudio;

    // States
    private PlayerStateDefault playerStateDefault = new PlayerStateDefault();
    private PlayerStateInstrument playerStateInstrument = new PlayerStateInstrument();
    // TODO: add additional states
    private IPlayerState currentState;

    // Movement
    private Vector2 movement;
    private bool isWalking => movement.sqrMagnitude > 0;

    void Start()
    {
        playerMovement = new PlayerMovement_New(playerConfig);
        playerAudio = new PlayerAudio_New();
        currentState = playerStateDefault;
        // TODO: is this needed?
        currentState.EnterState();
    }

    void Update()
    {
        // TODO: improve player input manager
        movement = PlayerInputManager.Movement;

        currentState.Update(this);
    }

    void FixedUpdate()
    {
        currentState.FixedUpdate(this);
    }

    public void ChangeState(PlayerState_New playerState)
    {
        switch (playerState)
        {
            case PlayerState_New.Default:
                currentState = playerStateDefault;
                break;
            case PlayerState_New.Instrument:
                currentState = playerStateInstrument;
                break;
        }
    }

    public void Move()
    {
        playerMovement.Move(rb, movement);
    }

    public void PlayWalkingAudio()
    {

    }
}
