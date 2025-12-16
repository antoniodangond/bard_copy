using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
[DefaultExecutionOrder(-1000)]

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private GameObject dialogueBox; // Assign via Inspector
    [SerializeField] private TextMeshProUGUI dialogueText; // Text component for dialogue
    [SerializeField] private float letterSpeed = 0.05f; // Speed of letter appearance

    private Dialogue currentDialogue;
    private FacingDirection currentDirection;
    private List<string> currentLines;
    private int currentLineIndex;

    // New, for accessibility on larger font sizes
    private int currentPageIndex = 1;  // TMP pages are 1-based
    private int currentPageCount = 1;
    private string currentProcessedLine = null;
    private int _currentPageStartChar = 0;
    private int _currentPageLastChar = 0;




    private bool isTyping = false;
    public static SignController CurrentSpeaker { get; private set; }
    public static void SetCurrentSpeaker(SignController speaker) => CurrentSpeaker = speaker;


    public static bool IsOpen => Instance != null && Instance.currentLines != null && Instance.currentLines.Count > 0;
    

    void Awake()
    {
        // Ensure only one instance exists (singleton pattern)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            return; // Exit to prevent further execution of Awake
        }

        //Deactivate the dialogue box at the start
        dialogueBox.SetActive(false);

        //Make sure we are in proper overflow mode
        dialogueText.overflowMode = TextOverflowModes.Page;
        dialogueText.pageToDisplay = 1;


        // Subscribe to custom events
        CustomEvents.OnDialogueStart.AddListener(OnDialogueStart);
    }

    void OnDestroy()
    {
        // Remove listener on destroy to prevent memory leaks
        CustomEvents.OnDialogueStart.RemoveListener(OnDialogueStart);
    }

    private void OnDialogueStart(Dialogue dialogue)
    {
        currentDialogue = dialogue;
        currentDirection = PlayerController.FacingDirection;
        currentLines = dialogue.GetLines(currentDirection);
        currentLineIndex = 0;

        dialogueBox.SetActive(true); // Show the dialogue box
        var canvasGroup = dialogueBox.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
        DisplayLine();
    }

    public static void StartDialogue(Dialogue dialogue, FacingDirection direction)
    {
        if (Instance == null)
            Instance = Object.FindFirstObjectByType<DialogueManager>();


        if (Instance == null)
        {
            Debug.LogError("DialogueManager Instance is null! Ensure a DialogueManager is in the scene and enabled.");
            return;
        }
        if (dialogue == null)
        {
            Debug.LogWarning("Attempted to start dialogue with a null Dialogue object!");
            return;
        }

        Instance.currentDialogue = dialogue;
        Instance.currentDirection = direction;
        Instance.currentLines = dialogue.GetLines(direction);
        Instance.currentLineIndex = 0;

        // Activate the dialogue box and set alpha to 1
        Instance.dialogueBox.SetActive(true);
        var canvasGroup = Instance.dialogueBox.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        CustomEvents.OnDialogueStart?.Invoke(dialogue);
    }



    private void DisplayLine() // Displays the current line with typewriter effect
    {
        if (currentLines != null && currentLineIndex < currentLines.Count)
        {
            string line = currentLines[currentLineIndex];

            // Check for choice trigger token
            if (line == "[CHOICE]" && CurrentSpeaker != null && CurrentSpeaker.askYesNoAfterDialogue)
            {
                // Show the choice UI instead of displaying a line
                Debug.Log($"DialogueChoiceUI.Instance: {DialogueChoiceUI.Instance}");
                Debug.Log($"CurrentSpeaker: {CurrentSpeaker}");
                Debug.Log($"CurrentSpeaker.askYesNoAfterDialogue: {CurrentSpeaker?.askYesNoAfterDialogue}");
                Debug.Log($"CurrentSpeaker.choicePrompt: {CurrentSpeaker?.choicePrompt}");
                DialogueChoiceUI.Instance.Ask(
                    CurrentSpeaker.choicePrompt,
                    CurrentSpeaker.OnChoiceAnswered,
                    0
                );
                // Optionally close the dialogue box if you want only the choice visible
                // dialogueBox.SetActive(false);
                dialogueText.text = string.Empty;
                return;
            }
            string processedLine = ReplaceControlPlaceholders(line);
            currentProcessedLine = processedLine;

            dialogueText.text = processedLine;
            dialogueText.ForceMeshUpdate();

            currentPageCount = Mathf.Max(1, dialogueText.textInfo.pageCount);
            currentPageIndex = 1;
            dialogueText.pageToDisplay = 1;

            StartCoroutine(TypeCurrentPage());

        }
        else
        {
            EndDialogue();
        }
    }


    private IEnumerator TypeCurrentPage()
    {
        isTyping = true;

        // Ensure full line text is present for TMP to compute page boundaries
        dialogueText.text = currentProcessedLine ?? "";
        dialogueText.maxVisibleCharacters = 0;

        dialogueText.pageToDisplay = currentPageIndex;
        dialogueText.ForceMeshUpdate();


        var info = dialogueText.textInfo;
        int page = currentPageIndex - 1;

        if (page >= info.pageInfo.Length)
        {
            isTyping = false;
            yield break;
        }

        int start = info.pageInfo[page].firstCharacterIndex;
        int count = info.pageInfo[page].lastCharacterIndex - start + 1;

        _currentPageStartChar = start;
        _currentPageLastChar = info.pageInfo[page].lastCharacterIndex;


        for (int i = 0; i < count; i++)
        {
            dialogueText.maxVisibleCharacters = start + i + 1;
            yield return new WaitForSeconds(letterSpeed);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }



    private System.Collections.IEnumerator TypeLine(string line)
    {
        if (isTyping) yield break;
        isTyping = true;

        dialogueText.text = ""; // clear before typing

        // Split line into tokens (chars + full rich-text tags)
        var tokens = TokenizeRichText(line);

        foreach (var token in tokens)
        {
            dialogueText.text += token;
            yield return new WaitForSeconds(letterSpeed);
        }

        isTyping = false;

        // Now that the full line is present, TMP can compute pages
        dialogueText.pageToDisplay = 1;
        dialogueText.ForceMeshUpdate();

        currentPageCount = Mathf.Max(1, dialogueText.textInfo.pageCount);
        currentPageIndex = 1;

    }

    /// <summary>
    /// Splits a string into tokens where TMP rich-text tags
    /// (e.g. <sprite name="Playstation_Options">, <b>, </b>)
    /// are treated as single units.
    /// </summary>
    private static List<string> TokenizeRichText(string line)
    {
        var tokens = new List<string>();
        int i = 0;

        while (i < line.Length)
        {
            if (line[i] == '<') // Check to see if there is tag
            {
                int close = line.IndexOf('>', i);
                if (close == -1)
                {
                    // No closing '>' = treat rest as plain text
                    tokens.Add(line.Substring(i));
                    break;
                }

                // If there is a tag, add entire tag as a single token
                tokens.Add(line.Substring(i, close - i + 1));
                i = close + 1;
            }
            else
            {
                // Normal character
                tokens.Add(line[i].ToString());
                i++;
            }
        }

        return tokens;
    }


    public static void AdvanceCurrentDialogue()
    {
        if (Instance == null) return;

        if (Instance.currentLines == null || Instance.currentLines.Count == 0)
            return;

        // 1) If typing, finish the current line immediately (and compute pages)
        if (Instance.isTyping)
        {
            // Stop only the page-typing coroutine
            Instance.StopAllCoroutines();
            Instance.isTyping = false;

            // Ensure text is set to the full current line (so TMP indices are valid)
            Instance.dialogueText.text = Instance.currentProcessedLine ??
                                        ReplaceControlPlaceholders(Instance.currentLines[Instance.currentLineIndex]);

            Instance.dialogueText.pageToDisplay = Instance.currentPageIndex;
            Instance.dialogueText.ForceMeshUpdate();

            // Reveal the rest of the CURRENT PAGE (not the next page / not the next line)
            // +1 because maxVisibleCharacters is a count, and lastCharacterIndex is 0-based index.
            Instance.dialogueText.maxVisibleCharacters = Instance._currentPageLastChar + 1;

            return;
        }


        // 2) If there are more TMP pages for this same line, advance page first
        if (Instance.currentPageIndex < Instance.currentPageCount)
        {
            Instance.currentPageIndex++;
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.TypeCurrentPage());
            return;
        }


        // 3) Otherwise advance to next dialogue line
        if (Instance.currentLineIndex + 1 < Instance.currentLines.Count)
        {
            Instance.currentLineIndex++;
            Instance.DisplayLine();
        }
        else
        {
            Instance.EndDialogue();
        }
    }



    public void EndDialogue()
    {
        dialogueBox.SetActive(false);

        var finishedDialogue = currentDialogue;

        // Reset internal dialogue state
        currentDialogue = null;
        currentLines = null;
        currentLineIndex = 0;

        // IMPORTANT: Invoke end event while CurrentSpeaker is still set
        CustomEvents.OnDialogueEnd?.Invoke(finishedDialogue);

        // Now it's safe to clear
        CurrentSpeaker = null;
    }

    public static string ReplaceControlPlaceholders(string text)
    {
        // Replace tokens in dialogue text
        string dashKey = InputDisplayUtil.GetBindingForAction("Player", "Dash");
        string interactKey = InputDisplayUtil.GetBindingForAction("Player", "Interact"); // if you use it
        string openMenuKey  = InputDisplayUtil.GetBindingForAction("Player", "OpenMenu");

        return text
            .Replace("{DASH_KEY}", dashKey)
            .Replace("{INTERACT_KEY}", interactKey)
            .Replace("{OPEN_MENU_KEY}", openMenuKey);
    }


}
