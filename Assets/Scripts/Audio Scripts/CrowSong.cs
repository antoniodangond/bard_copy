using System.Collections;
using UnityEngine;

public class CrowSong : MonoBehaviour
{
    [Header("Note Display Settings")]
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private Transform beakPositionLeft;
    [SerializeField] private Transform beakPositionRight;
    [SerializeField] private float noteSpawnInterval = 0.75f;
    [SerializeField] private float noteLifetime = 2f;

    [Header("Animation Settings")]
    [SerializeField] private Animator crowAnimator; // Animator reference

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] noteSounds;
    [SerializeField, Range(0f, 1f)] private float maxVolume = 1.0f;
    [SerializeField, Range(1f, 10f)] private float maxHearingRadius = 5.0f;
    private int currentFrame = 0; // Stores the current animation frame

    [Header("Visual Settings")]
    [SerializeField, Range(0f, 1f)] private float minOpacity = 0.2f;

    [Header("Song Settings")]
    [SerializeField] private string[] songSequence = { "NoteB", "NoteE", "NoteD", "NoteC" };
    private bool isSinging = false;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Ensure the player is tagged correctly.");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= maxHearingRadius && !isSinging)
        {
            StartCoroutine(PlaySong(distance));
        }

        UpdateBeakPosition(); // Dynamically update beak position
    }

    private void UpdateBeakPosition()
    {
        if (crowAnimator == null)
        {
            Debug.LogError("Animator not assigned to CrowSong!");
            return;
        }

        // Get normalized time (0 to 1), then scale it to frame count (4 frames)
        float normalizedTime = crowAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        int currentFrame = Mathf.FloorToInt((normalizedTime % 1) * 4); // Modulo keeps looping cleanly

        if (currentFrame == 0 || currentFrame == 1) // Left frames
        {
            beakPositionLeft.gameObject.SetActive(true);
            beakPositionRight.gameObject.SetActive(false);
        }
        else if (currentFrame == 2 || currentFrame == 3) // Right frames
        {
            beakPositionLeft.gameObject.SetActive(false);
            beakPositionRight.gameObject.SetActive(true);
        }
    }


    private IEnumerator PlaySong(float distance)
    {
        isSinging = true;

        for (int i = 0; i < songSequence.Length; i++)
        {
            string note = songSequence[i];
            Debug.Log($"Spawning Note: {note}");

            // Ensure the beak position is updated before spawning
            UpdateBeakPosition();

            // Select the correct beak position at the moment of spawning
            Transform beakPosition = (currentFrame == 0 || currentFrame == 1) ? beakPositionLeft : beakPositionRight;

            SpawnNoteVisual(note, beakPosition.position, distance);
            PlayNoteSound(i, distance);

            yield return new WaitForSeconds(noteSpawnInterval);
        }

        isSinging = false;
    }


    private void SpawnNoteVisual(string noteName, Vector3 spawnPosition, float distance)
    {
        if (notePrefab == null)
        {
            Debug.LogError("NotePrefab not assigned in CrowSong script!");
            return;
        }

        GameObject note = Instantiate(notePrefab, spawnPosition, Quaternion.identity);
        NoteDisplay noteDisplay = note.GetComponent<NoteDisplay>();

        if (noteDisplay != null)
        {
            noteDisplay.Initialize(noteLifetime, noteName, distance, maxHearingRadius, minOpacity);
        }
        else
        {
            Debug.LogError("NoteDisplay script missing from NotePrefab!");
        }
    }

    private void PlayNoteSound(int noteIndex, float distance)
    {
        if (audioSource == null || noteSounds == null || noteIndex >= noteSounds.Length)
        {
            Debug.LogError("AudioSource or NoteSounds array is missing or index is out of bounds.");
            return;
        }

        float volume = Mathf.Lerp(minOpacity, maxVolume, 1 - (distance / maxHearingRadius));
        audioSource.PlayOneShot(noteSounds[noteIndex], volume);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxHearingRadius);
    }
}
