using System.Collections;
using UnityEngine;
using UnityEngine.iOS;

public class AudioFader : MonoBehaviour
{
    // public BackgroundAudio audioData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     private IEnumerator FadeOutCoroutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        source.volume = 0;
        // source.Stop(); // Optionally stop the audio once faded out
    }

     private IEnumerator FadeInCoroutine(AudioSource source, float duration, float targetVolume)
    {
        // float startVolume = source.volume;

        while (source.volume < targetVolume)
        {
            source.volume += Time.deltaTime / duration;
            yield return null;
        }

        source.volume = targetVolume;
        // source.Stop(); // Optionally stop the audio once faded out
    }

}
