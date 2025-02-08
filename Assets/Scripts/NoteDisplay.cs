using UnityEngine;

public class NoteDisplay : MonoBehaviour
{
    private float lifetime;
    private Vector3 floatDirection = new Vector3(0, 1.5f, 0);
    private float floatSpeed = 1.5f;
    private SpriteRenderer sr;

    public void Initialize(float duration, string noteName, float distance, float maxDistance, float minOpacity)
    {
        lifetime = duration;
        sr = GetComponent<SpriteRenderer>();

        SetSprite(noteName);
        AdjustOpacity(distance, maxDistance, minOpacity);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += floatDirection * floatSpeed * Time.deltaTime;
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

        float opacity = Mathf.Lerp(minOpacity, 1f, 1 - (distance / maxDistance)); // Closer = more visible
        Color newColor = sr.color;
        newColor.a = opacity;
        sr.color = newColor;
    }
}
