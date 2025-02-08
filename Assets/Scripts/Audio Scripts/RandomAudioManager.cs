// using System;
using System.Collections;
using UnityEngine;

public class RandomAudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    // public AudioClip[] clips;
    public float minDelay = 3f;
    public float maxDelay = 9f;
    private Coroutine playbackCoroutine;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    public void StartRandomAudio(AudioClip[] clips)
    {
        if (audioSource != null && clips.Length > 0)
        {
            playbackCoroutine = StartCoroutine(PlayRandomAudio(clips));
        }
        else
        {
            Debug.LogError($"{name}: AudioSource or SoundClips not set!");
        }
    }

    public void StopRandomAudio()
    {
        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
            playbackCoroutine = null;
        }
    }

    private IEnumerator PlayRandomAudio(AudioClip[] clips)
    {
        while(true)
        {
            var randomClip = clips[Random.Range(0, clips.Length)];

            audioSource.clip = randomClip;
            audioSource.Play();
            // Debug.Log($"Played {audioSource.clip}");


            float randomDelay = Random.Range(minDelay,maxDelay);
            float clipLength = randomClip.length;

            yield return new WaitForSeconds(clipLength + randomDelay);
        }
    }
}
