using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BackgroundAudio))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Make it a singleton

    public BackgroundAudio backgroundAudio;
    private AudioMixerScript audioMixerScript;
    public int numQuestNPCs = 3;
    private string[] questNPCs = new string[3];
    [HideInInspector] public int NPCQuestsSolved;
    private bool allQuestsSolved;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject); // this manager should persist between scenes


        audioMixerScript = GetComponent<AudioMixerScript>();
        
        backgroundAudio = GetComponent<BackgroundAudio>();
        
        HandleAudioMixerGroupRouting();

        // Subscribe to custom event
        CustomEvents.OnCombatEncounterCleared.AddListener(OnCombatEncounterCleared);

        InitializeQuestNPCsList();
        NPCQuestsSolved = 0;
        allQuestsSolved = false;
    }

    void Start() {
        // Start background music
        backgroundAudio.StartBackgroundMusic();
        backgroundAudio.StartAmbience();
        if (PlayerProgress.Instance != null)
        {
            ApplySavedStateFromProgress();
        }
        // backgroundAudio.PlayRandomBreaths();
        // backgroundAudio.PlayRandomFrogs();
    }

    void Update()
    {
        if (allQuestsSolved == false && NPCQuestsSolved == 3)
        {
            allQuestsSolved = true;
        }       
    }

    private void InitializeQuestNPCsList()
    {
        questNPCs[0] = "NPC_Ghostboy"; 
        questNPCs[1] = "NPC_Captain";
        questNPCs[2] = "NPC_Mountaineer";
    }

    void OnDestroy()
    {
        // Remove listener on destroy to prevent memory leaks
        CustomEvents.OnCombatEncounterCleared.RemoveListener(OnCombatEncounterCleared);
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
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicMountain.Source);
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicDungeonA.Source);
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicDungeonB.Source);
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicDungeonC.Source);
        audioMixerScript.assignMUSGroup(backgroundAudio.AudioData.BackgroundMusicMainMenu.Source);
    }

    private void ApplySavedStateFromProgress()
    {
        foreach (string name in questNPCs)
        {
            if (PlayerProgress.Instance.GetNPCStatus(name) == "MelodySolved") NPCQuestsSolved += 1;
        }

        if (NPCQuestsSolved == 3) allQuestsSolved = true;
    }
}
