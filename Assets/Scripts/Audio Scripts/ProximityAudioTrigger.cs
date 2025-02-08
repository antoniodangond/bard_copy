using UnityEngine;

public class ProximityAudioTrigger : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;  // Assign an AudioSource
    [SerializeField] private AudioClip audioClip;      // Assign the sound clip
    [SerializeField, Range(0f, 1f)] private float maxVolume = 1.0f;  // Max volume when at center
    [SerializeField, Range(0f, 10f)] private float fadeSpeed = 1.5f; // Smoothness of volume changes

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5.0f;  // The max detection range

    private Transform player;
    private bool isPlaying = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("ERROR: Player NOT found! Check if Player has the correct tag.");
        }
        else
        {
            Debug.Log($"Player found: {player.name}");
        }

        // Ensure AudioSource exists
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource assigned. Adding one now.");
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ensure an AudioClip is assigned
        if (audioClip == null)
        {
            Debug.LogError("ERROR: No AudioClip assigned! Assign one in the Inspector.");
        }
        else
        {
            audioSource.clip = audioClip;
        }

        // Set default audio properties
        audioSource.loop = true;  // Ensure the sound loops while the player is near
        audioSource.volume = 0;   // Start at 0 volume
        audioSource.playOnAwake = false;  // Ensure it doesn't start playing automatically
    }

    void Update()
    {
        if (player == null) return; // Prevents further errors if player is missing

        float distance = Vector2.Distance(transform.position, player.position);

        // Adjust volume based on proximity
        float volumeScale = Mathf.Clamp01(1 - (distance / detectionRadius)); // Closer = louder
        audioSource.volume = Mathf.Lerp(audioSource.volume, volumeScale * maxVolume, Time.deltaTime * fadeSpeed);

        // Start playing sound if entering range
        if (distance <= detectionRadius && !isPlaying)
        {
            Debug.Log("Player entered range, playing sound!");
            audioSource.Play();
            isPlaying = true;
        }
        // Stop playing sound when leaving range
        else if (distance > detectionRadius && isPlaying)
        {
            Debug.Log("Player left range, stopping sound.");
            audioSource.Stop();
            isPlaying = false;
        }
    }

    // Draw the detection radius in the Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
