#if UNITY_EDITOR

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
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
        // show the input action and let users modify it
        //_textPopup.InputAction = (InputActionReference)EditorGUILayout.ObjectField("Input Action", _textPopup.InputAction, typeof(InputActionReference), true);


        // a field to edit the text
        GUILayout.Label("Text Style Edior");

        EditorGUI.BeginChangeCheck();

        _textPopup.TextStyle.InputActionReference = (InputActionReference)EditorGUILayout.ObjectField("Input Action", 
            _textPopup.TextStyle.InputActionReference, typeof(InputActionReference), true);
        // Font Info
        _textPopup.TextStyle.FontStyle = (FontStyle)GUILayout.Toolbar((int)_textPopup.TextStyle.FontStyle, new string[] { "Normal", "Bold", "Italic", "Bold and Italic" });
        _textPopup.TextStyle.FontSize = EditorGUILayout.IntField("Font Size", _textPopup.TextStyle.FontSize);
        // Colors
        _textPopup.TextStyle.TextColor = EditorGUILayout.ColorField("Text Colour", _textPopup.TextStyle.TextColor);
        _textPopup.KeyTextColour = EditorGUILayout.ColorField("Key Text Colour", _textPopup.KeyTextColour);
        // Position and Formatting
        _textPopup.TextStyle.Alignment = (TextAlignmentOptions)EditorGUILayout.EnumPopup("Text Alignment", _textPopup.TextStyle.Alignment);
        _textPopup.TextStyle.WordWrap = EditorGUILayout.Toggle("Word Wrap", _textPopup.TextStyle.WordWrap);
        _textPopup.TextStyle.RichText = EditorGUILayout.Toggle("Rich Text", _textPopup.TextStyle.RichText);
        _textPopup.TextStyle.StretchWidth = EditorGUILayout.Toggle("Stretch Width", _textPopup.TextStyle.StretchWidth);
        _textPopup.TextStyle.StretchHeight = EditorGUILayout.Toggle("Stretch Height", _textPopup.TextStyle.StretchHeight);
        _textPopup.TextStyle.Clipping = (TextClipping)EditorGUILayout.EnumPopup("Text Clipping", _textPopup.TextStyle.Clipping);
        _textPopup.TextStyle.Font = (Font)EditorGUILayout.ObjectField("Font", _textPopup.TextStyle.Font, typeof(Font), true);

        _style.normal.background = Texture2D.linearGrayTexture;
        _style.padding = new RectOffset(15, 15, 15, 15); // set padding to 15 on all sides

        // set the style to the text popup
        _style.fontStyle = _textPopup.TextStyle.FontStyle;
        _style.fontSize = _textPopup.TextStyle.FontSize;
        _style.normal.textColor = _textPopup.TextStyle.TextColor;
        _style.alignment = (TextAnchor)_textPopup.TextStyle.Alignment;
        _style.wordWrap = _textPopup.TextStyle.WordWrap;
        _style.richText = _textPopup.TextStyle.RichText;
        _style.stretchWidth = _textPopup.TextStyle.StretchWidth;
        _style.stretchHeight = _textPopup.TextStyle.StretchHeight;
        _style.clipping = _textPopup.TextStyle.Clipping;
        _style.font = _textPopup.TextStyle.Font;

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

        GUILayout.Label("Text Field: to mark key words, put a - on either side of the text\nTo insert input actions put an * in the text", labelOption);
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