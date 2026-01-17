using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class SettingsApplier : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer mixer;

    [Tooltip("Exposed AudioMixer parameter names")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string sfxParam = "SFXVolume";
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string playerSfxParam = "PlayerSFXVolume";
    [SerializeField] private string uiParam = "UIVolume";
    [SerializeField] private string crowParam = "CrowVolume";

    [Header("Text (optional)")]
    [Tooltip("If you have a global default text size for dialogue, put it here.")]
    [SerializeField] private float defaultDialogueFontSize = 36f;

    private void Awake()
    {
        // This object must persist across all scenes.
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Apply now (covers first boot when PlayerProgress already loaded in its Awake)
        ApplyAll();

        // Re-apply on scene loads in case anything overwrites mixer/text settings per-scene
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Re-apply whenever PlayerProgress signals a load
        if (PlayerProgress.Instance != null)
            PlayerProgress.Instance.OnLoaded += ApplyAll;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (PlayerProgress.Instance != null)
            PlayerProgress.Instance.OnLoaded -= ApplyAll;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyAll();
    }

    private void ApplyAll()
    {
        ApplyAudio();
        ApplyDialogueTextSize();
    }

    private void ApplyAudio()
    {
        if (mixer == null || PlayerProgress.Instance == null) return;

        Apply01ToMixer(masterParam,   PlayerProgress.Instance.GetMasterVol01());
        Apply01ToMixer(sfxParam,      PlayerProgress.Instance.GetSfxVol01());
        Apply01ToMixer(musicParam,    PlayerProgress.Instance.GetMusicVol01());
        Apply01ToMixer(playerSfxParam,PlayerProgress.Instance.GetPlayerSfxVol01());
        Apply01ToMixer(uiParam,       PlayerProgress.Instance.GetUiVol01());
    }

    private void Apply01ToMixer(string param, float v01)
    {
        if (string.IsNullOrEmpty(param)) return;

        float safe01 = Mathf.Clamp(v01, 0.0001f, 1f);
        float db = Mathf.Log10(safe01) * 20f;
        mixer.SetFloat(param, db);
    }

    private void ApplyDialogueTextSize()
    {
        if (PlayerProgress.Instance == null) return;

        float size = PlayerProgress.Instance.GetDialogueFontSize();
        if (size <= 0) size = defaultDialogueFontSize;

        // If you want this to affect ALL dialogue text, tag those objects "DialogueText"
        // and make sure they have TextMeshProUGUI.
        var texts = GameObject.FindGameObjectsWithTag("DialogueText");
        foreach (var go in texts)
        {
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.fontSize = size;
        }
    }

    public void ApplyAudioPublic(bool snapshot, Dictionary<string, float> settings=null)
    {
        if (!snapshot)
        {
            if (mixer == null || PlayerProgress.Instance == null) return;

            Apply01ToMixer(masterParam, PlayerProgress.Instance.GetMasterVol01());
            Apply01ToMixer(sfxParam, PlayerProgress.Instance.GetSfxVol01());
            Apply01ToMixer(musicParam, PlayerProgress.Instance.GetMusicVol01());
            Apply01ToMixer(playerSfxParam, PlayerProgress.Instance.GetPlayerSfxVol01());
            Apply01ToMixer(uiParam, PlayerProgress.Instance.GetUiVol01());
        }
        else
        {
            Apply01ToMixer(masterParam, settings["master"]);
            Apply01ToMixer(sfxParam, settings["sfx"]);
            Apply01ToMixer(musicParam, settings["music"]);
            Apply01ToMixer(playerSfxParam, settings["playersfx"]);
            Apply01ToMixer(uiParam, settings["ui"]);
            Apply01ToMixer(crowParam, settings["crow"]);
        }
    }
}
