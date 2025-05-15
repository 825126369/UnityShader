using UnityEngine;
using UnityEditor;
using System.Reflection;

/// <summary>
/// Editor class used to edit UI Graphics.
/// Extend this class to write your own graphic editor.
/// </summary>

[CustomEditor(typeof(CustomerTextMesh), true)]
[CanEditMultipleObjects]
public class CustomerTextMeshEditor : Editor
{
    CustomerTextMesh mCustomerTextMesh = null;

    SerializedProperty m_Text;
    SerializedProperty m_Color;
    SerializedProperty m_Font;

    SerializedProperty mTextAlignment;
    SerializedProperty m_OffsetY;

    SerializedProperty m_CharacterSize;
    SerializedProperty m_AutoSize;

    SerializedProperty m_AutoSizeMaxWidth;
    SerializedProperty m_AutoSizeMaxSize;

    protected void OnEnable()
    {
        mCustomerTextMesh = target as CustomerTextMesh;
        
        m_Text = serializedObject.FindProperty("m_Text");
        m_Color = serializedObject.FindProperty("m_Color");
        m_Font = serializedObject.FindProperty("m_Font");
        mTextAlignment = serializedObject.FindProperty("mTextAlignment");
        m_OffsetY = serializedObject.FindProperty("m_OffsetY");
        m_CharacterSize = serializedObject.FindProperty("m_CharacterSize");
        m_AutoSize = serializedObject.FindProperty("m_AutoSize");
        m_AutoSizeMaxWidth = serializedObject.FindProperty("m_AutoSizeMaxWidth");
        m_AutoSizeMaxSize = serializedObject.FindProperty("m_AutoSizeMaxSize");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawInspectorGUI();
        serializedObject.ApplyModifiedProperties();
        
        if (GUI.changed)
        {
            mCustomerTextMesh.GetType().InvokeMember("EditorInit", BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, mCustomerTextMesh, new object[] { });
            GUI.changed = false;
        }
    }

    void DrawInspectorGUI()
    {
        m_Text.stringValue = EditorGUILayout.TextField("Text", m_Text.stringValue);
        m_Color.colorValue = EditorGUILayout.ColorField("Color", m_Color.colorValue);
        EditorGUILayout.PropertyField(m_Font);
        EditorGUILayout.PropertyField(mTextAlignment);
        EditorGUILayout.PropertyField(m_OffsetY);
        EditorGUILayout.PropertyField(m_CharacterSize);
        m_AutoSize.boolValue = EditorGUILayout.Toggle("Auto Size", m_AutoSize.boolValue);

        if (m_AutoSize.boolValue)
        {
            EditorGUILayout.PropertyField(m_AutoSizeMaxWidth);
            EditorGUILayout.PropertyField(m_AutoSizeMaxSize);
        }
    }

}
