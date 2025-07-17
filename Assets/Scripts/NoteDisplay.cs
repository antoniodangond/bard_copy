using System.Collections.Generic;
using UnityEngine;

public class NoteDisplay : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 1.5f;

    [Header("Sine Wave Motion")]
    [SerializeField] private float waveFrequency = 2f;   // Cycles per second
    [SerializeField] private float waveAmplitude = 0.1f; // Vertical swing

    [Header("Rotation Wobble")]
    [SerializeField] private bool applyRotationWobble = true;
    [SerializeField] private float wobbleFrequency = 2f;     // Cycles per second
    [SerializeField] private float wobbleAngleRange = 10f;   // Degrees

    [Header("Fade & Lifetime")]
    [SerializeField] private float defaultLifetime = 2f;
    [SerializeField, Range(0f, 1f)] private float minOpacity = 0.2f;

    [Header("Sprites Per Note")]
    [SerializeField] private Sprite noteB_Layer0;
    [SerializeField] private Sprite noteB_Layer1;
    [SerializeField] private Sprite noteC_Layer0;
    [SerializeField] private Sprite noteC_Layer1;
    [SerializeField] private Sprite noteD_Layer0;
    [SerializeField] private Sprite noteD_Layer1;
    [SerializeField] private Sprite noteE_Layer0;
    [SerializeField] private Sprite noteE_Layer1;

    [Header("Child Renderers")]
    [SerializeField] private SpriteRenderer layer0Renderer;
    [SerializeField] private SpriteRenderer layer1Renderer;

    private float lifetime;
    private float elapsedTime = 0f;
    private Vector3 moveDirection;
    private bool particlesSpawned = false;

    private Vector3 basePosition;
    private Quaternion initialRotation;

    private Dictionary<string, (Sprite, Sprite)> spriteMap;

    public void Initialize(float lifetime, string noteName, float distance, float maxDistance, float minOpacityOverride, Vector3 moveDirection)
    {
        this.lifetime = lifetime > 0 ? lifetime : defaultLifetime;
        this.moveDirection = moveDirection;
        this.minOpacity = minOpacityOverride;

        basePosition = transform.position;
        initialRotation = transform.rotation;

        SetupSpriteMap();

        // Opacity fade based on distance
        float alpha = Mathf.Lerp(1f, minOpacity, distance / maxDistance);
        ApplyOpacity(alpha);

        // Assign layered sprites
        if (spriteMap.TryGetValue(noteName, out var spritePair))
        {
            layer0Renderer.sprite = spritePair.Item1;
            layer1Renderer.sprite = spritePair.Item2;
        }
        else
        {
            Debug.LogWarning($"NoteDisplay: Missing sprite pair for {noteName}");
        }
    }

    private void SetupSpriteMap()
    {
        spriteMap = new Dictionary<string, (Sprite, Sprite)>
        {
            { "NoteB", (noteB_Layer0, noteB_Layer1) },
            { "NoteC", (noteC_Layer0, noteC_Layer1) },
            { "NoteD", (noteD_Layer0, noteD_Layer1) },
            { "NoteE", (noteE_Layer0, noteE_Layer1) },
        };
    }

    private void ApplyOpacity(float alpha)
    {
        if (layer0Renderer != null)
        {
            Color c = layer0Renderer.color;
            c.a = alpha;
            layer0Renderer.color = c;
        }
        if (layer1Renderer != null)
        {
            Color c = layer1Renderer.color;
            c.a = alpha;
            layer1Renderer.color = c;
        }
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Move base position forward
        basePosition += moveDirection * speed * Time.deltaTime;

        // Calculate sine wave offset
        float sineOffset = Mathf.Sin(elapsedTime * waveFrequency * Mathf.PI * 2f) * waveAmplitude;
        Vector3 verticalOffset = new Vector3(0, sineOffset, 0);

        // Combine movement + sine offset
        transform.position = basePosition + verticalOffset;

        // Rotation wobble
        if (applyRotationWobble)
        {
            float angle = Mathf.Sin(elapsedTime * wobbleFrequency * Mathf.PI * 2f) * wobbleAngleRange;
            transform.rotation = initialRotation * Quaternion.Euler(0, 0, angle);
        }

        // Lifetime expiration
        if (elapsedTime >= lifetime)
        {
            if (!particlesSpawned)
            {
                TriggerParticles();
            }
            Destroy(gameObject);
        }
    }

    private void TriggerParticles()
    {
        // Optional: spawn VFX here
        particlesSpawned = true;
    }
}
