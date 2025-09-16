using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

public class SignController : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue defaultDialogue;
    [SerializeField] private Dialogue updatedDialogue;
    [SerializeField] private bool isDialogueUpdated = false;

    [Header("Sign Properties")]
    public string signName;
    public PlayerInput playerInput;
    private string attackButton_1;
    private string attackButton_2;
    private string toggleInstrumentButton;
    private string lyreButtons;
    public bool HasDialogueOnMelody = false;
    public bool IsPlayingSuccessAudio = false;
    public SpriteRenderer spriteRenderer;

    [Header("Audio Settings")]
    [SerializeField] private float soundVolume = 0.8f;  // Adjustable in Inspector
    [SerializeField] private float soundPlayDelay = 0.15f;  // Adjustable in Inspector
    [SerializeField] private AudioSource[] audioSources;

    [Header("Animation Settings")]
    [SerializeField] private Animator successAnimator;

    // Property to get correct dialogue
    public Dialogue CurrentDialogue => isDialogueUpdated ? updatedDialogue : defaultDialogue;

    public void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        handleTutorialDialog(signName);

    }
    public void Interact()
    {
        Dialogue dialogue = CurrentDialogue ?? defaultDialogue;
        if (dialogue != null)
        {
            DialogueManager.StartDialogue(CurrentDialogue, PlayerController.FacingDirection);
        }
    }

    public Dialogue GetDialogueFromApproach(Transform playerTransform)
    {
        Vector2 delta = playerTransform.position - transform.position;

        FacingDirection approachedFrom;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            approachedFrom = delta.x > 0 ? FacingDirection.Left : FacingDirection.Right;
        else
            approachedFrom = delta.y > 0 ? FacingDirection.Down : FacingDirection.Up;

        return (isDialogueUpdated ? updatedDialogue : defaultDialogue)?.GetLines(approachedFrom) != null
            ? (isDialogueUpdated ? updatedDialogue : defaultDialogue)
            : null;
    }

    public FacingDirection GetApproachDirection(Transform player)
    {
        Vector2 delta = player.position - transform.position;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? FacingDirection.Left : FacingDirection.Right;
        else
            return delta.y > 0 ? FacingDirection.Down : FacingDirection.Up;
    }


    // Method called when a song is played nearby
    public void OnSongPlayed(string melody)
    {
        if (isDialogueUpdated) return; // Prevent multiple activations

        switch (melody)
        {
            case MelodyData.Melody1:
                if (signName == "Log") { HandleSuccessFeedback(signName); }
                if (signName == "Ghostboy") { HandleSuccessFeedback(signName); }
                break;

            case MelodyData.Melody2:
                if (signName == "Captain") { HandleSuccessFeedback(signName); }
                if (signName == "Vines") { HandleSuccessFeedback(signName); }
                break;
        }
    }

    private void HandleSuccessFeedback(string signName)
    {
        successAnimator = gameObject.GetComponent<Animator>();
        audioSources = GetComponents<AudioSource>();

        // Play all available sounds using inspector-defined volume & delay
        foreach (var audiosource in audioSources)
        {
            audiosource.volume = soundVolume;  // Uses adjustable volume
            audiosource.PlayDelayed(soundPlayDelay);  // Uses adjustable delay
            Debug.Log($"Playing sound: {audiosource.clip?.name} with volume {soundVolume} after {soundPlayDelay}s");
        }

        if (signName != "Log" && signName != "Vines")
        {
            HasDialogueOnMelody = true;
            isDialogueUpdated = true;
            // STEP 1: Autoplay updated dialogue
            DialogueManager.StartDialogue(updatedDialogue, PlayerController.FacingDirection);
        }

        // STEP 2: Play success animation
        if (successAnimator != null)
        {
            StartCoroutine(PlaySuccessAnimation(signName));
        }
        else { Debug.Log("null"); }
    }

    private IEnumerator PlaySuccessAnimation(string signName)
    {
        successAnimator.SetTrigger("Success");

        // Wait for animation to complete
        AnimatorStateInfo stateInfo = successAnimator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        // Disable object
        Debug.Log($"{gameObject.name} has completed its success sequence and will be disabled.");
        // if (signName != "Vines")
        // {
        //     spriteRenderer.enabled = false;
        // }
        foreach (Collider2D collider in gameObject.GetComponents<BoxCollider2D>())
        {
            collider.enabled = false;
            Debug.Log("Collider Disabled");
        }
        foreach (Collider2D collider in gameObject.GetComponents<PolygonCollider2D>())
        {
            collider.enabled = false;
            Debug.Log("Collider Disabled");
        }
    }
    private void handleTutorialDialog(string signName)
    {
        if (signName == "E_Grave")
        {
            // clear dialogue lines set last time
            // because these will update depensing on what controller is connected
            defaultDialogue.upLines.Clear();

            string[] lyreNotes = new string[] { "NoteB", "NoteC", "NoteD", "NoteE" };

            defaultDialogue.upLines.Add("Orpheus. I have found a way. Beyond life and death.");
            defaultDialogue.upLines.Add("A place where we will never part...");
            defaultDialogue.upLines.Add("Use your LYRE to quell lost souls.");
            if (Gamepad.current == null)
            {
                attackButton_1 = playerInput.actions.FindActionMap("Player").FindAction("Attack").GetBindingDisplayString(0);
                attackButton_2 = playerInput.actions.FindActionMap("Player").FindAction("Attack").GetBindingDisplayString(1);
                toggleInstrumentButton = getCorrectButton("Instrument", "ToggleInstrument", "Keyboard");
                setLyreButtons(false, lyreNotes);

                defaultDialogue.upLines.Add($"[Press {attackButton_1} or {attackButton_2} to ATTACK]");
                defaultDialogue.upLines.Add($"[Press {toggleInstrumentButton} to enter PLAY mode. Use {lyreButtons} to play NOTES]");
            }
            else
            {
                attackButton_1 = playerInput.actions.FindActionMap("Player").FindAction("Attack").GetBindingDisplayString(2);
                toggleInstrumentButton = getCorrectButton("Instrument", "ToggleInstrument", "Gamepad");
                setLyreButtons(true, lyreNotes);

                defaultDialogue.upLines.Add($"[Press {attackButton_1} to ATTACK]");
                defaultDialogue.upLines.Add($"[Press {toggleInstrumentButton} to enter PLAY mode. Use {lyreButtons} to play NOTES]");
            }
        }
        else if (signName == "SongDecay")
        {
            // clear lines because these will update depending on what controller (or keyboard) is connected
            defaultDialogue.universalLines.Clear();

            // initialize varibale for song
            string[] songOfDecay = new string[] { "NoteC", "NoteB", "NoteC", "NoteD", "NoteE"};
            if (Gamepad.current == null)
            {
                setLyreButtons(false, songOfDecay);
            }
            else
            {
                setLyreButtons(true, songOfDecay);
            }
            defaultDialogue.universalLines.Add("SONG OF DECAY");
            defaultDialogue.universalLines.Add(lyreButtons);
        }
        else { return; }
    }

    private void setLyreButtons(bool gamepadActive, string[] notes)
    {
        for (int i = 0; i < notes.Length; i++)
        {
            if (!gamepadActive)
            {
                if (i == 0)
                {
                    lyreButtons = getCorrectButton("Instrument", notes[i], "Keyboard");
                }
                else
                {
                    lyreButtons = lyreButtons + ", " + getCorrectButton("Instrument", notes[i], "Keyboard");
                }
            }
            else
            {
                if (i == 0)
                {
                    lyreButtons = getCorrectButton("Instrument", notes[i], "Gamepad");

                }
                else
                {
                    lyreButtons = lyreButtons + ", " + getCorrectButton("Instrument", notes[i], "Gamepad");
                }
            }
        }
    }

    // Doesn't work for attack action 
    private string getCorrectButton(string actionMap, string action, string controlScheme)
    {
        if (controlScheme == "Keyboard")
        {
            return playerInput.actions.FindActionMap(actionMap).FindAction(action).GetBindingDisplayString(0);
        }
        else
        {
            return playerInput.actions.FindActionMap(actionMap).FindAction(action).GetBindingDisplayString(1);
        }
    }
}