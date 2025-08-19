using System.Collections;
using Unity.VisualScripting;

// using System.Numerics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Set shouldRotateOnTurn to true if only 1 animation is used for horizontal movement
    public bool shouldRotateOnTurn = false;
    private bool isFacingRight = false;
    private float dashTime = 1f;
    private float dashAmount = 50f;
    private Vector2 dashDirection;

    public float moveSpeed;
    public Rigidbody2D rb;

    private void RotateTransform(bool shouldFaceRight)
    {
        float yRotation = shouldFaceRight ? 180f : 0f;
        Vector3 rotator = new Vector3(transform.rotation.x, yRotation, transform.rotation.z);
        transform.rotation = Quaternion.Euler(rotator);
        isFacingRight = shouldFaceRight;
    }

    private void Dash(Vector2 movement)
    {
        dashDirection = movement;
        // if (PlayerInputManager.canDash)
        // {
            StartCoroutine(DashRoutine(dashDirection));
        // }
    }

    private IEnumerator DashRoutine(Vector2 movement)
    {
        float _elapsedTime = 0;
        Vector2 Force = movement.normalized * dashAmount;
        while (_elapsedTime < dashTime)
        {
            // Increment timer
            _elapsedTime += Time.fixedDeltaTime;

            // Apply dash
            rb.linearVelocity = Force;
            yield return new WaitForFixedUpdate();
        }
    }

    private void HandleRotation(Vector2 movement)
    {
        // If moving right, rotate the player transform so that the
        // left-facing sprite is facing right
        if (!isFacingRight && movement.x > 0)
        {
            RotateTransform(true);
        }
        // If moving left, rotate the transform back to the default direction
        else if (isFacingRight && movement.x < 0)
        {
            RotateTransform(false);
        }
    }

    public void Move(Vector2 movement)
    {
        if (shouldRotateOnTurn)
        {
            HandleRotation(movement);
        }
        // Normalize movement vector to set mangitutude to 1. This prevents speed
        // increase when moving diagonally. Set linear velocity to movement vector,
        // so that physics are respected.
        if (PlayerInputManager.wasDashPressed)
        {
            Dash(movement);
        }
        else { rb.linearVelocity = movement * moveSpeed; }
    }
}
