using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    int _openPanels = 0;
    public static UIManager Instance { get; private set; }

    [SerializeField] GameObject _pauseMenu;
    [SerializeField] GameObject _optionsMenu;
    [SerializeField] GameObject _instructionsMenu;
    [SerializeField] GameObject _quitMenu;
    PlayerInput _playerInput;

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

        if (!_playerInput)
            return;
        _playerInput.actions["Pause"].performed += ctx => TogglePauseMenu();
        _playerInput.actions["Cancel"].performed += ctx => TogglePauseMenu();

    }

    public void TogglePauseMenu() 
    {
        // if the pause menu is closed, open it and change the input map to UI
        if (!_pauseMenu.activeSelf)
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

    // this shouldnt be needed as every panel will be blocking the other buttons from being pressed
    // the only way to open a new panel will be to close the active one and open the pause menu
    void DisableOtherPanels(GameObject activePanel) 
    {
        foreach (var panel in _openPanelsStack)
        {
            if (panel == activePanel)
                panel.SetActive(true);
            else
                panel.SetActive(false);
        }
    }

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
