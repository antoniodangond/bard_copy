using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum DialogueChoice { Yes = 0, No = 1 }

public class DialogueChoiceUI : MonoBehaviour
{
    public static DialogueChoiceUI Instance { get; private set; }

    [Header("Wiring")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private RectTransform optionYes;
    [SerializeField] private RectTransform optionNo;

    [Header("Timings")]
    [SerializeField] private float fadeTime = 0.08f;
    [SerializeField] private float navRepeatDelay = 0.15f; // optional anti-spam for arrows/WASD

    private RectTransform[] options;
    private int index;
    private bool isOpen;
    private Action<DialogueChoice> onDone;

    private GameObject lastSelected;  // the last option we told to "select"
    private float nextNavTime;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!canvasGroup) Debug.LogWarning("[ChoiceUI] CanvasGroup not assigned.", this);
        if (!bodyText)    Debug.LogWarning("[ChoiceUI] BodyText not assigned.", this);
        if (!optionYes)   Debug.LogWarning("[ChoiceUI] OptionYes not assigned.", this);
        if (!optionNo)    Debug.LogWarning("[ChoiceUI] OptionNo not assigned.", this);
    }
#endif

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        options = new[] { optionYes, optionNo };

        // start hidden
        gameObject.SetActive(false);
        if (canvasGroup) canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (!isOpen) return;

        // --- Navigation (arrows / WASD) ---
        if (Time.unscaledTime >= nextNavTime)
        {
            bool moved = false;

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                SetIndex(Mathf.Min(index + 1, options.Length - 1));
                moved = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) ||
                     Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                SetIndex(Mathf.Max(index - 1, 0));
                moved = true;
            }

            if (moved) nextNavTime = Time.unscaledTime + navRepeatDelay;
        }

        // --- Confirm: ONLY your Interact button ---
        if (PlayerInputManager.WasDialoguePressed)
        {
            Close((DialogueChoice)index);
            return;
        }

        // No back/cancel key: to say "No", navigate to No and press Interact.
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

    private void SetIndex(int newIdx)
    {
        if (newIdx == index && lastSelected != null) return;

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
    }

    private IEnumerator OpenRoutine()
    {
        // Let DialogueBox disable + layout settle for one frame
        yield return null;

        // Freeze the player and pause the world while the choice is open
        PlayerController.CurrentState = PlayerState.Dialogue;
        Time.timeScale = 0f;

        if (canvasGroup)
        {
            canvasGroup.blocksRaycasts = true;
            for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        isOpen = true;
    }

    private void Close(DialogueChoice choice)
    {
        if (!isOpen) return;
        isOpen = false;
        StartCoroutine(CloseRoutine(choice));
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
