using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(SubPanel), true)]
	[CanEditMultipleObjects]
    public class SubPanelEditor : Editor
    {
		SerializedProperty m_panelGUID;
		GameObject panel;

        protected virtual void OnEnable()
        {
			m_panelGUID = serializedObject.FindProperty("m_panelGUID");
			panel = ((SubPanel)serializedObject.targetObject).LoadPanel();
        }

        public override void OnInspectorGUI()
        {
			{
				bool bMultiplePanelValue = false;
				foreach (SubPanel subPanel in serializedObject.targetObjects)
				{
					if (subPanel.LoadPanel() != panel)
						bMultiplePanelValue = true;
				}

				EditorGUI.BeginChangeCheck();

				bool oldShowMixedValue = EditorGUI.showMixedValue;
				if (bMultiplePanelValue)
					EditorGUI.showMixedValue = true;

				panel = EditorGUILayout.ObjectField("panel", panel, typeof(GameObject), false) as GameObject;

				EditorGUI.showMixedValue = oldShowMixedValue;

				if (EditorGUI.EndChangeCheck())
				{
					foreach (SubPanel subPanel in serializedObject.targetObjects)
					{
						subPanel.SetPanel(panel);
						EditorUtility.SetDirty(subPanel);
					}
				}
			}
			serializedObject.Update();
			EditorGUILayout.PropertyField(m_panelGUID, true);
			serializedObject.ApplyModifiedProperties();
        }
    }
}
