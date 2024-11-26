using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignController : MonoBehaviour
{
    [SerializeField] public Dialogue dialogue;

    public void Interact()
    {
        if (dialogue != null)
        {
            DialogueManager.StartDialogue(dialogue, PlayerController.FacingDirection);
        }
    }
}