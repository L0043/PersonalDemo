using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [SerializeField] CanvasGroup _textPanel;
    [SerializeField] TMP_Text _text;

    public float FadeDelay = 4f;
    float _textFadeTime = 2f;
    float _alphaDecrement = 0.0f;

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

    public void DisplayText(string text, float duration) 
    {
        _textPanel.alpha = 1f;
        _text.text = text;
        StopAllCoroutines();
        StartCoroutine(HideText(duration));
    }

    IEnumerator HideText(float duration) 
    {
        if (duration > FadeDelay)
            yield return new WaitForSeconds(FadeDelay);

        float startAlpha = _textPanel.alpha;
        float endAlpha = 0f;
        float elapsedTime = 0f;


        while (elapsedTime < duration)
        {
            _textPanel.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _textPanel.alpha = endAlpha;
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
