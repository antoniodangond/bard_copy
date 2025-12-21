using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ResetSettingsButton : MonoBehaviour
{
    [Header("Defaults")]
    [Range(0f, 1f)] [SerializeField] private float defaultMasterVol01 = 1f;
    [Range(0f, 1f)] [SerializeField] private float defaultSfxVol01 = 1f;
    [Range(0f, 1f)] [SerializeField] private float defaultMusicVol01 = 1f;
    [Range(0f, 1f)] [SerializeField] private float defaultPlayerSfxVol01 = 1f;
    [Range(0f, 1f)] [SerializeField] private float defaultUiVol01 = 1f;

    [SerializeField] private float defaultDialogueFontSize = 36f;

    [Header("Sliders (for instant visual feedback)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider playerSfxSlider;
    [SerializeField] private Slider uiSlider;
    [SerializeField] private Slider textSizeSlider;

    [Header("Optional: apply immediately to mixer too")]
    [SerializeField] private AudioMixer mixer; // assign or leave null if SettingsApplier always handles it

    [Tooltip("Exposed AudioMixer parameter names")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string sfxParam = "SFXVolume";
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string playerSfxParam = "PlayerSFXVolume";
    [SerializeField] private string uiParam = "UIVolume";

    private Button resetToDefault;

    /// <summary>
    /// Hook this up to your Options Menu "Reset to Defaults" button OnClick().
    /// </summary>
     
    void Start()
    {
        resetToDefault = gameObject.GetComponent<Button>();
        resetToDefault.onClick.AddListener(ResetToDefaults);
    }
     
    public void ResetToDefaults()
    {
        // 1) Move sliders immediately without triggering their onValueChanged listeners
        if (masterSlider != null)     masterSlider.SetValueWithoutNotify(defaultMasterVol01);
        if (sfxSlider != null)        sfxSlider.SetValueWithoutNotify(defaultSfxVol01);
        if (musicSlider != null)      musicSlider.SetValueWithoutNotify(defaultMusicVol01);
        if (playerSfxSlider != null)  playerSfxSlider.SetValueWithoutNotify(defaultPlayerSfxVol01);
        if (uiSlider != null)         uiSlider.SetValueWithoutNotify(defaultUiVol01);
        if (textSizeSlider != null)   textSizeSlider.SetValueWithoutNotify(defaultDialogueFontSize);

        // 2) Persist defaults into PlayerProgress (this saves to disk via your setters)
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.SetMasterVol01(defaultMasterVol01);
            PlayerProgress.Instance.SetSfxVol01(defaultSfxVol01);
            PlayerProgress.Instance.SetMusicVol01(defaultMusicVol01);
            PlayerProgress.Instance.SetPlayerSfxVol01(defaultPlayerSfxVol01);
            PlayerProgress.Instance.SetUiVol01(defaultUiVol01);
            PlayerProgress.Instance.SetDialogueFontSize(defaultDialogueFontSize);

            // Nudge listeners (SettingsApplier, DialogueManager, etc.) to re-apply defaults immediately
            PlayerProgress.Instance.RaiseLoaded();
        }

        // 3) Optional: apply immediately to mixer as well (useful if SettingsApplier isn't subscribed)
        ApplyToMixerIfAssigned();
    }

    private void ApplyToMixerIfAssigned()
    {
        if (mixer == null) return;

        SetMixerDb(masterParam, defaultMasterVol01);
        SetMixerDb(sfxParam, defaultSfxVol01);
        SetMixerDb(musicParam, defaultMusicVol01);
        SetMixerDb(playerSfxParam, defaultPlayerSfxVol01);
        SetMixerDb(uiParam, defaultUiVol01);
    }

    private void SetMixerDb(string param, float v01)
    {
        if (string.IsNullOrEmpty(param)) return;

        // Avoid log(0)
        float safe01 = Mathf.Clamp(v01, 0.0001f, 1f);
        float db = Mathf.Log10(safe01) * 20f;

        // Fail silently if the param doesn't exist/exposed
        if (!mixer.GetFloat(param, out _))
        {
            Debug.LogWarning($"[ResetSettingsButton] Missing exposed AudioMixer param: {param}");
            return;
        }

        mixer.SetFloat(param, db);
    }

}
