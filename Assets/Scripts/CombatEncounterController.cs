using System.Collections.Generic;
using UnityEngine;

public class CombatEncounterController : MonoBehaviour
{
    public LayerMask enemyLayer;

    private Dictionary<int, GameObject> enemies = new Dictionary<int, GameObject>();

    void Awake()
    {
        foreach (Transform childTransform in transform)
        {
            GameObject child = childTransform.gameObject;
            if (Utils.HasTargetLayer(enemyLayer, child))
            {
                EnemyController enemyController = child.GetComponent<EnemyController>();
                enemies.Add(child.GetInstanceID(), child);
            }
        }

        // Subscribe to custom event
        CustomEvents.OnEnemyDeath.AddListener(OnEnemyDeath);
    }

    void OnDestroy()
    {
        // Remove listener on destroy to prevent memory leaks
        CustomEvents.OnEnemyDeath.RemoveListener(OnEnemyDeath);
    }

    void OnEnemyDeath(GameObject enemy)
    {
        enemies.Remove(enemy.GetInstanceID());
        // Remove enemy GameObject instance from the scene
        Destroy(enemy);

        if (enemies.Count == 0)
        {
            CustomEvents.OnCombatEncounterCleared?.Invoke(gameObject);
        }
    }
}
