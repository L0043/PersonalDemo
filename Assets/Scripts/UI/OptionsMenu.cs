using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] Slider _fovSlider;
    [SerializeField] Slider _sensitivitySlider;
    [SerializeField] TMP_Text _fovValueText;
    [SerializeField] TMP_Text _sensitivityValueText;


    [SerializeField] TMP_Dropdown _screenModeDropdown;
    [SerializeField] TMP_Dropdown _graphicsQualityDropdown;
    [SerializeField] TMP_Dropdown _fpsLimitDropdown;
    [SerializeField] TMP_Dropdown _resolutionDropdown;

    int[] _fpsLimits = { -1, 1, 144, 120, 60, 30 };

    // Start is called before the first frame update
    void Start()
    {
        _fovSlider.onValueChanged.AddListener(ChangeFOV);
        _fovValueText.text = _fovSlider.value.ToString("0");
        _sensitivitySlider.onValueChanged.AddListener(ChangeSensitivity);
        _sensitivityValueText.text = _sensitivitySlider.value.ToString();

        _screenModeDropdown.onValueChanged.AddListener(ChangeWindowMode);
        _graphicsQualityDropdown.onValueChanged.AddListener(ChangeGraphicsQuality);
        _fpsLimitDropdown.onValueChanged.AddListener(ChangeFPS);
        _resolutionDropdown.onValueChanged.AddListener(ChangeResolution);

        // set fps values
        _fpsLimits[1] = (int)Screen.currentResolution.refreshRateRatio.value * 2;

        _resolutionDropdown.ClearOptions();
        Resolution[] resolutions = Screen.resolutions;
        List<string> resolutionOptions = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            resolutionOptions.Add(resolutions[i].width + " x " + resolutions[i].height);
        }
        _resolutionDropdown.AddOptions(resolutionOptions);
        _resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", _resolutionDropdown.options.Count - 1);
        _resolutionDropdown.RefreshShownValue();

    }

    void UpdatePlayerPrefs() 
    {
        PlayerPrefs.Save();
        EventManager.PlayerPrefsUpdated.Invoke();
    }

    void ChangeFOV(float value)
    {
        PlayerPrefs.SetFloat("FOV", value);
        _fovValueText.text = value.ToString("0");
        UpdatePlayerPrefs();
    }

    void ChangeSensitivity(float value)
    {
        //value /= 20f; // Normalize the value to a range of 0-1
        PlayerPrefs.SetFloat("Sensitivity", value / 20);
        _sensitivityValueText.text = value.ToString("0");
        UpdatePlayerPrefs();
    }

    void ChangeGraphicsQuality(int value)
    {
        PlayerPrefs.SetInt("GraphicsQuality", value);
        UpdatePlayerPrefs();
    }

    void ChangeWindowMode(int value)
    {
        PlayerPrefs.SetInt("WindowMode", value);
        UpdatePlayerPrefs();
    }
    void ChangeFPS(int value)
    {
        if (value >= _fpsLimits.Length || value < 0)
            value = 0;



        PlayerPrefs.SetInt("FPS", _fpsLimits[value]);
        UpdatePlayerPrefs();
    }

    void ChangeResolution(int value)
    {
        PlayerPrefs.SetInt("Resolution", value);
        UpdatePlayerPrefs();
    }
}
