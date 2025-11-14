using UnityEngine;
using TMPro;
using System.Collections;

public class SaveGameToast : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string message = "Game Saved!";
    [SerializeField] private float duration = 1.5f;

    private Coroutine _toastRoutine;

    private void Awake()
    {
        // Make sure we're invisible at start, but still active
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (PlayerProgress.Instance != null)
            PlayerProgress.Instance.OnSaved -= HandleSaved;
    }

    private void TrySubscribe()
    {
        if (PlayerProgress.Instance == null)
        {
            Debug.LogWarning("[SaveGameToast] PlayerProgress.Instance is null; can't subscribe yet.");
            return;
        }

        // Avoid double-subscribe
        PlayerProgress.Instance.OnSaved -= HandleSaved;
        PlayerProgress.Instance.OnSaved += HandleSaved;
    }

    private void HandleSaved()
    {
        Debug.Log("[SaveGameToast] HandleSaved received; showing toast.");

        if (_toastRoutine != null)
            StopCoroutine(_toastRoutine);

        _toastRoutine = StartCoroutine(ShowToastRoutine());
    }

    private IEnumerator ShowToastRoutine()
    {
        if (label != null)
            label.text = message;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // Use realtime so it still shows while paused (timeScale = 0)
        yield return new WaitForSecondsRealtime(duration);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        _toastRoutine = null;
    }
}
