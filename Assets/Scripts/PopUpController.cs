using System.Collections;
using UnityEngine;

public class PopUpController : MonoBehaviour
{
     [SerializeField] private KeyCode deactivateKey = KeyCode.Z;

     [Header("Dialogue Settings")]
    [SerializeField] private Dialogue defaultDialogue;

    void Start()
    {
        DialogueManager.StartDialogue(defaultDialogue, FacingDirection.Up);
    }

    void Update()
    {
        if (Input.GetKeyDown(deactivateKey)) // Check if the key is pressed
        {
            DialogueManager.Instance.EndDialogue();
            Destroy(gameObject);
        }
    }

    //  public void DeactivateAfterDelay()
    // {
    //     StartCoroutine(DeactivateCanvasCoroutine());
    // }

    // private IEnumerator DeactivateCanvasCoroutine()
    // {
    //     yield return new WaitForSeconds(delay); // Wait for the specified delay
    //     popUpPanel.SetActive(false);          // Deactivate the canvas
    // }
}
