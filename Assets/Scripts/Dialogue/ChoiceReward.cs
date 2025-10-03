// Assets/Scripts/Dialogue/ChoiceReward.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ChoiceReward : MonoBehaviour
{
    [Header("One-time control")]
    [Tooltip("Unique id, e.g. 'statue_forest_dash'")]
    public string rewardId;
    public bool oneTime = true;

    [Header("Grants on YES")]
    public UpgradeSO[] upgrades;

    [Header("Follow-up dialogue after FX")]
    [TextArea] public string[] followupLines = new[] { "Power surges through you." };

    [Header("FX")]
    public AudioClip sfx;
    public int flashCount = 2;
    public float flashDuration = 0.12f;
    public CanvasGroup flashOverlay; // full-screen Image + CanvasGroup (alpha 0)

    [Header("Events (optional)")]
    public UnityEvent onGranted;        // hook VFX/SFX/animators
    public UnityEvent onAlreadyClaimed; // feedback if revisited

    public bool IsAlreadyClaimed() =>
        oneTime && PlayerProgress.Instance.IsRewardClaimed(rewardId);

    public IEnumerator BestowAndExplain()
    {
        // 1) Do nothing if already claimed
        if (IsAlreadyClaimed())
        {
            onAlreadyClaimed?.Invoke();
            yield break;
        }

        // 2) Lock player in Dialogue state (already true if we came from choice UI, but ensure)
        PlayerController.CurrentState = PlayerState.Dialogue;

        // 3) Simple flash + sfx
        if (sfx) AudioSource.PlayClipAtPoint(sfx, Camera.main.transform.position, 0.9f);
        if (flashOverlay) yield return StartCoroutine(DoFlashes(flashOverlay, flashCount, flashDuration));

        // 4) Unlock upgrades
        foreach (var u in upgrades) PlayerProgress.Instance.Unlock(u);
        onGranted?.Invoke();

        // 5) Mark as claimed
        if (oneTime) PlayerProgress.Instance.MarkRewardClaimed(rewardId);

        // 6) Show follow-up lines via a temporary Dialogue asset
        if (followupLines != null && followupLines.Length > 0)
        {
            var temp = ScriptableObject.CreateInstance<Dialogue>();
            temp.universalLines.AddRange(followupLines);
            DialogueManager.StartDialogue(temp, PlayerController.FacingDirection);
            // Wait until the follow-up dialogue closes
            while (DialogueManager.IsOpen) yield return null;
        }

        // 7) Release control back to the player (DialogueManager does this on close, but ensure)
        PlayerController.CurrentState = PlayerState.Default;
    }

    private IEnumerator DoFlashes(CanvasGroup cg, int count, float dur)
    {
        for (int i = 0; i < count; i++)
        {
            // fade up
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            {
                cg.alpha = Mathf.Lerp(0f, 1f, t / dur);
                yield return null;
            }
            cg.alpha = 1f;

            // fade down
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            {
                cg.alpha = Mathf.Lerp(1f, 0f, t / dur);
                yield return null;
            }
            cg.alpha = 0f;
        }
    }
}
