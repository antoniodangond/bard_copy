using System.Collections;
using UnityEngine;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        imageComponent = GetComponent<Image>();
    }
    
    void Start()
    {
        PlayEnding();
    }

    private void PlayEnding()
    {
        if (ending == endingType.bad)
        {
            Sprite[] sprites = endingData.badEndingSprites;
            Sprite creditsBG = endingData.badEndCreditsBG;
            // imageComponent.sprite = sprites[0];
            StartCoroutine(PlayEndRoutine(sprites, creditsBG,imageCycleTime));
        }
    }
    
    private IEnumerator PlayEndRoutine(Sprite[] endingSprites, Sprite creditsSprite,float imageCycleTime)
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
        StartCoroutine(FadeInSprite(imageComponent, creditsSprite, fadeInTime));
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
            float t = elapsedTime/fadeOutTime;

            image.color = Color.Lerp(opaque, transparent, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
