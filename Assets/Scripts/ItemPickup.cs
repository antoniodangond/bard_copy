using System;
using System.Collections;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public LayerMask playerLayer;
    public string itemName;
    private BoxCollider2D boxCollider2D;
    private bool collected;
    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        collected = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected == false && Utils.HasTargetLayer(playerLayer, collision.gameObject))
        {
            collected = true;
            PlayerProgress.Instance.CollectTablet(itemName);
            Destroy(gameObject);
        }
    }
}
