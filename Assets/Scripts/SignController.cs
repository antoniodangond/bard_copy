using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

public class SignController : MonoBehaviour
{
    [SerializeField] private UniqueId uniqueId; // For save system


    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue defaultDialogue;
    [SerializeField] private Dialogue updatedDialogue;
    [SerializeField] private bool isDialogueUpdated = false;

    [Header("Choice (optional)")]
    [SerializeField] public bool askYesNoAfterDialogue = false;
    [TextArea][SerializeField] public string choicePrompt = "Receive the blessing?";
    [SerializeField] private bool disableAfterYes = false;
    [SerializeField] private ChoiceReward choiceReward;

    [Header("Sign Properties")]
    public string signName;
    public PlayerInput playerInput;
    private string currentControlScheme;
    private Dictionary<string, string> playerControls = new Dictionary<string, string> { };
    private ControlsMapper mapper = new ControlsMapper();
    private string attackButton_1;
    private string attackButton_2;
    private string toggleInstrumentButton;
    private string lyreButtons;
    private string dashButton;
    private string AOEAttackButton1;
    private string AOEAttackButton2;
    public bool HasDialogueOnMelody = false;
    public bool IsPlayingSuccessAudio = false;

    [Header("Audio Settings")]
    [SerializeField] private float soundVolume = 0.8f;  // Adjustable in Inspector
    [SerializeField] private float soundPlayDelay = 0.15f;  // Adjustable in Inspector
    [SerializeField] private AudioSource[] audioSources;

    [Header("Animation Settings")]
    [SerializeField] private Animator successAnimator;
    [Header("GameObjects to Effect")]
    [SerializeField] public GameObject teleporterFromObj;
    [SerializeField] public GameObject teleporterToObj;
    [SerializeField] public GameObject hatchToOpenObj;
    [SerializeField] public Sprite openHatch;
    
    private Teleporter teleporterFrom;
    private Teleporter teleporterTo;
    private SpriteRenderer hatchToOpen;
    [Header("Sign Renderer")]
    public SpriteRenderer spriteRenderer;

    // Property to get correct dialogue
    public Dialogue CurrentDialogue => isDialogueUpdated ? updatedDialogue : defaultDialogue;

    private bool waitingForChoice = false; // Track if waiting for choice
    
    private void OnEnable()
    {
        Debug.Log($"[SignController:{name}] OnEnable");
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.OnLoaded += ApplySavedStateFromProgress;
        }
    }

private void OnDisable()
    {
        Debug.Log($"[SignController:{name}] OnDisable");
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.OnLoaded -= ApplySavedStateFromProgress;
        }
    }

