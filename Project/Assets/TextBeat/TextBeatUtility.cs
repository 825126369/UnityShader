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

    internal class CustomerTextMeshMeshInfo
    {
        public class CharacterInfo : InterfaceCanRecycleObj
        {
            public char character;
            public bool isVisible;
            public float characterSize;

            public void Clear() { }
        }

        public List<CharacterInfo> mCharacterList = new List<CharacterInfo>();
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector2> uvs0 = new List<Vector2>();
        public List<Color32> colors32 = new List<Color32>();
        public List<int> triangles = new List<int>();

        public void ReplaceQuad(int nBeginVertex, CustomerTextMeshMeshInfo OtherMeshInfo, int nOhterBeginVertex)
        {
            for (int i = 0; i < 4; i++)
            {
                int index = nBeginVertex + i;
                int otherIndex = nOhterBeginVertex + i;

                vertices[index] = OtherMeshInfo.vertices[otherIndex];
                uvs0[index] = OtherMeshInfo.uvs0[otherIndex];
                colors32[index] = OtherMeshInfo.colors32[otherIndex];
            }
        }

        public void AddQuad(CustomerTextMeshMeshInfo OtherMeshInfo, int nOhterBeginVertex)
        {
            for (int i = 0; i < 4; i++)
            {
                int otherIndex = nOhterBeginVertex + i;

                vertices.Add(OtherMeshInfo.vertices[otherIndex]);
                uvs0.Add(OtherMeshInfo.uvs0[otherIndex]);
                colors32.Add(OtherMeshInfo.colors32[otherIndex]);
            }
        }

        public void InsertQuadAt(int nBeginVertex, CustomerTextMeshMeshInfo OtherMeshInfo, int nOhterBeginVertex)
        {
            for (int i = 0; i < 4; i++)
            {
                int index = nBeginVertex + i;
                int otherIndex = nOhterBeginVertex + i;

                vertices.Insert(index, OtherMeshInfo.vertices[otherIndex]);
                uvs0.Insert(index, OtherMeshInfo.uvs0[otherIndex]);
                colors32.Insert(index, OtherMeshInfo.colors32[otherIndex]);
            }
        }

        public void RemoveQuadAt(int nBeginVertex)
        {
            vertices.RemoveRange(nBeginVertex, 4);
            uvs0.RemoveRange(nBeginVertex, 4);
            colors32.RemoveRange(nBeginVertex, 4);
        }

        public void ReplaceCharacter(int nIndex, CharacterInfo OtherCharacterInfo)
        {
            mCharacterList[nIndex].character = OtherCharacterInfo.character;
            mCharacterList[nIndex].isVisible = OtherCharacterInfo.isVisible;
            mCharacterList[nIndex].characterSize = OtherCharacterInfo.characterSize;
        }

        public void AddCharacter(CharacterInfo OtherCharacterInfo)
        {
            CharacterInfo mInfo = ObjectPool<CharacterInfo>.Pop();
            mCharacterList.Add(mInfo);
            int nIndex = mCharacterList.Count - 1;
            ReplaceCharacter(nIndex, OtherCharacterInfo);
        }

        public void RemoveCharacter(int nIndex)
        {
            ObjectPool<CharacterInfo>.recycle(mCharacterList[nIndex]);
            mCharacterList.RemoveAt(nIndex);
        }

        public void Clear()
        {
            vertices.Clear();
            uvs0.Clear();
            colors32.Clear();
            triangles.Clear();

            for (int i = 0; i < mCharacterList.Count; i++)
            {
                ObjectPool<CharacterInfo>.recycle(mCharacterList[i]);
            }

            mCharacterList.Clear();
        }
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

            public void ReplaceQuad(int nBeginVertex, MeshInfo OtherMeshInfo, int nOhterBeginVertex)
            {
                for (int i = 0; i < 4; i++)
                {
                    int index = nBeginVertex + i;
                    int otherIndex = nOhterBeginVertex + i;
                    
                    vertices[index] = OtherMeshInfo.vertices[otherIndex];
                    normals[index] = OtherMeshInfo.normals[otherIndex];
                    tangents[index] = OtherMeshInfo.tangents[otherIndex];
                    uvs0[index] = OtherMeshInfo.uvs0[otherIndex];
                    uvs2[index] = OtherMeshInfo.uvs2[otherIndex];
                    colors32[index] = OtherMeshInfo.colors32[otherIndex];

                    uvs2ScaleY[index] = OtherMeshInfo.uvs2ScaleY[otherIndex];
                }
            }

            public void AddQuad(MeshInfo OtherMeshInfo, int nOhterBeginVertex)
            {
                for (int i = 0; i < 4; i++)
                {
                    int otherIndex = nOhterBeginVertex + i;

                    vertices.Add(OtherMeshInfo.vertices[otherIndex]);
                    normals.Add(OtherMeshInfo.normals[otherIndex]);
                    tangents.Add(OtherMeshInfo.tangents[otherIndex]);
                    uvs0.Add(OtherMeshInfo.uvs0[otherIndex]);
                    uvs2.Add(OtherMeshInfo.uvs2[otherIndex]);
                    colors32.Add(OtherMeshInfo.colors32[otherIndex]);

                    uvs2ScaleY.Add(OtherMeshInfo.uvs2ScaleY[otherIndex]);
                }
            }

            public void InsertQuadAt(int nBeginVertex, MeshInfo OtherMeshInfo, int nOhterBeginVertex)
            {
                for (int i = 0; i < 4; i++)
                {
                    int index = nBeginVertex + i;
                    int otherIndex = nOhterBeginVertex + i;

                    vertices.Insert(index, OtherMeshInfo.vertices[otherIndex]);
                    normals.Insert(index, OtherMeshInfo.normals[otherIndex]);
                    tangents.Insert(index, OtherMeshInfo.tangents[otherIndex]);
                    uvs0.Insert(index, OtherMeshInfo.uvs0[otherIndex]);
                    uvs2.Insert(index, OtherMeshInfo.uvs2[otherIndex]);
                    colors32.Insert(index, OtherMeshInfo.colors32[otherIndex]);

                    uvs2ScaleY.Insert(index, OtherMeshInfo.uvs2ScaleY[otherIndex]);
                }
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

        public bool Check()
        {
            
            for(int i = 0; i < mListMeshInfo.Count; i++)
            {
                int nVertexCount = 0;
                for(int j = 0; j < mListCharacterInfo.Count; j++)
                {
                    if (mListCharacterInfo[j].materialReferenceIndex == i && mListCharacterInfo[j].isVisible)
                    {
                        nVertexCount+= 4;
                    }
                }

                if (nVertexCount != mListMeshInfo[i].vertices.Count)
                {
                    Debug.Assert(false, nVertexCount + " | " + mListMeshInfo[i].vertices.Count);
                    return true;
                }
            }

            return false;
        }

    }

    internal static class TextBeatUtility
    {
        public static TextBeatAlign GetAlign(TextAlignment align)
        {
            if (align == TextAlignment.Left)
            {
                return TextBeatAlign.Left;
            }
            else if (align == TextAlignment.Center)
            {
                return TextBeatAlign.Center;
            }
            else
            {
                return TextBeatAlign.Right;
            }
        }

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

                int nVertexCount = mInputInfo.meshInfo[i].vertexCount;

                for (int j = 0; j < nVertexCount; j++)
                {
                    mMeshInfo.vertices.Add(mInputInfo.meshInfo[i].vertices[j]);
                    mMeshInfo.uvs0.Add(mInputInfo.meshInfo[i].uvs0[j]);
                    mMeshInfo.uvs2.Add(mInputInfo.meshInfo[i].uvs2[j]);
                    mMeshInfo.colors32.Add(mInputInfo.meshInfo[i].colors32[j]);
                    mMeshInfo.normals.Add(mInputInfo.meshInfo[i].normals[j]);
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

        public static void CopyTo(CustomerTextMeshMeshInfo mOutInfo, CustomerTextMesh mInputInfo)
        {
            mOutInfo.Clear();

            int nVertexCount = mInputInfo.vertexCount;

            for (int j = 0; j < nVertexCount; j++)
            {
                mOutInfo.vertices.Add(mInputInfo.vertices[j]);
                mOutInfo.uvs0.Add(mInputInfo.uvs0[j]);
                mOutInfo.colors32.Add(mInputInfo.colors32[j]);
            }
            
            for (int i = 0; i < mInputInfo.text.Length; i++)
            {
                char c = mInputInfo.text[i];
                UnityEngine.CharacterInfo mTemp;
                if (mInputInfo.font.GetCharacterInfo(c, out mTemp))
                {
                    CustomerTextMeshMeshInfo.CharacterInfo mCharacterInfo = ObjectPool<CustomerTextMeshMeshInfo.CharacterInfo>.Pop();
                    mCharacterInfo.character = c;
                    mCharacterInfo.isVisible = true;
                    mCharacterInfo.characterSize = mInputInfo.characterSize;
                    mOutInfo.mCharacterList.Add(mCharacterInfo);
                }
            }

        }

    }

}
