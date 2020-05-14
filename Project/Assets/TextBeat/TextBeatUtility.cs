using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace TextBeat
{
    internal class TextMeshProMeshInfo : InterfaceCanRecycleObj
    {   
        public class MeshInfo : InterfaceCanRecycleObj
        {
            public List<Vector3> vertices = new List<Vector3>();
            public List<Vector3> normals = new List<Vector3>();
            public List<Vector4> tangents = new List<Vector4>();
            public List<Vector2> uvs0 = new List<Vector2>();
            public List<Vector2> uvs2 = new List<Vector2>();
            public List<Color32> colors32 = new List<Color32>();
            public List<int> triangles = new List<int>();

            public void Clear()
            {
                vertices.Clear();
                uvs0.Clear();
                uvs2.Clear();
                colors32.Clear();
                normals.Clear();
                tangents.Clear();
                triangles.Clear();
            }
        }

        public class CharacterInfo : InterfaceCanRecycleObj
        {
            public char character;
            public int materialReferenceIndex;
            public bool isVisible;

            // 是否在做动画
            public bool isPlayingAni = false;

            public void Clear()
            {
                isPlayingAni = false;
            }
        }

        public List<MeshInfo> mListMeshInfo = new List<MeshInfo>();
        public List<CharacterInfo> mListCharacterInfo = new List<CharacterInfo>();
        
        public void Clear()
        {
            for (int i = 0; i < mListMeshInfo.Count; i++)
            {
                ObjectPool<MeshInfo>.recycle(mListMeshInfo[i]);
            }
            
            for (int i = 0; i < mListCharacterInfo.Count; i++)
            {
                ObjectPool<CharacterInfo>.recycle(mListCharacterInfo[i]);
            }

            mListMeshInfo.Clear();
            mListCharacterInfo.Clear();
        }
    }

    internal class TextMeshProInputInfo : InterfaceCanRecycleObj
    {
        public float fBeginAniTime;
        public TextMeshProMeshInfo Input = null;

        public void Clear()
        {
            if(Input != null)
            {
                ObjectPool<TextMeshProMeshInfo>.recycle(Input);
                Input = null;
            }
        }
    }

    internal static class TextBeatUtility
    {
        public static void CopyTo(TextMeshProMeshInfo mOutInfo, TMP_TextInfo mInputInfo)
        {
            mOutInfo.Clear();

            for(int i = 0; i < mInputInfo.materialCount; i++)
            {
                TextMeshProMeshInfo.MeshInfo mMeshInfo = ObjectPool<TextMeshProMeshInfo.MeshInfo>.Pop();
                mMeshInfo.Clear();
                
                for (int j = 0; j < mInputInfo.meshInfo[i].vertices.Length; j++)
                {
                    mMeshInfo.vertices.Add(mInputInfo.meshInfo[i].vertices[j]);
                }

                for (int j = 0; j < mInputInfo.meshInfo[i].uvs0.Length; j++)
                {
                    mMeshInfo.uvs0.Add(mInputInfo.meshInfo[i].uvs0[j]);
                }

                for (int j = 0; j < mInputInfo.meshInfo[i].uvs2.Length; j++)
                {
                    mMeshInfo.uvs2.Add(mInputInfo.meshInfo[i].uvs2[j]);
                }

                for (int j = 0; j < mInputInfo.meshInfo[i].colors32.Length; j++)
                {
                    mMeshInfo.colors32.Add(mInputInfo.meshInfo[i].colors32[j]);
                }

                for (int j = 0; j < mInputInfo.meshInfo[i].normals.Length; j++)
                {
                    mMeshInfo.normals.Add(mInputInfo.meshInfo[i].normals[j]);
                }

                for (int j = 0; j < mInputInfo.meshInfo[i].tangents.Length; j++)
                {
                    mMeshInfo.tangents.Add(mInputInfo.meshInfo[i].tangents[j]);
                }

                mOutInfo.mListMeshInfo.Add(mMeshInfo);
            }
            
            for(int i = 0; i < mInputInfo.characterCount; i++)
            {
                TextMeshProMeshInfo.CharacterInfo mCharacterInfo = ObjectPool<TextMeshProMeshInfo.CharacterInfo>.Pop();
                mCharacterInfo.Clear();

                mCharacterInfo.character = mInputInfo.characterInfo[i].character;
                mCharacterInfo.materialReferenceIndex = mInputInfo.characterInfo[i].materialReferenceIndex;
                mCharacterInfo.isVisible = mInputInfo.characterInfo[i].isVisible;
                mOutInfo.mListCharacterInfo.Add(mCharacterInfo);
            }
        }
    }

}
