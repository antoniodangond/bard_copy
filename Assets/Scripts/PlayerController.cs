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

    public float Health;
    private PlayerAnimation playerAnimation;
    private PlayerAttack playerAttack;
    private PlayerAudio playerAudio;
    private PlayerMovement playerMovement;
    private Vector2 movement;
    private bool isPlayingLyre;
    private Gravestone gravestone;
    private string[] Melodies = new string[2]{
        MelodyData.Melody1, MelodyData.Melody2
    };
    private Queue<string> lastPlayedNotes = new Queue<string>(new string[MelodyData.MelodyLength]{
        null,
        null,
        null,
        null,
        null,
    });

    void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        playerAttack = GetComponent<PlayerAttack>();
        playerAudio = GetComponent<PlayerAudio>();
        playerMovement = GetComponent<PlayerMovement>();
        // Subscribe to custom events
        CustomEvents.OnDialogueEnd.AddListener(OnDialogueEnd);
        CustomEvents.OnAttackFinished.AddListener(OnAttackFinished);
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

    private void ToggleInstrument()
    {
        if (CurrentState == PlayerState.Default)
        {
            CurrentState = PlayerState.Instrument;
            isPlayingLyre = true;
            // reset movement when switching to Instrument state
            movement = Vector2.zero;
            // stop walking animation
            playerAnimation.SetAnimationParams(movement, isPlayingLyre);
        }
        else if (CurrentState == PlayerState.Instrument)
        {
            CurrentState = PlayerState.Default;
            isPlayingLyre = false;

            // Clear the note queue when lyre is put away (to prevent accidental triggers)
            lastPlayedNotes.Clear();
        }
        // Set animation params after determining movement and isPlayingLyre
        playerAnimation.SetAnimationParams(movement, isPlayingLyre);
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
        lastPlayedNotes.Clear();

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
                    CurrentState = PlayerState.Dialogue;
                    playerAnimation.SetAnimationParams(movement, false);
                    Debug.Log("About to exit");
                    yield break;
                }

            }
        }
        if (shouldPlayNormalMelody)
        {
            playerAudio.PlayMelody(melody);
        }
        Debug.Log("Did not find sign");

        // After starting the melody audio, wait before giving control back to the player
        yield return new WaitForSeconds(AudioData.MelodyCooldownTime);
        isPlayingLyre = false;
        CurrentState = PlayerState.Default;
        playerAnimation.SetAnimationParams(movement, false);
    }

    // Debug proximity check
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 5.0f); // Match the interaction radius
    }

    public IEnumerator TakeDamage()
    {
        Health -= 1;

        if (CurrentState == PlayerState.Default)
        {
            // TODO: implement "stunned" state
            CurrentState = PlayerState.Stunned;
            StartCoroutine(playerAttack.DamageColorChangeRoutine());
            playerAudio.PlayHit();
            yield return StartCoroutine(playerAttack.AttackCooldown());
            Debug.Log("Ouch!");
            CurrentState = PlayerState.Default;
            Debug.Log($"Player health: {Health}"); 
        }
        // TODO: Implement Player Death
        // else if (Health <= 0)
        // {
        //     // PlayerController.CurrentState = PlayerState.Dead;
        //     Debug.Log("Dead!");
        // }
    }

    public void OnAttackFinished()
    {
        isPlayingLyre = false;
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

    private Dialogue checkForDialogueCollision()
    {
        float interactionYOffset = FacingDirection == FacingDirection.Down ? InteractionYOffsetFacingDown : InteractionYOffset;
        Vector3 interactPos = transform.position + new Vector3(0, interactionYOffset, 0); // Player's center position + y offset

        // Define the size of the rectangle (adjust dynamically based on FacingDirection)
        Vector2 interactionZoneSize;

        switch (FacingDirection)
        {
            case FacingDirection.Up:
            case FacingDirection.Down:
                interactionZoneSize = new Vector2(0.5f, 1f); // Narrow width, taller height
                break;
            case FacingDirection.Left:
            case FacingDirection.Right:
                interactionZoneSize = new Vector2(1f, 0.5f); // Wider width, shorter height
                break;
            default:
                interactionZoneSize = new Vector2(0.5f, 0.5f); // Default size (failsafe)
                break;
        }

        // Offset the interaction rectangle based on FacingDirection
        switch (FacingDirection)
        {
            case FacingDirection.Up:
                interactPos += Vector3.up * interactionOffset;
                break;
            case FacingDirection.Down:
                interactPos += Vector3.down * interactionOffset;
                break;
            case FacingDirection.Left:
                interactPos += Vector3.left * interactionOffset;
                break;
            case FacingDirection.Right:
                interactPos += Vector3.right * interactionOffset;
                break;
        }

        // DEBUG: Make interaction zone visible in Scene view via red rectangle
        DebugDrawRectangle(interactPos, interactionZoneSize, Color.red);

        //Check for interactable objects in the interaction zone
        Collider2D hit = Physics2D.OverlapBox(interactPos, interactionZoneSize, 0, DialogueLayer);
        if (hit != null)
        {
            SignController signController = hit.GetComponent<SignController>();
            if (signController != null)
            {
                Debug.Log($"Found SignController on object: {hit.gameObject.name}");
                return signController.GetDialogue(); // Return the assigned Dialogue ScriptableObject
            }
        }

        Debug.Log("No interactable object found in the interaction zone.");
        return null;
    }



    //DEBUG: Make interaction zone visible in Scene view via red rectangle
    private void DebugDrawRectangle(Vector3 center, Vector2 size, Color color, float duration = 0.1f)
    {
        // Calculate the corners of the rectangle
        Vector3 topLeft = center + new Vector3(-size.x / 2, size.y / 2, 0);
        Vector3 topRight = center + new Vector3(size.x / 2, size.y / 2, 0);
        Vector3 bottomLeft = center + new Vector3(-size.x / 2, -size.y / 2, 0);
        Vector3 bottomRight = center + new Vector3(size.x / 2, -size.y / 2, 0);

        // Draw the edges of the rectangle with a longer duration
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
        // Handle pause state
        if (PlayerInputManager.isPaused)
        {
            return;
        }
        // Check if instrument was toggled
        if (PlayerInputManager.WasToggleInstrumentPressed)
        {
            ToggleInstrument();
        }

        // Perform actions depending on player state
        if (CurrentState == PlayerState.Default)
        {
            if (PlayerInputManager.WasDialoguePressed)
            {
                Dialogue dialogue = checkForDialogueCollision();
                if (dialogue != null)
                {
                    CurrentState = PlayerState.Dialogue;
                    CustomEvents.OnDialogueStart?.Invoke(dialogue);

                    // Pass facing direction to the DialogueManager
                    DialogueManager.StartDialogue(dialogue, FacingDirection);
                }
            }

            movement = PlayerInputManager.Movement;
            // If moving, set FacingDirection
            if (movement != Vector2.zero)
            {
                FacingDirection = determineFacingDirection(movement);
            }

            if (PlayerInputManager.WasAttackPressed && PlayerAttack.CanAttack)
            {
                isPlayingLyre = true;
                playerAttack.Attack();
                playerAudio.PlayAttackChord();
            }

            // Set animation params after determining isPlayingLyre
            playerAnimation.SetAnimationParams(movement, isPlayingLyre);
            // playerAudio.PlayWalkingAudio(movement);
        }
        else if (CurrentState == PlayerState.Instrument)
        {
            string notePressed = PlayerInputManager.NotePressed;
            if (notePressed is not null)
            {
                playerAudio.PlayNote(notePressed);
                // Remove 1st note from queue, and add new note to end of queue,
                // so that the new 1st note is now the 4th-last note played
                lastPlayedNotes.Dequeue();
                lastPlayedNotes.Enqueue(notePressed);
                // Check if should play Melody
                string melodyToPlay = FindMelodyToPlay(lastPlayedNotes);
                if (melodyToPlay != null)
                {
                    // Start coroutine to change state, play song, and then return to default state
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

    void FixedUpdate()
    {
        // Move player rigidbody during FixedUpdate so that movement
        // is independent of framerate
        playerMovement.Move(movement);
        // If colliding with a pushable object while moving in a straight upward direction,
        // attempt to move it
        if (gravestone != null && movement.x == 0 && movement.y == 1)
        {
            gravestone.Move();
        }
    }
}
