using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EmojiUI
{
    [CustomEditor(typeof(EmojiManager),true)]
    [CanEditMultipleObjects]
    public class EmojiManagerEditor : Editor
    {
        SerializedProperty m_Script;
    
        private bool foldout =true;

        private Dictionary<string, EmojiSpriteAsset> assetDic = new Dictionary<string, EmojiSpriteAsset>();

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
        }

        public override void OnInspectorGUI()
        {
            EmojiManager manager = target as EmojiManager;
            EditorGUILayout.PropertyField(m_Script);

            EditorGUILayout.Space();
            serializedObject.Update();

            //manager.OpenDebug =EditorGUILayout.Toggle("Debug", manager.OpenDebug);
            manager.RenderType = (EmojiRenderType)EditorGUILayout.EnumPopup("Rendetype", manager.RenderType);
            manager.AnimationSpeed = EditorGUILayout.Slider("AnimationSpeed", manager.AnimationSpeed, 0, 100);

            foldout = EditorGUILayout.Foldout(foldout, "prepared:"+manager.PreparedAtlas.Count);
            if(foldout)
            {
                EditorGUI.indentLevel++;
                for (int i =0; i < manager.PreparedAtlas.Count;++i)
                {
                    EmojiSpriteAsset asset = manager.PreparedAtlas[i];
                    EmojiSpriteAsset newasset = (EmojiSpriteAsset)EditorGUILayout.ObjectField(i.ToString(), asset, typeof(EmojiSpriteAsset),false);
                    if(newasset != asset)
                    {
                        if(newasset == null)
                        {
                            manager.PreparedAtlas[i] = asset;
                        }
                        else
                        {
                            manager.PreparedAtlas[i] = newasset;
                        }
                        EditorUtility.SetDirty(manager);
                    }

                }

                EditorGUI.indentLevel--;

                EditorGUILayout.BeginHorizontal();

                if(GUILayout.Button("add",GUILayout.Width(100)))
                {
                    manager.PreparedAtlas.Add(null);
                }

                if (GUILayout.Button("remove", GUILayout.Width(100)))
                {
                    if (manager.PreparedAtlas.Count > 0)
                        manager.PreparedAtlas.RemoveAt(manager.PreparedAtlas.Count - 1);
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}


