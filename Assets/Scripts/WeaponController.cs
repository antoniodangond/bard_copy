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
                gameObject.transform.position = transformValue + new Vector3(0, 2, 0);
                break;
            case FacingDirection.Down:
                gameObject.transform.position = transformValue - new Vector3(0, 1, 0);
                break;
            case FacingDirection.Left:
                gameObject.transform.position = transformValue - new Vector3(1, 0, 0) + new Vector3(0, 0.5f, 0);
                break;
            case FacingDirection.Right:
                gameObject.transform.position = transformValue + new Vector3(1, 0.5f, 0);
                break;
            default:
                break;
        }
    }

    private Collider2D[] getHitColliders() {
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
                attackRangeV2 = new Vector2(AttackRange + 1, 2);
                return Physics2D.OverlapBoxAll(transform.position + new Vector3(-1, 0, 0), attackRangeV2, 90, enemyLayer);
            // return new Vector2((AttackRange * -1) - 1, 2.5f);
            // gameObject.transform.position = transformValue - new Vector3(1, 0, 0) + new Vector3 (0,0.5f,0);
            case FacingDirection.Right:
                attackRangeV2 = new Vector2(AttackRange + 1, 2);
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
    public void Attack()
    {
        // attackRangeV2 = getAttackRangeV2();

        // Collider2D[] hitColliders = Physics2D.OverlapBoxAll(transform.position, attackRangeV2, 0, enemyLayer);
        Collider2D[] hitColliders = getHitColliders();
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Ignore enemy trigger colliders, which are just used for agro range
            if (hitCollider.isTrigger)
            {
                continue;
            }
            if (PlayerInputManager.Movement == new Vector2(0, 0))
            {
                attackDirection = PlayerController.getFacingDirectionVector2();
            }
            else { attackDirection = PlayerInputManager.Movement; }
            // Vector2 attackDirection = PlayerController.getFacingDirectionVector2(PlayerController.FacingDirection);
            EnemyController enemyController = hitCollider.GetComponent<EnemyController>();
            enemyController.TakeDamage(damage, playerController, attackDirection);
        }
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
