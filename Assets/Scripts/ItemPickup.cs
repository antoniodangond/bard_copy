using System;
using System.Collections;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public LayerMask playerLayer;
    public string itemName;
    private BoxCollider2D boxCollider2D;
    private SpriteRenderer spriteRenderer;
    private bool collected;
    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collected = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected == false && Utils.HasTargetLayer(playerLayer, collision.gameObject))
        {
            collected = true;
            PlayerProgress.Instance.CollectItem(itemName);
            Destroy(gameObject);
        }
    }
}
