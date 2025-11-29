using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public AudioSource audioSource;
    public int flashCount = 2;
    public float flashDuration = 0.12f;
    public CanvasGroup flashOverlay; // full-screen Image + CanvasGroup (alpha 0)

    [Header("Events (optional)")]
    public UnityEvent onGranted;
    public UnityEvent onAlreadyClaimed;

    public bool IsAlreadyClaimed()
    {
        if (!oneTime) return false;
        if (PlayerProgress.Instance == null) return false;
        return PlayerProgress.Instance.IsRewardClaimed(rewardId);
    }

    public IEnumerator BestowAndExplain(Dictionary<string, string> playerControls, string currentControlScheme)
    {
        // 1) Already claimed?
        if (IsAlreadyClaimed())
        {
            onAlreadyClaimed?.Invoke();
            yield break;
        }

        // 2) Lock player in dialogue state
        PlayerController.CurrentState = PlayerState.Dialogue;

        // 3) FX: sound + flashes
        if (sfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(sfx);
        }

        if (flashOverlay != null)
        {
            yield return StartCoroutine(DoFlashes(flashOverlay, flashCount, flashDuration));
        }

        // 4) Unlock upgrades in PlayerProgress
        if (PlayerProgress.Instance != null && upgrades != null)
        {
            foreach (var u in upgrades)
            {
                if (u == null) continue;
                PlayerProgress.Instance.Unlock(u);
            }
        }

        onGranted?.Invoke();

        // 5) Mark as claimed
        if (oneTime && PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.MarkRewardClaimed(rewardId);
        }

        // 6) Follow-up dialogue, including dynamic button text
        if (followupLines != null && followupLines.Length > 0)
        {
            // Safely pull out the buttons (with fallbacks)
            playerControls.TryGetValue("dash", out var dashButton);
            playerControls.TryGetValue("aoe_attack_1", out var aoe1);
            playerControls.TryGetValue("aoe_attack_2", out var aoe2);

            // Make a copy so we don't mutate the original asset
            var lines = (string[])followupLines.Clone();

            // Choose what to inject based on rewardId
            if (rewardId == "dash")
            {
                string line = !string.IsNullOrEmpty(dashButton)
                    ? $"Press {dashButton} to Nereid Step"
                    : "You feel lighter on your feet.";

                // Prefer index 1 if it exists, otherwise clobber index 0
                int targetIndex = (lines.Length > 1) ? 1 : 0;
                lines[targetIndex] = line;
            }
            else if (rewardId == "aoe" || rewardId == "aoe_attack")
            {
                string line;

                if (currentControlScheme == "Keyboard")
                {
                    // Keyboard: show both keys if you have them
                    if (!string.IsNullOrEmpty(aoe1) && !string.IsNullOrEmpty(aoe2))
                        line = $"Press {aoe1} or {aoe2} to unleash your AOE ATTACK.";
                    else
                        line = $"Press {aoe1} to unleash a dissonant chord in all directions.";
                }
                else
                {
                    // Gamepad: aoe1 should already be a sprite-tagged icon string
                    line = !string.IsNullOrEmpty(aoe1)
                        ? $"Press {aoe1} to unleash a dissonant chord in all directions."
                        : "You feel a new power stirring within you.";
                }

                int targetIndex = (lines.Length > 1) ? 1 : 0;
                lines[targetIndex] = line;
            }

            var temp = ScriptableObject.CreateInstance<Dialogue>();
            temp.universalLines.AddRange(lines);

            DialogueManager.StartDialogue(temp, PlayerController.FacingDirection);
            while (DialogueManager.IsOpen)
                yield return null;
        }

        // 7) Ensure control returns to player (DialogueManager also does this on close)
        PlayerController.CurrentState = PlayerState.Default;
    }

    private IEnumerator DoFlashes(CanvasGroup cg, int count, float dur)
    {
        for (int i = 0; i < count; i++)
        {
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            {
                cg.alpha = Mathf.Lerp(0f, 1f, t / dur);
                yield return null;
            }
            cg.alpha = 1f;

            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            {
                cg.alpha = Mathf.Lerp(1f, 0f, t / dur);
                yield return null;
            }
            cg.alpha = 0f;
        }
    }
}
