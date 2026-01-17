using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(BackgroundAudio))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Make it a singleton

    public BackgroundAudio backgroundAudio;
    private AudioMixerScript audioMixerScript;
    public AudioMixer mixer;
    [Tooltip("Exposed AudioMixer parameter names")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string sfxParam = "SFXVolume";
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string playerSfxParam = "PlayerSFXVolume";
    [SerializeField] private string uiParam = "UIVolume";
    [SerializeField] private string crowParam = "CrowVolume";
    private Dictionary<string, float> defaultAudioValues = new Dictionary<string, float> { };
    [SerializeField] private int numQuestNPCs = 3;
    private string[] questNPCs = new string[] { "NPC_Ghostboy", "NPC_Captain", "NPC_Mountaineer"};
    [HideInInspector] public int NPCQuestsSolved;
    private bool allQuestsSolved;
    private int numOfStatuePieces = 9;
    private string[] statuePieces = new string[] { "LeftHead", "RightHead", "MiddleHead", "LeftArm", "RightArm", "LeftLeg", "RightArm", "Torso", "Tail" };
    [HideInInspector] public int collectedStatuePieces;
    [HideInInspector] public bool allStatuePiecesCollected;
    public EnemyVoiceCountManager EnemyVoices = new EnemyVoiceCountManager();

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

        ResetGameProgress();
    }

    void Start() {
        // Start background music
        backgroundAudio.StartBackgroundMusic();
        backgroundAudio.StartAmbience();
        if (PlayerProgress.Instance != null)
        {
            ApplySavedStateFromProgress();
            Debug.Log(collectedStatuePieces + " pieces collected");
        }
        // backgroundAudio.PlayRandomBreaths();
        // backgroundAudio.PlayRandomFrogs();
    }

    void Update()
    {
        if (allQuestsSolved == false && NPCQuestsSolved == numQuestNPCs)
            allQuestsSolved = true;

        if (allStatuePiecesCollected == false && collectedStatuePieces == numOfStatuePieces)
        {
            allStatuePiecesCollected = true;
            Debug.Log("all statue pieces collected");
        }
    }

    public void ResetGameProgress()
    {
        NPCQuestsSolved = 0;
        allQuestsSolved = false;
        collectedStatuePieces = 0;
        allStatuePiecesCollected = false;
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
            if (PlayerProgress.Instance.GetNPCStatus(name) == "MelodySolved") NPCQuestsSolved += 1;

        Debug.Log(NPCQuestsSolved + " Quests Solved");
        if (NPCQuestsSolved == numQuestNPCs) allQuestsSolved = true;

        // for (int i = 0; i < numOfStatuePieces; i++)
        // {
        //     if (PlayerProgress.Instance.HasCollected(statuePieces[i])) collectedStatuePieces += 1;
        // }
        collectedStatuePieces = PlayerProgress.Instance.GetNumCollectedCollectibles();

        if (collectedStatuePieces == numOfStatuePieces) allStatuePiecesCollected = true;
    }

    public void TriggerSnapshot(bool snapshot)
    {
        if (snapshot)
        {
            string[] exposedParamNames = new string[] { masterParam, sfxParam, musicParam, playerSfxParam, uiParam, crowParam };
            string[] names = new string[] { "master", "sfx", "music", "playersfx", "ui", "crow" };
            float[] values = new float[]  { 1f      , 0.025f, 0.025f, 1f         , 1f  , 1f };
            for (int i = 0; i < names.Length; i++)
            {
                float value;
                if (mixer.GetFloat(exposedParamNames[i], out value))
                { defaultAudioValues[names[i]] = Mathf.Pow(10f, value / 20f); }
            }
            Dictionary<string, float> settings = CreateSnapshotDictionary(names, values);
            ApplyAudio(settings);
        }
        else
        { ApplyAudio(defaultAudioValues); }
    }

    private Dictionary<string, float> CreateSnapshotDictionary(string[] names, float[] values)
    {
        Dictionary<string, float> dict = new Dictionary<string, float> { };
        for (int i = 0; i < names.Length; i++)
        {
            dict[names[i]] = values[i];
        }
        return dict;
    }

    private void Apply01ToMixer(string param, float v01)
    {
        if (string.IsNullOrEmpty(param)) return;

        float safe01 = Mathf.Clamp(v01, 0.0001f, 1f);
        float db = Mathf.Log10(safe01) * 20f;
        mixer.SetFloat(param, db);
    }
    
    private void ApplyAudio(Dictionary<string, float> settings)
    {
        if (mixer == null || PlayerProgress.Instance == null) return;

        Apply01ToMixer(masterParam,   settings["master"]);
        Apply01ToMixer(sfxParam,      settings["sfx"]);
        Apply01ToMixer(musicParam,    settings["music"]);
        Apply01ToMixer(playerSfxParam, settings["playersfx"]);
        Apply01ToMixer(uiParam,       settings["ui"]);
        Apply01ToMixer(crowParam,     settings["crow"]);
    }
}
