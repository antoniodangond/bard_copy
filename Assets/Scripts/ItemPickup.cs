using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public LayerMask playerLayer;
    [SerializeField] private bool hideOnPickup = true;

    [Header("Refs")]
    [SerializeField] private UniqueId uid;
    [SerializeField] public String pieceName;

    private Collider2D triggerCol;
    private bool collected;

    void Reset()
    {
        triggerCol = GetComponent<Collider2D>();
        if (triggerCol) triggerCol.isTrigger = true;
        uid = GetComponent<UniqueId>();
    }

    void Awake()
    {
        if (uid == null)
            uid = GetComponent<UniqueId>();
        triggerCol = GetComponent<Collider2D>();
        if (triggerCol) triggerCol.isTrigger = true;
        if (!uid) uid = GetComponent<UniqueId>();

        collected = false;

        if (PlayerProgress.Instance != null)
            PlayerProgress.Instance.OnLoaded += ApplyLoadedState;
    }

    void OnDestroy()
    {
        if (PlayerProgress.Instance != null)
            PlayerProgress.Instance.OnLoaded -= ApplyLoadedState;
    }

    void Start()
    {
        ApplyLoadedState();
    }

    private void ApplyLoadedState()
    {
        if (PlayerProgress.Instance == null || uid == null) return;

        if (PlayerProgress.Instance.HasCollected(uid.Id))
        {
            collected = true;
            if (hideOnPickup) gameObject.SetActive(false);
            else if (triggerCol) triggerCol.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!Utils.HasTargetLayer(playerLayer, other.gameObject)) return;
        if (PlayerProgress.Instance == null) return;

        if (pieceName != "GravePiece")
        {
            // Persist this pickup
            if (uid != null)
                PlayerProgress.Instance.MarkCollected(uid.Id);
                
            CerberusStatue.Instance.ActivateStatuePiece(pieceName);
            GameManager.Instance.backgroundAudio.PlayStatuePiecePickupSting();
            GameManager.Instance.collectedStatuePieces += 1;
        }

        collected = true;

        // Visual/interaction change
        if (hideOnPickup) gameObject.SetActive(false);
        else if (triggerCol) triggerCol.enabled = false;

        // Optional: play SFX/VFX/UI toast here
        // AudioManager.Play("Pickup");
        // Vfx.Spawn("Sparkle", transform.position);
    }
}
