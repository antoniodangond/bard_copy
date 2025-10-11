using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState {
    Default,
    Dialogue,
    Instrument,
    InstrumentMelody,
    Stunned,
    Dead
}

public enum FacingDirection {
    Up,
    Down,
    Left,
    Right,
}

[RequireComponent(typeof(PlayerAnimation))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerAudio))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAudioData))]
public class PlayerController : MonoBehaviour
{
    // Trigger custom event when CurrentState public variable is modified
    private static PlayerState _currentState = PlayerState.Default;
    public static PlayerState CurrentState
    {
        get { return _currentState; }
        set {
            _currentState = value;
            CustomEvents.OnPlayerStateChange?.Invoke(value);
        }
    }
    public static FacingDirection FacingDirection = FacingDirection.Down;
    // Set in editor
    public PlayerAudioData AudioData;
    public LayerMask DialogueLayer;
    public LayerMask PushableLayer;

    public float MaxHealth;
    // Make Health public so it's easily editable in the Inspector
    public float Health;
    // On death, teleport player to last checkpoint touched
    public Checkpoint LastCheckpoint;

    private PlayerAnimation playerAnimation;
    private PlayerAttack playerAttack;
    private PlayerAudio playerAudio;
    private PlayerMovement playerMovement;
    private Vector2 movement;
    private bool isPlayingLyre;
    private bool isAttacking;
    public static bool isDashing;
    public static bool canDash;

    [SerializeField] private UpgradeSO dashUpgrade;

    public static class AbilityGate // Might move canDash in here... This is where we will store ability unlocks
    {
        public static bool AOEUnlocked;

    }


    private Gravestone gravestone;
    [HideInInspector]
    public bool isTakingDamage;
    private string[] Melodies = new string[2]{
        MelodyData.Melody1, MelodyData.Melody2
    };
    // Track the notes played while in Instrument mode
    private Queue<string> lastPlayedNotes;

    void Awake()
    {
        isTakingDamage = false;
        // Debug.Log(isTakingDamage); // For some reason this kept getting set to true at start of game
        playerAnimation = GetComponent<PlayerAnimation>();
        playerAttack = GetComponent<PlayerAttack>();
        playerAudio = GetComponent<PlayerAudio>();
        playerMovement = GetComponent<PlayerMovement>();
        // Subscribe to custom events
        CustomEvents.OnDialogueEnd.AddListener(OnDialogueEnd);
        CustomEvents.OnAttackFinished.AddListener(OnAttackFinished);
        // Set health to max
        Health = MaxHealth;
        // Initialize lastPlayedNotes to a queue of null values
        lastPlayedNotes = BuildEmptyNotesQueue();

        // --- Hard reset statics so domain-reload quirks can�t leak previous values ---
        canDash = false;
        AbilityGate.AOEUnlocked = false;

        // Re-apply gates from progress (if any)
        if (PlayerProgress.Instance != null && dashUpgrade != null)
        {
            canDash = PlayerProgress.Instance.HasUpgrade(dashUpgrade);
        }

        // Optional: quick debug
        Debug.Log($"[PlayerController] Start gates => canDash={canDash}, AOE={AbilityGate.AOEUnlocked}");

    }

    void OnDestroy()
    {
        // Remove listeners on destroy to prevent memory leaks
        CustomEvents.OnDialogueEnd.RemoveListener(OnDialogueEnd);
        CustomEvents.OnAttackFinished.RemoveListener(OnAttackFinished);
    }

    private void OnDialogueEnd(Dialogue dialogue)
    {
        CurrentState = PlayerState.Default;
        Debug.Log("dialogue completed");
    }

    private Queue<string> BuildEmptyNotesQueue() {
        return new Queue<string>(new string[MelodyData.MelodyLength]{
            null,
            null,
            null,
            null,
            null,
        });
    }

    private void ToggleInstrument()
    {
        if (CurrentState == PlayerState.Default)
        {
            CurrentState = PlayerState.Instrument;
            isPlayingLyre = true;
            isAttacking = false;
            // reset movement when switching to Instrument state
            movement = Vector2.zero;
            // stop walking animation
            playerAnimation.SetAnimationParams(movement, isPlayingLyre, isAttacking);
        }
        else if (CurrentState == PlayerState.Instrument)
        {
            CurrentState = PlayerState.Default;
            isPlayingLyre = false;
            isAttacking = false;

            // Clear the note queue when lyre is put away (to prevent accidental triggers)
            lastPlayedNotes = BuildEmptyNotesQueue();
        }
        // Set animation params after determining movement and isPlayingLyre
        playerAnimation.SetAnimationParams(movement, isPlayingLyre, isAttacking);
    }

    private string FindMelodyToPlay(Queue<string> lastPlayedNotes)
    {
        string lastPlayedNotesString = string.Join("", lastPlayedNotes);
        foreach (string melody in Melodies)
        {
            string[] melodyInputs = MelodyData.MelodyInputs[melody];
            string melodyInputsString = string.Join("", melodyInputs);
            if (lastPlayedNotesString == melodyInputsString)
            {
                Debug.Log("melody played");
                return melody;
            }
        }
        return null;
    }

