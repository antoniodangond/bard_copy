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
    [SerializeField] private RectTransform selectionFrame;
    [SerializeField] private float frameLerp = 20f;

    [Header("Input")]
    [SerializeField] private KeyCode confirm = KeyCode.Z;
    [SerializeField] private KeyCode cancel = KeyCode.X;

    private RectTransform[] options;
    private int index;
    private bool isOpen;
    private Action<DialogueChoice> onDone;
    private GameObject lastAnimated;  // to reverse animation on change

#if UNITY_EDITOR
void OnValidate()
{
    if (!canvasGroup) Debug.LogWarning("[ChoiceUI] CanvasGroup not assigned.", this);
    if (!bodyText) Debug.LogWarning("[ChoiceUI] BodyText not assigned.", this);
    if (!optionYes) Debug.LogWarning("[ChoiceUI] OptionYes not assigned.", this);
    if (!optionNo) Debug.LogWarning("[ChoiceUI] OptionNo not assigned.", this);
    if (!selectionFrame) Debug.LogWarning("[ChoiceUI] SelectionFrame not assigned.", this);
}
#endif


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        options = new[] { optionYes, optionNo };
        gameObject.SetActive(false);
        if (canvasGroup) canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (!isOpen) return;

        // Navigate (supports arrow/WASD)
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            SetIndex(Mathf.Min(index + 1, options.Length - 1));
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) ||
                 Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            SetIndex(Mathf.Max(index - 1, 0));
        }

        // Confirm / Cancel
        if (Input.GetKeyDown(confirm) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            Close((DialogueChoice)index);
            return;
        }
        if (Input.GetKeyDown(cancel) || Input.GetKeyDown(KeyCode.Escape))
        {
            Close(DialogueChoice.No);
            return;
        }

        // Move selection frame
        var target = options[index];
        if (selectionFrame)
        {
            selectionFrame.position = Vector3.Lerp(selectionFrame.position, target.position, Time.unscaledDeltaTime * frameLerp);
            selectionFrame.sizeDelta = Vector2.Lerp(selectionFrame.sizeDelta, target.sizeDelta, Time.unscaledDeltaTime * frameLerp);
        }
    }

    public void Ask(string prompt, Action<DialogueChoice> onComplete, int defaultIndex = 0)
    {
        if (!ValidateWiring()) return;

        onDone = onComplete;
        bodyText.text = prompt;

        if (options == null || options.Length != 2 || options[0] == null || options[1] == null)
            options = new[] { optionYes, optionNo };

        index = Mathf.Clamp(defaultIndex, 0, options.Length - 1);

        // ---- activate FIRST so StartCoroutine is legal ----
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        // (Optional) force a layout update so selectionFrame sizes correctly
        Canvas.ForceUpdateCanvases();

        // snap frame & animate selected
        if (selectionFrame)
        {
            selectionFrame.position = options[index].position;
            selectionFrame.sizeDelta = options[index].sizeDelta;
        }
        AnimateSelected(starting: true);

        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();

        StartCoroutine(OpenRoutine());
    }


    private bool ValidateWiring()
    {
        bool ok = true;

        if (canvasGroup == null) { Debug.LogError("[ChoiceUI] CanvasGroup is not assigned.", this); ok = false; }
        if (bodyText == null) { Debug.LogError("[ChoiceUI] BodyText is not assigned.", this); ok = false; }
        if (optionYes == null) { Debug.LogError("[ChoiceUI] OptionYes is not assigned.", this); ok = false; }
        if (optionNo == null) { Debug.LogError("[ChoiceUI] OptionNo is not assigned.", this); ok = false; }
        if (selectionFrame == null) { Debug.LogError("[ChoiceUI] SelectionFrame is not assigned.", this); ok = false; }

        // Optional: try to auto-wire by name if missing (helps prevent user error)
        if (!ok)
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            bodyText ??= transform.Find("BodyText")?.GetComponent<TMPro.TextMeshProUGUI>();
            optionYes ??= transform.Find("Options/OptionYes") as RectTransform;
            optionNo ??= transform.Find("Options/OptionNo") as RectTransform;
            selectionFrame ??= transform.Find("SelectionFrame") as RectTransform;

            // Re-check after auto-wire
            ok = canvasGroup && bodyText && optionYes && optionNo && selectionFrame;
            if (!ok) Debug.LogError("[ChoiceUI] Required references still missing after auto-wire.", this);
        }

        return ok;
    }


    private void SetIndex(int newIdx)
    {
        if (newIdx == index) return;
        AnimateSelected(starting: false);           // un-highlight old
        index = newIdx;
        AnimateSelected(starting: true);            // highlight new
    }

    private void AnimateSelected(bool starting)
    {
        if (MenuManager.Instance == null) return;

        if (lastAnimated != null)
        {
            MenuManager.Instance.TriggerButtonAnimation(lastAnimated, startingAnimation: false);
            var prev = lastAnimated.GetComponent<ButtonSelectionHandler>();
            if (prev) prev.SetSelected(false, playSound: false);
        }

        lastAnimated = options[index].gameObject;

        MenuManager.Instance.TriggerButtonAnimation(lastAnimated, startingAnimation: starting);
        var handler = lastAnimated.GetComponent<ButtonSelectionHandler>();
        if (handler) handler.SetSelected(true, playSound: starting);
    }


    private IEnumerator OpenRoutine()
    {
        // tiny safety delay so layout/sibling canvases settle
        yield return null; // one frame
        PlayerController.CurrentState = PlayerState.Dialogue;
        Time.timeScale = 0f;

        if (canvasGroup)
        {
            canvasGroup.blocksRaycasts = true;
            for (float t = 0; t < 0.08f; t += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.08f);
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
        // reverse highlight
        AnimateSelected(starting: false);

        if (canvasGroup)
        {
            for (float t = 0; t < 0.08f; t += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.08f);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(false);
        Time.timeScale = 1f;

        PlayerController.CurrentState = PlayerState.Default;    // hand control back

        var cb = onDone; onDone = null;
        cb?.Invoke(choice);
    }
}
