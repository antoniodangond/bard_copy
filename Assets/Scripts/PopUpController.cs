using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PopUpController : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue defaultDialogue;
    private string startingText;
    public PlayerInput playerInput;
    private string keyboardDialogueButton;
    private string gamepadDialogueButton;

    void Start()
    {
        keyboardDialogueButton = playerInput.actions.FindActionMap("Player").FindAction("Dialogue").GetBindingDisplayString(0);
        gamepadDialogueButton = playerInput.actions.FindActionMap("Player").FindAction("Dialogue").GetBindingDisplayString(1);
        if (Gamepad.current != null)
        {
            startingText = $"Press {gamepadDialogueButton} on gamepad to interact";
        }
        else
        {
            startingText = $"Press {keyboardDialogueButton} to interact";
        }
        defaultDialogue.universalLines.Add(startingText);
        DialogueManager.StartDialogue(defaultDialogue, FacingDirection.Up);
        defaultDialogue.universalLines.Clear();
    }

    void Update()
    {
        if (PlayerInputManager.WasDialoguePressed)
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.EndDialogue();

            Destroy(gameObject);
        }
    }

}
