using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("# Panels")]
    public GameObject pausePanel;       // 暂停主面板
    public GameObject settingsPanel;    // 设置面板(暂停中的)

    [Header("# Settings UI")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    bool isPaused;
    bool isInitializing;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Update()
    {
        // 只在游戏进行中响应ESC
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // 如果设置面板开着，先关设置
        if (settingsPanel.activeSelf)
        {
            CloseSettings();
            return;
        }

        if (isPaused)
            OnResume();
        else if (GameManager.instance.isLive)
            OnPause();
    }

    public void OnPause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        GameManager.instance.Stop();
    }

    public void OnResume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        GameManager.instance.Resume();
    }

    public void OpenSettings()
    {
        isInitializing = true;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
        InitSettingsUI();
        isInitializing = false;
    }

    public void CloseSettings()
    {
        SettingsManager.instance.SaveAll();
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void OnClickQuit()
    {
        // 暂停中退出 → 回到主菜单
        // 统一通过 GameManager 管理时间缩放，避免双轨控制
        isPaused = false;
        pausePanel.SetActive(false);
        GameManager.instance.Resume();
        GameManager.instance.BackToCharSelect();
    }

    // ─── Settings 回调 ───

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

    // ─── 初始化UI控件值 ───

    void InitSettingsUI()
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
