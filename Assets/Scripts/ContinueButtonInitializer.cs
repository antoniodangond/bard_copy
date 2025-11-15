using UnityEngine;
using UnityEngine.UI;

public class ContinueButtonInitializer : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button continueButton;

    void Start()
    {
        if (continueButton == null) return;

        bool hasSave =
            PlayerProgress.Instance != null &&
            PlayerProgress.Instance.HasSaveFile();

        continueButton.interactable = hasSave;
    }
}
