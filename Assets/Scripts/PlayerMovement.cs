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
    private float dashAmount = 15f;
    // private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem dashParticles_1;
    [SerializeField] private ParticleSystem dashParticles_2;
    private ParticleSystem dashParticles_1_Instance;
    private ParticleSystem dashParticles_2_Instance;
    public float moveSpeed;
    public Rigidbody2D rb;

    private void Awake()
    {
        // trailRenderer = gameObject.GetComponent<TrailRenderer>();
    }

    private void RotateTransform(bool shouldFaceRight)
    {
        float yRotation = shouldFaceRight ? 180f : 0f;
        Vector3 rotator = new Vector3(transform.rotation.x, yRotation, transform.rotation.z);
        transform.rotation = Quaternion.Euler(rotator);
        isFacingRight = shouldFaceRight;
    }

    private void spawndashParticles_1(Vector2 movement)
    {
        Quaternion spawnRotation_1 = Quaternion.FromToRotation(movement * -1, movement);
        Quaternion spawnRotation_2 = Quaternion.FromToRotation(Vector2.right, movement);
        Vector3 new_move = movement;
        var shape = dashParticles_1.shape;
        SpriteRenderer spr = gameObject.GetComponent<SpriteRenderer>();
        shape.spriteRenderer = spr;
        shape.rotation = PlayerController.getFacingDirectionVector2();
        dashParticles_1_Instance = Instantiate(dashParticles_1, transform.position + new Vector3(0, 1, 0), spawnRotation_1);
        dashParticles_1_Instance.transform.parent = gameObject.transform;
        dashParticles_2_Instance = Instantiate(dashParticles_2, transform.position + new_move, spawnRotation_2);
        dashParticles_2_Instance.transform.parent = gameObject.transform;
       
    }

    public void Dash(Vector2 movement)
    {
        if (movement.normalized == new Vector2(0, 0))
        {
            movement = PlayerController.getFacingDirectionVector2();
        }
        //no longer needed now that collision detection is continuous
        // RaycastHit2D raycastHit = Physics2D.Raycast(gameObject.transform.position, movement.normalized, 5f, 1<<11);
        // Debug.DrawRay(gameObject.transform.position, movement.normalized * 5f, Color.yellow, 1f);
        if (PlayerController.canDash == true && PlayerController.isDashing == true)
        {
            Vector2 dashDirection = movement;
            spawndashParticles_1(dashDirection * -1);
            PlayerAudio.instance.PlayPlayerDash();
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
            // Since changing the collision detection mode this is unnecessary,
            // but in case there is a desire to improve efficiency later,
            // I'm leaving this as a starting point

            // if (collider != null && rb.IsTouching(collider))
            // {
            //     // rb.linearVelocity = new Vector2(0, 0).normalized;
            //     break;
            // }
            _elapsedTime += Time.fixedDeltaTime;
            Vector2 Force = movement.normalized * moveSpeed * dashAmount;
            rb.linearVelocity = Force;
            // if (_elapsedTime >= 0.2f)
            // {
            //     trailRenderer.emitting = false;
            // }
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
