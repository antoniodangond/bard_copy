using System.Collections;
using UnityEngine;

public enum EnemyState {
    Default,
    Agro,
    Attacking,
    AttackCooldown,
    Dead,
}

public class EnemyController : MonoBehaviour
{
    public LayerMask PlayerLayer;
    public float AgroTimeBeforeAttack;
    public float AttackCooldownTime;
    public float MoveSpeed;
    public float AttackDurationSeconds;

    // TODO: make customizable in editor
    private float health = 1f;
    private EnemyState currentState = EnemyState.Default;
    private Vector2 targetDirection;
    private GameObject target;
    private CircleCollider2D circleCollider2D;
    private Animator animator;
    private bool isFacingRight = false;
    private EnemyAudio enemyAudio;
    private Rigidbody2D rb;

    void Awake()
    {
        enemyAudio = GetComponent<EnemyAudio>();
        isFacingRight = transform.rotation.eulerAngles.y == 180;
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        circleCollider2D = GetComponent<CircleCollider2D>();
        animator = GetComponent<Animator>();
    }

    // Triggered from animation
    public void OnEnemyDeath()
    {
        CustomEvents.OnEnemyDeath?.Invoke(gameObject);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            Debug.Log("Enemy dead");
            currentState = EnemyState.Dead;
            animator.SetBool(AnimatorParams.IsDead, true);
            // TODO: BUG - don't destroy game object before attempting to play hit
            // enemyAudio.PlayHit();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == EnemyState.Attacking && Utils.HasTargetLayer(PlayerLayer, collision.gameObject))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            // TODO: improve this so that player can only take damage once per attack
            playerController.TakeDamage();
        }
    }

    private Vector2 getDirectionToTarget(Rigidbody2D targetRigidbody)
    {
        return (targetRigidbody.position - (Vector2)transform.position).normalized;
    }

    private IEnumerator StartAttack(GameObject other)
    {
        // Exit early if enemy has died
        if (currentState == EnemyState.Dead) { yield break; }
        // Enter agro state and handle facing direction if necessary
        Rigidbody2D targetRigidbody = other.GetComponent<Rigidbody2D>();
        Debug.Log("Enemy aggro");
        currentState = EnemyState.Agro;
        // Calculate the direction towards the target, in case we need to change facing direction
        Vector2 direction = getDirectionToTarget(targetRigidbody);
        // Rotate transform if necessary
        HandleRotation(direction);
        enemyAudio.PlayAggro();
        yield return new WaitForSeconds(AgroTimeBeforeAttack);

        // Start attack
        // Exit early if enemy has died
        if (currentState == EnemyState.Dead) { yield break; }
        Debug.Log("Enemy attack");
        currentState = EnemyState.Attacking;
        // Calculate the direction towards the target again, in case the player has moved
        targetDirection = getDirectionToTarget(targetRigidbody);
        // Rotate transform again if necessary
        HandleRotation(direction);
        animator.SetBool(AnimatorParams.IsMoving, true);
        enemyAudio.PlayAttack();
        // After attack duration, begin cooldown
        yield return new WaitForSeconds(AttackDurationSeconds);
        StartCoroutine(AttackCooldown());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == EnemyState.Default && Utils.HasTargetLayer(PlayerLayer, other.gameObject))
        {
            target = other.gameObject;
            StartCoroutine(StartAttack(other.gameObject));
        }
    }

    private IEnumerator AttackCooldown()
    {
        // Exit early if enemy has died
        if (currentState == EnemyState.Dead) { yield break; }
        Debug.Log("Enemy cooldown");
        currentState = EnemyState.AttackCooldown;
        // Reset target direction
        targetDirection = Vector2.zero;
        animator.SetBool(AnimatorParams.IsMoving, false);
        yield return new WaitForSeconds(AttackCooldownTime);
        // Exit early if enemy has died
        if (currentState == EnemyState.Dead) { yield break; }
        Debug.Log("Enemy default");
        currentState = EnemyState.Default;
        // Check if player is still in agro range. If so, attack again
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, circleCollider2D.radius, PlayerLayer);
        if (hitPlayers.Length > 0)
        {
            target = hitPlayers[0].gameObject;
            StartCoroutine(StartAttack(target));
        }
    }

    // TODO: consolidate with PlayerController function
    private void RotateTransform(bool shouldFaceRight)
    {
        float yRotation = shouldFaceRight ? 180f : 0f;
        Vector3 rotator = new Vector3(transform.rotation.x, yRotation, transform.rotation.z);
        transform.rotation = Quaternion.Euler(rotator);
        isFacingRight = shouldFaceRight;
    }

    // TODO: consolidate with PlayerController function
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

    void FixedUpdate()
    {
        // Move only if attacking
        rb.linearVelocity = currentState == EnemyState.Attacking ? targetDirection * MoveSpeed : Vector2.zero;
    }
}
