using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(InlineText))]
public class InlineTextInspector : Editor
{
    //private TextAsset asset = null;
    //public InlineTextInspector(): base("TextEditor"){ }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InlineText text = target as InlineText;
        text.emojiContent = EditorGUILayout.ObjectField("EmojiAsset", text.emojiContent, typeof(TextAsset), true) as TextAsset;
        //text.EmojiContent = asset;
    }

}