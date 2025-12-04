using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EurydiceGrave : MonoBehaviour
{
    [Header("Settings")]
    public float fadeInTime;
    [Header("Grave Pieces")] 
    [SerializeField] private GameObject pieceOne;
    [SerializeField] private GameObject pieceTwo;
    [SerializeField] private GameObject pieceThree;
    [SerializeField] private Sprite completedGrave;
    [SerializeField] private GameObject spriteRendererObject;
    private SpriteRenderer spriteRenderer;
    [Header("Gateway")]
    [SerializeField] private GameObject underworldGateway;
    [Header("Effects")]
    [SerializeField] private ParticleSystem glowingParticles;
    private int songsPlayed;
    private BoxCollider2D boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = spriteRendererObject.GetComponent<SpriteRenderer>();
    }

    public void OnSongPlayed(string melody)
    {
        switch (melody)
        {
            case MelodyData.Melody1:
                songsPlayed += 1;
                HandleGameCompleteProgression(songsPlayed);
                break;

            case MelodyData.Melody2:
                songsPlayed += 1;
                HandleGameCompleteProgression(songsPlayed);
                break;

            case MelodyData.Melody3:
                songsPlayed += 1;
                HandleGameCompleteProgression(songsPlayed);
                break;
            default:
                break;
        }
    }

    private void HandleGameCompleteProgression(int songsPlayed)
    {
        switch (songsPlayed)
        {
            case 1:
                StartCoroutine(FadeInGameObject(pieceOne, fadeInTime));
                break;
            case 2:
                StartCoroutine(FadeInGameObject(pieceTwo, fadeInTime));
                break;
            case 3:
                StartCoroutine(GameCompleteRoutine());
                break;
            default:
                break;
        }
    }

    private IEnumerator GameCompleteRoutine()
    {
        StartCoroutine(FadeInGameObject(pieceThree, fadeInTime));

        yield return new WaitForSeconds(fadeInTime);

        spriteRenderer.sprite = completedGrave;

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(OpenDoorway());

        yield return new WaitForSeconds(fadeInTime);

        Destroy(boxCollider);
    }
    
    private IEnumerator OpenDoorway()
    {
        StartCoroutine(FadeInGameObject(underworldGateway, fadeInTime));

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(MoveGrave(0.8f, fadeInTime));

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(FadeOutSprite(spriteRenderer, fadeInTime));
        
    }

    private IEnumerator FadeInGameObject(GameObject currentGameObject, float fadeInTime)
    {
        SpriteRenderer sr = currentGameObject.GetComponent<SpriteRenderer>();
        Color transparent = sr.color;
        Color opaque = transparent;
        opaque.a = 1;
        transparent.a = 0;
        currentGameObject.SetActive(true);

        float elapsedTime = 0;

        while (elapsedTime < fadeInTime)
        {
            float t = elapsedTime/fadeInTime;

            sr.color = Color.Lerp(transparent, opaque, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOutGameObject(GameObject go, float fadeOutTime)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        Color transparent = sr.color;
        Color opaque = transparent;
        opaque.a = 1;
        transparent.a = 0;

        float elapsedTime = 0;

        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;

            sr.color = Color.Lerp(opaque, transparent, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(go);
    }
    private IEnumerator FadeOutSprite(SpriteRenderer sr, float fadeOutTime)
    {
        Color transparent = sr.color;
        Color opaque = transparent;
        opaque.a = 1;
        transparent.a = 0;

        float elapsedTime = 0;

        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;

            sr.color = Color.Lerp(opaque, transparent, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator MoveGrave(float distance, float moveTime)
    {
        float elapsedTime = 0;

        Vector3 gravePos = spriteRenderer.transform.localPosition;
        Vector3 graveDest = gravePos + new Vector3(0, distance);

        while (elapsedTime < moveTime)
        {
            float t = elapsedTime / moveTime;

            spriteRenderer.transform.localPosition = Vector3.Lerp(gravePos, graveDest, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
