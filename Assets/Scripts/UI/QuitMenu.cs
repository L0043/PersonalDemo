using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitMenu : MonoBehaviour
{
    [SerializeField] Button _yesButton;
    [SerializeField] Button _noButton;
    // Start is called before the first frame update
    void Start()
    {
        _yesButton.onClick.AddListener(() => 
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }) ;
        _noButton.onClick.AddListener(UIManager.Instance.PopPanel);
    }
}
