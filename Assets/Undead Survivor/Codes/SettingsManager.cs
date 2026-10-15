using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    // 预设分辨率列表
    public static readonly Resolution[] Presets = new Resolution[]
    {
        new Resolution { width = 800,  height = 600 },
        new Resolution { width = 1024, height = 768 },
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1366, height = 768 },
        new Resolution { width = 1600, height = 900 },
        new Resolution { width = 1920, height = 1080 },
    };

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LoadSettings();
    }

    void LoadSettings()
    {
        // 音量
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        ApplyBGMVolume(bgm);
        ApplySFXVolume(sfx);

        // 分辨率
        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
        if (resIndex >= 0 && resIndex < Presets.Length)
        {
            ApplyResolution(resIndex);
        }

        // 全屏
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 0) == 1;
        Screen.fullScreen = fullscreen;
    }

    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat("BGMVolume", 0.5f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    }

    public int GetResolutionIndex()
    {
        return PlayerPrefs.GetInt("ResolutionIndex", 5); // 默认1920×1080
    }

    public bool GetFullscreen()
    {
        return PlayerPrefs.GetInt("Fullscreen", 0) == 1;
    }

    public void SetBGMVolume(float vol)
    {
        PlayerPrefs.SetFloat("BGMVolume", vol);
        ApplyBGMVolume(vol);
    }

    public void SetSFXVolume(float vol)
    {
        PlayerPrefs.SetFloat("SFXVolume", vol);
        ApplySFXVolume(vol);
    }

    public void SetResolution(int index)
    {
        PlayerPrefs.SetInt("ResolutionIndex", index);
        ApplyResolution(index);
    }

    public void SetFullscreen(bool fullscreen)
    {
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        Screen.fullScreen = fullscreen;
    }

    public void SaveAll()
    {
        PlayerPrefs.Save();
    }

    void ApplyBGMVolume(float vol)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetBgmVolume(vol);
    }

    void ApplySFXVolume(float vol)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetSfxVolume(vol);
    }

    void ApplyResolution(int index)
    {
        if (index < 0 || index >= Presets.Length) return;
        Resolution res = Presets[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
