using LuaInterface;
using System;
using UnityAssist;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Sub Panel")]
    public class SubPanel : MonoBehaviour
    {
		[SerializeField]
		private String m_panelGUID;

#if UNITY_EDITOR
		[UnityEditorOnly]
        [NoToLua]
		public GameObject LoadPanel()
		{
			if (m_panelGUID != null)
				return AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(m_panelGUID)) as GameObject;
			else
				return null;
		}

		[UnityEditorOnly]
        [NoToLua]
        public void SetPanel(GameObject panel)
		{
			if (panel != null)
			{
				m_panelGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(panel));
			}
			else
			{
				m_panelGUID = null;
			}
		}

		[ContextMenu("Generate")]
		[UnityEditorOnly]
        [NoToLua]
        public void Generate()
		{
			ClearInternal();

			m_generated = true;

			GameObject panel = LoadPanel();

			if (panel != null)
			{
				m_panelInstance = GameObject.Instantiate(panel);
				m_panelInstance.name = panel.name.ToLower();
				m_panelInstance.transform.SetParent(gameObject.transform, false);
			}
		}
#endif

		[ContextMenu("Clear")]
		public void Clear()
		{
			ClearInternal();
			m_generated = false;
		}

		private void ClearInternal()
		{
			if (m_panelInstance != null)
			{
				GameObject.DestroyImmediate(m_panelInstance);
			}
			m_panelInstance = null;
		}

		private bool m_generated = false;
		private GameObject m_panelInstance;
   }
}