    private IEnumerator PlayMelodyAfterDelay(string melody)
    {
        CurrentState = PlayerState.InstrumentMelody;
        // After playing last note, wait before starting the melody audio
        yield return new WaitForSeconds(AudioData.TimeBeforeMelody);

        // Clears note queue when melody is completed
        // The below appears to have been causing a bug where only the first melody played would
        //     trigger the melody
        // I think this functionality is taken care of by the fact that the queue gets cleared
        //     when the player puts the lyre away, which happens automatically after the song
        //     is played?
        // Would love to hear others' thoughts
        // lastPlayedNotes.Clear();

        // Proximity check for objects affectable by melody
        float interactionRadius = 5.0f; // Adjust this as needed
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        bool shouldPlayNormalMelody = true;
        foreach (Collider2D obj in nearbyObjects)
        {
            SignController sign = obj.GetComponent<SignController>();
            if (sign != null)
            {
                Debug.Log("Playing melody for sign");
                sign.OnSongPlayed(melody);
                // Trigger the sign's response to the song
                if (sign.HasDialogueOnMelody)
                {
                    shouldPlayNormalMelody = !sign.IsPlayingSuccessAudio;
                    isPlayingLyre = false;
                    isAttacking = false;
                    CurrentState = PlayerState.Dialogue;
                    playerAnimation.SetAnimationParams(movement, isPlayingLyre, isAttacking);
                    Debug.Log("About to exit");
                    yield break;
                }

            }
        }
        if (shouldPlayNormalMelody)
        {
            playerAudio.PlayMelody(melody);
        }
        // Debug.Log("Did not find sign");

        // After starting the melody audio, wait before giving control back to the player
        yield return new WaitForSeconds(AudioData.MelodyCooldownTime);
        isPlayingLyre = false;
        isAttacking = false;
        CurrentState = PlayerState.Default;
        playerAnimation.SetAnimationParams(movement, isPlayingLyre, isAttacking);
    }

