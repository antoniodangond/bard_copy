using System;
using System.Collections;
using UnityEngine;

public class EnemyAudio : AudioController
{
    public EnemyAudioData AudioData;

    void Awake() {
        AudioData.RandomHits = gameObject.AddComponent<RandomAudioManager>();
        AudioData.OwlRandomIdle = gameObject.AddComponent<RandomAudioManager>();
        // AudioData.RandomHits.audioSource = gameObject.AddComponent<AudioSource>();
        // AudioData.RandomAggro = gameObject.AddComponent<RandomAudioManager>();
        // AudioData.RandomAggro.audioSource = gameObject.AddComponent<AudioSource>();
        // AudioData.RandomAttacks = gameObject.AddComponent<RandomAudioManager>();
        // AudioData.RandomAttacks.audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayHit(AudioSource audioSource, string name)
    {
        switch (name)
        {
            case "Snake":
                if (AudioData.Hits.Length == 0) {
                    Debug.LogError("Enemy audio 'hits' length is 0");
                    return;
                }
                AudioData.RandomHits.PlayRandomAudioNoDelayWithFX(AudioData.Hits, 0.8f, 1.15f, true, audioSource);
                break;
            case "Owl":
                if (AudioData.OwlHits.Length == 0) {
                    Debug.LogError("Enemy audio 'hits' length is 0");
                    return;
                }
                AudioData.RandomHits.PlayRandomAudioNoDelayWithFX(AudioData.OwlHits, 0.8f, 1.15f, true, audioSource);
                break;
            default:
                return;
            
        }
    }
    public void PlayAttack(AudioSource audioSource, string name)
    {
        switch (name)
        {
            case "Snake":
                if (AudioData.Attacks.Length == 0) {
                    Debug.LogError("Enemy audio 'attacks' length is 0");
                    return;
                }
                AudioData.RandomHits.PlayRandomAudioNoDelayWithFX(AudioData.Attacks, 0.8f, 1.15f, true, audioSource);
                break;
            case "Owl":
                if (AudioData.OwlAttacks.Length == 0) {
                    Debug.LogError("Enemy audio 'owl attacks' length is 0");
                    return;
                }
                AudioData.RandomHits.PlayRandomAudioNoDelayWithFX(AudioData.OwlAttacks, 0.8f, 1.15f, true, audioSource);
                break;
            default:
                break;
        }
    }

    public void PlayAggro(AudioSource audioSource, string name)
    {
        switch (name)
        {
            case "Snake":
                if (AudioData.Aggro.Length == 0)
                {
                    Debug.LogError("Enemy audio 'aggro' length is 0");
                    return;
                }
                AudioData.RandomHits.PlayRandomAudioNoDelayWithFX(AudioData.Aggro, 0.8f, 1.15f, true, audioSource);
                break;
            case "Owl":
                if (AudioData.OwlAggro.Length == 0)
                {
                    Debug.LogError("Enemy audio 'owl aggro' length is 0");
                    return;
                }
                AudioData.RandomHits.PlayRandomAudioNoDelayWithFX(AudioData.OwlAggro, 0.8f, 1.15f, true, audioSource);
                break;
            default:
                return;
        }
    }
    
    public void PlayIdleSounds(String EnemyName, AudioSource audioSource)
    {
        if (EnemyName == "Owl" && AudioData.OwlIdle_1.Length == 0)
        {
            Debug.LogError("Enemy Owl 'idle' length is 0");
            return;
        }
        StartCoroutine(PlayOwlIdleSoundsRoutine(audioSource));
    }

    private IEnumerator PlayOwlIdleSoundsRoutine(AudioSource audioSource)
    {
        AudioData.OwlRandomIdle.PlayRandomAudioNoDelayWithFX(AudioData.OwlIdle_1, 1f, 1.1f, false, audioSource);
        yield return new WaitForSeconds(AudioData.OwlIdle_1[0].length + 0.5f);
        AudioData.OwlRandomIdle.PlayRandomAudioNoDelayWithFX(AudioData.OwlIdle_2, 1f, 1.2f, false, audioSource);
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 1.2f));
        
    }

    // TODO: implement
    public override void OnPause(bool isPaused)
    {

    }

    // TODO: implement
    public override void OnUnPause(bool isPaused)
    {

    }
}
