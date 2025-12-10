using System;
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
    public Sprite initialSprite; // use this to reset sprites as needed for new games (when save states are involved)
    public Sprite updatedSprite; // use this to save states of obstacles for loaded game states
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
    private List<string> statueHintList = new List<string>(9);
    private int currentHintIndex;

    [Header("Audio Settings")]
    [SerializeField] private float soundVolume = 0.8f;  // Adjustable in Inspector
    [SerializeField] private float soundPlayDelay = 0.15f;  // Adjustable in Inspector
    [SerializeField] private AudioSource[] audioSources;

    [Header("Animation Settings")]
    [SerializeField] private Animator successAnimator;
    [Header("GameObjects to Effect")]
    [SerializeField] public GameObject teleporterFromObj;
    [SerializeField] public GameObject teleporterToObj;
    [SerializeField] public GameObject ClosedHatch;
    [SerializeField] public GameObject OpenedHatch;
    public GameObject statuePieceToGive;
    public float statuePieceFadeInTime;
    
    private Teleporter teleporterFrom;
    private Teleporter teleporterTo;
    // private SpriteRenderer hatchToOpen;
    [Header("Sign Renderer")]
    public SpriteRenderer spriteRenderer;

    // Property to get correct dialogue
    public Dialogue CurrentDialogue => isDialogueUpdated ? updatedDialogue : defaultDialogue;

    private bool waitingForChoice = false; // Track if waiting for choice
    private EurydiceGrave eurydiceGrave;
    
    private void OnEnable()
    {
        // Debug.Log($"[SignController:{name}] OnEnable");
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.OnLoaded += ApplySavedStateFromProgress;
        }
    }

private void OnDisable()
    {
        // Debug.Log($"[SignController:{name}] OnDisable");
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.OnLoaded -= ApplySavedStateFromProgress;
        }
    }

