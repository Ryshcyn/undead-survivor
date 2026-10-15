using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettings : MonoBehaviour
{
    [Header("# Settings UI")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("# Panels")]
    public GameObject settingsPanel;
    public GameObject mainMenuPanel;

    bool isInitializing;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        isInitializing = true;
        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        InitUI();
        isInitializing = false;
    }

    public void CloseSettings()
    {
        SettingsManager.instance.SaveAll();
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnBGMVolumeChanged(float val)
    {
        if (isInitializing) return;
        SettingsManager.instance.SetBGMVolume(val);
    }

    public void OnSFXVolumeChanged(float val)
    {
        if (isInitializing) return;
        SettingsManager.instance.SetSFXVolume(val);
    }

    public void OnResolutionChanged(int index)
    {
        if (isInitializing) return;
        SettingsManager.instance.SetResolution(index);
    }

    public void OnFullscreenToggled(bool isOn)
    {
        if (isInitializing) return;
        SettingsManager.instance.SetFullscreen(isOn);
    }

    void InitUI()
    {
        if (bgmSlider != null)
            bgmSlider.value = SettingsManager.instance.GetBGMVolume();
        if (sfxSlider != null)
            sfxSlider.value = SettingsManager.instance.GetSFXVolume();
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = SettingsManager.instance.GetFullscreen();

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>();
            foreach (var res in SettingsManager.Presets)
            {
                options.Add(res.width + " × " + res.height);
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = SettingsManager.instance.GetResolutionIndex();
            resolutionDropdown.RefreshShownValue();
        }
    }
}
