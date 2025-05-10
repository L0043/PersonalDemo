using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// custom class to hold the text style for the editor script
[Serializable]
public class CustomTextStyle
{
    public FontStyle fontStyle = FontStyle.Normal;
    public int fontSize = 14;
    public Color textColor = Color.black;
    public TextAnchor alignment = TextAnchor.UpperLeft;
    public bool wordWrap = false;
    public bool richText = false;
    public bool stretchWidth = false;
    public bool stretchHeight = false;
    public TextClipping clipping = TextClipping.Overflow;
    public Font font = null;
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
        string startTag = "<color=#" + ColorUtility.ToHtmlStringRGB(TextStyle.textColor) + ">";
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
            TutorialManager.Instance.DisplayText(Text, Duration);
        _hasDisplayed = true;
        //maybe we want to destroy the object at this point, depends on which is more performant
        //Destroy(gameObject);
    }

}
