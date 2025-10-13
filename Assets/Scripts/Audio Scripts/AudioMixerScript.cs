using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerScript : MonoBehaviour
{
    public static AudioMixerScript Instance;
    public AudioMixerGroup SFXGroup;
    public AudioMixerGroup MUSGroup;
    public AudioMixerGroup PlayerSFXGroup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void assignSFXGroup(AudioSource audioSource)
    {
        audioSource.outputAudioMixerGroup = SFXGroup;
    }
    public void assignMUSGroup(AudioSource audioSource)
    {
        audioSource.outputAudioMixerGroup = MUSGroup;
    }
    public void assignPlayerSFXGroup(AudioSource audioSource)
    {
        audioSource.outputAudioMixerGroup = PlayerSFXGroup;
    }

    // There is surely a way to loop through everything in each script that inherits from the AudiController class
    // and update the mixer groups dynamically instead of handling it manually, but that can be improved next game
}
