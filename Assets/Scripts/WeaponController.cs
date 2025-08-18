using UnityEngine;

[RequireComponent(typeof(LayerMask))]
public class WeaponController : MonoBehaviour
{
    public float AttackRange;
    public LayerMask enemyLayer;
    public PlayerController playerController;
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
                gameObject.transform.position = transformValue - new Vector3(1, 0, 0) + new Vector3 (0,0.5f,0);
                break;
            case FacingDirection.Right:
                gameObject.transform.position = transformValue + new Vector3(1, 0.5f, 0);
                break;
            default:
                break;
        }
    }

    public void Attack()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, AttackRange, enemyLayer);
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Ignore enemy trigger colliders, which are just used for agro range
            if (hitCollider.isTrigger)
            {
                continue;
            }
            EnemyController enemyController = hitCollider.GetComponent<EnemyController>();
            enemyController.TakeDamage(damage, playerController);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
