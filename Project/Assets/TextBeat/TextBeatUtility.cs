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

            public List<float> uvs2ScaleY = new List<float>();
            public int vertexCount = 0;

            public void ReplaceQuad(int nBeginVertex, MeshInfo OtherMeshInfo, int nOhterBeginVertex)
            {
                for (int i = 0; i < 4; i++)
                {
                    vertices[nBeginVertex + i] = OtherMeshInfo.vertices[nOhterBeginVertex + i];
                    normals[nBeginVertex + i] = OtherMeshInfo.normals[nOhterBeginVertex + i];
                    tangents[nBeginVertex + i] = OtherMeshInfo.tangents[nOhterBeginVertex + i];
                    uvs0[nBeginVertex + i] = OtherMeshInfo.uvs0[nOhterBeginVertex + i];
                    uvs2[nBeginVertex + i] = OtherMeshInfo.uvs2[nOhterBeginVertex + i];
                    colors32[nBeginVertex + i] = OtherMeshInfo.colors32[nOhterBeginVertex + i];

                    uvs2ScaleY[nBeginVertex + i] = OtherMeshInfo.uvs2ScaleY[nOhterBeginVertex + i];
                }
            }

            public void AddQuad(MeshInfo OtherMeshInfo, int nOhterBeginVertex)
            {
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(OtherMeshInfo.vertices[nOhterBeginVertex + i]);
                    normals.Add(OtherMeshInfo.normals[nOhterBeginVertex + i]);
                    tangents.Add(OtherMeshInfo.tangents[nOhterBeginVertex + i]);
                    uvs0.Add(OtherMeshInfo.uvs0[nOhterBeginVertex + i]);
                    uvs2.Add(OtherMeshInfo.uvs2[nOhterBeginVertex + i]);
                    colors32.Add(OtherMeshInfo.colors32[nOhterBeginVertex + i]);

                    uvs2ScaleY.Add(OtherMeshInfo.uvs2ScaleY[nOhterBeginVertex + i]);
                }

                vertexCount += 4;
            }

            public void RemoveQuadAt(int nBeginVertex)
            {
                vertices.RemoveRange(nBeginVertex, 4);
                normals.RemoveRange(nBeginVertex, 4);
                tangents.RemoveRange(nBeginVertex, 4);
                uvs0.RemoveRange(nBeginVertex, 4);
                uvs2.RemoveRange(nBeginVertex, 4);
                colors32.RemoveRange(nBeginVertex, 4);

                uvs2ScaleY.RemoveRange(nBeginVertex, 4);
                vertexCount -= 4;
            }

            public void Clear()
            {
                vertices.Clear();
                uvs0.Clear();
                uvs2.Clear();
                colors32.Clear();
                normals.Clear();
                tangents.Clear();
                triangles.Clear();

                uvs2ScaleY.Clear();
                vertexCount = 0;
            }

            public void Clear1()
            {
                triangles.Clear();
                vertexCount = 0;
            }
        }

        public class CharacterInfo : InterfaceCanRecycleObj
        {
            public char character;
            public int materialReferenceIndex;
            public bool isVisible;

            public void ReplaceCharacter(CharacterInfo OtherCharacterInfo)
            {
                character = OtherCharacterInfo.character;
                materialReferenceIndex = OtherCharacterInfo.materialReferenceIndex;
                isVisible = OtherCharacterInfo.isVisible;
            }

            public void Clear()
            {
                
            }
        }

        public List<MeshInfo> mListMeshInfo = new List<MeshInfo>();
        public List<CharacterInfo> mListCharacterInfo = new List<CharacterInfo>();

        public bool Equal(TMP_Text mText)
        {
            if (mListCharacterInfo.Count != mText.textInfo.characterCount)
            {
                return false;
            }

            for (int i = 0; i < mListCharacterInfo.Count; i++)
            {
                if (mListCharacterInfo[i].character != mText.textInfo.characterInfo[i].character)
                {
                    return false;
                }
            }

            return true;
        }
        
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

        public void RemoveCharacter(int index)
        {
            CharacterInfo mRemove = mListCharacterInfo[index];
            mListCharacterInfo.RemoveAt(index);
            ObjectPool<CharacterInfo>.recycle(mRemove);
        }

        public void Check()
        {

            for (int i = 0; i < mListMeshInfo.Count; i++)
            {
                int nVertexCount = 0;
                for (int j = 0; j < mListCharacterInfo.Count; j++)
                {
                    if (mListCharacterInfo[j].materialReferenceIndex == i && mListCharacterInfo[j].isVisible)
                    {
                        nVertexCount += 4;
                    }
                }

                Debug.Assert(nVertexCount == mListMeshInfo[i].vertices.Count);
                Debug.Assert(mListMeshInfo[i].vertexCount == mListMeshInfo[i].vertices.Count);
            }

        }

    }

    internal static class TextBeatUtility
    {
        public static TextBeatAlign GetAlign(TextAnchor align)
        {
            if (align == TextAnchor.LowerLeft || align == TextAnchor.MiddleLeft || align == TextAnchor.UpperLeft)
            {
                return TextBeatAlign.Left;
            }
            else if (align == TextAnchor.LowerCenter || align == TextAnchor.MiddleCenter || align == TextAnchor.UpperCenter)
            {
                return TextBeatAlign.Center;
            }
            else
            {
                return TextBeatAlign.Right;
            }
        }

        public static TextBeatAlign GetAlign(TMPro.TextAlignmentOptions align)
        {
            if (align == TMPro.TextAlignmentOptions.Left || align == TMPro.TextAlignmentOptions.BottomLeft || align == TMPro.TextAlignmentOptions.TopLeft)
            {
                return TextBeatAlign.Left;
            }
            else if (align == TMPro.TextAlignmentOptions.Center || align == TMPro.TextAlignmentOptions.Top || align == TMPro.TextAlignmentOptions.Bottom)
            {
                return TextBeatAlign.Center;
            }
            else
            {
                return TextBeatAlign.Right;
            }
        }

        public static bool orEuqalString(string A, string B)
        {
            if (A.Length != B.Length)
            {
                return false;
            }

            for (int i = 0; i < A.Length; i++)
            {
                if (A[i] != B[i])
                {
                    return false;
                }
            }

            return true;
        }
        
        public static void CopyTo(TextMeshProMeshInfo mOutInfo, TMP_TextInfo mInputInfo)
        {
            mOutInfo.Clear();

            for (int i = 0; i < mInputInfo.materialCount; i++)
            {
                TextMeshProMeshInfo.MeshInfo mMeshInfo = ObjectPool<TextMeshProMeshInfo.MeshInfo>.Pop();
                mOutInfo.mListMeshInfo.Add(mMeshInfo);

                mMeshInfo.vertexCount = mInputInfo.meshInfo[i].vertexCount;

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
            }
            
            for(int i = 0; i < mInputInfo.characterCount; i++)
            {
                TextMeshProMeshInfo.CharacterInfo mCharacterInfo = ObjectPool<TextMeshProMeshInfo.CharacterInfo>.Pop();

                mCharacterInfo.character = mInputInfo.characterInfo[i].character;
                mCharacterInfo.materialReferenceIndex = mInputInfo.characterInfo[i].materialReferenceIndex;
                mCharacterInfo.isVisible = mInputInfo.characterInfo[i].isVisible;
                mOutInfo.mListCharacterInfo.Add(mCharacterInfo);
            }
        }
    }

}
