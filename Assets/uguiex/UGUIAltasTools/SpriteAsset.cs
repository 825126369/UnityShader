using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SpriteAsset : MonoBehaviour
{
    /// <summary>
    /// 所有sprite信息 SpriteAssetInfor类为具体的信息类
    /// </summary>
    public List<SpriteInfo> listSpriteInfor;

    public Sprite GetSpriteWithName(string name)
    {
        for(int i = 0; i < listSpriteInfor.Count;i++)
        {
            if(listSpriteInfor[i].name.Equals(name))
            {
                return listSpriteInfor[i].sprite;
            }
        }
        return null;
    }

    [ContextMenu("分离图集")]
    public void SplitSpriteToPng()
    {
#if UNITY_EDITOR
        string path = EditorUtility.SaveFolderPanel("分离图集", Application.dataPath.Replace("/Assets", "/"), "");
        if (path.Length == 0)
            return;

        string dir = path + "/分离图集_" + name + "/";
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }

        if (listSpriteInfor.Count > 0)
        {
            FileStream fileStream = new FileStream(AssetDatabase.GetAssetPath(listSpriteInfor[0].sprite.texture), FileMode.Open, FileAccess.Read);
            fileStream.Seek(0, SeekOrigin.Begin);
            //创建文件长度的缓冲区
            byte[] bytes = new byte[fileStream.Length];
            //读取文件
            fileStream.Read(bytes, 0, (int)fileStream.Length);
            //释放文件读取流
            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;
            Texture2D atlas = new Texture2D(1, 1);
            atlas.LoadImage(bytes);

            for (int i = 0; i < listSpriteInfor.Count; i++)
            {
                Sprite sp = listSpriteInfor[i].sprite;

                Texture2D temp = new Texture2D((int)sp.rect.width,(int)sp.rect.height,TextureFormat.ARGB32,false);
                Color[] arrayColor = atlas.GetPixels((int)sp.rect.x, (int)sp.rect.y, (int)sp.rect.width, (int)sp.rect.height);
                temp.SetPixels(arrayColor);
                temp.Apply();
                byte[] pixes = temp.EncodeToPNG();

                System.IO.File.WriteAllBytes(dir + sp.name + ".png", pixes);
            }
            EditorUtility.OpenWithDefaultApp(dir);
        }
#endif
    }

}

[System.Serializable]
public class SpriteInfo
{
    /// <summary>
    /// ID
    /// </summary>
    public int ID;
    /// <summary>
    /// 名称
    /// </summary>
    public string name;
    /// <summary>
    /// 精灵
    /// </summary>
    public Sprite sprite;
}