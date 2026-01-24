using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

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
    [SerializeField, Range(1f, 30f)] private float maxHearingRadius = 20.0f;


    [Header("Visual Settings")]
    [SerializeField, Range(0f, 1f)] private float minOpacity = 0.2f;

    [Header("Song Settings")]
    [SerializeField] private string[] songSequence = { "NoteB", "NoteE", "NoteD", "NoteC" };

    private bool isSinging = false;
    private Transform player;

    void Start()
    {
        if (notePrefab == null)
        {
            notePrefab = Resources.Load<GameObject>("Prefabs");
            Debug.LogWarning("CrowSong: notePrefab was null. Loaded via Resources.");
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Ensure the player is tagged correctly.");
        }

        if (notePrefab == null)
        {
            Debug.LogError("CrowSong: NotePrefab is NULL in Start()");
        }
        else
        {
            // Debug.Log("CrowSong: NotePrefab is assigned correctly: " + notePrefab.name);
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

        double startTime = AudioSettings.dspTime;

        for (int i = 0; i < noteSequence.Length; i++)
        {
            NoteData noteData = noteSequence[i];
            double scheduledTime = startTime + GetCumulativeTiming(i);

            StartCoroutine(ScheduleNote(noteData, i, distance, scheduledTime));
        }

        // Wait until song ends before resetting
        double totalDuration = GetCumulativeTiming(noteSequence.Length);
        yield return new WaitForSeconds((float)totalDuration + 0.5f);

        isSinging = false;
    }

    private double GetCumulativeTiming(int index)
    {
        double total = 0;
        for (int i = 0; i < index; i++)
        {
            float t = noteSequence[i].noteTiming;
            total += (t > 0f) ? t : noteSpawnInterval;
        }
        return total;
    }


    private IEnumerator ScheduleNote(NoteData noteData, int index, float distance, double scheduledTime)
    {
        // Wait until it's time for this note
        while (AudioSettings.dspTime < scheduledTime)
            yield return null;

        // Play audio
        PlayNoteSoundScheduled(noteData, scheduledTime);

        // Spawn visual
        string spriteNoteName = songSequence[index % songSequence.Length];
        Vector3 moveDirection = RotateVector(Vector3.right, movementAngle);
        Vector3 spawnPosition = transform.position + noteSpawnOffset;
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
        // Adjust all SpriteRenderers inside the note prefab (e.g., Layer0, Layer1)
        SpriteRenderer[] renderers = note.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length > 0)
        {
            int baseSorting = GetComponent<SpriteRenderer>()?.sortingOrder ?? 0;
            foreach (SpriteRenderer r in renderers)
            {
                r.sortingOrder = baseSorting + 1;
            }
        }
        else
        {
            Debug.LogError("NotePrefab has no SpriteRenderers in children!");
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

        // Debug.Log($"Scheduled {noteData.noteSound.name} to play at {scheduledTime}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxHearingRadius);
    }
}