// This handles the case where PlayerProgress has already loaded
// by the time this SignController appears in the scene.
private void Start()
{
    Debug.Log($"[SignController:{name}] Start → calling ApplySavedStateFromProgress");
    ApplySavedStateFromProgress();
}

    public void Awake()
    {
        if (uniqueId == null)
            uniqueId = GetComponent<UniqueId>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Input mapping (only if available)
        if (playerInput != null)
        {
            currentControlScheme = Gamepad.current == null ? "Keyboard" : "Gamepad";

            dashButton = mapper.getCorrectButton("Player", "Dash", currentControlScheme, false, playerInput);
            AOEAttackButton1 = currentControlScheme == "Keyboard"
                ? mapper.getCorrectButton("Player", "AOEAttack", currentControlScheme, true, playerInput).Split(",")[0]
                : mapper.getCorrectButton("Player", "AOEAttack", currentControlScheme, true, playerInput);
            AOEAttackButton2 = currentControlScheme == "Keyboard"
                ? mapper.getCorrectButton("Player", "AOEAttack", currentControlScheme, true, playerInput).Split(",")[1]
                : mapper.getCorrectButton("Player", "AOEAttack", currentControlScheme, true, playerInput);

            playerControls["dash"] = dashButton;
            playerControls["aoe_attack_1"] = AOEAttackButton1;
            playerControls["aoe_attack_2"] = AOEAttackButton2;
        }

        if (signName == "Captain")
        {
            teleporterFrom = teleporterFromObj != null ? teleporterFromObj.GetComponent<Teleporter>() : null;
            teleporterTo   = teleporterToObj   != null ? teleporterToObj.GetComponent<Teleporter>() : null;
            hatchToOpen    = hatchToOpenObj    != null ? hatchToOpenObj.GetComponent<SpriteRenderer>() : null;
        }

        // Do NOT call ApplySavedStateFromProgress() here anymore.
        // We'll let OnLoaded + Start handle that, so order is safe.

        handleTutorialDialog(signName);
    }


    public void Interact()
    {
        // If reward exists and already taken, don't show the choice box
        if (choiceReward != null && choiceReward.IsAlreadyClaimed())
        {
            // Optionally, show a simple one-liner dialogue here instead:
            //   "The statue is silent."
            Dialogue dialogue = CurrentDialogue ?? defaultDialogue;
            if (dialogue != null)
                DialogueManager.StartDialogue(dialogue, PlayerController.FacingDirection);
            return;
        }

        // Set waitingForChoice so DialogueManager knows to show the choice UI if needed
        waitingForChoice = (choiceReward != null) && !choiceReward.IsAlreadyClaimed();
        DialogueManager.SetCurrentSpeaker(this);
        DialogueManager.StartDialogue(CurrentDialogue, PlayerController.FacingDirection);
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
public bool OnSongPlayed(string melody)
{
    Debug.Log($"[SignController:{name}] OnSongPlayed melody={melody}, signName='{signName}'");
    // Already solved / updated? Then do nothing and don't start dialogue.
    if (isDialogueUpdated) return false;

    bool triggered = false;

    switch (melody)
    {
        case MelodyData.Melody1:
            if (signName == "Log" || signName == "Ghostboy")
            {
                HandleSuccessFeedback(signName);
                triggered = true;
            }
            break;

        case MelodyData.Melody2:
            if (signName == "Captain" ||
                signName == "Vines" ||
                signName == "Crow" ||
                signName == "Mountaineer" ||
                signName == "Ice")
            {
                HandleSuccessFeedback(signName);
                triggered = true;
            }
            break;

        case MelodyData.Melody3:
            if (signName == "Mountaineer" || signName == "Ice")
            {
                HandleSuccessFeedback(signName);
                triggered = true;
            }
            break;
    }

    return triggered;
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
        }

        // Mark world state in PlayerProgress, using UniqueId as key
        if (PlayerProgress.Instance != null && uniqueId != null)
        {
        // “Pure” obstacles we want gone forever
        if (signName == "Log" || signName == "Vines" || signName == "Ice")
        {
            Debug.Log($"[SignController:{name}] MarkObstacleRemoved for UniqueId={uniqueId.Id}");
            PlayerProgress.Instance.MarkObstacleRemoved(uniqueId.Id);
        }
        else
        {
            // NPCs whose dialog/state should advance
            Debug.Log($"[SignController:{name}] SetNPCStatus MelodySolved for UniqueId={uniqueId.Id}");
            PlayerProgress.Instance.SetNPCStatus(uniqueId.Id, "MelodySolved");
        }
        }
        if (signName != "Log" && signName != "Vines" && signName != "Ice")
        {
            HasDialogueOnMelody = true;
            isDialogueUpdated = true;
            // STEP 1: Autoplay updated dialogue
            DialogueManager.StartDialogue(updatedDialogue, PlayerController.FacingDirection);
        }
        if (signName == "Captain")
        {
            teleporterFrom.Activate();
            teleporterTo.Activate();
            hatchToOpenObj.transform.position += new Vector3 (0,2f);
            hatchToOpen.sprite = openHatch;
        }

        // STEP 2: Play success animation
        if (successAnimator != null)
        {
            StartCoroutine(PlaySuccessAnimation(signName));
        }
    }

    private IEnumerator PlaySuccessAnimation(string signName)
    {
        successAnimator.SetTrigger("Success");

        // Wait for animation to complete
        AnimatorStateInfo stateInfo = successAnimator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        // Disable object
        // if (signName != "Vines")
        // {
        //     spriteRenderer.enabled = false;
        // }
        foreach (Collider2D collider in gameObject.GetComponents<BoxCollider2D>())
        {
            collider.enabled = false;
        }
        foreach (Collider2D collider in gameObject.GetComponents<PolygonCollider2D>())
        {
            collider.enabled = false;
        }
    }

    private void ApplySavedStateFromProgress()
    {
        if (PlayerProgress.Instance == null || uniqueId == null)
            return;

        Debug.Log($"[SignController] ApplySavedStateFromProgress for {signName} ({uniqueId?.Id}) obstacleRemoved={PlayerProgress.Instance?.IsObstacleRemoved(uniqueId.Id)} npcStatus={PlayerProgress.Instance?.GetNPCStatus(uniqueId.Id)}");

        // 1) Obstacles that should be gone forever
        if (PlayerProgress.Instance.IsObstacleRemoved(uniqueId.Id))
        {
            // This obstacle was already solved/removed in a prior session
            // Match what PlaySuccessAnimation does at the end:
            foreach (Collider2D collider in gameObject.GetComponents<BoxCollider2D>())
                collider.enabled = false;
            foreach (Collider2D collider in gameObject.GetComponents<PolygonCollider2D>())
                collider.enabled = false;

            // For pure obstacles like log/vines/ice you can also hide sprite:
            if (signName == "Log" || signName == "Vines" || signName == "Ice")
            {
                if (spriteRenderer != null)
                    spriteRenderer.enabled = false;
            }

            // If you want them completely gone:
            // gameObject.SetActive(false);
            return;
        }

        // 2) NPCs / interactive signs with advanced dialogue or world effects
        string npcStatus = PlayerProgress.Instance.GetNPCStatus(uniqueId.Id);
        if (string.IsNullOrEmpty(npcStatus))
            return;

        if (npcStatus == "MelodySolved")
        {
            // Match the “post-melody” state:
            isDialogueUpdated = true;
            HasDialogueOnMelody = true;

            if (signName == "Captain")
            {
                if (teleporterFrom != null) teleporterFrom.Activate();
                if (teleporterTo != null) teleporterTo.Activate();
                if (hatchToOpenObj != null)
                    hatchToOpenObj.transform.position += new Vector3(0, 2f);
                if (hatchToOpen != null)
                    hatchToOpen.sprite = openHatch;
            }

            // Also disable colliders if that’s what the success animation does
            foreach (Collider2D collider in gameObject.GetComponents<BoxCollider2D>())
                collider.enabled = false;
            foreach (Collider2D collider in gameObject.GetComponents<PolygonCollider2D>())
                collider.enabled = false;
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
                attackButton_1 = mapper.getCorrectButton("Player", "Attack", currentControlScheme, true, playerInput).Split(",")[0];
                attackButton_2 = mapper.getCorrectButton("Player", "Attack", currentControlScheme, true, playerInput).Split(",")[1];
                toggleInstrumentButton = mapper.getCorrectButton("Instrument", "ToggleInstrument", currentControlScheme, false, playerInput);
                lyreButtons = mapper.setLyreButtons(false, lyreNotes, playerInput);

                defaultDialogue.upLines.Add($"[Press {attackButton_1} or {attackButton_2} to ATTACK]");
                defaultDialogue.upLines.Add($"[Press {toggleInstrumentButton} to enter PLAY mode. Use {lyreButtons} to play NOTES]");
            }
            else
            {
                attackButton_1 = mapper.getCorrectButton("Player", "Attack", currentControlScheme, true, playerInput);
                toggleInstrumentButton = mapper.getCorrectButton("Instrument", "ToggleInstrument", currentControlScheme,false, playerInput);
                lyreButtons = mapper.setLyreButtons(true, lyreNotes, playerInput);

                defaultDialogue.upLines.Add($"[Press {attackButton_1} to ATTACK]");
                defaultDialogue.upLines.Add($"[Press {toggleInstrumentButton} to enter PLAY mode. Use {lyreButtons} to play NOTES]");
            }
        }
        else if (signName == "SongDecay")
        {
            // clear lines because these will update depending on what controller (or keyboard) is connected
            defaultDialogue.universalLines.Clear();

            // initialize variable for song
            string[] songOfDecay = new string[] { "NoteC", "NoteB", "NoteC", "NoteD", "NoteE"};
            if (Gamepad.current == null)
            {
                lyreButtons = mapper.setLyreButtons(false, songOfDecay, playerInput);
            }
            else
            {
                lyreButtons = mapper.setLyreButtons(true, songOfDecay, playerInput);
            }
            defaultDialogue.universalLines.Add("SONG OF DECAY");
            defaultDialogue.universalLines.Add(lyreButtons);
        }
        else { return; }
    }

    // These are used by DialogueManager when it encounters a [CHOICE] line
    public bool WaitingForChoice => waitingForChoice;
    public string ChoicePrompt => choicePrompt;
    public void OnChoiceAnswered(DialogueChoice choice) // Callback from DialogueChoiceUI
    {
        if (choice == DialogueChoice.Yes)
        {
            StartCoroutine(BestowRoutine());

            if (disableAfterYes) // disable this sign after accepting
            {
                foreach (var c in GetComponents<Collider2D>()) c.enabled = false;
                if (spriteRenderer) spriteRenderer.enabled = false;
                enabled = false;
            }
        }
        else
        {
            // Optional: handle "No" choice
        }
    }

    private IEnumerator BestowRoutine()
    {
        if (choiceReward == null) yield break;

        var hadDash = PlayerController.canDash;
        var hadAOE  = PlayerController.AbilityGate.AOEUnlocked;

        // Build up-to-date control text
        var controlsDict = new Dictionary<string, string>();
        var (dashPrimary, _) = InputDisplayUtil.GetActionDisplay("Player", "Dash", false, playerInput: playerInput) 
            is string dash ? (dash, "") : ("?", "");

        controlsDict["dash"] = dashPrimary;

        var (aoe1, aoe2) = InputDisplayUtil.GetAOEButtons(playerInput);
        if (!string.IsNullOrEmpty(aoe1))
            controlsDict["aoe_attack_1"] = aoe1;
        if (!string.IsNullOrEmpty(aoe2))
            controlsDict["aoe_attack_2"] = aoe2;

        string scheme = InputDisplayUtil.IsGamepad(playerInput) ? "Gamepad" : "Keyboard";

        // This shows the statue's explanation, with correct keys/icons
        yield return StartCoroutine(choiceReward.BestowAndExplain(controlsDict, scheme));

        foreach (var upg in choiceReward.upgrades)
        {
            if (upg == null) continue;
            PlayerProgress.Instance?.Unlock(upg);

            if (upg.id == "dash") PlayerController.canDash = true;
            if (upg.id == "aoe")  PlayerController.AbilityGate.AOEUnlocked = true;
        }
    }



    public void BeginDialogue(FacingDirection direction)
    {
        bool alreadyClaimed = (choiceReward != null) && choiceReward.IsAlreadyClaimed();

        // Only ask if configured to ask AND not already claimed
        waitingForChoice = askYesNoAfterDialogue &&
                   (choiceReward == null || !choiceReward.IsAlreadyClaimed());

        Debug.Log($"[Sign] BeginDialogue {name} ask={askYesNoAfterDialogue} " +
                  $"hasReward={choiceReward != null} claimed={(choiceReward != null && choiceReward.IsAlreadyClaimed())} " +
                  $"waiting={waitingForChoice}");

        DialogueManager.SetCurrentSpeaker(this);
        DialogueManager.StartDialogue(CurrentDialogue, direction);
    }

    // I know that this should live somewhere else, but we're so close to shipping I'm just doing it here
    private string MapGamepadIcons(string bindingDisplayString)
    {
        switch (bindingDisplayString)
        {
            case "Cross":
                return "<sprite name=\"Playstation_Cross\">";
            case "Circle":
                return "<sprite name=\"Playstation_Circle\">";
            case "Triangle":
                return "<sprite name=\"Playstation_Triangle\">";
            case "Square":
                return "<sprite name=\"Playstation_Square\">";
            case "R1":
                return "<sprite name=\"Playstation_R1\">";
            case "A":
                return "<sprite name=\"XBox_A\">";
            case "B":
                return "<sprite name=\"XBox_B\">";
            case "X":
                return "<sprite name=\"XBox_X\">";
            case "Y":
                return "<sprite name=\"XBox_Y\">";
            case "RB":
                return "<sprite name=\"XBox_RB\">";
            // case "Square":
            //     return "<sprite name="Playstation_Square">";
            default:
                return "";
        }
    }
}
