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
    [SerializeField] private float soundVolume = 0.8f;
    [SerializeField] private float soundPlayDelay = 0.15f;
    [SerializeField] private AudioSource[] audioSources;

    [Header("Animation Settings")]
    [SerializeField] private Animator successAnimator;
    [SerializeField] private string successAnimationName = "Success"; // Ensure this matches the animation name

    private void Awake()
    {
        successAnimator = GetComponent<Animator>();
        audioSources = GetComponents<AudioSource>();
    }

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

    public void OnSongPlayed(string melody)
    {
        if (isDialogueUpdated) return; // Prevent multiple activations

        switch (melody)
        {
            case MelodyData.Melody1:
                if (signName == "Log" || signName == "Ghostboy") HandleSuccessFeedback();
                break;
            case MelodyData.Melody2:
                if (signName == "Pirate") HandleSuccessFeedback();
                break;
        }
    }

    private void HandleSuccessFeedback()
    {
        isDialogueUpdated = true;

        // Play sounds
        foreach (var audiosource in audioSources)
        {
            audiosource.volume = soundVolume;
            audiosource.PlayDelayed(soundPlayDelay);
        }

        // Start dialogue
        DialogueManager.StartDialogue(updatedDialogue, PlayerController.FacingDirection);

        // Play animation
        if (successAnimator != null)
        {
            StartCoroutine(PlaySuccessAnimation());
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no assigned success animation.");
            RemoveObject();
        }
    }

    private IEnumerator PlaySuccessAnimation()
    {
        successAnimator.SetTrigger("Success");

        // Get animation clip length dynamically
        float animationLength = GetAnimationLength(successAnimationName);
        yield return new WaitForSeconds(animationLength);

        RemoveObject();
    }

    private float GetAnimationLength(string animationName)
    {
        if (successAnimator == null) return 0.5f; // Fallback duration

        RuntimeAnimatorController controller = successAnimator.runtimeAnimatorController;
        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name == animationName) return clip.length;
        }

        Debug.LogWarning($"Animation '{animationName}' not found for {gameObject.name}");
        return 0.5f; // Default to 0.5s if animation is missing
    }

    private void RemoveObject()
    {
        Debug.Log($"{gameObject.name} has completed its success sequence and will be disabled.");
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        foreach (Collider2D collider in gameObject.GetComponents<BoxCollider2D>())
        {
            collider.enabled = false;
        }
    }
}
