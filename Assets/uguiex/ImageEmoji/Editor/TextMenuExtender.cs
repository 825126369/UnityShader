﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EmojiUI;

public class TextMenuExtender
{
    [MenuItem("GameObject/UI/EmojiText", false, 10)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        GameObject go = null;
        EmojiManager _inline = AssetDatabase.LoadAssetAtPath<EmojiManager>("Assets/3rd/uguiex/ImageEmoji/Prefabs/emojitext.prefab");
        if (_inline)
        {
            go = GameObject.Instantiate(_inline).gameObject;
        }
        else
        {
            go = new GameObject();
            go.AddComponent<EmojiText>();
        }
        go.name = "EmojiText";
        GameObject _parent = menuCommand.context as GameObject;
        if (_parent == null)
        {
            _parent = new GameObject("Canvas");
            _parent.layer = LayerMask.NameToLayer("UI");
            _parent.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            _parent.AddComponent<CanvasScaler>();
            _parent.AddComponent<GraphicRaycaster>();

            EventSystem _es = GameObject.FindObjectOfType<EventSystem>();
            if (!_es)
            {
                _es = new GameObject("EventSystem").AddComponent<EventSystem>();
                _es.gameObject.AddComponent<StandaloneInputModule>();
            }
        }
        GameObjectUtility.SetParentAndAlign(go, _parent);
        //注册返回事件
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
    
 }

