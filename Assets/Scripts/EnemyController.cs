using System;
using System.Collections;
// using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public enum EnemyState
{
    Default,
    Agro,
    Attacking,
    AttackCooldown,
    Dead,
}

public class EnemyController : MonoBehaviour
{
    public LayerMask PlayerLayer;
    public string enemyName;
    public float AgroTimeBeforeAttack;
    public float AttackCooldownTime;
    public float MoveSpeed;
    public float AttackDurationSeconds;
    public float retreatLength;
    public float Health;
    [SerializeField] private ParticleSystem damageParticles;
    private ParticleSystem damageParticlesInstance;
    private EnemyState currentState = EnemyState.Default;
    private Vector2 targetDirection;
    private GameObject target;
    private CircleCollider2D circleCollider2D;
    private Animator animator;
    private bool isFacingRight = false;
    private Vector2 knockBackDirection;
    private float knockBackForce;
    private float knockBackTime;
    private bool isBeingKnockedBack= false;
    private EnemyAudio enemyAudio;
    public AudioSource audioSource;
    public AudioSource audioSource3D;
    private AudioMixerScript audioMixerScript;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color defaultSpriteColor;
    private float colorChangeDuration = 0.25f;

    void Awake()
    {
        enemyAudio = FindAnyObjectByType<EnemyAudio>();
        audioMixerScript = GetComponent<AudioMixerScript>();
        audioSource = gameObject.GetComponent<AudioSource>();
        if (enemyName == "Phantom") {isFacingRight = transform.rotation.eulerAngles.y == -180;}
        else {isFacingRight = transform.rotation.eulerAngles.y == 180;}
        // isFacingRight = transform.rotation.eulerAngles.y == 180;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultSpriteColor = spriteRenderer.color;
        knockBackTime = 0.25f;
        if (enemyName == "Owl")
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("PhysicalBarrier");
            foreach (GameObject item in gameObjects)
            {
                if (item.GetComponent<BoxCollider2D>() != null)
                {
                    Physics2D.IgnoreCollision(item.GetComponent<BoxCollider2D>(), gameObject.GetComponent<PolygonCollider2D>());
                }
                else if (item.GetComponent<PolygonCollider2D>() != null)
                {
                    Physics2D.IgnoreCollision(item.GetComponent<PolygonCollider2D>(), gameObject.GetComponent<PolygonCollider2D>());
                }
                else { continue; }
            }

            StartCoroutine(IdleSounds(enemyName, audioSource));
        }
        HandleAudioMixerGroupRouting();
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

