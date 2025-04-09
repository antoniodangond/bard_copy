using System.Collections;
using UnityEngine;

[System.Serializable]
public struct NoteData
{
    public AudioClip noteSound;
    public float noteTiming;
}

public class CrowSong : MonoBehaviour
{
    [Header("Note Display Settings")]
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private float noteSpawnInterval = 0.75f;
    [SerializeField] private float noteLifetime = 2f;
    [SerializeField] private Vector3 noteSpawnOffset = new Vector3(0, 0.5f, 0); // Adjustable in Editor

    [Header("Note Settings")]
    [SerializeField] private NoteData[] noteSequence; // Notes Assignments & Timings  

    [Header("Movement Settings")]
    [SerializeField] private float movementAngle = 0f; // Rotation of Note Path  

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField, Range(0f, 1f)] private float maxVolume = 1.0f;
    [SerializeField, Range(1f, 30f)] private float maxHearingRadius = 20.0f;

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
        if (player == null || isSinging) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= maxHearingRadius)
        {
            StartCoroutine(PlaySong(distance));
        }
    }

    private IEnumerator PlaySong(float distance)
    {
        isSinging = true;
        double nextPlayTime = AudioSettings.dspTime; // Get precise audio time

        // For each note in the sequence
        for (int i = 0; i < noteSequence.Length; i++)
        {
            NoteData noteData = noteSequence[i];

            // Always spawn notes towards the right
            Vector3 moveDirection = Vector3.right;
            Vector3 spawnPosition = transform.position + noteSpawnOffset;

            // Spawn note & schedule sound
            StartCoroutine(SpawnNoteWithDelay(noteData, spawnPosition, distance, moveDirection, nextPlayTime, i));
            nextPlayTime += noteData.noteTiming;
            yield return new WaitForSeconds(noteData.noteTiming);
        }

        isSinging = false;
    }


    private IEnumerator SpawnNoteWithDelay(NoteData noteData, Vector3 spawnPosition, float distance, Vector3 moveDirection, double scheduledTime, int noteIndex)
    {
        while (AudioSettings.dspTime < scheduledTime)
        {
            yield return null;
        }

        PlayNoteSoundScheduled(noteData, scheduledTime);

        // Use the note name from `songSequence` at the correct index
        string spriteNoteName = songSequence[noteIndex % songSequence.Length];
        SpawnNoteVisual(spriteNoteName, spawnPosition, distance, moveDirection);
    }

    private void SpawnNoteVisual(string noteName, Vector3 spawnPosition, float distance, Vector3 moveDirection)
    {
        if (notePrefab == null)
        {
            Debug.LogError("NotePrefab not assigned in CrowSong script!");
            return;
        }

        GameObject note = Instantiate(notePrefab, spawnPosition, Quaternion.identity);

        // Set sorting order to be in front of the crow
        SpriteRenderer noteRenderer = note.GetComponent<SpriteRenderer>();
        if (noteRenderer != null)
        {
            noteRenderer.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
        }
        else
        {
            Debug.LogError("NotePrefab is missing a SpriteRenderer!");
        }

        note.transform.SetParent(transform, true); // Keep world position even if parented

        NoteDisplay noteDisplay = note.GetComponent<NoteDisplay>();
        if (noteDisplay != null)
        {
            noteDisplay.Initialize(noteLifetime, noteName, distance, maxHearingRadius, minOpacity, moveDirection);
        }
        else
        {
            Debug.LogError("NoteDisplay script missing from NotePrefab!");
        }
    }

    private Vector3 RotateVector(Vector3 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad; // Convert degrees to radians
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector3(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos,
            vector.z
        );
    }

    private void PlayNoteSoundScheduled(NoteData noteData, double scheduledTime)
    {
        if (audioSource == null || noteData.noteSound == null)
        {
            Debug.LogError("Missing AudioSource or Note Sound.");
            return;
        }

        audioSource.clip = noteData.noteSound;
        audioSource.PlayScheduled(scheduledTime);

        Debug.Log($"Scheduled {noteData.noteSound.name} to play at {scheduledTime}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxHearingRadius);
    }
}
