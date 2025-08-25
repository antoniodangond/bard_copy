using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PopUpController : MonoBehaviour
{

     [Header("Dialogue Settings")]
    [SerializeField] private Dialogue defaultDialogue;

    void Start()
    {
        DialogueManager.StartDialogue(defaultDialogue, FacingDirection.Up);
    }

    void Update()
    {
        if (PlayerInputManager.WasDialoguePressed)
        {
            DialogueManager.Instance.EndDialogue();
            Destroy(gameObject);
        }
    }
}
