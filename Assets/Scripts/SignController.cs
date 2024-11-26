using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignController : MonoBehaviour
{
    [SerializeField] private Dialogue defaultDialogue; // Renamed from `dialogue` for clarity
    [SerializeField] private Dialogue updatedDialogue;
    [SerializeField] private Animator successAnimator; // Reference to the Animator for success animation
    private bool isDialogueUpdated = false;
    public bool HasDialogueOnMelody = false;

    // Property to get the correct dialogue based on state
    public Dialogue CurrentDialogue => isDialogueUpdated ? updatedDialogue : defaultDialogue;

    public void Interact()
    {
        if (CurrentDialogue != null) // Use the property to fetch the correct dialogue
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
        Destroy(gameObject);
    }
}