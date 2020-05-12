using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace TextBeat
{
    public enum TextBeatAlign
    {
        Left,
        Right,
        Center,
    }

    public class TextMeshProMeshInfo
    {
        public class MeshInfo
        {
            public List<Vector3> vertices = new List<Vector3>();
            public List<Vector3> normals = new List<Vector3>();
            public List<Vector4> tangents = new List<Vector4>();
            public List<Vector2> uvs0 = new List<Vector2>();
            public List<Vector2> uvs2 = new List<Vector2>();
            public List<Color32> colors32 = new List<Color32>();
            public List<int> triangles = new List<int>();
        }

        public class CharacterInfo
        {
            public char character;
            public int materialReferenceIndex;
        }

        public List<MeshInfo> mListMeshInfo = new List<MeshInfo>();
        public List<CharacterInfo> mListCharacterInfo = new List<CharacterInfo>();

        public static Queue<CharacterInfo> mCharacterInfoPool = new Queue<CharacterInfo>();
        public static Queue<MeshInfo> mMeshInfoPool = new Queue<MeshInfo>();


        public void Clear()
        {
            for (int i = 0; i < mListMeshInfo.Count; i++)
            {
                mMeshInfoPool.Enqueue(mListMeshInfo[i]);
            }

            mListMeshInfo.Clear();


            for (int i = 0; i < mListCharacterInfo.Count; i++)
            {
                mCharacterInfoPool.Enqueue(mListCharacterInfo[i]);
            }

            mListCharacterInfo.Clear();
        }
    }
    
    public class TextBeatUtility
    {
        public static void CopyTo(TextMeshProMeshInfo mOutInfo, TMP_TextInfo mInputInfo)
        {
            mOutInfo.Clear();

            for(int i = 0; i < mInputInfo.materialCount; i++)
            {
                TextMeshProMeshInfo.MeshInfo mMeshInfo = null;
                if (TextMeshProMeshInfo.mMeshInfoPool.Count == 0)
                {
                    mMeshInfo = new TextMeshProMeshInfo.MeshInfo();
                }else
                {
                    mMeshInfo = TextMeshProMeshInfo.mMeshInfoPool.Dequeue();
                }

                mMeshInfo.vertices.Clear();
                mMeshInfo.uvs0.Clear();
                mMeshInfo.uvs2.Clear();
                mMeshInfo.colors32.Clear();
                mMeshInfo.normals.Clear();
                mMeshInfo.tangents.Clear();
                
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
                TextMeshProMeshInfo.CharacterInfo mCharacterInfo = null;
                if(TextMeshProMeshInfo.mCharacterInfoPool.Count == 0)
                {
                    mCharacterInfo = new TextMeshProMeshInfo.CharacterInfo();
                }
                else
                {
                    mCharacterInfo = TextMeshProMeshInfo.mCharacterInfoPool.Dequeue();
                }

                mCharacterInfo.character = mInputInfo.characterInfo[i].character;
                mCharacterInfo.materialReferenceIndex = mInputInfo.characterInfo[i].materialReferenceIndex;
                mOutInfo.mListCharacterInfo.Add(mCharacterInfo);
            }
        }
    }

}
