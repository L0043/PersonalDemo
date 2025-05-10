using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    Button _button;


    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button == null)
        {
            Debug.LogError("Button component not found on this GameObject.");
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _button.onClick.AddListener(UIManager.Instance.PopPanel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
