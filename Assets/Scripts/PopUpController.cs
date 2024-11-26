using System.Collections;
using UnityEngine;

public class PopUpController : MonoBehaviour
{
    [SerializeField] private GameObject popUpPanel; // Assign in the Inspector
    // [SerializeField] private float delay = 2f;       // Delay before deactivation in seconds
     [SerializeField] private KeyCode deactivateKey = KeyCode.Z;

    void Start()
    {
        // Check if the scene was entered from the start menu
        if (SceneTracker.Instance != null && SceneTracker.Instance.CameFromStartMenu)
        {
            ShowPopUp();
            SceneTracker.Instance.ResetTracker(); // Ensure it only triggers once
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(deactivateKey)) // Check if the key is pressed
        {
            popUpPanel.SetActive(false);
        }
    }

    private void ShowPopUp()
    {
        if (popUpPanel != null)
        {
            popUpPanel.SetActive(true); // Show the pop-up
            // DeactivateAfterDelay();
        }
        else
        {
            Debug.LogWarning("Pop-up panel not assigned!");
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
