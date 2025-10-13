using UnityEngine;

[RequireComponent(typeof(BackgroundAudio))]
public class GameManager : MonoBehaviour
{
    private BackgroundAudio backgroundAudio;
    private AudioMixerScript audioMixerScript;

    void Awake()
    {
        audioMixerScript = GetComponent<AudioMixerScript>();
        backgroundAudio = GetComponent<BackgroundAudio>();
        HandleAudioMixerGroupRouting();

        // Subscribe to custom event
        CustomEvents.OnCombatEncounterCleared.AddListener(OnCombatEncounterCleared);
    }

    void OnDestroy()
    {
        // Remove listener on destroy to prevent memory leaks
        CustomEvents.OnCombatEncounterCleared.RemoveListener(OnCombatEncounterCleared);
    }

    void Start() {
        // Start background music
        backgroundAudio.StartBackgroundMusic();
        backgroundAudio.StartAmbience();
        // backgroundAudio.PlayRandomBreaths();
        // backgroundAudio.PlayRandomFrogs();
    }

    void OnCombatEncounterCleared(GameObject combatEncounter)
    {
        Debug.Log("Combat cleared!");
    }

    void HandleAudioMixerGroupRouting()
    {
        // Assign all SFX tracks, starting with looping ambience
        audioMixerScript.assignSFXGroup(backgroundAudio.AudioData.OverworldAmbience.Source);
        audioMixerScript.assignSFXGroup(backgroundAudio.AudioData.UnderworldAmbience.Source);
        audioMixerScript.assignSFXGroup(backgroundAudio.AudioData.BeachAmbience.Source);
        // Still SFX, but now one shots, not using the "Sound" class
        audioMixerScript.assignSFXGroup(backgroundAudio.AudioData.RandomAmbienceBreaths.audioSource);
        audioMixerScript.assignSFXGroup(backgroundAudio.AudioData.RandomAmbienceFrogs.audioSource);
        audioMixerScript.assignSFXGroup(backgroundAudio.AudioData.RandomAmbienceLoudBirds.audioSource);
        audioMixerScript.assignSFXGroup(backgroundAudio.AudioData.RandomAmbienceQuietBirds.audioSource);

        // Now assign audio sources to music bus
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusic.Source);
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicUnderworld.Source);
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicMausoleum.Source);
    }
}
