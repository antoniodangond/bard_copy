using UnityEngine;

public class EnemyAudio : AudioController
{
    public EnemyAudioData AudioData;

    // Store last played clip indexes to avoid playing it twice in a row
    private int lastHitClipIndex = 0;

    // TODO: refactor to use less audio sources and just change clips
    void Awake() {
        // TODO: improve this
        // Instantiate audio sources
        foreach (Sound sound in AudioData.Hits)
        {
            InitializeSound(sound);
        }
        foreach (Sound sound in AudioData.Aggro)
        {
            InitializeSound(sound);
        }
        foreach (Sound sound in AudioData.Attacks)
        {
            InitializeSound(sound);
        }
    }

    public void PlayHit()
    {
        if (AudioData.Hits.Length == 0) {
            Debug.LogError("Enemy audio 'hits' length is 0");
            return;
        }
        // Prevent playing the same clip twice in a row
        int clipIndex = lastHitClipIndex;
        while (clipIndex == lastHitClipIndex)
        {
            clipIndex = Random.Range(0, AudioData.Hits.Length);
        }
        // Play the sound with default pitch and volume
        AudioData.Hits[clipIndex].Play();
        // Set lastHitClipIndex to the index just used
        lastHitClipIndex = clipIndex;
    }
    public void PlayAttack()
    {
        if (AudioData.Attacks.Length == 0) {
            Debug.LogError("Enemy audio 'attacks' length is 0");
            return;
        }
        AudioData.Attacks[0].SetVolume(2f);
        AudioData.Attacks[0].Play();
    }

    public void PlayAggro()
    {
        if (AudioData.Aggro.Length == 0) {
            Debug.LogError("Enemy audio 'aggro' length is 0");
            return;
        }
        AudioData.Aggro[0].SetVolume(2f);
        AudioData.Aggro[0].Play();
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
