using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputBinding;
// custom class to hold the text style for the editor script
[Serializable]
public class CustomTextStyle
{
    public FontStyle FontStyle = FontStyle.Normal;
    public int FontSize = 14;
    public Color TextColor = Color.black;
    public TextAlignmentOptions Alignment = TextAlignmentOptions.TopLeft;
    public bool WordWrap = false;
    public bool RichText = false;
    public bool StretchWidth = false;
    public bool StretchHeight = false;
    public TextClipping Clipping = TextClipping.Overflow;
    public Font Font = null;
}


[RequireComponent(typeof(BoxCollider))]
public class TextPopup : MonoBehaviour
{
    public string Text;
    public float Duration = 2f;
    bool _hasDisplayed = false;
    public BoxCollider Collider;
    public Color KeyTextColour = new Color();
    public CustomTextStyle TextStyle = new CustomTextStyle();

    // TODO: Add a way to have the input actions display their current bindings in the text

    // Start is called before the first frame update
    void Start()
    {
        if(!Collider)
            Collider = GetComponent<BoxCollider>();

        Collider.isTrigger = true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        string startTag = "<color=#" + UnityEngine.ColorUtility.ToHtmlStringRGB(TextStyle.TextColor) + ">";
        string endTag = "</color>";

        if (!Text.StartsWith(startTag))
        {
            Text = startTag + Text;
        }

        if (!Text.EndsWith(endTag))
        {
            Text += endTag;
        }
    }
#endif

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != GameManager.Instance.Player)
            return;
        if(!_hasDisplayed)
            TutorialManager.Instance.DisplayText(Text, Duration, TextStyle);
        _hasDisplayed = true;
        //maybe we want to destroy the object at this point, depends on which is more performant
        //Destroy(gameObject);
    }

}