// This handles the case where PlayerProgress has already loaded
// by the time this SignController appears in the scene.
private void Start()
{
    // Debug.Log($"[SignController:{name}] Start → calling ApplySavedStateFromProgress");
    ApplySavedStateFromProgress();
    if (signName == "E_Grave") { eurydiceGrave = gameObject.GetComponent<EurydiceGrave>();}
}

    public void Awake()
    {
        if (uniqueId == null)
            uniqueId = GetComponent<UniqueId>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (statuePieceToGive) { statuePieceToGive.SetActive(false); }

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
            // hatchToOpen    = OpenedHatch    != null ? hatchToOpenObj.GetComponent<SpriteRenderer>() : null;
        }

        // Do NOT call ApplySavedStateFromProgress() here anymore.
        // We'll let OnLoaded + Start handle that, so order is safe.

        handleTutorialDialog(signName);
        currentHintIndex = 0;
        InitializeHintDialogue(signName);
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
        if (signName == "E_Grave" && eurydiceGrave) 
        {
            eurydiceGrave.OnSongPlayed(melody);
            return true;
        }
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
                    signName == "Crow")
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

    public void MarkDialogueUpdated()
    {
        isDialogueUpdated = true;

        // Optional: change sprite when upgraded. Maybe use this for Cerberus Statue?
        if (updatedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = updatedSprite;
        }
    }

    private void HandleSuccessFeedback(string signName)
    {
        successAnimator = gameObject.GetComponent<Animator>();
        audioSources = GetComponents<AudioSource>();

        // Play all available sounds using inspector-defined volume & delay
        foreach (var audiosource in audioSources)
        {
            if (signName != "Charon")
            {
                audiosource.volume = soundVolume;  // Uses adjustable volume
                audiosource.PlayDelayed(soundPlayDelay);  // Uses adjustable delay
            }
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
            if (signName != "Charon") GameManager.Instance.NPCQuestsSolved += 1;
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
            ClosedHatch.SetActive(false);
            OpenedHatch.SetActive(true);
        }

        if (statuePieceToGive) {FadeInStatuePiece();}

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
        if (signName == "Ice") { Destroy(gameObject); }
    }

    private void ApplySavedStateFromProgress()
    {
        if (PlayerProgress.Instance == null || uniqueId == null)
            return;

        if (choiceReward != null && PlayerProgress.Instance.IsRewardClaimed(choiceReward.rewardId))
            {
                MarkDialogueUpdated();
            }


        // Debug.Log($"[SignController] ApplySavedStateFromProgress for {signName} ({uniqueId?.Id}) obstacleRemoved={PlayerProgress.Instance?.IsObstacleRemoved(uniqueId.Id)} npcStatus={PlayerProgress.Instance?.GetNPCStatus(uniqueId.Id)}");

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
                    if (signName == "Vines")
                    {
                        Destroy(gameObject.GetComponent<Animator>());
                        // replace sprites with original sprites rather than disable
                        SpriteRenderer attachedSR = GetComponent<SpriteRenderer>();
                        attachedSR.sprite = updatedSprite;
                    } 
                    else { spriteRenderer.enabled = false; }
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
            // isDialogueUpdated = true;
            // HasDialogueOnMelody = true;
            // Destroy(GetComponent<Animator>());
            // spriteRenderer.enabled = false;

            if (signName == "Captain")
            {
                if (teleporterFrom != null) teleporterFrom.Activate();
                if (teleporterTo != null) teleporterTo.Activate();
                if (ClosedHatch != null)
                    ClosedHatch.SetActive(false);
                if (OpenedHatch != null)
                    OpenedHatch.SetActive(true);
            }

            // Also disable colliders if that’s what the success animation does
            // foreach (Collider2D collider in gameObject.GetComponents<BoxCollider2D>())
            //     collider.enabled = false;
            // foreach (Collider2D collider in gameObject.GetComponents<PolygonCollider2D>())
            //     collider.enabled = false;
            
            Destroy(gameObject);
        }

        // 3) For Hint System
        if (signName == "HintSystem")
        {
            InitializeHintDialogue(signName);
            UpdateHintDialogue();
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
        else if (signName == "SongGrowth" || signName == "SongDecay" || signName == "SongWarmth")
        {
            // clear lines because these will update depending on what controller (or keyboard) is connected
            defaultDialogue.universalLines.Clear();

            string songTitle;
            string melodyKey;

            // Decide melody based on signName
            if (signName == "SongGrowth")
            {
                songTitle = "SONG OF GROWTH";
                melodyKey = MelodyData.Melody2;
            }
            else if (signName == "SongDecay")
            {
                songTitle = "SONG OF DECAY";
                melodyKey = MelodyData.Melody1;
            }
            else // "SongWarmth" = mountaintop statue that grants Melody3
            {
                songTitle = "SONG OF WARMTH";
                melodyKey = MelodyData.Melody3;
            }

            // Look up the correct note sequence from MelodyData
            string[] notes = MelodyData.MelodyInputs[melodyKey];

            // Use ControlsMapper to convert notes -> button icons / letters
            bool gamepadActive = Gamepad.current != null;
            lyreButtons = mapper.setLyreButtons(gamepadActive, notes, playerInput);

            // Fill in the dialogue lines
            defaultDialogue.universalLines.Add(songTitle);
            defaultDialogue.universalLines.Add(lyreButtons);
        }
        else
        {
            return;
        }
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
        MarkDialogueUpdated();
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

        if (signName == "HintSystem") {UpdateHintDialogue();}
        DialogueManager.SetCurrentSpeaker(this);
        DialogueManager.StartDialogue(CurrentDialogue, direction);
        if (signName == "Charon") {HandleSuccessFeedback(signName);}
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
            default:
                return "";
        }
    }

    private void FadeInStatuePiece()
    {
        // get ref to sprite renderer
        SpriteRenderer statuePieceSpriteRenderer = statuePieceToGive.GetComponent<SpriteRenderer>();
        // get transparent and opaque alpha colors to use in fade in coroutine
        Color transparent = statuePieceSpriteRenderer.color;
        transparent.a = 0;
        Color opaque = statuePieceSpriteRenderer.color;
        opaque.a = 1;
        // set alpha of sprite renderer to transparent and then activate it, so it starts invisible
        statuePieceToGive.GetComponent<SpriteRenderer>().color = transparent;
        statuePieceToGive.SetActive(true);
        // fade in the sprite
        StartCoroutine(StatuePieceFadeInRoutine(statuePieceSpriteRenderer, statuePieceFadeInTime, transparent, opaque));
    }

    private IEnumerator StatuePieceFadeInRoutine(SpriteRenderer sr,float fadeInTime, Color transparent, Color opaque)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeInTime)
        {
            float t = elapsedTime/fadeInTime;

            sr.color = Color.Lerp(transparent, opaque, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void InitializeHintDialogue(string signName)
    {
        if (signName == null || signName != "HintSystem") {return;}

        if (defaultDialogue.universalLines.Count > 0) {defaultDialogue.universalLines.Clear();}

        for (int i = 0; i < defaultDialogue.leftLines.Count; i++)
        {
            statueHintList.Add(defaultDialogue.leftLines[i]);
        }

    }

    private void UpdateHintDialogue()
    {

        defaultDialogue.universalLines.Clear();
        // Loop through each statue piece in the list of contained in statueHintList
        for (int i = 0; i < statueHintList.Count; i++) 
        {
            // If that piece isn't in the corresponding list maintained by the cerberus statue, it has been found!
            // We should remove it from the list
            if (!CerberusStatue.Instance.statuePieceHintsDict.Values.Contains<string>(statueHintList[i]))
            {
                Debug.Log(name);
                statueHintList.Remove(statueHintList[i]);
            }
        }

        // (Hopefully) update currentHitIndex to account for size changes to the list
        currentHintIndex = GetCorrectHintIndex(currentHintIndex);
        defaultDialogue.universalLines.Add(statueHintList[currentHintIndex]);

    }

    private int GetCorrectHintIndex(int index)
    {
        if (index >= statueHintList.Count - 1)
        {
            return 0;
        }
        else
        {
            return index + 1;
        }
    }
}