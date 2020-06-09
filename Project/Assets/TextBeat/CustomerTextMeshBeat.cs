using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;

namespace TextBeat
{
    [RequireComponent(typeof(CustomerTextMesh))]
    [XLua.LuaCallCSharp]
    public class CustomerTextMeshBeat : MonoBehaviour
    {
        public string prefix = string.Empty;
        public float fAlphaTime = 0.5f;
        public float fAniHeight = 100;

        private CustomerTextMeshMeshInfo lastInput = new CustomerTextMeshMeshInfo();
        private CustomerTextMeshMeshInfo Input = new CustomerTextMeshMeshInfo();
        private CustomerTextMeshMeshInfo mWillFillInput = new CustomerTextMeshMeshInfo();

        private List<float> mWorldAniBeginTimeList = new List<float>();
        private List<bool> mWorldisPlayingAniList = new List<bool>();

        private CustomerTextMesh mText;

        void Start()
        {
            mText = GetComponent<CustomerTextMesh>();
            mText.mProperityChangedEvent += ON_TEXT_CHANGED;
            ON_TEXT_CHANGED();
        }

        private void OnDestroy()
        {
            mText.mProperityChangedEvent -= ON_TEXT_CHANGED;
        }

        public void UpdateText(UInt64 value)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                mText.text = value.ToString();
            }
            else
            {
                mText.text = prefix + value.ToString();
            }
        }

        public void UpdateText(string text)
        {
            mText.text = text;
        }

        private bool orOneWoldFinishAni(int index)
        {
            return Time.time - mWorldAniBeginTimeList[index] > fAlphaTime;
        }

        private void AddVertexInfo(Vector3 pos, Vector2 uv0, Color32 color32)
        {
            int nIndex = mText.vertexCount;

            mText.vertices[nIndex] = pos;
            mText.uvs0[nIndex] = uv0;
            mText.colors32[nIndex] = color32;

            mText.vertexCount++;
        }

        private void ClearOutputMeshInfoList()
        {
            mText.vertexCount = 0;
        }

        private void ReSizeWorldAniBeginTimeList(CustomerTextMeshMeshInfo Input, CustomerTextMeshMeshInfo lastInput)
        {
            int nMaxLength = Mathf.Max(Input.mCharacterList.Count, lastInput.mCharacterList.Count);
            for (int i = mWorldAniBeginTimeList.Count; i < nMaxLength; i++)
            {
                mWorldAniBeginTimeList.Add(-fAlphaTime - 1.0f);
                mWorldisPlayingAniList.Add(false);
            }
        }

        private void ForceChangeLastInputOffsetXMeshInfo()
        {
            if (lastInput.vertices.Count > 0 && Input.vertices.Count > 0)
            {
                float fOffsetX = Input.vertices[0].x - lastInput.vertices[0].x;

                List<Vector3> vertices = lastInput.vertices;
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vector3 oriPos = vertices[i];
                    vertices[i] = oriPos + new Vector3(fOffsetX, 0, 0);
                }
            }
        }

        private void FillInput()
        {
            ReSizeWorldAniBeginTimeList(mWillFillInput, Input);

            bool bInputCountEqual = mWillFillInput.mCharacterList.Count == Input.mCharacterList.Count;
            bool bForceChangeOffsetXMeshInfo = false;
            if (!bInputCountEqual)
            {
                if (TextBeatUtility.GetAlign(mText.mTextAlignment) == TextBeatAlign.Left)
                {
                    bForceChangeOffsetXMeshInfo = false;
                }
                else if (TextBeatUtility.GetAlign(mText.mTextAlignment) == TextBeatAlign.Right)
                {
                    bForceChangeOffsetXMeshInfo = true;
                }
                else if (TextBeatUtility.GetAlign(mText.mTextAlignment) == TextBeatAlign.Center)
                {
                    bForceChangeOffsetXMeshInfo = true; // 顶点位置 都改变了
                }
            }

            if (bForceChangeOffsetXMeshInfo)
            {
                for (int i = 0; i < Input.mCharacterList.Count; i++)
                {
                    if (!orOneWoldFinishAni(i))
                    {
                        return;
                    }
                }
            }

            // 填充Input
            for (int i = 0; i < mWillFillInput.mCharacterList.Count; i++)
            {
                if (orOneWoldFinishAni(i))
                {
                    bool bChanged = false;
                    if (i < Input.mCharacterList.Count)
                    {
                        bChanged = mWillFillInput.mCharacterList[i].character != Input.mCharacterList[i].character;
                    }
                    else
                    {
                        bChanged = true;
                    }

                    if (bForceChangeOffsetXMeshInfo)
                    {
                        bChanged = true;
                    }

                    if (bChanged)
                    {
                        if (i < Input.mCharacterList.Count)
                        {
                            int nBeginIndex = i * 4;
                            Input.ReplaceQuad(nBeginIndex, mWillFillInput, nBeginIndex);
                            Input.ReplaceCharacter(i, mWillFillInput.mCharacterList[i]);
                        }
                        else
                        {
                            int nOtherBeginIndex = i * 4;
                            Input.AddQuad(mWillFillInput, nOtherBeginIndex);
                            Input.AddCharacter(mWillFillInput.mCharacterList[i]);
                        }
                    }
                }
            }

            for (int i = Input.mCharacterList.Count - 1; i >= mWillFillInput.mCharacterList.Count; i--)
            {
                if (orOneWoldFinishAni(i))
                {
                    int nBeginIndex = i * 4;
                    Input.RemoveQuadAt(nBeginIndex);
                    Input.RemoveCharacter(i);
                }
            }

            if (bForceChangeOffsetXMeshInfo)
            {
                ForceChangeLastInputOffsetXMeshInfo();
            }
        }

        private void FillLastInput()
        {
            ReSizeWorldAniBeginTimeList(lastInput, Input);

            for (int i = 0; i < Input.mCharacterList.Count; i++)
            {
                if (orOneWoldFinishAni(i) && mWorldisPlayingAniList[i])
                {
                    if (i < lastInput.mCharacterList.Count)
                    {
                        int nBeginIndex = i * 4;
                        lastInput.ReplaceQuad(nBeginIndex, Input, nBeginIndex);
                        lastInput.ReplaceCharacter(i, Input.mCharacterList[i]);
                    }
                    else
                    {
                        int nOtherBeginIndex = i * 4;
                        lastInput.AddQuad(Input, nOtherBeginIndex);
                        lastInput.AddCharacter(Input.mCharacterList[i]);
                    }
                }
            }

            for (int i = lastInput.mCharacterList.Count - 1; i >= Input.mCharacterList.Count; i--)
            {
                if (orOneWoldFinishAni(i) && mWorldisPlayingAniList[i])
                {
                    int nBeginIndex = i * 4;
                    lastInput.RemoveQuadAt(nBeginIndex);
                    lastInput.RemoveCharacter(i);
                }
            }

        }

        private void PlayAni()
        {
            ClearOutputMeshInfoList();

            FillLastInput();
            FillInput();

            for (int i = 0; i < mWorldisPlayingAniList.Count; i++)
            {
                if (orOneWoldFinishAni(i) && mWorldisPlayingAniList[i])
                {
                    mWorldisPlayingAniList[i] = false;
                }
            }

            ReSizeWorldAniBeginTimeList(Input, lastInput);

            for (int i = 0; i < Input.mCharacterList.Count; i++)
            {
                if (orOneWoldFinishAni(i) && !mWorldisPlayingAniList[i])
                {
                    bool bChanged = false;
                    if (i < lastInput.mCharacterList.Count)
                    {
                        bChanged = lastInput.mCharacterList[i].character != Input.mCharacterList[i].character;
                    }
                    else
                    {
                        bChanged = true;
                    }

                    if (bChanged)
                    {
                        mWorldisPlayingAniList[i] = true;
                        mWorldAniBeginTimeList[i] = Time.time;
                    }
                }

                if (!orOneWoldFinishAni(i))
                {
                    float fTimePercent = Mathf.Clamp01((Time.time - mWorldAniBeginTimeList[i]) / fAlphaTime);

                    if (i < lastInput.mCharacterList.Count)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            int nOirIndex = i * 4 + j;
                            Vector3 oriPos = lastInput.vertices[nOirIndex];
                            Color32 oriColor32 = lastInput.colors32[nOirIndex];
                            Vector2 uv0 = lastInput.uvs0[nOirIndex];

                            Vector3 targetPos = new Vector3(oriPos.x, oriPos.y + fTimePercent * fAniHeight, oriPos.z);
                            Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)((1 - fTimePercent) * 255));
                            AddVertexInfo(targetPos, uv0, targetColor32);
                        }
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        int nOirIndex = i * 4 + j;
                        Vector3 oriPos = Input.vertices[nOirIndex];
                        Color32 oriColor32 = Input.colors32[nOirIndex];
                        Vector2 uv0 = Input.uvs0[nOirIndex];

                        Vector3 targetPos = new Vector3(oriPos.x, oriPos.y - (1 - fTimePercent) * fAniHeight, oriPos.z);
                        Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)(fTimePercent * 255));
                        AddVertexInfo(targetPos, uv0, targetColor32);
                    };
                }
                else
                {
                    for (int j = 0; j < 4; j++)
                    {
                        int nOirIndex = i * 4 + j;
                        Vector3 oriPos = Input.vertices[nOirIndex];
                        Color32 oriColor32 = Input.colors32[nOirIndex];
                        Vector2 uv0 = Input.uvs0[nOirIndex];
                        AddVertexInfo(oriPos, uv0, oriColor32);
                    };
                }
            }

            for (int i = Input.mCharacterList.Count; i < lastInput.mCharacterList.Count; i++)
            {
                if (orOneWoldFinishAni(i) && !mWorldisPlayingAniList[i])
                {
                    bool bChanged = true;
                    if (!lastInput.mCharacterList[i].isVisible)
                    {
                        bChanged = false;
                    }

                    if (bChanged)
                    {
                        mWorldAniBeginTimeList[i] = Time.time;
                        mWorldisPlayingAniList[i] = true;
                    }
                }

                if (!orOneWoldFinishAni(i))
                {
                    float fTimePercent = Mathf.Clamp01((Time.time - mWorldAniBeginTimeList[i]) / fAlphaTime);
                    for (int j = 0; j < 4; j++)
                    {
                        int nOirIndex = i * 4 + j;
                        Vector3 oriPos = lastInput.vertices[nOirIndex];
                        Color32 oriColor32 = lastInput.colors32[nOirIndex];
                        Vector2 uv0 = lastInput.uvs0[nOirIndex];

                        Vector3 targetPos = new Vector3(oriPos.x, oriPos.y + fTimePercent * fAniHeight, oriPos.z);
                        Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)((1 - fTimePercent) * 255));
                        AddVertexInfo(targetPos, uv0, targetColor32);
                    }
                }
            }

            UpdateMesh();
        }

        void UpdateMesh()
        {
            mText.ClearUnusedVertices();
            mText.mesh.vertices = mText.vertices;
            mText.mesh.uv = mText.uvs0;
            mText.mesh.colors32 = mText.colors32;
            mText.mesh.RecalculateBounds();
        }

        void ON_TEXT_CHANGED()
        {
            int nLength = mText.text.Length * 2;
            mText.ResetMeshSize(nLength);
            TextBeatUtility.CopyTo(mWillFillInput, mText);
            BuildAni();
        }

        private void LateUpdate()
        {
            BuildAni();
        }

        void BuildAni()
        {
            PlayAni();
        }

    }
}