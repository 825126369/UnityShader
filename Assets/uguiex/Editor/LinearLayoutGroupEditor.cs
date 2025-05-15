using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(LinearLayoutGroup), true)]
    [CanEditMultipleObjects]
    public class LinearLayoutGroupEditor : HorizontalOrVerticalLayoutGroupEditor
    {
        SerializedProperty m_Orientation;
        SerializedProperty m_SplitSetting;
		bool m_showHorizontalSetting = true;
		bool m_showVerticalSetting = true;

		private class OneSplitSetting
		{
			public SerializedProperty Padding;
			public SerializedProperty Spacing;
			public SerializedProperty ChildAlignment;
			public SerializedProperty ChildForceExpandWidth;
			public SerializedProperty ChildForceExpandHeight;
		}
		OneSplitSetting m_HorizontalSetting = new OneSplitSetting();
		OneSplitSetting m_VerticalSetting = new OneSplitSetting();

        protected virtual void OnEnable()
        {
			base.OnEnable();
            m_Orientation = serializedObject.FindProperty("m_Orientation");
            m_SplitSetting = serializedObject.FindProperty("m_SplitSetting");

			OnSplitEnable(m_HorizontalSetting, "Horizontal");
			OnSplitEnable(m_VerticalSetting, "Vertical");
        }

		void OnSplitEnable(OneSplitSetting setting, string which)
		{
            setting.Padding = serializedObject.FindProperty("m_"+which+"Padding");
            setting.Spacing = serializedObject.FindProperty("m_"+which+"Spacing");
            setting.ChildAlignment = serializedObject.FindProperty("m_"+which+"ChildAlignment");
            setting.ChildForceExpandWidth = serializedObject.FindProperty("m_"+which+"ChildForceExpandWidth");
            setting.ChildForceExpandHeight = serializedObject.FindProperty("m_"+which+"ChildForceExpandHeight");
		}

		void OnSplitSetting(OneSplitSetting setting, bool debug=false)
		{
            EditorGUILayout.PropertyField(setting.Padding, new GUIContent("Padding"), true);
            EditorGUILayout.PropertyField(setting.Spacing, new GUIContent("Spacing"), true);
            EditorGUILayout.PropertyField(setting.ChildAlignment, new GUIContent("Child Alignment"), true);

            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, -1, new GUIContent("Child Force Expand"));
            rect.width = Mathf.Max(50, (rect.width - 4) / 3);
            EditorGUIUtility.labelWidth = 50;
            ToggleLeft(rect, setting.ChildForceExpandWidth, new GUIContent("Width"));
            rect.x += rect.width + 2;
            ToggleLeft(rect, setting.ChildForceExpandHeight, new GUIContent("Height"));
			EditorGUIUtility.labelWidth = 0;
		}

        void ToggleLeft(Rect position, SerializedProperty property, GUIContent label)
        {
            bool toggle = property.boolValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
            toggle = EditorGUI.ToggleLeft(position, label, toggle);
			EditorGUI.indentLevel = oldIndent;
            if (EditorGUI.EndChangeCheck())
            {
                property.boolValue = property.hasMultipleDifferentValues ? true : !property.boolValue;
            }
            EditorGUI.showMixedValue = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Orientation, true);
            EditorGUILayout.PropertyField(m_SplitSetting, true);

			if (m_SplitSetting.boolValue)
			{
				m_showHorizontalSetting = EditorGUILayout.Foldout(m_showHorizontalSetting, "Horizontal setting");
				if (m_showHorizontalSetting)
				{
					++EditorGUI.indentLevel;
					OnSplitSetting(m_HorizontalSetting);
					--EditorGUI.indentLevel;
				}
				m_showVerticalSetting = EditorGUILayout.Foldout(m_showVerticalSetting, "Vertical setting");
				if (m_showVerticalSetting)
				{
					++EditorGUI.indentLevel;
					OnSplitSetting(m_VerticalSetting);
					--EditorGUI.indentLevel;
				}
			}
			serializedObject.ApplyModifiedProperties();

			if (!m_SplitSetting.boolValue)
			{
				base.OnInspectorGUI();
			}
        }
    }
}
