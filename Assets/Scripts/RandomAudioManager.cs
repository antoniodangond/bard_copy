// using System;
using System.Collections;
using UnityEngine;

public class RandomAudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public float minDelay = 3f;
    public float maxDelay = 9f;
    private Coroutine playbackCoroutine;
    private AudioClip lastPlayedClip;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    public void StartRandomAudioWithDelay(AudioClip[] clips)
    {
        if (audioSource != null && clips.Length > 0)
        {
            playbackCoroutine = StartCoroutine(PlayRandomAudioWithDelay(clips));
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

    private IEnumerator PlayRandomAudioWithDelay(AudioClip[] clips)
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

    public void PlayRandomAudioNoDelayWithFX(AudioClip[] clips, float minPitch, float maxPitch, bool volMod)
    {
        AudioClip randomClip;
        if (audioSource != null && clips.Length > 0)
        {

            if (clips.Length == 1)
            {
                audioSource.clip = clips[0];
                audioSource.Play();
                lastPlayedClip = clips[0];
                return;
            }
            
            do 
            {
                randomClip = clips[Random.Range(0,clips.Length)];
            }
            while (randomClip == lastPlayedClip);
            audioSource.clip = randomClip;
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            if (volMod)
            {
                audioSource.volume = Random.Range(0.75f, 1.25f);
            }
            audioSource.Play();
        }
        else
        {
            Debug.LogError("AudioSource or SoundClips not set!");
            return;
        }
    }
}
