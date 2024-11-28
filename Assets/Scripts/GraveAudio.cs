using UnityEngine;

public class GraveAudio : MonoBehaviour
{
    public EnvironmentAudioData audioData;
    public AudioSource graveAudioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGravePushSound()
    {
        var randommGraveSound = audioData.gravePushes[Random.Range(0,audioData.gravePushes.Length)];
        graveAudioSource.clip = randommGraveSound;
        graveAudioSource.Play();
        // var randomGraveSound = audioData.gravePushes[Random.Range(0,audioData.gravePushes.Length)];

        // audioData.randomGraveSounds.audioSource.clip = randomGraveSound;
        // audioData.randomGraveSounds.audioSource.Play();
    }
}
