using System.Collections;
using Unity.VisualScripting;
// using System.Numerics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public ContactFilter2D contactFilter;
    // Set shouldRotateOnTurn to true if only 1 animation is used for horizontal movement
    public bool shouldRotateOnTurn = false;
    private bool isFacingRight = false;
    public float dashCooldownTime = 1f;
    public float dashTime = 0.25f;
    private float dashAmount = 35f;
    private TrailRenderer trailRenderer;
    public float moveSpeed;
    // private Vector2[] dashDirections = new Vector2[]
    // {
    //     new Vector2(0,0) // Nothing
    //     , new Vector2(1,0) // Right
    //     , new Vector2(1,1) // Top-Right
    //     , new Vector2(0,1) // Up
    //     , new Vector2(-1,1) // Top-Left
    //     , new Vector2(-1,0) // Left
    //     , new Vector2(-1,-1) // Down-Left
    //     , new Vector2(0,-1) // Down
    //     , new Vector2(1,-1) // Down-Right
    // };
    public Rigidbody2D rb;

    private void Awake()
    {
        trailRenderer = gameObject.GetComponent<TrailRenderer>();
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
        if (movement.normalized == new Vector2(0, 0))
        {
            switch (PlayerController.FacingDirection)
            {
                case FacingDirection.Up:
                    movement = new Vector2(0, 1);
                    break;
                case FacingDirection.Right:
                    movement = new Vector2(1, 0);
                    break;
                case FacingDirection.Down:
                    movement = new Vector2(0, -1);
                    break;
                case FacingDirection.Left:
                    movement = new Vector2(-1, 0);
                    break;
                default:
                    break;
            }
        }
        RaycastHit2D raycastHit = Physics2D.Raycast(gameObject.transform.position, movement.normalized, 10f);
        Debug.Log(raycastHit.collider);
        Debug.DrawRay(gameObject.transform.position, movement.normalized * 20f, Color.yellow);
        if (PlayerController.canDash == true && PlayerController.isDashing == true)
        {
            Vector2 dashDirection = movement;
            Debug.Log("hi");
            StartCoroutine(DashRoutine(dashDirection));
            StartCoroutine(DashCooldownRoutine());
            PlayerController.isDashing = false;
        }
        else { return; }
    }

    private IEnumerator DashRoutine(Vector2 movement)
    {
        float _elapsedTime = 0f;
        PlayerController.canDash = false;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        while (_elapsedTime < dashTime)
        {
            trailRenderer.emitting = true;
            _elapsedTime += Time.fixedDeltaTime;
            Vector2 Force = movement.normalized * moveSpeed * dashAmount;
            rb.linearVelocity = Force;
            if (_elapsedTime >= 0.2f)
            {
                trailRenderer.emitting = false;
            }
            yield return new WaitForFixedUpdate();
        }
        rb.gravityScale = originalGravity;
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
        PlayerController.canDash = true;
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