    // Debug proximity check
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 5.0f); // Match the interaction radius
    }

    public IEnumerator TakeDamageRoutine()
    {
        isTakingDamage = true;

        if (CurrentState == PlayerState.Default)
        {
            Health -= 1;
            PlayerUIManager.Instance.UpdateHealthUI((int)Health);
            if (Health > 0)
            {
                StartCoroutine(HandleStun());
            }
            else
            {
                HandleDeath();
            }
        }
        yield return new WaitForSeconds(0.2f);
        isTakingDamage = false;
    }

    private IEnumerator HandleStun()
    {
        CurrentState = PlayerState.Stunned;
        StartCoroutine(playerAttack.DamageColorChangeRoutine());
        playerAudio.PlayHit();
        yield return StartCoroutine(playerAttack.AttackCooldown());
        CurrentState = PlayerState.Default;
    }

    private void HandleDeath()
    {
        CurrentState = PlayerState.Dead;
        playerAudio.PlayPlayerSound("PlayerDeath", 0.75f);
        if (LastCheckpoint != null)
        {
            // Teleport player to last teleporter touched
            LastCheckpoint.TeleportPlayer(gameObject);
        }
        // Reset health and state
        Health = MaxHealth;
        PlayerUIManager.Instance.UpdateHealthUI((int)Health);
        CurrentState = PlayerState.Default;
    }

    public void OnAttackFinished()
    {
        isPlayingLyre = false;
        isAttacking = false;
    }

    private FacingDirection determineFacingDirection(Vector2 movement)
    {
        // If moving diagonal, use horizontal direction
        bool isMovingHorizontal = Math.Abs(movement.x) > 0;
        if (isMovingHorizontal)
        {
            return movement.x > 0 ? FacingDirection.Right : FacingDirection.Left;
        }
        else
        {
            return movement.y > 0 ? FacingDirection.Up : FacingDirection.Down;
        }
    }

    //Get player's direction relative to object position
    private FacingDirection GetRelativeDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? FacingDirection.Right : FacingDirection.Left;
        else
            return delta.y > 0 ? FacingDirection.Up : FacingDirection.Down;
    }

    //Check to see if player is facing an object w/ dialogue
    private (Dialogue, SignController) checkForDialogueCollision()
    {
        float yOffset = FacingDirection == FacingDirection.Down ? InteractionYOffsetFacingDown : InteractionYOffset;
        Vector3 interactPos = transform.position + new Vector3(0, yOffset, 0);

        Vector2 adjustedZoneSize = interactionZoneSize;
        if (FacingDirection == FacingDirection.Left || FacingDirection == FacingDirection.Right)
            adjustedZoneSize = new Vector2(interactionZoneSize.y, interactionZoneSize.x);

        switch (FacingDirection)
        {
            case FacingDirection.Up: interactPos += Vector3.up * interactionOffset; break;
            case FacingDirection.Down: interactPos += Vector3.down * interactionOffset; break;
            case FacingDirection.Left: interactPos += Vector3.left * interactionOffset; break;
            case FacingDirection.Right: interactPos += Vector3.right * interactionOffset; break;
        }

        DebugDrawRectangle(interactPos, adjustedZoneSize, Color.red);

        Collider2D[] hits = Physics2D.OverlapBoxAll(interactPos, adjustedZoneSize, 0f, DialogueLayer);

        foreach (Collider2D hit in hits)
        {
            SignController sign = hit.GetComponentInParent<SignController>();
            if (sign != null)
            {
                // Filter out signs that are not in front of the player
                Vector2 toSign = sign.transform.position - transform.position;
                FacingDirection trueApproach = GetRelativeDirection(toSign);

                if (trueApproach != FacingDirection) continue; // not facing the object!

                Dialogue dialogue = sign.GetDialogueFromApproach(transform);
                if (dialogue != null)
                {
                    Debug.Log($"Found SignController on: {sign.gameObject.name}");
                    return (dialogue, sign);
                }
            }
        }

        return (null, null);
    }





    // Helper method to draw a wireframe rectangle using Debug.DrawLine
    private void DebugDrawRectangle(Vector3 center, Vector2 size, Color color, float duration = 0.1f)
    {
        Vector3 topLeft = center + new Vector3(-size.x / 2, size.y / 2, 0);
        Vector3 topRight = center + new Vector3(size.x / 2, size.y / 2, 0);
        Vector3 bottomLeft = center + new Vector3(-size.x / 2, -size.y / 2, 0);
        Vector3 bottomRight = center + new Vector3(size.x / 2, -size.y / 2, 0);

        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
    }



    //Interaction zone adjustable in Inspector
    [SerializeField] private Vector2 interactionZoneSize = new Vector2(1f, 0.5f); // Width and Height
    [SerializeField] private float interactionOffset = 0.5f; // Distance from the player's center
    [SerializeField] public float InteractionYOffset; // Add vertical offset from the player's center while facing up
    [SerializeField] public float InteractionYOffsetFacingDown; // Add vertical offset from the player's center while facing down

    void Update()
    {
        if (PlayerInputManager.isPaused)
            return;

        if (PlayerInputManager.WasToggleInstrumentPressed)
        {
            ToggleInstrument();
        }

        if (CurrentState == PlayerState.Default)
        {
            if (PlayerInputManager.WasDialoguePressed)
            {
                var (dialogue, sign) = checkForDialogueCollision();
                if (dialogue != null && sign != null)
                {
                    CurrentState = PlayerState.Dialogue;

                    FacingDirection approachedFrom = sign.GetApproachDirection(transform);
                    sign.BeginDialogue(approachedFrom);

                }
            }

            movement = PlayerInputManager.Movement;
            if (movement != Vector2.zero)
            {
                FacingDirection = determineFacingDirection(movement);
            }

            if (PlayerInputManager.WasAttackPressed && PlayerAttack.CanAttack)
            {
                isPlayingLyre = false;
                isAttacking = true;
                playerAttack.Attack();
                playerAudio.PlayAttackChord();
            }

            if (PlayerInputManager.wasDashPressed && canDash)
            {
                isDashing = true;
            }

            playerAnimation.SetAnimationParams(movement, isPlayingLyre, isAttacking);
        }
        else if (CurrentState == PlayerState.Instrument)
        {
            string notePressed = PlayerInputManager.NotePressed;
            if (notePressed is not null)
            {
                playerAudio.PlayNote(notePressed);
                lastPlayedNotes.Dequeue();
                lastPlayedNotes.Enqueue(notePressed);
                string melodyToPlay = FindMelodyToPlay(lastPlayedNotes);
                if (melodyToPlay != null)
                {
                    StartCoroutine(PlayMelodyAfterDelay(melodyToPlay));
                }
            }
        }
        else if (CurrentState == PlayerState.Dialogue)
        {
            if (PlayerInputManager.WasDialoguePressed)
            {
                DialogueManager.AdvanceCurrentDialogue();
            }
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.HasTargetLayer(PushableLayer, other.gameObject))
        {
            gravestone = other.gameObject.GetComponent<Gravestone>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (gravestone != null && Utils.HasTargetLayer(PushableLayer, other.gameObject))
        {
            gravestone.Stop();
            gravestone = null;
        }
    }

    public static Vector2 getFacingDirectionVector2()
    {
        switch (FacingDirection)
            {
                case FacingDirection.Up:
                    return new Vector2(0, 1);
                case FacingDirection.Right:
                    return new Vector2(1, 0);
                case FacingDirection.Down:
                    return new Vector2(0, -1);
                case FacingDirection.Left:
                    return new Vector2(-1, 0);
                default:
                    return new Vector2(0,0);
            }
    }

    void FixedUpdate()
    {
        // Move player rigidbody during FixedUpdate so that movement
        // is independent of framerate
        if (isDashing)
        {
            playerMovement.Dash(movement);
        }
        else {playerMovement.Move(movement);}
        
        // If colliding with a pushable object while moving in a straight upward direction,
        // attempt to move it
        if (gravestone != null && movement.x == 0 && movement.y == 1)
        {
            gravestone.Move();
        }
    }
}
