using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public Slider volumeSlider;
    public string mixerGroupToControl;
    private float volume;
    private float currentVolume;
    // private AudioMixerGroup groupToControl;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volumeSlider = gameObject.GetComponent<Slider>();
        // groupToControl = MenuManager.Instance.audioMixer.FindMatchingGroups(mixerGroupToControl)[0];
        volumeSlider.onValueChanged.AddListener(delegate { SliderChanged(); });
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SliderChanged()
    {
        UpdateVolume();
    }

    void UpdateVolume()
    {
        float currentVolume;
        if (MenuManager.Instance.audioMixer.GetFloat(mixerGroupToControl, out currentVolume))
        {
            volume = (float)Mathf.Log10(volumeSlider.value)*20;
            MenuManager.Instance.audioMixer.SetFloat(mixerGroupToControl, volume);
            // Debug.Log((float)Mathf.Log10(volumeSlider.value)*20);
        }
        else { Debug.Log($"{mixerGroupToControl} doens't exist!"); }
        
    }
}
