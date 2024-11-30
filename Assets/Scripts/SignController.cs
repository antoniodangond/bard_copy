using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SignController : MonoBehaviour
{
    [SerializeField] private Dialogue defaultDialogue; // Renamed from `dialogue` for clarity
    [SerializeField] private Dialogue updatedDialogue;
    [SerializeField] private Animator successAnimator; // Reference to the Animator for success animation
    private bool isDialogueUpdated = false;
    public bool HasDialogueOnMelody = false;
    public string signName;
    public bool IsPlayingSuccessAudio = false;
    // public AudioSource sound;
    public AudioSource[] audioSources;

    // Property to get the correct dialogue based on state
    public Dialogue CurrentDialogue => isDialogueUpdated ? updatedDialogue : defaultDialogue;

    public void Interact()
    {
        Dialogue dialogue = CurrentDialogue == null ? defaultDialogue : CurrentDialogue;
        if (dialogue != null) // Use the property to fetch the correct dialogue
        {
            DialogueManager.StartDialogue(CurrentDialogue, PlayerController.FacingDirection);
        }
    }

    // Method called when a song is played nearby
    public void OnSongPlayed(string melody)
    {
        if (melody == MelodyData.Melody1) // Check if it's the correct song
        {
            if (!isDialogueUpdated) // Prevent triggering multiple times
            {
                isDialogueUpdated = true;
                Debug.Log($"{gameObject.name}'s dialogue has been updated!");

                //Add visual/audio feedback of SUCCESS here
                 switch (signName)
                {
                    case "Log":
                        successAnimator = gameObject.GetComponent<Animator>();
                        // sound = gameObject.GetComponent<AudioSource>();
                        // sound.volume = .8f;
                        // sound.PlayDelayed(0.15f);
                        audioSources = GetComponents<AudioSource>();
                        foreach (var audiosource in audioSources)
                        {
                            audiosource.volume = .8f;
                            audiosource.PlayDelayed(0.15f);
                            Debug.Log(audiosource);
                        }
                        break;

                    case "Ghostboy":
                        successAnimator = gameObject.GetComponent<Animator>();
                        Debug.Log(successAnimator);
                        IsPlayingSuccessAudio = true;
                        // sound = gameObject.GetComponent<AudioSource>();
                        // optional_sound = gameObject.AddComponent<AudioSource>();
                        audioSources = GetComponents<AudioSource>();
                        foreach (var audiosource in audioSources)
                        {
                            audiosource.Play();
                            Debug.Log(audiosource.clip);
                        }
                        // optional_sound.Play();
                        break;
                }


                // STEP 1: Autoplay the updated dialogue 
                DialogueManager.StartDialogue(updatedDialogue, PlayerController.FacingDirection);

                // Step 2: Play success animation (if exists)
                if (successAnimator != null)
                {
                    StartCoroutine(PlaySuccessAnimation());
                }
            }
        }
    }

    private IEnumerator PlaySuccessAnimation()
    {
        // Trigger success animation
        
        successAnimator.SetTrigger("Success");

        // Wait for the animation to complete
        AnimatorStateInfo stateInfo = successAnimator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        // Step 3: Delete this object
        Debug.Log($"{gameObject.name} has completed its success sequence and will be destroyed.");
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        var colliders = gameObject.GetComponents<BoxCollider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

    }
}