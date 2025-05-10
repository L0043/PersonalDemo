#if UNITY_EDITOR

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.Text;


[CustomEditor(typeof(TextPopup))]
public class Editor_TextPopup : Editor
{
    private TextPopup _textPopup;
    public GUIStyle _style = new GUIStyle();

    private void OnEnable()
    {
        _textPopup = (TextPopup)target;
    }

    public override void OnInspectorGUI()
    {
        //GUILayout.Label("Text Popup Editor", EditorStyles.boldLabel);
        //base.OnInspectorGUI();
        // show the duration and collider and allow them to be modified
        _textPopup.Duration = EditorGUILayout.FloatField("Duration", _textPopup.Duration);
        _textPopup.Collider = (BoxCollider)EditorGUILayout.ObjectField("Collider", _textPopup.Collider, typeof(BoxCollider), true);


        // a field to edit the text
        GUILayout.Label("Text Style Edior");

        EditorGUI.BeginChangeCheck();

        _textPopup.TextStyle.fontStyle = (FontStyle)GUILayout.Toolbar((int)_textPopup.TextStyle.fontStyle, new string[] { "Normal", "Bold", "Italic", "Bold and Italic" });
        _textPopup.TextStyle.fontSize = EditorGUILayout.IntField("Font Size", _textPopup.TextStyle.fontSize);

        _textPopup.TextStyle.textColor = EditorGUILayout.ColorField("Text Colour", _textPopup.TextStyle.textColor);
        _textPopup.KeyTextColour = EditorGUILayout.ColorField("Key Text Colour", _textPopup.KeyTextColour);

        _textPopup.TextStyle.alignment = (TextAnchor)EditorGUILayout.EnumPopup("Text Alignment", _textPopup.TextStyle.alignment);
        _textPopup.TextStyle.wordWrap = EditorGUILayout.Toggle("Word Wrap", _textPopup.TextStyle.wordWrap);
        _textPopup.TextStyle.richText = EditorGUILayout.Toggle("Rich Text", _textPopup.TextStyle.richText);
        _textPopup.TextStyle.stretchWidth = EditorGUILayout.Toggle("Stretch Width", _textPopup.TextStyle.stretchWidth);
        _textPopup.TextStyle.stretchHeight = EditorGUILayout.Toggle("Stretch Height", _textPopup.TextStyle.stretchHeight);
        _textPopup.TextStyle.clipping = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", _textPopup.TextStyle.clipping);
        _textPopup.TextStyle.font = (Font)EditorGUILayout.ObjectField("Font", _textPopup.TextStyle.font, typeof(Font), true);

        _style.normal.background = Texture2D.linearGrayTexture;
        _style.padding = new RectOffset(15, 15, 15, 15); // set padding to 15 on all sides

        // set the style to the text popup
        _style.fontStyle = _textPopup.TextStyle.fontStyle;
        _style.fontSize = _textPopup.TextStyle.fontSize;
        _style.normal.textColor = _textPopup.TextStyle.textColor;
        _style.alignment = _textPopup.TextStyle.alignment;
        _style.wordWrap = _textPopup.TextStyle.wordWrap;
        _style.richText = _textPopup.TextStyle.richText;
        _style.stretchWidth = _textPopup.TextStyle.stretchWidth;
        _style.stretchHeight = _textPopup.TextStyle.stretchHeight;
        _style.clipping = _textPopup.TextStyle.clipping;
        _style.font = _textPopup.TextStyle.font;

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_textPopup);
            PrefabUtility.RecordPrefabInstancePropertyModifications(_textPopup);
        }


        GUILayout.Space(20);
        //increase label font size and bolden
        GUIStyle labelOption = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        EditorGUI.BeginChangeCheck();

        GUILayout.Label("Text Field: to mark key words, put a - on either side of the text", labelOption);
        GUILayoutOption[] options = { GUILayout.ExpandWidth(true), GUILayout.MaxHeight(400) };
        _textPopup.Text = EditorGUILayout.TextArea(_textPopup.Text, options);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_textPopup);
            PrefabUtility.RecordPrefabInstancePropertyModifications(_textPopup);
        }

        /*
         string color1 = "<color#=" + ColorUtility.ToHtmlStringRGB(_textPopup.KeyTextColour) + ">";
            string color2 = "</color>";
            string newText = _textPopup.Text;
            newText.Remove(index1, 1);
            newText.Insert(index1, color1);
            
            _textPopup.Text = newText;
         */

        // find every pair of '-' in the text and replace it with the color
        if (_textPopup.Text.Contains('-')) 
        {
            var texts = _textPopup.Text.Split('-');
            string newText = "";
            if(texts.Length >= 3) 
            {
                for(int i = 0; i < texts.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        if(i != texts.Length -1)
                        texts[i] += "<color=#" + ColorUtility.ToHtmlStringRGB(_textPopup.KeyTextColour) + ">";
                    }
                    else 
                    {
                        texts[i] += "</color>";
                    }
                    newText += texts[i];
                }
                _textPopup.Text = newText;
            }
        }
        




        if (GUILayout.Button("Reset"))
        {
            _textPopup.Text = "";
        }
        EditorUtility.SetDirty(_textPopup);
        PrefabUtility.RecordPrefabInstancePropertyModifications(_textPopup);
    }
}
#endif