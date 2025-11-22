using UnityEngine;

[RequireComponent(typeof(BackgroundAudio))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Make it a singleton

    public BackgroundAudio backgroundAudio;
    private AudioMixerScript audioMixerScript;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject); // this manager should persist between scenes


        audioMixerScript = GetComponent<AudioMixerScript>();
        
        backgroundAudio = GetComponent<BackgroundAudio>();
        DontDestroyOnLoad(backgroundAudio); // all music and ambience is controlled by this, it should peresist between scenes
        
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
        audioMixerScript.assignSFXGroup(backgroundAudio.breathsAudioSource);
        audioMixerScript.assignSFXGroup(backgroundAudio.frogsAudioSource);
        audioMixerScript.assignSFXGroup(backgroundAudio.loudBirdsAudioSource);
        audioMixerScript.assignSFXGroup(backgroundAudio.quietBirdsAudioSource);

        // Now assign audio sources to music bus
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusic.Source);
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicUnderworld.Source);
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicMausoleum.Source);
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicForest.Source);
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicBeach.Source);
    }
}
