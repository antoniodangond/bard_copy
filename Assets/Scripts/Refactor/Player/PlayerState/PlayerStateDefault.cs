using UnityEngine;

public class PlayerStateDefault: IPlayerState
{
    // TODO: probably better to have on the controller?
    // private Vector2 movement;
    // private bool isWalking => movement.sqrMagnitude > 0;

    public void Update(PlayerController_New playerController)
    {
        // TODO:
        // check input? or leave that in controller? maybe a handle input? or just take that as an argument on input
        // TODO: make any of these default implementations?
        // move - should be handled by movement controller
        // footstep audio - should be handled by audio controller

        // handle input, decide if a new state should be entered. if so, should exit early

        // TODO:
        // check if damaged? or better to have public iface method TakeDamage?
        // check if instrument toggled
        // check if attacking
        // set local movement variable
        // if moving, attempt to play walking audio via playerAudio

        // TODO: improve PlayerInputManager
        if (PlayerInputManager.WasToggleInstrumentPressed)
        {
            playerController.ChangeState(PlayerState_New.Instrument);
            return;
        }
    }

    public void FixedUpdate(PlayerController_New playerController)
    {
        // TODO: use playerMovement to move - should this state class really have its own instance of player audio and player movement? should they all be static?
        playerController.Move();
    }
}
