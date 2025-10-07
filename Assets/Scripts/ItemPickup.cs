using System;
using System.Collections;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private BoxCollider2D boxCollider2D;
    private SpriteRenderer spriteRenderer;
    public string itemName;
    // this should be a reference to some more global tracker
    private bool collected;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        collected = true;
        Debug.Log($"It's {collected}, you picked up the item!");
        spriteRenderer.enabled = false;
        // Destroy(gameObject);
    }
}
