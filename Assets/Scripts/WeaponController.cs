using UnityEngine;

[RequireComponent(typeof(LayerMask))]
public class WeaponController : MonoBehaviour
{
    public float AttackRange;
    public LayerMask enemyLayer;
    public PlayerController playerController;

    // TODO: make customizable in editor
    private float damage = 1f;

    public void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
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
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
