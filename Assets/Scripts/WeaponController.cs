using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LayerMask))]
public class WeaponController : MonoBehaviour
{
    public float AttackRange;
    private Vector2 attackRangeV2;
    public LayerMask enemyLayer;
    public PlayerController playerController;
    private Vector2 attackDirection;
    Vector3 transformValue;
    [SerializeField] private ParticleSystem attackParticles;
    private ParticleSystem attackParticlesInstance;

    // TODO: make customizable in editor
    private float damage = 1f;

    public void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    public void Update()
    {
        transformValue = playerController.transform.position;
        switch (PlayerController.FacingDirection)
        {
            case FacingDirection.Up:
                gameObject.transform.position = transformValue + new Vector3(0, 4, 0);
                break;
            case FacingDirection.Down:
                gameObject.transform.position = transformValue - new Vector3(0, 1, 0);
                break;
            case FacingDirection.Left:
                gameObject.transform.position = transformValue - new Vector3(2f, 0, 0) + new Vector3(0, 1.5f, 0);
                break;
            case FacingDirection.Right:
                gameObject.transform.position = transformValue + new Vector3(2f, 1.5f, 0);
                break;
            default:
                break;
        }
    }

    private Collider2D[] getHitColliders()
    {
        switch (PlayerController.FacingDirection)
        {
            case FacingDirection.Up:
                attackRangeV2 = new Vector2(2, AttackRange + 2);
                return Physics2D.OverlapBoxAll(transform.position + new Vector3(0, 1, 0), attackRangeV2, 0, enemyLayer);
            // gameObject.transform.position = transformValue + new Vector3(0, 2, 0);
            case FacingDirection.Down:
                attackRangeV2 = new Vector2(2, AttackRange + 1);
                return Physics2D.OverlapBoxAll(transform.position + new Vector3(0, -1, 0), attackRangeV2, 0, enemyLayer);
            // return new Vector2(2.5f, AttackRange * -1);
            // gameObject.transform.position = transformValue - new Vector3(0, 1, 0);
            case FacingDirection.Left:
                attackRangeV2 = new Vector2(AttackRange + 1.5f, 2);
                return Physics2D.OverlapBoxAll(transform.position + new Vector3(-1, 0, 0), attackRangeV2, 90, enemyLayer);
            // return new Vector2((AttackRange * -1) - 1, 2.5f);
            // gameObject.transform.position = transformValue - new Vector3(1, 0, 0) + new Vector3 (0,0.5f,0);
            case FacingDirection.Right:
                attackRangeV2 = new Vector2(AttackRange + 1.5f, 2);
                return Physics2D.OverlapBoxAll(transform.position + new Vector3(1, 0, 0), attackRangeV2, -90, enemyLayer);
            // return new Vector2(AttackRange+ 1, 2.5f);
            // gameObject.transform.position = transformValue + new Vector3(1, 0.5f, 0);
            default:
                return new Collider2D[0];
        }
    }
    // trying to replace with the "getHitColliders" function, keeping in case I mess it up
    // private Vector2 getAttackRangeV2() {
    //     switch (PlayerController.FacingDirection)
    //     {
    //         case FacingDirection.Up:
    //             return new Vector2(2.5f, AttackRange + 2);
    //         // gameObject.transform.position = transformValue + new Vector3(0, 2, 0);
    //         case FacingDirection.Down:
    //             return new Vector2(2.5f, AttackRange * -1);
    //         // gameObject.transform.position = transformValue - new Vector3(0, 1, 0);
    //         case FacingDirection.Left:
    //             return new Vector2((AttackRange * -1) - 1, 2.5f);
    //         // gameObject.transform.position = transformValue - new Vector3(1, 0, 0) + new Vector3 (0,0.5f,0);
    //         case FacingDirection.Right:
    //             return new Vector2(AttackRange+ 1, 2.5f);
    //         // gameObject.transform.position = transformValue + new Vector3(1, 0.5f, 0);
    //         default:
    //             return new Vector2(0, 0);
    //     }
    // }
    private Vector2 getAttackDirection() 
    {
        if (PlayerInputManager.Movement == new Vector2(0, 0)) { return PlayerController.getFacingDirectionVector2(); }
        else { return PlayerInputManager.Movement; }
    }
    private void handleAttackParticles()
    {
        // Spawn particles halfway between player and end of attack and increase height to be away from Player's feet
        Vector3 heightOffset;
        Vector3 eulerAngles = gameObject.transform.rotation.eulerAngles;
        Quaternion particleRotation;
        // Vector3 particleSpawnOffset = (Vector3)(attackDirection.normalized * (AttackRange / 2f)) + new Vector3(0, 1.5f, 0);
        switch (PlayerController.FacingDirection)
        {
            case FacingDirection.Up:
                heightOffset = new Vector3(0, 0, 0);
                eulerAngles.z += 90f;
                particleRotation = Quaternion.Euler(eulerAngles);
                break;
            case FacingDirection.Down:
                heightOffset = new Vector3(0, 0, 0);
                eulerAngles.z -= 90f;
                particleRotation = Quaternion.Euler(eulerAngles);
                break;
            case FacingDirection.Left:
                heightOffset = new Vector3(0, 1.5f, 0);
                eulerAngles.z += 180f;
                particleRotation = Quaternion.Euler(eulerAngles);
                break;
            case FacingDirection.Right:
                heightOffset = new Vector3(0, 1.5f, 0);
                particleRotation = Quaternion.Euler(eulerAngles);
                break;
            default:
                heightOffset = new Vector3(0, 1, 0);
                particleRotation = Quaternion.Euler(eulerAngles);
                break;
        }
        attackDirection = getAttackDirection();
        // Vector3 particleSpawnOffset = (Vector3) attackDirection + heightOffset;
        // Vector3 particleSpawnPosition = transform.position + particleSpawnOffset;
        Vector3 particleSpawnPosition = transform.position - (Vector3) attackDirection + heightOffset;
        // attackParticles.velocityOverLifetime.x

        // Rotate particle system to face the attack direction
        // float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        // Quaternion particleRotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        attackParticlesInstance = Instantiate(attackParticles, particleSpawnPosition, particleRotation);
        // attackParticlesInstance = Instantiate(attackParticles, particleSpawnPosition, playerController.transform.rotation);
    }
    public void Attack()
    {
        // attackRangeV2 = getAttackRangeV2();

        attackDirection = getAttackDirection();
        Collider2D[] hitColliders = getHitColliders();
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Ignore enemy trigger colliders, which are just used for agro range
            if (hitCollider.isTrigger)
            {
                continue;
            }
            
            EnemyController enemyController = hitCollider.GetComponent<EnemyController>();
            enemyController.TakeDamage(damage, playerController, attackDirection);
        }
        handleAttackParticles();
    }
    // saving old code for attack as it. This will be the AOE attack that the player will get in the dungeon
    // will be renamed accordingly
    // public void Attack()
    // {
    //     Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, AttackRange, enemyLayer);
    //     foreach (Collider2D hitCollider in hitColliders)
    //     {
    //         // Ignore enemy trigger colliders, which are just used for agro range
    //         if (hitCollider.isTrigger)
    //         {
    //             continue;
    //         }
    //         if (PlayerInputManager.Movement == new Vector2(0, 0))
    //         {
    //             attackDirection = PlayerController.getFacingDirectionVector2();
    //         }
    //         else { attackDirection = PlayerInputManager.Movement; }
    //         // Vector2 attackDirection = PlayerController.getFacingDirectionVector2(PlayerController.FacingDirection);
    //         EnemyController enemyController = hitCollider.GetComponent<EnemyController>();
    //         enemyController.TakeDamage(damage, playerController, attackDirection);
    //     }
    // }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3 attackRangeV3 = attackRangeV2;
        Gizmos.DrawWireCube(gameObject.transform.position, attackRangeV3);
    }
}
