using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    int _openPanels = 0;
    public static UIManager Instance { get; private set; }

    [Header("Pause Menu Objects")]
        [SerializeField] GameObject _pauseMenu;
        [SerializeField] GameObject _optionsMenu;
        [SerializeField] GameObject _instructionsMenu;
        [SerializeField] GameObject _quitMenu;
    [Space]
    [Header("HUD")]
    [SerializeField] Image _dashIcon;
    bool _isDashOnCooldown = false;
    float _dashFillTime = 0f;
    [SerializeField] Image _teleportIcon;
    bool _isTeleportOnCooldown = false;
    float _teleportFillTime = 0f;



    PlayerInput _playerInput;
    Controls _controls;

    Stack<GameObject> _openPanelsStack = new Stack<GameObject>();

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        _playerInput = GameManager.Instance.Player.GetComponent<PlayerInput>();
        _controls = GameManager.Instance.Player.GetComponent<Controls>();

        if (!_playerInput)
            return;
        _playerInput.actions["Pause"].performed += ctx => TogglePauseMenu();
        _playerInput.actions["Cancel"].performed += ctx => TogglePauseMenu();

        EventManager.OnDash.AddListener(RefreshDashIcon);
        EventManager.OnTeleport.AddListener(RefreshTeleportIcon);

    }

    private void Update()
    {
        if (_isDashOnCooldown) 
        {
            if (_dashFillTime >= _controls.DashCooldown)
            {
                _isDashOnCooldown = false;
                _dashIcon.fillAmount = 1f;
            }
            else 
            {
                _dashFillTime += Time.deltaTime;
                _dashIcon.fillAmount = _dashFillTime / _controls.DashCooldown;
            }
        }

        if (_isTeleportOnCooldown) 
        {
            if (_teleportFillTime >= _controls.TeleportCooldown)
            {
                _isTeleportOnCooldown = false;
                _teleportIcon.fillAmount = 1f;
            }
            else
            {
                _teleportFillTime += Time.deltaTime;
                _teleportIcon.fillAmount = _teleportFillTime / _controls.TeleportCooldown;
            }
        }
    }

    void RefreshDashIcon() 
    {
        _dashIcon.fillAmount = 0f;
        _dashFillTime = 0f;
        _isDashOnCooldown = true;
    }

    void RefreshTeleportIcon()
    {
        _teleportIcon.fillAmount = 0f;
        _teleportFillTime = 0f;
        _isTeleportOnCooldown = true;
    }

    public void TogglePauseMenu() 
    {
        // if the pause menu is closed, open it and change the input map to UI
        if (!_pauseMenu.activeSelf && _openPanels <= 0)
        {
            ++_openPanels;
            _playerInput.SwitchCurrentActionMap("UI");
            GameManager.Instance.TogglePause();
            _openPanelsStack.Push(_pauseMenu);
            _pauseMenu.SetActive(true);
        }
        // if the pause menu is open, close it and change the input map to player unless there is another panel open
        else
        {
            if (_openPanels <= 1)
            {
                --_openPanels;
                _playerInput.SwitchCurrentActionMap("Player");
                GameManager.Instance.TogglePause();
                _openPanelsStack.Clear();
                _openPanels = 0;
                _pauseMenu.SetActive(false);
            }
            else 
            {
                // close the open panel above the pause menu
                var go = _openPanelsStack.Pop();
                if(go)
                    go.SetActive(false);
                --_openPanels;

                if(_openPanels == 1)
                    _pauseMenu.SetActive(true);
            }
        }

    }

    public void ToggleInstructions() 
    {
        if (!_instructionsMenu.activeSelf)
        {
            ++_openPanels;
            _openPanelsStack.Push(_instructionsMenu);
            _instructionsMenu.SetActive(true);
            _pauseMenu.SetActive(false);
        }
        else
        {
            --_openPanels;
            if (_openPanelsStack.Peek() == _instructionsMenu)
                _openPanelsStack.Pop();
            _instructionsMenu.SetActive(true);
        }
    }

    public void ToggleOptions()
    {
        if (!_optionsMenu.activeSelf)
        {
            ++_openPanels;
            _openPanelsStack.Push(_optionsMenu);
            _optionsMenu.SetActive(true);
            _pauseMenu.SetActive(false);
        }
        else 
        { 
            --_openPanels;
            if (_openPanelsStack.Peek() == _optionsMenu)
                _openPanelsStack.Pop();
            _optionsMenu.SetActive(false);
        }

    }

    public void ToggleQuit() 
    {
        if (!_quitMenu.activeSelf) 
        {
            ++_openPanels;
            _openPanelsStack.Push(_quitMenu);
            _quitMenu.SetActive(true);
            _pauseMenu.SetActive(false);
        }
        else 
        {
            --_openPanels;
            if (_openPanelsStack.Peek() == _quitMenu)
                _openPanelsStack.Pop();
            _quitMenu.SetActive(false);
        }
    }

    //disable the current panel, go into the panel stack and turn on the next one in the list
    public void PopPanel() 
    {
        --_openPanels;
        var go = _openPanelsStack.Pop();
        if (go)
            go.SetActive(false);
        var go2 = _openPanelsStack.Peek();
        if (go2)
            go2.SetActive(true);
    }

    // this shouldnt be needed as every panel will be blocking the other buttons from being pressed
    // the only way to open a new panel will be to close the active one and open the pause menu
    //void DisableOtherPanels(GameObject activePanel) 
    //{
    //    foreach (var panel in _openPanelsStack)
    //    {
    //        if (panel == activePanel)
    //            panel.SetActive(true);
    //        else
    //            panel.SetActive(false);
    //    }
    //}

    // technically I could use this, but what would the point be for a project of this size?
    //void TogglePanel(GameObject panel) 
    //{
    //    if (!panel.activeSelf)
    //    {
    //        ++_openPanels;
    //        _openPanelsStack.Push(panel);
    //        panel.SetActive(true);
    //    }
    //    else
    //    {
    //        --_openPanels;
    //        if (_openPanelsStack.Peek() == panel)
    //            _openPanelsStack.Pop();
    //        panel.SetActive(false);
    //    }
    //}

    

}
