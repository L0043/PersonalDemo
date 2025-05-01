using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] Button _resumeButton;
    [SerializeField] Button _instructionsButton;
    [SerializeField] Button _optionsButton;
    [SerializeField] Button _quitButton;
    [SerializeField] Button _quitConfirmationButton;
    [SerializeField] GameObject _quitConfirmationPanel;
    // Start is called before the first frame update
    void Start()
    {

        _resumeButton.onClick.AddListener(UIManager.Instance.TogglePauseMenu);     
        _instructionsButton.onClick.AddListener(UIManager.Instance.ToggleInstructions);
        _optionsButton.onClick.AddListener(UIManager.Instance.ToggleOptions);
        _quitButton.onClick.AddListener(UIManager.Instance.ToggleQuit);
    }
}
