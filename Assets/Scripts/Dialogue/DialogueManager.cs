using UnityEngine;
using System.Collections.Generic;
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
        Instance.DisplayLine();
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
                dialogueBox.SetActive(false);
                return;
            }
            string processedLine = ReplaceControlPlaceholders(line);
            StartCoroutine(TypeLine(processedLine));
        }
        else
        {
            EndDialogue();
        }
    }


    private System.Collections.IEnumerator TypeLine(string line)
    {
        //If already typing, don't start a new coroutine.
        if (isTyping) yield break;

        isTyping = true;
        dialogueText.text = line; // Set the full line at once

        // Wait for the letterSpeed interval before starting the typing effect
        yield return new WaitForSeconds(letterSpeed);

        // dialogueText.text = ""; // Clear the text box

        // foreach (char letter in line.ToCharArray())
        // {
        //     dialogueText.text += letter;
        //     yield return new WaitForSeconds(letterSpeed); // Wait before displaying the next letter
        // }

        isTyping = false;
    }

    public static void AdvanceCurrentDialogue()
    {
        if (Instance == null) return;

        // If no dialogue is active, ignore the call safely.
        if (Instance.currentLines == null || Instance.currentLines.Count == 0)
        {
            return;
        }

        if (Instance.isTyping)
        {
            Instance.StopAllCoroutines();
            Instance.dialogueText.text = Instance.currentLines[Instance.currentLineIndex];
            Instance.isTyping = false;
            return;
        }

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

    return text
        .Replace("{DASH_KEY}", dashKey)
        .Replace("{INTERACT_KEY}", interactKey);
}


}
