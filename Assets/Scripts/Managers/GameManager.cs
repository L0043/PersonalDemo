using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;


public class GameManager : MonoBehaviour
{
    
    public InputDevice ActiveInputDevice { get; private set; }
    PlayerInput _playerInput;
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
            _playerInput = Player.GetComponent<PlayerInput>();
        }

        EventManager.PlayerPrefsUpdated.AddListener(UpdatePlayerPrefs);
        UpdatePlayerPrefs();
        //_playerInput.controlsChangedEvent.AddListener(UpdateActiveDevice);
        UpdateActiveDevice(_playerInput);
        
    }

    void UpdateActiveDevice(PlayerInput playerInput) 
    {
        if(playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            ActiveInputDevice = Keyboard.current;
        }
        else if (playerInput.currentControlScheme == "Gamepad")
        {
            ActiveInputDevice = Gamepad.current;
        }
        else
        {
            Debug.LogError("Unknown control scheme.");
        }
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

    public void ResetPlayerPosition() 
    {
        _playerControls.ResetPosition();
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
        // unfortunately, unity's input system does not appear to have an event
        // that can inform me when the device has changed, unless I destroy my performance.
        // because of this, I will be perpetually checking if the active device has changed
    }

    public PlayerInput GetPlayerInput() 
    {
        return _playerInput;
    }
}
