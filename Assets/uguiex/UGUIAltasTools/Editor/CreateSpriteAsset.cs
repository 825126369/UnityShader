using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class CreateSpriteAsset
{
    [MenuItem("Assets/Create or Update Sprite Asset")]         //,false,10
    public static void main()
    {
        Object target = Selection.activeObject;

        Create(target,null);
    }

    public static void Create(Object target,string filePathWithName)
    {
        if (target == null || target.GetType() != typeof(Texture2D))
            return;

        Texture2D sourceTex = target as Texture2D;
        //整体路径
        if (string.IsNullOrEmpty(filePathWithName))
        {
            filePathWithName = AssetDatabase.GetAssetPath(sourceTex);
        }
        //带后缀的文件名
        string fileNameWithExtension = Path.GetFileName(filePathWithName);
        //不带后缀的文件名
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePathWithName);
        //不带文件名的路径
        string filePath = filePathWithName.Replace(fileNameWithExtension, "");

        string assetPath = filePath + fileNameWithoutExtension + ".prefab";
        SpriteAsset spriteAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(SpriteAsset)) as SpriteAsset;
        if (spriteAsset == null)
        {
            GameObject prefab = null;
            GameObject tempGo = new GameObject(fileNameWithoutExtension);
            prefab = PrefabUtility.CreatePrefab(assetPath, tempGo);
            spriteAsset = prefab.AddComponent<SpriteAsset>();
            GameObject.DestroyImmediate(tempGo);
        }
        spriteAsset.listSpriteInfor = GetSpritesInfor(sourceTex);
        EditorUtility.SetDirty(spriteAsset.gameObject);
        AssetDatabase.SaveAssets();
    }

    public static List<SpriteInfo> GetSpritesInfor(Texture2D tex)
    {
        List<SpriteInfo> m_sprites = new List<SpriteInfo>();

        string filePath = UnityEditor.AssetDatabase.GetAssetPath(tex);

        Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].GetType() == typeof(Sprite))
            {
                SpriteInfo temp = new SpriteInfo();
                Sprite sprite = objects[i] as Sprite;
                temp.ID = i;
                temp.name = sprite.name;
                temp.sprite = sprite;
                m_sprites.Add(temp);
            }
        }
        return m_sprites;
    }
}
