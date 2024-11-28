using System.Collections;
using UnityEngine;
using UnityEngine.iOS;

public class AudioFader : MonoBehaviour
{
    static public IEnumerator FadeOutCoroutine(AudioSource source, float duration)
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

    static public IEnumerator FadeInCoroutine(AudioSource source, float duration, float targetVolume)
    {
        while (source.volume < targetVolume)
        {
            source.volume += Time.deltaTime / duration;
            yield return null;
        }

        source.volume = targetVolume;
        // source.Stop(); // Optionally stop the audio once faded out
    }

}
