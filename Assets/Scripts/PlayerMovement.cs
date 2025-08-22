using System.Collections;
using Unity.VisualScripting;

// using System.Numerics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Set shouldRotateOnTurn to true if only 1 animation is used for horizontal movement
    public bool shouldRotateOnTurn = false;
    private bool isFacingRight = false;
    public float dashCooldownTime = 1f;
    private float dashAmount = 100f;
    private bool canDash;
    public float moveSpeed;
    public Rigidbody2D rb;

    private void Awake()
    {
        canDash = true;
    }
    private void RotateTransform(bool shouldFaceRight)
    {
        float yRotation = shouldFaceRight ? 180f : 0f;
        Vector3 rotator = new Vector3(transform.rotation.x, yRotation, transform.rotation.z);
        transform.rotation = Quaternion.Euler(rotator);
        isFacingRight = shouldFaceRight;
    }

    public void Dash(Vector2 movement)
    {
        if (canDash == true && PlayerController.isDashing == true)
        {
            Debug.Log("Dash!");
            canDash = false;
            Vector2 Force = movement.normalized * dashAmount;
            rb.linearVelocity = Force;
            StartCoroutine(DashCooldownRoutine());
            PlayerController.isDashing = false;
        }
        else { return; }
    }

    private IEnumerator DashCooldownRoutine()
    {
        float _elapsedTime = 0;
        while (_elapsedTime < dashCooldownTime)
        {
            // Increment timer
            _elapsedTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }
        canDash = true;
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
        rb.linearVelocity = movement * moveSpeed;
    }
}
