using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
[DefaultExecutionOrder(-900)]

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    public GameObject MenuUI;
    public GameObject OptionsMenuUI;
    public ButtonSelectionHandler[] buttons { get; private set; }
    private Dictionary<GameObject, (Vector3 _startPos, Vector3 _startScale)> buttonStates;
    // public bool isPaused {get; private set;}
    void Awake()
    {
        // Ensure only one instance of MenuManager exists
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);  // Optional: persists across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate instances
            return;
        }
    }
    private void Start()
    {
        buttonStates = new Dictionary<GameObject, (Vector3, Vector3)>();
        buttons = FindObjectsByType<ButtonSelectionHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var h in buttons)
        {
            var tgt = h.animTarget ? h.animTarget.gameObject : h.gameObject;
            buttonStates[tgt] = (tgt.transform.position, tgt.transform.localScale);
        }
    }

    public void TriggerButtonAnimation(GameObject button, bool startingAnimation)
    {
        if (buttonStates.ContainsKey(button))
        {
            StartCoroutine(AnimateButton(button, startingAnimation));
        }
        else
        {
            Debug.LogError($"Button {button.name} not found in buttonStates dictionary!");
        }
        // StartCoroutine(AnimateButton(button, startingAnimation));
    }

    private IEnumerator AnimateButton(GameObject button, bool startingAnimation)
    {
        if (button == null) yield break;

        float moveTime = 0.1f;
        float elapsedTime = 0f;
        float _verticalMoveAmount = 10f;
        float _scaleAmount = 1.1f;

        var (initialPos, initalScale) = buttonStates[button];

        // Define animation targets
        Vector3 _endPos;
        Vector3 _endscale;


        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / moveTime;
            if (startingAnimation)
            {
                _endPos = initialPos + new Vector3(0f, _verticalMoveAmount, 0f);
                _endscale = initalScale * _scaleAmount;
            }
            else
            {
                _endPos = initialPos;
                _endscale = initalScale;
            }

            // Interpolate position and scale
            // button.transform.position = Vector3.Lerp(initialPos, _endPos, t);
            // button.transform.localScale = Vector3.Lerp(initalScale, _endscale, t);
            Vector3 LerpedPos = Vector3.Lerp(button.transform.position, _endPos, t);
            Vector3 LerpedScale = Vector3.Lerp(button.transform.localScale, _endscale, t);

            button.transform.position = LerpedPos;
            button.transform.localScale = LerpedScale;

            yield return null;
        }

    }

    public void RegisterAnimTarget(GameObject target)
    {
        if (target == null) return;
        if (!buttonStates.ContainsKey(target))
        {
            buttonStates[target] = (target.transform.position, target.transform.localScale);
        }
    }

    public void EnsureRegistered(GameObject target)
    {
        if (target == null) return;
        if (!buttonStates.ContainsKey(target))
        {
            buttonStates[target] = (target.transform.position, target.transform.localScale);
        }
    }

    [SerializeField] private TMP_Text songListText;

    public void UpdatePauseMenuSongs() // Call this method when the pause menu is opened
    {
        var discoveredSongs = PlayerProgress.Instance.GetSavedSongs(); // returns HashSet<string>
        songListText.text = "";

        foreach (var songId in discoveredSongs)
        {
            if (MelodyData.MelodyInputs.TryGetValue(songId, out var notes))
            {
                string songName = GetSongDisplayName(songId); // e.g., "SONG OF DECAY"
                string buttonString = GetButtonStringForNotes(notes); // e.g., "A, B, X, Y, A"
                songListText.text += $"{songName}\n{buttonString}\n\n";
            }
        }
    }

    // Helper to convert note array to button string
    string GetButtonStringForNotes(string[] notes)
    {
        List<string> buttons = new List<string>();
        foreach (var note in notes)
        {
            buttons.Add(GetButtonForNote(note)); // Implement this based on your control scheme
        }
        return string.Join(", ", buttons);
    }

    private string GetSongDisplayName(string songId)
    {
        switch (songId)
        {
            case "Melody1": return "SONG OF DECAY";
            case "Melody2": return "SONG OF LIGHT";
            case "Melody3": return "SONG OF WARMTH";
            default: return songId.ToUpperInvariant();
        }
    }
    private string GetButtonForNote(string note)
    {
        // Use PlayerInputManager's dynamic mapping
        return PlayerInputManager.GetButtonForNote(note);
    }
}