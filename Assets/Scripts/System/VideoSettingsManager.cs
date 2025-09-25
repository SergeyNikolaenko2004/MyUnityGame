using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class VideoSettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public TMP_Dropdown aspectRatioDropdown;
    public TMP_Dropdown particleQualityDropdown;

    private Resolution[] allResolutions;
    private Resolution[] filteredResolutions;
    private Vector2[] aspectRatios = {
        new Vector2(4, 3),
        new Vector2(16, 9),
        new Vector2(16, 10),
        new Vector2(21, 9)
    };

    private readonly string[] particleQualityNames = {
        "Низкое",
        "Среднее",
        "Высокое"
    };

    private bool isInitializing = true;
    public bool isDropdownOpen = false;

    void Start()
    {
        allResolutions = Screen.resolutions;
        filteredResolutions = allResolutions;

        InitializeResolutions();
        InitializeAspectRatios();
        InitializeParticleQuality();
        LoadSettings();

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        if (aspectRatioDropdown != null)
        {
            aspectRatioDropdown.onValueChanged.AddListener(OnAspectRatioChanged);
        }

        if (particleQualityDropdown != null)
        {
            particleQualityDropdown.onValueChanged.AddListener(OnParticleQualityChanged);
        }

        isInitializing = false;
    }

    void InitializeParticleQuality()
    {
        if (particleQualityDropdown == null) return;

        particleQualityDropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (var qualityName in particleQualityNames)
        {
            options.Add(qualityName);
        }

        particleQualityDropdown.AddOptions(options);
        particleQualityDropdown.value = 2;
        particleQualityDropdown.RefreshShownValue();
    }

    void InitializeResolutions()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            string option = allResolutions[i].width + " x " + allResolutions[i].height + " @ " +
                           allResolutions[i].refreshRateRatio.value.ToString("F0") + "Hz";
            options.Add(option);

            if (allResolutions[i].width == Screen.currentResolution.width &&
                allResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        filteredResolutions = allResolutions;
    }

    void InitializeAspectRatios()
    {
        if (aspectRatioDropdown != null)
        {
            aspectRatioDropdown.ClearOptions();
            List<string> options = new List<string>();

            foreach (var ratio in aspectRatios)
            {
                options.Add(ratio.x + ":" + ratio.y);
            }

            aspectRatioDropdown.AddOptions(options);
            aspectRatioDropdown.value = 0;
            aspectRatioDropdown.RefreshShownValue();
        }
    }

    void LoadSettings()
    {
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;

        if (resolutionDropdown != null && PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
            if (resolutionIndex < resolutionDropdown.options.Count)
            {
                resolutionDropdown.value = resolutionIndex;
            }
        }

        if (aspectRatioDropdown != null && PlayerPrefs.HasKey("AspectRatioIndex"))
        {
            int aspectRatioIndex = PlayerPrefs.GetInt("AspectRatioIndex");
            if (aspectRatioIndex < aspectRatios.Length)
            {
                aspectRatioDropdown.value = aspectRatioIndex;
                FilterResolutionsByAspectRatio(aspectRatioIndex);
            }
        }

        if (particleQualityDropdown != null && PlayerPrefs.HasKey("ParticleQuality"))
        {
            int particleQuality = PlayerPrefs.GetInt("ParticleQuality");
            if (particleQuality < particleQualityNames.Length)
            {
                particleQualityDropdown.value = particleQuality;
                ApplyParticleQuality(particleQuality);
            }
        }
        else if (particleQualityDropdown != null)
        {
            ApplyParticleQuality(2);
        }
    }

    void SaveSettings()
    {
        if (resolutionDropdown != null)
            PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        if (aspectRatioDropdown != null)
            PlayerPrefs.SetInt("AspectRatioIndex", aspectRatioDropdown.value);
        if (particleQualityDropdown != null)
            PlayerPrefs.SetInt("ParticleQuality", particleQualityDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnParticleQualityChanged(int qualityIndex)
    {
        if (isInitializing) return;

        ApplyParticleQuality(qualityIndex);
        SaveSettings();
    }

    void ApplyParticleQuality(int qualityIndex)
    {
        switch (qualityIndex)
        {
            case 0:
                QualitySettings.particleRaycastBudget = 16;
                QualitySettings.softParticles = false;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.globalTextureMipmapLimit = 0;
                QualitySettings.lodBias = 0.5f;
                break;

            case 1:
                QualitySettings.particleRaycastBudget = 64;
                QualitySettings.softParticles = false;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.globalTextureMipmapLimit = 0;
                QualitySettings.lodBias = 0.8f;
                break;

            case 2:
                QualitySettings.particleRaycastBudget = 256;
                QualitySettings.softParticles = false;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.globalTextureMipmapLimit = 0;
                QualitySettings.lodBias = 1.0f;
                QualitySettings.antiAliasing = 0;
                break;
        }

        QualitySettings.SetQualityLevel(qualityIndex, false);
        ApplyShaderSettings(qualityIndex);
    }

    void ApplyShaderSettings(int qualityIndex)
    {
        Shader.globalMaximumLOD = 1000;

        switch (qualityIndex)
        {
            case 0:
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.skinWeights = SkinWeights.OneBone;
                break;
            case 1:
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.skinWeights = SkinWeights.TwoBones;
                break;
            case 2:
                QualitySettings.realtimeReflectionProbes = true;
                QualitySettings.skinWeights = SkinWeights.FourBones;
                break;
        }
    }

    public void OnResolutionChanged(int index)
    {
        if (isInitializing) return;

        if (index < filteredResolutions.Length)
        {
            Resolution resolution = filteredResolutions[index];
            Screen.SetResolution(
                resolution.width,
                resolution.height,
                fullscreenToggle.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed,
                resolution.refreshRateRatio
            );
            SaveSettings();
        }
    }

    public void OnAspectRatioChanged(int index)
    {
        if (isInitializing) return;

        FilterResolutionsByAspectRatio(index);
        SaveSettings();
    }

    void FilterResolutionsByAspectRatio(int aspectRatioIndex)
    {
        if (resolutionDropdown == null || aspectRatioIndex >= aspectRatios.Length) return;

        Vector2 targetRatio = aspectRatios[aspectRatioIndex];
        float targetAspect = targetRatio.x / targetRatio.y;

        List<Resolution> tempFiltered = new List<Resolution>();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            float aspect = (float)allResolutions[i].width / allResolutions[i].height;

            if (Mathf.Abs(aspect - targetAspect) < 0.05f)
            {
                string option = allResolutions[i].width + " x " + allResolutions[i].height + " @ " +
                               allResolutions[i].refreshRateRatio.value.ToString("F0") + "Hz";
                options.Add(option);
                tempFiltered.Add(allResolutions[i]);

                if (allResolutions[i].width == Screen.currentResolution.width &&
                    allResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = tempFiltered.Count - 1;
                }
            }
        }

        filteredResolutions = tempFiltered.ToArray();

        resolutionDropdown.ClearOptions();

        if (options.Count > 0)
        {
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();

            if (currentResolutionIndex < filteredResolutions.Length)
            {
                Resolution resolution = filteredResolutions[currentResolutionIndex];
                Screen.SetResolution(
                    resolution.width,
                    resolution.height,
                    fullscreenToggle.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed,
                    resolution.refreshRateRatio
                );
            }
        }
        else
        {
            ShowAllResolutions();
        }
    }

    void ShowAllResolutions()
    {
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            string option = allResolutions[i].width + " x " + allResolutions[i].height + " @ " +
                           allResolutions[i].refreshRateRatio.value.ToString("F0") + "Hz";
            options.Add(option);

            if (allResolutions[i].width == Screen.currentResolution.width &&
                allResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        filteredResolutions = allResolutions;
    }

    public void OnFullscreenToggle(bool isFullscreen)
    {
        if (isInitializing) return;

        Screen.fullScreen = isFullscreen;

        if (resolutionDropdown != null && resolutionDropdown.value < filteredResolutions.Length)
        {
            Resolution resolution = filteredResolutions[resolutionDropdown.value];
            Screen.SetResolution(
                resolution.width,
                resolution.height,
                isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed,
                resolution.refreshRateRatio
            );
        }

        SaveSettings();
    }

    public void OnBackButtonClick()
    {
        PauseManager pauseManager = FindFirstObjectByType<PauseManager>();
        if (pauseManager != null)
        {
            pauseManager.CloseVideoSettings();
        }
    }

    public void SetDefaultSettings()
    {
        if (allResolutions.Length == 0) return;

        Resolution defaultResolution = allResolutions[allResolutions.Length - 1];

        Screen.SetResolution(
            defaultResolution.width,
            defaultResolution.height,
            FullScreenMode.FullScreenWindow,
            defaultResolution.refreshRateRatio
        );

        filteredResolutions = allResolutions;

        if (fullscreenToggle != null) fullscreenToggle.isOn = true;
        if (resolutionDropdown != null)
        {
            for (int i = 0; i < allResolutions.Length; i++)
            {
                if (allResolutions[i].width == defaultResolution.width &&
                    allResolutions[i].height == defaultResolution.height)
                {
                    resolutionDropdown.value = i;
                    break;
                }
            }
        }
        if (aspectRatioDropdown != null) aspectRatioDropdown.value = 0;
        if (particleQualityDropdown != null) particleQualityDropdown.value = 2;

        ApplyParticleQuality(2);
        InitializeResolutions();

        SaveSettings();
    }

    public bool IsDropdownOpen()
    {
        return isDropdownOpen;
    }

    public void CloseDropdown()
    {
        isDropdownOpen = false;
    }

}