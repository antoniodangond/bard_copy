using UnityEngine;

public class PlayerMovement_New
{
    private PlayerConfig _playerConfig;

    public PlayerMovement_New(PlayerConfig playerConfig)
    {
        _playerConfig = playerConfig;
    }

    public void Move(Rigidbody2D rb, Vector2 movement)
    {
        // TODO: handle rotation
        // if (shouldRotateOnTurn)
        // {
        //     HandleRotation(movement);
        // }

        // Normalize movement vector to set mangitutude to 1. This prevents speed
        // increase when moving diagonally. Set linear velocity to movement vector,
        // so that physics are respected.
        rb.linearVelocity = movement.normalized * _playerConfig.MoveSpeed;
    }
}
