using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider volumeSlider;

    [Header("Audio")]
    [SerializeField] private AudioMixer mixer;          // assign in inspector or pulled from MenuManager
    [SerializeField] private string mixerParam = "MasterVolume"; // exposed param name

    public enum Channel { Master, SFX, Music, PlayerSFX, UI }
    [SerializeField] private Channel channel;

    private void Awake()
    {
        if (volumeSlider == null) volumeSlider = GetComponent<Slider>();
        if (mixer == null && MenuManager.Instance != null) mixer = MenuManager.Instance.audioMixer;
    }

    private void Start()
    {
        float saved01 = 1f;
        if (PlayerProgress.Instance != null)
        {
            saved01 = channel switch
            {
                Channel.Master    => PlayerProgress.Instance.GetMasterVol01(),
                Channel.SFX       => PlayerProgress.Instance.GetSfxVol01(),
                Channel.Music     => PlayerProgress.Instance.GetMusicVol01(),
                Channel.PlayerSFX => PlayerProgress.Instance.GetPlayerSfxVol01(),
                Channel.UI        => PlayerProgress.Instance.GetUiVol01(),
                _ => 1f
            };
        }

        volumeSlider.SetValueWithoutNotify(saved01);
        ApplyToMixer(saved01);

        volumeSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float v01)
    {
        ApplyToMixer(v01);

        if (PlayerProgress.Instance == null) return;

        switch (channel)
        {
            case Channel.Master:    PlayerProgress.Instance.SetMasterVol01(v01); break;
            case Channel.SFX:       PlayerProgress.Instance.SetSfxVol01(v01); break;
            case Channel.Music:     PlayerProgress.Instance.SetMusicVol01(v01); break;
            case Channel.PlayerSFX: PlayerProgress.Instance.SetPlayerSfxVol01(v01); break;
            case Channel.UI:        PlayerProgress.Instance.SetUiVol01(v01); break;
        }
    }

    private void ApplyToMixer(float v01)
    {
        if (mixer == null) return;

        float safe01 = Mathf.Clamp(v01, 0.0001f, 1f);
        float db = Mathf.Log10(safe01) * 20f;
        mixer.SetFloat(mixerParam, db);
    }
}
