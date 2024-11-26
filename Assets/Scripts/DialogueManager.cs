using UnityEngine;
using System.Collections.Generic;
using TMPro;

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
        DisplayLine();
    }

    public static void StartDialogue(Dialogue dialogue, FacingDirection direction)
    {
        if (Instance == null)
        {
            Debug.LogError("DialogueManager Instance is null!");
            return;
        }
        if (dialogue == null)
        {
            Debug.LogError("Attempted to start dialogue with a null Dialogue object!");
            return;
        }

        // Set up direction and lines
        Instance.currentDialogue = dialogue;
        Instance.currentDirection = direction;
        Instance.currentLines = dialogue.GetLines(direction);
        Instance.currentLineIndex = 0;

        // Trigger the OnDialogueStart event
        CustomEvents.OnDialogueStart?.Invoke(dialogue);

        // Show the first line
        Instance.DisplayLine();
    }

    private void DisplayLine()
    {
        if (currentLineIndex < currentLines.Count)
        {
            StartCoroutine(TypeLine(currentLines[currentLineIndex]));
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
        dialogueText.text = ""; // Clear the text box

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(letterSpeed); // Wait before displaying the next letter
        }

        isTyping = false;
    }

    public static void AdvanceCurrentDialogue()
    {
        if (Instance.isTyping)
        {
            // Skip to the end of the current line if typing
            Instance.StopAllCoroutines();
            Instance.dialogueText.text = Instance.currentLines[Instance.currentLineIndex];
            Instance.isTyping = false;
        }
        else if (Instance.currentLineIndex + 1 < Instance.currentLines.Count)
        {
            // Advance to the next line
            Instance.currentLineIndex++;
            Instance.DisplayLine();
        }
        else
        {
            // End dialogue if no more lines
            Instance.EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueBox.SetActive(false); // Hide the dialogue box

        //Reset dialogue variables (so as to allow repeats)
        currentDialogue = null;
        currentLines = null;
        currentLineIndex = 0;

        //Notify other systems that dialogue has ended
        CustomEvents.OnDialogueEnd?.Invoke(currentDialogue);
    }
}
