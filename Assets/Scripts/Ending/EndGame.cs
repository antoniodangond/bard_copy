using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    public Image imageComponent;
    public EndingData endingData;
    public float imageCycleTime;
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
            // imageComponent.sprite = sprites[0];
            StartCoroutine(PlayEndRoutine(sprites, imageCycleTime));
        }
    }
    
    private IEnumerator PlayEndRoutine(Sprite[] sprites,float imageCycleTime)
    {
        int cycles = sprites.Length;

        for (int i = 0; i < cycles; i++)
        {
            imageComponent.sprite = sprites[i];
            yield return new WaitForSeconds(imageCycleTime);
        }
    }
}
