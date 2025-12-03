using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EurydiceGrave : MonoBehaviour
{
    [Header("Settings")]
    public float fadeInTime;
    private int songsPlayed;
    public Sprite completedGraveSprite;
    // Sprite renderer of initial grave, gets updated with completed grave on game complete
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Header("Grave Pieces")] 
    [SerializeField] private GameObject pieceOne;
    [SerializeField] private GameObject pieceTwo;
    [SerializeField] private GameObject pieceThree;
    [Header("Gateway")]
    [SerializeField] private GameObject underworldGateway;
    [Header("Effects")]
    [SerializeField] private ParticleSystem glowingParticles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleGameCompleteProgression();
    }

    public void OnSongPlayed(string melody)
    {
        switch (melody)
        {
            case MelodyData.Melody1:
                songsPlayed += 1;
                Debug.Log(songsPlayed + " songs played");
                break;

            case MelodyData.Melody2:
                songsPlayed += 1;
                Debug.Log(songsPlayed + " songs played");
                break;

            case MelodyData.Melody3:
                songsPlayed += 1;
                Debug.Log(songsPlayed + " songs played");
                break;
            default:
                break;
        }
    }

    private void HandleGameCompleteProgression()
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
        spriteRenderer.sprite = completedGraveSprite;
        StartCoroutine(FadeInGameObject(underworldGateway, fadeInTime));
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

            Color.Lerp(transparent, opaque, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
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
            float t = elapsedTime/fadeOutTime;

            Color.Lerp(opaque, transparent, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
