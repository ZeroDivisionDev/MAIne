using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    public TextMeshProUGUI masterText;
    public Slider masterSlider;

    public TextMeshProUGUI musicText;
    public Slider musicSlider;

    public TextMeshProUGUI sensitivityText;
    public Slider sensitivitySlider;

    private void OnEnable()
    {
        float masterVolume = MainGameManager.instance.settings.masterVolume;
        MainGameManager.instance.audioMixer.SetFloat("Master Volume", masterVolume);
        masterVolume = Mathf.Pow(10, masterVolume / 20f);
        masterText.text = "Master Volume : " + Mathf.RoundToInt(masterVolume * 33.33f) + "%";
        masterSlider.value = masterVolume;

        float musicVolume = MainGameManager.instance.settings.musicVolume;
        MainGameManager.instance.audioMixer.SetFloat("Music Volume", musicVolume);
        musicVolume = Mathf.Pow(10, musicVolume / 20f);
        musicText.text = "Music Volume : " + Mathf.RoundToInt(musicVolume * 33.33f) + "%";
        musicSlider.value = musicVolume;

        sensitivityText.text = "Mouse sensitivity : " + Mathf.RoundToInt((MainGameManager.instance.settings.mouseSensitivity-0.01f) * 500f) + "%";
        sensitivitySlider.value = MainGameManager.instance.settings.mouseSensitivity;

    }

    private void OnDisable()
    {
        MainGameManager.instance.SaveSettings();
    }

    public void SetMasterVolume(float volume)
    {
        MainGameManager.instance.settings.masterVolume = Mathf.Log10(volume) * 20;
        MainGameManager.instance.audioMixer.SetFloat("Master Volume", Mathf.Log10(volume) * 20);
        masterText.text = "Master Volume : " + Mathf.RoundToInt(volume*33.33f) + "%";
    }

    public void SetMusicVolume(float volume)
    {
        MainGameManager.instance.settings.musicVolume = Mathf.Log10(volume) * 20;
        MainGameManager.instance.audioMixer.SetFloat("Music Volume", Mathf.Log10(volume)*20);
        musicText.text = "Music Volume : " + Mathf.RoundToInt(volume * 33.33f) + "%";
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        MainGameManager.instance.settings.mouseSensitivity = sensitivity;
        if(PlayerController.instance != null)
        {
            PlayerController.instance.mouseSensitivity = sensitivity;
        }
        sensitivityText.text = "Mouse sensitivity : " + Mathf.RoundToInt((MainGameManager.instance.settings.mouseSensitivity - 0.01f) * 500f) + "%";
    }
}
