using UnityEngine;

public class NoteDisplay : MonoBehaviour
{
    private float lifetime;
    private Vector3 moveDirection;
    private float moveSpeed = 1.5f;
    private SpriteRenderer sr;

    // In NoteDisplay.cs (or wherever you're handling note motion)
    [SerializeField] private float speed = 1f;           // Horizontal speed
    [SerializeField] private float wobbleAmplitude = 0.2f;
    [SerializeField] private float wobbleFrequency = 2f;

    private float elapsedTime = 0f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Basic horizontal movement
        Vector3 move = Vector3.right * speed * Time.deltaTime;

        // Vertical wobble via sine
        float wobbleOffset = wobbleAmplitude * Mathf.Sin(wobbleFrequency * elapsedTime);

        // Apply movement + wobble
        transform.position += move;
        transform.position = new Vector3(
            transform.position.x,
            startPosition.y + wobbleOffset,
            transform.position.z
        );
    }


    public void Initialize(float duration, string noteName, float distance, float maxDistance, float minOpacity, Vector3 direction)
    {
        lifetime = duration;
        moveDirection = direction.normalized;
        sr = GetComponent<SpriteRenderer>();

        SetSprite(noteName);
        AdjustOpacity(distance, maxDistance, minOpacity);

        Destroy(gameObject, lifetime);
    }

    private void SetSprite(string noteName)
    {
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }

        Debug.Log($"Attempting to load sprite for note: {noteName}");
        Sprite noteSprite = Resources.Load<Sprite>($"MusicNotes/{noteName}");

        if (noteSprite != null)
        {
            sr.sprite = noteSprite;
            Debug.Log($"Successfully assigned sprite for {noteName}");
        }
        else
        {
            Debug.LogError($"Sprite for {noteName} not found in Resources/MusicNotes! Check spelling.");
        }
    }

    private void AdjustOpacity(float distance, float maxDistance, float minOpacity)
    {
        if (sr == null) return;

        float opacity = Mathf.Lerp(minOpacity, 1f, 1 - (distance / maxDistance));
        Color newColor = sr.color;
        newColor.a = opacity;
        sr.color = newColor;
    }
}
