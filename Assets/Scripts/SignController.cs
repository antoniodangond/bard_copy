using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SignController : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue defaultDialogue;
    [SerializeField] private Dialogue updatedDialogue;
    [SerializeField] private bool isDialogueUpdated = false;

    [Header("Sign Properties")]
    public string signName;
    public bool HasDialogueOnMelody = false;
    public bool IsPlayingSuccessAudio = false;

    [Header("Audio Settings")]
    [SerializeField] private float soundVolume = 0.8f;  // Adjustable in Inspector
    [SerializeField] private float soundPlayDelay = 0.15f;  // Adjustable in Inspector
    [SerializeField] private AudioSource[] audioSources;

    [Header("Animation Settings")]
    [SerializeField] private Animator successAnimator;

    // Property to get correct dialogue
    public Dialogue CurrentDialogue => isDialogueUpdated ? updatedDialogue : defaultDialogue;

    public void Interact()
    {
        Dialogue dialogue = CurrentDialogue ?? defaultDialogue;
        if (dialogue != null)
        {
            DialogueManager.StartDialogue(CurrentDialogue, PlayerController.FacingDirection);
        }
    }

    public Dialogue GetDialogue()
    {
        return CurrentDialogue ?? defaultDialogue;
    }

    // Method called when a song is played nearby
    public void OnSongPlayed(string melody)
    {
        if (isDialogueUpdated) return; // Prevent multiple activations

        switch (melody)
        {
            case MelodyData.Melody1:
                if (signName == "Log") { HandleSuccessFeedback(); }
                if (signName == "Ghostboy") { HandleSuccessFeedback(); }
                break;

            case MelodyData.Melody2:
                if (signName == "Pirate") { HandleSuccessFeedback(); }
                break;
        }
    }

    private void HandleSuccessFeedback()
    {
        isDialogueUpdated = true;
        successAnimator = gameObject.GetComponent<Animator>();
        audioSources = GetComponents<AudioSource>();

        // Play all available sounds using inspector-defined volume & delay
        foreach (var audiosource in audioSources)
        {
            audiosource.volume = soundVolume;  // Uses adjustable volume
            audiosource.PlayDelayed(soundPlayDelay);  // Uses adjustable delay
            Debug.Log($"Playing sound: {audiosource.clip?.name} with volume {soundVolume} after {soundPlayDelay}s");
        }

        // STEP 1: Autoplay updated dialogue
        DialogueManager.StartDialogue(updatedDialogue, PlayerController.FacingDirection);

        // STEP 2: Play success animation
        if (successAnimator != null)
        {
            StartCoroutine(PlaySuccessAnimation());
        }
    }

    private IEnumerator PlaySuccessAnimation()
    {
        successAnimator.SetTrigger("Success");

        // Wait for animation to complete
        AnimatorStateInfo stateInfo = successAnimator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        // Disable object
        Debug.Log($"{gameObject.name} has completed its success sequence and will be disabled.");
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        foreach (Collider2D collider in gameObject.GetComponents<BoxCollider2D>())
        {
            collider.enabled = false;
        }
    }
}
