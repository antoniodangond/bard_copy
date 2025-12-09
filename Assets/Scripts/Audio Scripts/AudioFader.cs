using System.Collections;
using UnityEngine;

public class AudioFader : MonoBehaviour
{
    // Default crossfade time in seconds
    public static float defaultCrossfadeTime = 2f;

    static public IEnumerator FadeOutCoroutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        source.volume = 0;
    }

    static public IEnumerator FadeInCoroutine(AudioSource source, float duration, float targetVolume)
    {
        while (source.volume < targetVolume)
        {
            source.volume += Time.deltaTime / duration;
            yield return null;
        }

        source.volume = targetVolume;
    }

    // Crossfade that automatically uses the default 2 second duration
    static public IEnumerator Crossfade(
        AudioSource fromSource,
        AudioSource toSource,
        float targetVolume
    )
    {
        yield return CrossfadeCoroutine(fromSource, toSource, defaultCrossfadeTime, targetVolume);
    }

    // Core crossfade coroutine (used by the method above)
    static public IEnumerator CrossfadeCoroutine(
        AudioSource fromSource,
        AudioSource toSource,
        float duration,
        float targetVolume
    )
    {
        float startVolumeFrom = fromSource.volume;
        toSource.volume = 0f;
        toSource.Play();

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            fromSource.volume = Mathf.Lerp(startVolumeFrom, 0f, progress);
            toSource.volume = Mathf.Lerp(0f, targetVolume, progress);

            yield return null;
        }

        fromSource.volume = 0f;
        toSource.volume = targetVolume;
    }
}

