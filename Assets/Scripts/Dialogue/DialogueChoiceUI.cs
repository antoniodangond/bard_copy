using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum DialogueChoice { Yes = 0, No = 1 }

public class DialogueChoiceUI : MonoBehaviour
{
    public static DialogueChoiceUI Instance { get; private set; }

    [Header("Wiring")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private RectTransform optionYes;   // assign: OptionYes
    [SerializeField] private RectTransform optionNo;    // assign: OptionNo
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Timings")]
    [SerializeField] private float fadeTime = 0.08f;
    [SerializeField] private float navRepeatDelay = 0.15f; // optional anti-spam for arrows/WASD

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;


    private RectTransform[] options;
    private int index;
    private bool isOpen;
    private Gamepad gamepad;
    private Action<DialogueChoice> onDone;

    private GameObject lastSelected;  // the last option we told to "select"
    private float nextNavTime;
    private TextAccessibilityScript acc;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        acc = FindFirstObjectByType<TextAccessibilityScript>();
        StartCoroutine(RegisterChoiceTextWhenReady());


        options = new[] { optionYes, optionNo };

        // start hidden
        gameObject.SetActive(false);
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = false; // we'll enable on open
        }

        // Explicitly wire the known buttons
        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(CloseYes);
            yesButton.onClick.AddListener(CloseYes);
            Debug.Log("[ChoiceUI] Wired yesButton.onClick → CloseYes");
        }
        else
        {
            Debug.LogError("[ChoiceUI] yesButton is NOT assigned in the inspector.", this);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(CloseNo);
            noButton.onClick.AddListener(CloseNo);
            Debug.Log("[ChoiceUI] Wired noButton.onClick → CloseNo");
        }
        else
        {
            Debug.LogError("[ChoiceUI] noButton is NOT assigned in the inspector.", this);
        }
    }

    private IEnumerator RegisterChoiceTextWhenReady()
    {
        // Ensure at least one frame passes so other singletons can initialize
        yield return null;

        // Wait until PlayerProgress exists (and has likely loaded or is about to)
        while (PlayerProgress.Instance == null)
            yield return null;

        // Apply the saved size to the choice prompt as soon as the choice box exists
        if (acc != null && bodyText != null)
            acc.RegisterChoiceText(bodyText);
        else if (bodyText != null)
            bodyText.fontSize = PlayerProgress.Instance.GetDialogueFontSize();
    }


    void Update()
    {
        if (!isOpen) return;

        // --- Navigation (arrows / WASD) ---
        if (Time.unscaledTime >= nextNavTime)
        {
            bool moved = false;

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) ||
                (gamepad != null && (
                gamepad.dpad.right.wasPressedThisFrame || gamepad.dpad.down.wasPressedThisFrame ||
                gamepad.leftStick.right.wasPressedThisFrame || gamepad.leftStick.down.wasPressedThisFrame))
                )
            {
                SetIndex(Mathf.Min(index + 1, options.Length - 1));
                moved = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) ||
                    Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) ||
                    (gamepad != null && (
                    gamepad.dpad.left.wasPressedThisFrame || gamepad.dpad.up.wasPressedThisFrame ||
                    gamepad.leftStick.left.wasPressedThisFrame || gamepad.leftStick.up.wasPressedThisFrame))
                    )
            {
                SetIndex(Mathf.Max(index - 1, 0));
                moved = true;
            }

            if (moved) nextNavTime = Time.unscaledTime + navRepeatDelay;
        }

        // Confirm via Dialogue (Z / Enter / LMB / etc.)
        if (PlayerInputManager.WasDialoguePressed)
        {
            // Before we close, align index with the currently selected UI object
            SyncIndexWithCurrentSelection();

            if (debugLog)
            {
                var sel = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
                Debug.Log($"[ChoiceUI] Confirm via Dialogue. index={index} selected={sel?.name}");
            }

            Close((DialogueChoice)index);
            return;
        }
    }


    public void Ask(string prompt, Action<DialogueChoice> onComplete, int defaultIndex = 0)
    {
        if (!ValidateWiring()) return;

        onDone = onComplete;
        bodyText.text = prompt;

        // (Re)build the options array if something got re-wired
        if (options == null || options.Length != 2 || options[0] == null || options[1] == null)
            options = new[] { optionYes, optionNo };

        index = Mathf.Clamp(defaultIndex, 0, options.Length - 1);

        // Activate FIRST so coroutines are legal and layout can run
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (acc == null) acc = FindFirstObjectByType<TextAccessibilityScript>();
        if (acc != null && bodyText != null)
            acc.RegisterChoiceText(bodyText);
        else if (bodyText != null && PlayerProgress.Instance != null)
            bodyText.fontSize = PlayerProgress.Instance.GetDialogueFontSize();


        // Make sure the layout has valid sizes/positions before we select
        Canvas.ForceUpdateCanvases();

        var yesHandler = optionYes.GetComponent<ButtonSelectionHandler>();
        var noHandler = optionNo.GetComponent<ButtonSelectionHandler>();
        if (MenuManager.Instance != null)
        {
            if (yesHandler && yesHandler.animTarget)
                MenuManager.Instance.RegisterAnimTarget(yesHandler.animTarget.gameObject);
            if (noHandler && noHandler.animTarget)
                MenuManager.Instance.RegisterAnimTarget(noHandler.animTarget.gameObject);
        }

        // Apply initial selection (drives ButtonSelectionHandler just like pause menu)
        SetIndex(index);

        StartCoroutine(OpenRoutine());
    }

    private bool ValidateWiring()
    {
        bool ok = true;
        if (canvasGroup == null) { Debug.LogError("[ChoiceUI] CanvasGroup is not assigned.", this); ok = false; }
        if (bodyText == null) { Debug.LogError("[ChoiceUI] BodyText is not assigned.", this); ok = false; }
        if (optionYes == null) { Debug.LogError("[ChoiceUI] OptionYes is not assigned.", this); ok = false; }
        if (optionNo == null) { Debug.LogError("[ChoiceUI] OptionNo is not assigned.", this); ok = false; }
        return ok;
    }

    public void SetIndex(int newIdx)
    {
        if (newIdx == index && lastSelected != null) return;
        if (debugLog)
            Debug.Log($"[ChoiceUI] SetIndex from {index} to {newIdx}");
            
        // Deselect previous
        if (lastSelected != null)
        {
            var prev = lastSelected.GetComponent<ButtonSelectionHandler>();
            prev?.SetSelected(false, playSound: false);
        }

        index = newIdx;

        // Select current
        lastSelected = options[index].gameObject;
        var cur = lastSelected.GetComponent<ButtonSelectionHandler>();
        cur?.SetSelected(true, playSound: true);

        // Make sure UI/Submit (Enter) acts on the correct object (later we can
        // point this at the actual Button if you want Enter to trigger click).
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
    }

    // These help mouse work better for selecting dialogue choice options.
    public void HoverYes() => SetIndex(0);
    public void HoverNo()  => SetIndex(1);

    private void SyncIndexWithCurrentSelection()
    {
        if (EventSystem.current == null) return;

        var sel = EventSystem.current.currentSelectedGameObject;
        if (sel == null) return;

        // Is the selected object OptionYes or a child of it?
        if (sel == optionYes.gameObject || sel.transform.IsChildOf(optionYes))
        {
            if (debugLog) Debug.Log($"[ChoiceUI] SyncIndex -> YES (0) from selected={sel.name}");
            index = 0;
        }
        // Is the selected object OptionNo or a child of it?
        else if (sel == optionNo.gameObject || sel.transform.IsChildOf(optionNo))
        {
            if (debugLog) Debug.Log($"[ChoiceUI] SyncIndex -> NO (1) from selected={sel.name}");
            index = 1;
        }
        else if (debugLog)
        {
            Debug.Log($"[ChoiceUI] SyncIndex: selected={sel.name} not under OptionYes/OptionNo, keeping index={index}");
        }
    }


    private IEnumerator OpenRoutine()
    {
        // Freeze the player and pause the world while the choice is open
        PlayerController.CurrentState = PlayerState.Dialogue;
        Time.timeScale = 0f;

        if (canvasGroup)
        {
            canvasGroup.blocksRaycasts = true;

            if (fadeTime <= 0.001f)
            {
                canvasGroup.alpha = 1f;
            }
            else
            {
                for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
                {
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
                    yield return null;
                }
                canvasGroup.alpha = 1f;
            }
        }
        if (acc != null && bodyText != null)
            acc.RegisterChoiceText(bodyText);
        isOpen = true;
    }

    // Called by buttons and by Dialogue input
    public void CloseYes()
    {
        Debug.Log("[ChoiceUI] CloseYes clicked");
        Close(DialogueChoice.Yes);
    }

    public void CloseNo()
    {
        Debug.Log("[ChoiceUI] CloseNo clicked");
        Close(DialogueChoice.No);
    }

    private void Close(DialogueChoice choice)
    {
        if (!isOpen)
        {
            Debug.LogWarning($"[ChoiceUI] Close called while isOpen == false. Proceeding anyway. choice={choice}");
        }

        isOpen = false;
        StartCoroutine(CloseRoutine(choice));
    }

    void Start()
    {
        if (Gamepad.current != null) { gamepad = Gamepad.current; }
    }

    private IEnumerator CloseRoutine(DialogueChoice choice)
    {
        // Deselect visual (optional)
        if (lastSelected != null)
        {
            var prev = lastSelected.GetComponent<ButtonSelectionHandler>();
            prev?.SetSelected(false, playSound: false);
        }
        lastSelected = null;

        if (canvasGroup)
        {
            for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
        Time.timeScale = 1f;
        PlayerController.CurrentState = PlayerState.Default;

        var cb = onDone; onDone = null;
        cb?.Invoke(choice);
    }
}
