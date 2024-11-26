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
    }

    public void PlayHit()
    {
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

    // TODO: implement
    public override void OnPause(bool isPaused)
    {

    }

    // TODO: implement
    public override void OnUnPause(bool isPaused)
    {

    }
}
