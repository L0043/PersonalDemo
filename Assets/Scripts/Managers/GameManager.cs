using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject Player { get; private set; }
    Controls _playerControls;

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if(!Player)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            if (Player == null)
            {
                Debug.LogError("Player not found in the scene.");
            }
            _playerControls = Player.GetComponent<Controls>();
        }

        EventManager.PlayerPrefsUpdated.AddListener(UpdatePlayerPrefs);
        UpdatePlayerPrefs();

    }

    void UpdatePlayerPrefs() 
    {
        Camera.main.fieldOfView = PlayerPrefs.GetFloat("FOV", 70);

        _playerControls.Sensitivity = PlayerPrefs.GetFloat("Sensitivity", 0.5f);

        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("GraphicsQuality", 2));

        int screenMode = PlayerPrefs.GetInt("ScreenMode", 0);
        switch (screenMode)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            default:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }


        int fpsLimit = PlayerPrefs.GetInt("FPSLimit", -1);

        Application.targetFrameRate = fpsLimit;

        int resolutionIndex = PlayerPrefs.GetInt("Resolution", 0);
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        }
        else
        {
            Debug.LogError("Invalid resolution index.");
        }


    }


    public void TogglePause() 
    {
        // if the time scale is 1, set it to 0, otherwise set it to 1
        Time.timeScale = Time.timeScale == 1 ? 0 : 1;
        Cursor.lockState = Time.timeScale == 1 ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = Time.timeScale == 1 ? false : true;

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