    private IEnumerator handleDeathRoutine()
    {
        enemyAudio.PlayHit(audioSource, enemyName);
        // yield return new WaitForSeconds(audioSource.clip.length);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

    public void KnockBack(PlayerController playerController)
    {
        isBeingKnockedBack = true;
        Vector2 playerTransform = playerController.transform.position;
        Vector2 enemyTransform = gameObject.transform.position;
        // Debug.Log($"player vector 2 is {playerTransform} and enemy's is {enemyTransform}");
        knockBackDirection = (enemyTransform - playerTransform).normalized;
        // Debug.Log($"knock back dir is {knockBackDirection}");
        knockBackForce = 10f;
        StartCoroutine(KnockBackAction(knockBackDirection));
    }
    

    public IEnumerator KnockBackAction(Vector2 hitDirection)
    {
        float _elapsedTime = 0f;

        Vector2 hitForce = hitDirection * knockBackForce;
        while (_elapsedTime < knockBackTime)
        {
            // Iterate the timer
            _elapsedTime += Time.fixedDeltaTime;

            // Apply Knock Back
            rb.linearVelocity = hitForce;

            yield return new WaitForFixedUpdate();
        }

        isBeingKnockedBack = false;
    }

    private void SpawnDamageParticles(Vector2 attackDirection)
    {
        Quaternion spawnRotation = Quaternion.FromToRotation(Vector2.up, attackDirection);

        damageParticlesInstance = Instantiate(damageParticles, transform.position + new Vector3(0, 1, 0), spawnRotation);
    }

    public void TakeDamage(float damage, PlayerController playerController, Vector2 attackDirection)
    {

        Health -= damage;
        enemyAudio.PlayHit(audioSource, enemyName);
        SpawnDamageParticles(attackDirection);
        StartCoroutine(EnemyDamageColorChangeRoutine());

        if (Health <= 0f)
        {
            // Debug.Log("Enemy dead");
            currentState = EnemyState.Dead;
            animator.SetBool(AnimatorParams.IsDead, true);
            // TODO: BUG - don't destroy game object before attempting to play hit
            StartCoroutine(handleDeathRoutine());
        }
        KnockBack(playerController);
    }

    public IEnumerator EnemyDamageColorChangeRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < colorChangeDuration)
        {
            // Calculate t based on elapsed time
            float t = elapsedTime / colorChangeDuration;

            // Lerp between defaultSpriteColor and Color.red
            spriteRenderer.color = Color.Lerp(defaultSpriteColor, Color.red, t);

            // Increment elapsed time
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        
        // Reset to default color
        spriteRenderer.color = defaultSpriteColor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Turning off the EnemyState check for now to make combat feel a little more responsive
        if (/*currentState == EnemyState.Attacking && */Utils.HasTargetLayer(PlayerLayer, collision.gameObject))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            // TODO: improve this so that player can only take damage once per attack
            DamagePlayer(playerController);
        }
    }
    

    private void DamagePlayer(PlayerController playerController)
    {
        if (playerController.isTakingDamage)
            {
                return;
            }
        StartCoroutine(playerController.TakeDamageRoutine());
    }

    private Vector2 getDirectionToTarget(Rigidbody2D targetRigidbody)
    {
        return (targetRigidbody.position - (Vector2)transform.position).normalized;
    }

    private float getAttackTime(Rigidbody2D targetRigidbody)
    {
        float travelDistance = Vector2.Distance(targetRigidbody.position, (Vector2)transform.position);
        return (travelDistance / MoveSpeed) + 0.25f;
    }

    private IEnumerator StartAttack(GameObject other)
    {
        // Exit early if enemy has died
        if (currentState == EnemyState.Dead) { yield break; }
        // Enter agro state and handle facing direction if necessary
        Rigidbody2D targetRigidbody = other.GetComponent<Rigidbody2D>();
        currentState = EnemyState.Agro;
        // Calculate the direction towards the target, in case we need to change facing direction
        Vector2 direction = getDirectionToTarget(targetRigidbody);
        // Rotate transform if necessary
        HandleRotation(direction);
        enemyAudio.PlayAggro(audioSource, enemyName);
        yield return new WaitForSeconds(AgroTimeBeforeAttack);

        // Start attack
        // Exit early if enemy has died
        if (currentState == EnemyState.Dead) { yield break; }
        // Debug.Log("Enemy attack");
        currentState = EnemyState.Attacking;
        // Calculate the direction towards the target again, in case the player has moved
        targetDirection = getDirectionToTarget(targetRigidbody);
        // Rotate transform again if necessary
        HandleRotation(direction);
        animator.SetBool(AnimatorParams.IsMoving, true);
        enemyAudio.PlayAttack(audioSource, enemyName);
        // After attack duration, begin cooldown
        if (enemyName == "Owl")
        {
            yield return new WaitForSeconds(getAttackTime(targetRigidbody));
        }
        else
        {
            yield return new WaitForSeconds(AttackDurationSeconds);
        }
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
        // float retreatLength = AttackDurationSeconds / 10;
        // Exit early if enemy has died
        if (currentState == EnemyState.Dead) { yield break; }
        StartCoroutine(Retreat(retreatLength));
        yield return new WaitForSeconds(retreatLength);
        // Debug.Log("Enemy cooldown");
        currentState = EnemyState.AttackCooldown;
        // Reset target direction
        targetDirection = Vector2.zero;
        animator.SetBool(AnimatorParams.IsMoving, false);
        yield return new WaitForSeconds(AttackCooldownTime);
        // Exit early if enemy has died
        if (currentState == EnemyState.Dead) { yield break; }
        // Debug.Log("Enemy default");
        currentState = EnemyState.Default;
        // Check if player is still in agro range. If so, attack again
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, circleCollider2D.radius, PlayerLayer);
        if (hitPlayers.Length > 0)
        {
            target = hitPlayers[0].gameObject;
            StartCoroutine(StartAttack(target));
        }
    }

    private IEnumerator Retreat(float retreatLength)
    {
        float retreatSpeed = UnityEngine.Random.Range(0,1.5f);
        float retreatTime = UnityEngine.Random.Range(0.125f, retreatLength);
        float origMoveSpeed = MoveSpeed;

        // Don't retreat if dead
        if (currentState == EnemyState.Dead) { yield break; }

        // rb.linearVelocity = rb.linearVelocity * retreatSpeed;
        MoveSpeed = rb.linearVelocity.magnitude * retreatSpeed;
        targetDirection = targetDirection * new Vector2(-1,-1);

        yield return new WaitForSeconds(retreatTime);
        MoveSpeed = origMoveSpeed;
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
        if (currentState == EnemyState.Attacking && !isBeingKnockedBack)
        {
            rb.linearVelocity = targetDirection * MoveSpeed;
        }
        else if (isBeingKnockedBack)
        {
            // Don't update linear velocity while being knocked back
        }
        else
        {
            // Stop moving after knock back or attack
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleAudioMixerGroupRouting()
    {
        try
        {
            audioMixerScript.assignSFXGroup(audioSource);
            if (audioSource3D) { audioMixerScript.assignSFXGroup(audioSource3D); }
        }
        catch { Debug.Log(gameObject.name); }
        // apparently i'm using just one audio source for all sounds, even though i'm instantiating 3...
        // audioMixerScript.assignSFXGroup(enemyAudio.AudioData.RandomAggro.audioSource);
        // audioMixerScript.assignSFXGroup(enemyAudio.AudioData.RandomAttacks.audioSource);
    }

    private IEnumerator IdleSounds (String EnemyName, AudioSource audioSource)
    {
        System.Random rnd = new System.Random();
        if (EnemyName == "Owl")
        {
            while (!gameObject.IsDestroyed())
            {
                float secondsToWait;
                secondsToWait = rnd.Next(0,10);
                yield return new WaitForSeconds(secondsToWait);
                enemyAudio.PlayIdleSounds(EnemyName, audioSource3D);
                secondsToWait = rnd.Next(3, 7) + audioSource3D.clip.length;
                yield return new WaitForSeconds(secondsToWait);
            }
        }
    }
}
