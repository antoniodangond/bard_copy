using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    public Image imageComponent;
    public EndingData endingData;
    public float imageCycleTime;
    public float fadeInTime;
    public float fadeOutTime;
    public enum endingType
    {
        bad
        , good
    }
    public endingType ending;
    private GameObject creditsObject;
    private Image creditsImage;
    [SerializeField] private GameObject buttonObject;
    private Button button;
    private bool endingPlayed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        imageComponent = GetComponent<Image>();
        creditsObject = gameObject.transform.GetChild(0).gameObject;
        creditsImage = creditsObject.GetComponent<Image>();
        button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(ReturnToMenu);
        endingPlayed = false;
    }

    void Start()
    {
        PlayEnding();
    }

    private void HandlePostCredits()
    {
        endingPlayed = true;
        buttonObject.SetActive(true);
    }

    private void ReturnToMenu()
    {
        if (endingPlayed)
        {
            SceneManager.LoadScene("Main Menu");
        }
    }

    private void PlayEnding()
    {
        if (ending == endingType.bad)
        {
            StartCoroutine(GameManager.Instance.backgroundAudio.ChangeBackgroundMusic("BadEnding"));
            Sprite[] sprites = endingData.badEndingSprites;
            Sprite[] creditsSprites = endingData.badEndingCreditsSprites;
            Sprite creditsBG = endingData.badEndCreditsBG;
            // imageComponent.sprite = sprites[0];
            StartCoroutine(PlayEndRoutine(sprites, creditsBG, creditsSprites, imageCycleTime));
        }
        else if (ending == endingType.good)
        {
            StartCoroutine(GameManager.Instance.backgroundAudio.ChangeBackgroundMusic("GoodEnding"));
            Sprite[] sprites = endingData.goodEndingSprites;
            Sprite[] creditsSprites = endingData.goodEndingCreditsSprites;
            Sprite creditsBG = endingData.goodEndCreditsBG;
            // imageComponent.sprite = sprites[0];
            StartCoroutine(PlayEndRoutine(sprites, creditsBG, creditsSprites, imageCycleTime));
        }
    }
    
    private IEnumerator PlayEndRoutine(Sprite[] endingSprites, Sprite creditsBGSprite, Sprite[] creditsSprites,float imageCycleTime)
    {
        int cycles = endingSprites.Length;

        for (int i = 0; i < cycles; i++)
        {
            imageComponent.sprite = endingSprites[i];
            yield return new WaitForSeconds(imageCycleTime);
        }
     
        yield return new WaitForSeconds(imageCycleTime);

        StartCoroutine(FadeOutSprite(imageComponent, imageComponent.sprite, fadeOutTime));
        yield return new WaitForSeconds(fadeOutTime);
        StartCoroutine(FadeInSprite(imageComponent, creditsBGSprite, fadeInTime));
        yield return new WaitForSeconds(fadeInTime);
        HandleCredits(creditsSprites);
    }

    private IEnumerator FadeInSprite(Image image, Sprite sprite, float fadeInTime)
    {
        // first assign variables
        Color transparent = image.color;
        Color opaque = transparent;
        opaque.a = 1;
        transparent.a = 0;

        // next make image transparent
        image.color = transparent;

        // assign new sprite
        image.sprite = sprite;

        float elapsedTime = 0;

        while (elapsedTime < fadeInTime)
        {
            float t = elapsedTime/fadeInTime;

            image.color = Color.Lerp(transparent, opaque, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator FadeOutSprite(Image image, Sprite sprite, float fadeOutTime)
    {
        Color transparent = image.color;
        Color opaque = transparent;
        opaque.a = 1;
        transparent.a = 0;

        float elapsedTime = 0;

        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;

            image.color = Color.Lerp(opaque, transparent, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void HandleCredits(Sprite[] sprites)
    {
        // set color on credits image to transparent
        Color transparent = creditsImage.color;
        Color opaque = transparent;
        transparent.a = 0;
        creditsImage.color = transparent;
        // activate GameObject
        creditsObject.SetActive(true);
        // loop "num credit images" times
        StartCoroutine(HandleCreditsRoutine(sprites));
    }
    
    private IEnumerator HandleCreditsRoutine(Sprite[] sprites)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            // set credits image
            creditsImage.sprite = sprites[i];
            // // fade in credits image
            StartCoroutine(FadeInSprite(creditsImage, creditsImage.sprite, fadeInTime));
            yield return new WaitForSeconds(fadeInTime*1.5f);
            if (i != sprites.Length - 1)
            {
                // // fade out credits image
                StartCoroutine(FadeOutSprite(creditsImage, creditsImage.sprite, fadeOutTime));
                yield return new WaitForSeconds(fadeOutTime);
            }
        }

        HandlePostCredits();
        
    }
}
