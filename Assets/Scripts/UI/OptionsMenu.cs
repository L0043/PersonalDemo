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

    // Start is called before the first frame update
    void Start()
    {
        _fovSlider.onValueChanged.AddListener(ChangeFOV);
        _sensitivitySlider.onValueChanged.AddListener(ChangeSensitivity);

        _screenModeDropdown.onValueChanged.AddListener(ChangeWindowMode);
        _graphicsQualityDropdown.onValueChanged.AddListener(ChangeGraphicsQuality);
        _fpsLimitDropdown.onValueChanged.AddListener(ChangeFPS);
        _resolutionDropdown.onValueChanged.AddListener(ChangeResolution);

    }

    void UpdatePlayerPrefs() 
    {
        PlayerPrefs.Save();
        //EventManager.PlayerPrefsUpdated.Invoke();
    }

    void ChangeFOV(float value)
    {
        PlayerPrefs.SetFloat("FOV", value);
        _fovValueText.text = value.ToString("0");
        UpdatePlayerPrefs();
    }

    void ChangeSensitivity(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
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
        PlayerPrefs.SetInt("FPS", value);
        UpdatePlayerPrefs();
    }

    void ChangeResolution(int value)
    {
        PlayerPrefs.SetInt("Resolution", value);
        UpdatePlayerPrefs();
    }
}
