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
    protected void OnEnable()
    {
        mCustomerTextMesh = target as CustomerTextMesh;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
        
        if (GUI.changed)
        {
            mCustomerTextMesh.GetType().InvokeMember("EditorInit", BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, mCustomerTextMesh, new object[] { });
            GUI.changed = false;
        }
    }

}
