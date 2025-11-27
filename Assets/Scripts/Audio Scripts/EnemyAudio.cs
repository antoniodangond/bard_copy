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

    public void PlayHit(AudioSource audioSource)
    {
        if (AudioData.Hits.Length == 0) {
            Debug.LogError("Enemy audio 'hits' length is 0");
            return;
        }
        AudioData.RandomHits.PlayRandomAudioNoDelayWithFX(AudioData.Hits, 0.8f, 1.15f, true, audioSource);
    }
    public void PlayAttack(AudioSource audioSource)
    {
        if (AudioData.Attacks.Length == 0) {
            Debug.LogError("Enemy audio 'attacks' length is 0");
            return;
        }
        AudioData.RandomHits.PlayRandomAudioNoDelayWithFX(AudioData.Attacks, 0.8f, 1.15f, true, audioSource);
    }

    public void PlayAggro(AudioSource audioSource)
    {
        if (AudioData.Aggro.Length == 0)
        {
            Debug.LogError("Enemy audio 'aggro' length is 0");
            return;
        }
        AudioData.RandomHits.PlayRandomAudioNoDelayWithFX(AudioData.Aggro, 0.8f, 1.15f, true, audioSource);
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
