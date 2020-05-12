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
        public TMP_MeshInfo[] m_TMP_MeshInfos;
        public TMP_CharacterInfo[] m_TMP_CharacterInfos;
        public int characterCount;
    }
    
    public class TextBeatUtility
    {
        public static void CopyTo(TextMeshProMeshInfo mInfo, TMP_TextInfo mTextInfo)
        {
            mInfo.characterCount = mTextInfo.characterCount;

            TMP_MeshInfo[] m_CachedMeshInfo = mInfo.m_TMP_MeshInfos;
            if (m_CachedMeshInfo == null || m_CachedMeshInfo.Length != mTextInfo.meshInfo.Length)
            {
                m_CachedMeshInfo = new TMP_MeshInfo[mTextInfo.meshInfo.Length];
                for (int i = 0; i < m_CachedMeshInfo.Length; i++)
                {
                    int num = mTextInfo.meshInfo[i].vertices.Length;
                    m_CachedMeshInfo[i].vertices = new Vector3[num];
                    m_CachedMeshInfo[i].uvs0 = new Vector2[num];
                    m_CachedMeshInfo[i].uvs2 = new Vector2[num];
                    m_CachedMeshInfo[i].colors32 = new Color32[num];
                    m_CachedMeshInfo[i].normals = new Vector3[num];
                    m_CachedMeshInfo[i].tangents = new Vector4[num];
                }
            }
            for (int j = 0; j < m_CachedMeshInfo.Length; j++)
            {
                int num2 = mTextInfo.meshInfo[j].vertices.Length;
                if (m_CachedMeshInfo[j].vertices.Length != num2)
                {
                    m_CachedMeshInfo[j].vertices = new Vector3[num2];
                    m_CachedMeshInfo[j].uvs0 = new Vector2[num2];
                    m_CachedMeshInfo[j].uvs2 = new Vector2[num2];
                    m_CachedMeshInfo[j].colors32 = new Color32[num2];
                    m_CachedMeshInfo[j].normals = new Vector3[num2];
                    m_CachedMeshInfo[j].tangents = new Vector4[num2];
                }

                Array.Copy(mTextInfo.meshInfo[j].vertices, m_CachedMeshInfo[j].vertices, num2);
                Array.Copy(mTextInfo.meshInfo[j].uvs0, m_CachedMeshInfo[j].uvs0, num2);
                Array.Copy(mTextInfo.meshInfo[j].uvs2, m_CachedMeshInfo[j].uvs2, num2);
                Array.Copy(mTextInfo.meshInfo[j].colors32, m_CachedMeshInfo[j].colors32, num2);
                Array.Copy(mTextInfo.meshInfo[j].normals, m_CachedMeshInfo[j].normals, num2);
                Array.Copy(mTextInfo.meshInfo[j].tangents, m_CachedMeshInfo[j].tangents, num2);
            }

            mInfo.m_TMP_CharacterInfos = mTextInfo.characterInfo.Clone() as TMP_CharacterInfo[];


            mInfo.m_TMP_MeshInfos = m_CachedMeshInfo;
        }


        public static void CopyTo(TMP_TextInfo mTextInfo, TextMeshProMeshInfo mInfo)
        {
            mInfo.characterCount = mTextInfo.characterCount;

            TMP_MeshInfo[] m_CachedMeshInfo = mInfo.m_TMP_MeshInfos;
            for (int j = 0; j < m_CachedMeshInfo.Length; j++)
            {
                int num2 = mTextInfo.meshInfo[j].vertices.Length;
                if (m_CachedMeshInfo[j].vertices.Length != num2)
                {
                    m_CachedMeshInfo[j].vertices = new Vector3[num2];
                    m_CachedMeshInfo[j].uvs0 = new Vector2[num2];
                    m_CachedMeshInfo[j].uvs2 = new Vector2[num2];
                    m_CachedMeshInfo[j].colors32 = new Color32[num2];
                    m_CachedMeshInfo[j].normals = new Vector3[num2];
                    m_CachedMeshInfo[j].tangents = new Vector4[num2];
                }

                Array.Copy(mTextInfo.meshInfo[j].vertices, m_CachedMeshInfo[j].vertices, num2);
                Array.Copy(mTextInfo.meshInfo[j].uvs0, m_CachedMeshInfo[j].uvs0, num2);
                Array.Copy(mTextInfo.meshInfo[j].uvs2, m_CachedMeshInfo[j].uvs2, num2);
                Array.Copy(mTextInfo.meshInfo[j].colors32, m_CachedMeshInfo[j].colors32, num2);
                Array.Copy(mTextInfo.meshInfo[j].normals, m_CachedMeshInfo[j].normals, num2);
                Array.Copy(mTextInfo.meshInfo[j].tangents, m_CachedMeshInfo[j].tangents, num2);
            }

            mInfo.m_TMP_CharacterInfos = mTextInfo.characterInfo.Clone() as TMP_CharacterInfo[];


            mInfo.m_TMP_MeshInfos = m_CachedMeshInfo;
        }
    }

}
