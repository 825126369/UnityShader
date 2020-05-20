using UnityEngine;
using System.Collections;
using TMPro;
using System;
using System.Text;
using System.Collections.Generic;

namespace TextBeat
{
    public class TextMeshProBeat : MonoBehaviour
    {
        public bool bUseNoGCStringBuilder = true;
        public string prefix = string.Empty;
        public UInt64 value = 0;
        public UInt64 targetValue = UInt64.MaxValue;
        public bool bImmediatelyToTargetValue = false;

        public float fAlphaTime = 0.5f;
        public float fAniHeight = 100;

        public float fUpdateTextMaxTime = 0f;
        private float fBeginUpdateTextTime;

        private TextMeshProMeshInfo lastInput = new TextMeshProMeshInfo();
        private TextMeshProMeshInfo Input = new TextMeshProMeshInfo();

        private TextMeshProMeshInfo mWillFillInput = new TextMeshProMeshInfo();
        private List<float> mWorldAniBeginTimeList = new List<float>();
        private List<bool> mWorldisPlayingAniList = new List<bool>();

        private List<int> mVisibleCharacterList1 = new List<int>();
        private List<int> mVisibleCharacterList2 = new List<int>();
        private List<TextMeshProMeshInfo.MeshInfo> outputMeshInfoList = new List<TextMeshProMeshInfo.MeshInfo>();

        private TMP_Text mText;
        private StringBuilder mStringBuilder;
        private String mString;

        private UInt64 lastValue = UInt64.MaxValue;

        private const int UInt64Length = 20;
        private const int oneSize = 4;

        private static readonly Color32 s_DefaultColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        private static readonly Vector3 s_DefaultNormal = new Vector3(0.0f, 0.0f, -1f);
        private static readonly Vector4 s_DefaultTangent = new Vector4(-1f, 0.0f, 0.0f, 1f);

        private float m_previousLossyScaleY;

        void Awake()
        {
            mText = GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }

        private void OnDestroy()
        {
            mWillFillInput.Clear();
            Input.Clear();
            lastInput.Clear();
            outputMeshInfoList.Clear();
            mStringBuilder = null;
        }

        void Start()
        {
            if (bUseNoGCStringBuilder)
            {
                InitNoGCStringBuilder();
            }
            
            mText.ForceMeshUpdate();
            ReSizeWorldAniBeginTimeList(lastInput, lastInput);
            //InitMaxMeshSize();
            ForceChangeLastInputOffsetXMeshInfo();

            //mText.isUsingLegacyAnimationComponent = true;
        }

        private int GetCharacterMaxCount()
        {
            //return UInt64Length + prefix.Length;
            return mText.textInfo.characterCount;
        }
        
        private void InitNoGCStringBuilder(int Capacity = UInt64Length)
        {
            if (mStringBuilder == null)
            {
                mStringBuilder = new StringBuilder(Capacity);
                mString = mStringBuilder.GetGarbageFreeString();
            }
        }

        private void ReSizeStringBuilder(int nLastStringBuilderCapacity)
        {
            if (nLastStringBuilderCapacity != mStringBuilder.Capacity)
            {
                mString = mStringBuilder.GetGarbageFreeString();
            }
        }

        private void Update()
        {
            if (Time.time - fBeginUpdateTextTime > fUpdateTextMaxTime)
            {
                fBeginUpdateTextTime = Time.time;
                //if (bImmediatelyToTargetValue)
                //{
                //    value = targetValue;
                //    bImmediatelyToTargetValue = false;
                //    UpdateText(value);
                //}
                //else if (value < targetValue)
                //{
                //    //value += (UInt64)UnityEngine.Random.Range(1, 9);
                //    value ++;

                //    if (value > targetValue)
                //    {
                //        value = targetValue;
                //    }

                //    UpdateText(value);
                //}
                //else if (value > targetValue)
                //{
                //    //value -= (UInt64)UnityEngine.Random.Range(1, 9);
                //    value--;

                //    if (value < targetValue)
                //    {
                //        value = targetValue;
                //    }

                //    UpdateText(value);
                //}

                value = (ulong)UnityEngine.Random.Range(1, 10000);
                if (UnityEngine.Random.Range(1, 3) == 1)
                {
                    value *= 10 * (ulong)UnityEngine.Random.Range(1, 5);
                }else
                {
                    value /= 10 * (ulong)UnityEngine.Random.Range(1, 5);
                }
                //value = (UInt64)UnityEngine.Random.Range(1, UInt64.MaxValue);
                UpdateText(value);
            }
        }


        //float testValue = 10005f;
        //private void Update()
        //{
        //    testValue -= Time.deltaTime;

        //    UInt64 t = (UInt64)Mathf.FloorToInt(testValue);
        //    UpdateText(t);
        //}

        public void UpdateText(Int64 value)
        {
            UpdateText((UInt64)value);
        }

        public void UpdateText(UInt64 value)
        {
            if (bUseNoGCStringBuilder)
            {
                // 这里加个判断，因为如果不加的话，会导致 Mesh 被重新刷新，看起来没有做动画
                if (lastValue != value)
                {
                    int nLastStringBuilderCapacity = mStringBuilder.Capacity;
                    mStringBuilder.GarbageFreeClear();
                    mStringBuilder.Append(prefix);
                    mStringBuilder.AppendUInt64WithCommas(value);
                    ReSizeStringBuilder(nLastStringBuilderCapacity);
                    mStringBuilder.Align(mText.alignment);
                    
                    mText.text = mString;
                    mText.havePropertiesChanged = true;
                    mText.SetVerticesDirty();
                    mText.SetLayoutDirty();
                    
                    lastValue = value;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    mText.text = value.ToString();
                }else
                {
                    mText.text = prefix + value.ToString();
                }
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

        private void AddVertexInfo(int materialIndex, Vector3 pos, Color32 color, Vector2 uv0, Vector2 uv1, Vector3 normal, Vector4 tangent)
        {
            outputMeshInfoList[materialIndex].vertices.Add(pos);
            outputMeshInfoList[materialIndex].colors32.Add(color);
            outputMeshInfoList[materialIndex].uvs0.Add(uv0);
            outputMeshInfoList[materialIndex].uvs2.Add(uv1);
            outputMeshInfoList[materialIndex].normals.Add(normal);
            outputMeshInfoList[materialIndex].tangents.Add(tangent);
        }

        private void AddIndices(int materialIndex, int nBeginIndex)
        {
            outputMeshInfoList[materialIndex].triangles.Add(nBeginIndex);
            outputMeshInfoList[materialIndex].triangles.Add(nBeginIndex + 1);
            outputMeshInfoList[materialIndex].triangles.Add(nBeginIndex + 2);

            outputMeshInfoList[materialIndex].triangles.Add(nBeginIndex + 2);
            outputMeshInfoList[materialIndex].triangles.Add(nBeginIndex + 3);
            outputMeshInfoList[materialIndex].triangles.Add(nBeginIndex + 0);
        }

        private void ClearOutputMeshInfoList()
        {
            if (outputMeshInfoList.Count != mText.textInfo.materialCount)
            {
                outputMeshInfoList.Clear();
                for (int i = 0; i < mText.textInfo.materialCount; i++)
                {
                    TextMeshProMeshInfo.MeshInfo mMeshInfo = ObjectPool<TextMeshProMeshInfo.MeshInfo>.Pop();
                    outputMeshInfoList.Add(mMeshInfo);
                }
            }
            else
            {
                for (int i = 0; i < mText.textInfo.materialCount; i++)
                {
                    outputMeshInfoList[i].Clear();
                }
            }
        }

        private void ReSizeWorldAniBeginTimeList(TextMeshProMeshInfo Input, TextMeshProMeshInfo lastInput)
        {
            int nMaxLength = Mathf.Max(Input.mListCharacterInfo.Count, lastInput.mListCharacterInfo.Count);
            for (int i = mWorldAniBeginTimeList.Count; i < nMaxLength; i++)
            {
                mWorldAniBeginTimeList.Add(-fAlphaTime - 1.0f);
                mWorldisPlayingAniList.Add(false);
            }
        }

        private void InitTextMeshProMeshInfo(TextMeshProMeshInfo mOutInfo)
        {
            if (mOutInfo.mListMeshInfo.Count == 0)
            {
                mOutInfo.Clear();

                for (int i = 0; i < mText.textInfo.materialCount; i++)
                {
                    TextMeshProMeshInfo.MeshInfo mMeshInfo = ObjectPool<TextMeshProMeshInfo.MeshInfo>.Pop();
                    mOutInfo.mListMeshInfo.Add(mMeshInfo);
                }
            }
        }

        private void ForceChangeLastInputOffsetXMeshInfo()
        {   
            for (int k = 0; k < mText.textInfo.materialCount; k++)
            {
                if (lastInput.mListMeshInfo[k].vertices.Count > 0)
                {
                    float fOffsetX = Input.mListMeshInfo[k].vertices[0].x - lastInput.mListMeshInfo[k].vertices[0].x;

                    List<Vector3> vertices = lastInput.mListMeshInfo[k].vertices;
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        Vector3 oriPos = vertices[i];
                        vertices[i] = oriPos + new Vector3(fOffsetX, 0, 0);
                    }
                }
            }
        }

        private void ResetVisibleCharacterList()
        {
            if (mVisibleCharacterList1.Count < mText.textInfo.materialCount || mVisibleCharacterList2.Count < mText.textInfo.materialCount)
            {
                mVisibleCharacterList1.Clear();
                mVisibleCharacterList2.Clear();

                for (int i = 0; i < mText.textInfo.materialCount; i++)
                {
                    mVisibleCharacterList1.Add(0);
                    mVisibleCharacterList2.Add(0);
                }
            }
            else
            {
                for (int i = 0; i < mText.textInfo.materialCount; i++)
                {
                    mVisibleCharacterList1[i] = 0;
                    mVisibleCharacterList2[i] = 0;
                }
            }
        }

        private void FillInput()
        {
            ResetVisibleCharacterList();
            List<int> nWillInputVisibleIndex = mVisibleCharacterList1;
            List<int> nInputVisibleIndex = mVisibleCharacterList2;
            
            ReSizeWorldAniBeginTimeList(mWillFillInput, Input);

            bool bInputCountEqual = mWillFillInput.mListCharacterInfo.Count == Input.mListCharacterInfo.Count;
            bool bForceChangeOffsetXMeshInfo = false;
            if (!bInputCountEqual)
            {
                if (TextBeatUtility.GetAlign(mText.alignment) == TextBeatAlign.Left)
                {
                    bForceChangeOffsetXMeshInfo = false;
                }
                else if (TextBeatUtility.GetAlign(mText.alignment) == TextBeatAlign.Right)
                {
                    bForceChangeOffsetXMeshInfo = true;
                }
                else if (TextBeatUtility.GetAlign(mText.alignment) == TextBeatAlign.Center)
                {
                    bForceChangeOffsetXMeshInfo = true; // 顶点位置 都改变了
                }
            }

            if (bForceChangeOffsetXMeshInfo)
            {
                for (int i = 0; i < Input.mListCharacterInfo.Count; i++)
                {
                    if (!orOneWoldFinishAni(i))
                    {
                        return;
                    }
                }
            }

            // 填充Input
            for (int i = 0; i < mWillFillInput.mListCharacterInfo.Count; i++)
            {
                if (orOneWoldFinishAni(i))
                {
                    bool bChanged = false;
                    if (i < Input.mListCharacterInfo.Count)
                    {
                        bChanged = mWillFillInput.mListCharacterInfo[i].character != Input.mListCharacterInfo[i].character;
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
                        int WillmaterialIndex = mWillFillInput.mListCharacterInfo[i].materialReferenceIndex;
                        if (i < Input.mListCharacterInfo.Count)
                        {
                            int materialIndex = Input.mListCharacterInfo[i].materialReferenceIndex;

                            int nBeginIndex = nInputVisibleIndex[materialIndex] * oneSize;
                            int nOtherBeginIndex = nWillInputVisibleIndex[WillmaterialIndex] * oneSize;
                            if (Input.mListCharacterInfo[i].isVisible && mWillFillInput.mListCharacterInfo[i].isVisible)
                            {
                                Input.mListMeshInfo[materialIndex].ReplaceQuad(nBeginIndex, mWillFillInput.mListMeshInfo[WillmaterialIndex], nOtherBeginIndex);
                            }
                            else
                            {
                                if (Input.mListCharacterInfo[i].isVisible)
                                {
                                    Input.mListMeshInfo[materialIndex].RemoveQuadAt(nBeginIndex);
                                }
                                else if (mWillFillInput.mListCharacterInfo[i].isVisible)
                                {
                                    Input.mListMeshInfo[materialIndex].AddQuad(mWillFillInput.mListMeshInfo[WillmaterialIndex], nOtherBeginIndex);
                                }
                            }

                            Input.mListCharacterInfo[i].ReplaceCharacter(mWillFillInput.mListCharacterInfo[i]);
                        }
                        else
                        {
                            if (mWillFillInput.mListCharacterInfo[i].isVisible)
                            {
                                int nOtherBeginIndex = nWillInputVisibleIndex[WillmaterialIndex] * oneSize;
                                Input.mListMeshInfo[WillmaterialIndex].AddQuad(mWillFillInput.mListMeshInfo[WillmaterialIndex], nOtherBeginIndex);
                            }

                            TextMeshProMeshInfo.CharacterInfo characterInfo = ObjectPool<TextMeshProMeshInfo.CharacterInfo>.Pop();
                            characterInfo.ReplaceCharacter(mWillFillInput.mListCharacterInfo[i]);
                            Input.mListCharacterInfo.Add(characterInfo);
                        }
                    }
                }

                if (mWillFillInput.mListCharacterInfo[i].isVisible)
                {
                    int WillmaterialIndex = mWillFillInput.mListCharacterInfo[i].materialReferenceIndex;
                    nWillInputVisibleIndex[WillmaterialIndex]++;
                }

                if (i < Input.mListCharacterInfo.Count && Input.mListCharacterInfo[i].isVisible)
                {
                    int materialIndex = Input.mListCharacterInfo[i].materialReferenceIndex;
                    nInputVisibleIndex[materialIndex]++;
                }
            }

            for (int i = mWillFillInput.mListCharacterInfo.Count; i < Input.mListCharacterInfo.Count; i++)
            {
                if (Input.mListCharacterInfo[i].isVisible)
                {
                    int materialIndex = Input.mListCharacterInfo[i].materialReferenceIndex;
                    nInputVisibleIndex[materialIndex]++;
                }
            }

            for (int i = Input.mListCharacterInfo.Count - 1; i >= mWillFillInput.mListCharacterInfo.Count; i--)
            {
                int materialIndex = Input.mListCharacterInfo[i].materialReferenceIndex;
                if (orOneWoldFinishAni(i))
                {
                    if (Input.mListCharacterInfo[i].isVisible)
                    {
                        nInputVisibleIndex[materialIndex]--;
                        int nBeginIndex = nInputVisibleIndex[materialIndex] * oneSize;
                        Input.mListMeshInfo[materialIndex].RemoveQuadAt(nBeginIndex);
                    }

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

            ResetVisibleCharacterList();
            List<int> nLastVisibleIndex = mVisibleCharacterList1;
            List<int> nNowVisibleIndex = mVisibleCharacterList2;

            for (int i = 0; i < Input.mListCharacterInfo.Count; i++)
            {
                if (orOneWoldFinishAni(i) && mWorldisPlayingAniList[i])
                {
                    int materialIndex = Input.mListCharacterInfo[i].materialReferenceIndex;

                    if (i < lastInput.mListCharacterInfo.Count)
                    {
                        int LastMaterialIndex = lastInput.mListCharacterInfo[i].materialReferenceIndex;

                        int nBeginIndex = nLastVisibleIndex[LastMaterialIndex] * oneSize;
                        int nOtherBeginIndex = nNowVisibleIndex[materialIndex] * oneSize;
                        if (lastInput.mListCharacterInfo[i].isVisible && Input.mListCharacterInfo[i].isVisible)
                        {
                            lastInput.mListMeshInfo[LastMaterialIndex].ReplaceQuad(nBeginIndex, Input.mListMeshInfo[materialIndex], nOtherBeginIndex);
                        }
                        else
                        {
                            if (lastInput.mListCharacterInfo[i].isVisible)
                            {
                                lastInput.mListMeshInfo[LastMaterialIndex].RemoveQuadAt(nBeginIndex);
                            }
                            else if (Input.mListCharacterInfo[i].isVisible)
                            {
                                lastInput.mListMeshInfo[LastMaterialIndex].AddQuad(Input.mListMeshInfo[materialIndex], nOtherBeginIndex);
                            }
                        }

                        lastInput.mListCharacterInfo[i].ReplaceCharacter(Input.mListCharacterInfo[i]);
                    }
                    else
                    {
                        if (Input.mListCharacterInfo[i].isVisible)
                        {
                            int nOtherBeginIndex = nNowVisibleIndex[materialIndex] * oneSize;
                            lastInput.mListMeshInfo[materialIndex].AddQuad(Input.mListMeshInfo[materialIndex], nOtherBeginIndex);
                        }

                        TextMeshProMeshInfo.CharacterInfo characterInfo = ObjectPool<TextMeshProMeshInfo.CharacterInfo>.Pop();
                        characterInfo.ReplaceCharacter(Input.mListCharacterInfo[i]);
                        lastInput.mListCharacterInfo.Add(characterInfo);
                    }
                }

                if (Input.mListCharacterInfo[i].isVisible)
                {
                    int materialIndex = Input.mListCharacterInfo[i].materialReferenceIndex;
                    nNowVisibleIndex[materialIndex]++;
                }

                if (i < lastInput.mListCharacterInfo.Count && lastInput.mListCharacterInfo[i].isVisible)
                {
                    int LastMaterialIndex = lastInput.mListCharacterInfo[i].materialReferenceIndex;
                    nLastVisibleIndex[LastMaterialIndex]++;
                }
            }

            for (int i = Input.mListCharacterInfo.Count; i < lastInput.mListCharacterInfo.Count; i++)
            {
                if (lastInput.mListCharacterInfo[i].isVisible)
                {
                    int LastMaterialIndex = lastInput.mListCharacterInfo[i].materialReferenceIndex;
                    nLastVisibleIndex[LastMaterialIndex]++;
                }
            }

            for (int i = lastInput.mListCharacterInfo.Count - 1; i >= Input.mListCharacterInfo.Count; i--)
            {
                int materialIndex = lastInput.mListCharacterInfo[i].materialReferenceIndex;
                if (orOneWoldFinishAni(i) && mWorldisPlayingAniList[i])
                {
                    if (lastInput.mListCharacterInfo[i].isVisible)
                    {
                        nLastVisibleIndex[materialIndex]--;
                        int nBeginIndex = nLastVisibleIndex[materialIndex] * oneSize;
                        lastInput.mListMeshInfo[materialIndex].RemoveQuadAt(nBeginIndex);
                    }

                    lastInput.RemoveCharacter(i);
                }
            }
        }

        private void PlayAni()
        {
            ClearOutputMeshInfoList();

            FillLastInput();
            FillInput();

            lastInput.Check();
            Input.Check();
            
            for(int i = 0; i < mWorldisPlayingAniList.Count; i++)
            {
                if (orOneWoldFinishAni(i) && mWorldisPlayingAniList[i])
                {
                    mWorldisPlayingAniList[i] = false;
                }
            }

            ResetVisibleCharacterList();
            List<int> nLastVisibleIndex = mVisibleCharacterList1;
            List<int> nNowVisibleIndex = mVisibleCharacterList2;

            ReSizeWorldAniBeginTimeList(Input, lastInput);

            for (int i = 0; i < Input.mListCharacterInfo.Count; i++)
            {
                int materialIndex = Input.mListCharacterInfo[i].materialReferenceIndex;
                if (orOneWoldFinishAni(i) && !mWorldisPlayingAniList[i])
                {
                    bool bChanged = false;
                    if (i < lastInput.mListCharacterInfo.Count)
                    {
                        bChanged = lastInput.mListCharacterInfo[i].character != Input.mListCharacterInfo[i].character;
                    }
                    else
                    {
                        bChanged = true;
                    }

                    if (!Input.mListCharacterInfo[i].isVisible)
                    {
                        bChanged = false;
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

                    if (i < lastInput.mListCharacterInfo.Count && lastInput.mListCharacterInfo[i].isVisible)
                    {
                        int LastMaterialIndex = lastInput.mListCharacterInfo[i].materialReferenceIndex;

                        int nBeginVertexIndex = outputMeshInfoList[LastMaterialIndex].vertices.Count;
                        AddIndices(LastMaterialIndex, nBeginVertexIndex);
                        for (int j = 0; j < oneSize; j++)
                        {
                            int nOirIndex = nLastVisibleIndex[LastMaterialIndex] * oneSize + j;
                            Vector3 oriPos = lastInput.mListMeshInfo[LastMaterialIndex].vertices[nOirIndex];
                            Color32 oriColor32 = lastInput.mListMeshInfo[LastMaterialIndex].colors32[nOirIndex];
                            Vector2 uv0 = lastInput.mListMeshInfo[LastMaterialIndex].uvs0[nOirIndex];
                            Vector2 uv2 = lastInput.mListMeshInfo[LastMaterialIndex].uvs2[nOirIndex];
                            Vector3 normal = lastInput.mListMeshInfo[LastMaterialIndex].normals[nOirIndex];
                            Vector4 tangent = lastInput.mListMeshInfo[LastMaterialIndex].tangents[nOirIndex];

                            float fScaleY = lastInput.mListMeshInfo[LastMaterialIndex].uvs2ScaleY[nOirIndex];

                            Vector3 targetPos = new Vector3(oriPos.x, oriPos.y + fTimePercent * fAniHeight, oriPos.z);
                            Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)((1 - fTimePercent) * 255));

                            GetUV2(ref uv2, ref fScaleY);
                            AddVertexInfo(LastMaterialIndex, targetPos, targetColor32, uv0, uv2, normal, tangent);
                        }
                    }

                    if (Input.mListCharacterInfo[i].isVisible)
                    {
                        int nBeginVertexIndex = outputMeshInfoList[materialIndex].vertices.Count;
                        AddIndices(materialIndex, nBeginVertexIndex);
                        for (int j = 0; j < oneSize; j++)
                        {
                            int nOirIndex = nNowVisibleIndex[materialIndex] * oneSize + j;
                            Vector3 oriPos = Input.mListMeshInfo[materialIndex].vertices[nOirIndex];
                            Color32 oriColor32 = Input.mListMeshInfo[materialIndex].colors32[nOirIndex];
                            Vector2 uv0 = Input.mListMeshInfo[materialIndex].uvs0[nOirIndex];
                            Vector2 uv2 = Input.mListMeshInfo[materialIndex].uvs2[nOirIndex];
                            Vector3 normal = Input.mListMeshInfo[materialIndex].normals[nOirIndex];
                            Vector4 tangent = Input.mListMeshInfo[materialIndex].tangents[nOirIndex];
                            float fScaleY = Input.mListMeshInfo[materialIndex].uvs2ScaleY[nOirIndex];

                            Vector3 targetPos = new Vector3(oriPos.x, oriPos.y - (1 - fTimePercent) * fAniHeight, oriPos.z);
                            Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)(fTimePercent * 255));
                            
                            GetUV2(ref uv2, ref fScaleY);
                            AddVertexInfo(materialIndex, targetPos, targetColor32, uv0, uv2, normal, tangent);
                        };


                    }
                }
                else
                {
                    if (Input.mListCharacterInfo[i].isVisible)
                    {
                        int nBeginVertexIndex = outputMeshInfoList[materialIndex].vertices.Count;
                        AddIndices(materialIndex, nBeginVertexIndex);
                        for (int j = 0; j < oneSize; j++)
                        {
                            int nOirIndex = nNowVisibleIndex[materialIndex] * oneSize + j;
                            Vector3 oriPos = Input.mListMeshInfo[materialIndex].vertices[nOirIndex];
                            Color32 oriColor32 = Input.mListMeshInfo[materialIndex].colors32[nOirIndex];
                            Vector2 uv0 = Input.mListMeshInfo[materialIndex].uvs0[nOirIndex];
                            Vector2 uv2 = Input.mListMeshInfo[materialIndex].uvs2[nOirIndex];
                            Vector3 normal = Input.mListMeshInfo[materialIndex].normals[nOirIndex];
                            Vector4 tangent = Input.mListMeshInfo[materialIndex].tangents[nOirIndex];
                            float fScaleY = Input.mListMeshInfo[materialIndex].uvs2ScaleY[nOirIndex];

                            GetUV2(ref uv2, ref fScaleY);
                            AddVertexInfo(materialIndex, oriPos, oriColor32, uv0, uv2, normal, tangent);
                        };
                    }
                }

                if (Input.mListCharacterInfo[i].isVisible)
                {
                    nNowVisibleIndex[materialIndex]++;
                }

                if (i < lastInput.mListCharacterInfo.Count && lastInput.mListCharacterInfo[i].isVisible)
                {
                    int LastMaterialIndex = lastInput.mListCharacterInfo[i].materialReferenceIndex;
                    nLastVisibleIndex[LastMaterialIndex]++;
                }

            }

            for (int i = Input.mListCharacterInfo.Count; i < lastInput.mListCharacterInfo.Count; i++)
            {
                int LastMaterialIndex = lastInput.mListCharacterInfo[i].materialReferenceIndex;

                if (orOneWoldFinishAni(i) && !mWorldisPlayingAniList[i])
                {
                    bool bChanged = true;
                    if (!lastInput.mListCharacterInfo[i].isVisible)
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
                    int nBeginVertexIndex = outputMeshInfoList[LastMaterialIndex].vertices.Count;
                    AddIndices(LastMaterialIndex, nBeginVertexIndex);
                    for (int j = 0; j < oneSize; j++)
                    {
                        int nOirIndex = nLastVisibleIndex[LastMaterialIndex] * oneSize + j;
                        Vector3 oriPos = lastInput.mListMeshInfo[LastMaterialIndex].vertices[nOirIndex];
                        Color32 oriColor32 = lastInput.mListMeshInfo[LastMaterialIndex].colors32[nOirIndex];
                        Vector2 uv0 = lastInput.mListMeshInfo[LastMaterialIndex].uvs0[nOirIndex];
                        Vector2 uv2 = lastInput.mListMeshInfo[LastMaterialIndex].uvs2[nOirIndex];
                        Vector3 normal = lastInput.mListMeshInfo[LastMaterialIndex].normals[nOirIndex];
                        Vector4 tangent = lastInput.mListMeshInfo[LastMaterialIndex].tangents[nOirIndex];
                        float fScaleY = lastInput.mListMeshInfo[LastMaterialIndex].uvs2ScaleY[nOirIndex];

                        Vector3 targetPos = new Vector3(oriPos.x, oriPos.y + fTimePercent * fAniHeight, oriPos.z);
                        Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)((1 - fTimePercent) * 255));
                        GetUV2(ref uv2, ref fScaleY);
                        AddVertexInfo(LastMaterialIndex, targetPos, targetColor32, uv0, uv2, normal, tangent);
                    }
                }

                if (lastInput.mListCharacterInfo[i].isVisible)
                {
                    nLastVisibleIndex[LastMaterialIndex]++;
                }

            }
            
            UpdateMesh();
        }
        
        public void UpdateMesh()
        {
            //Debug.Assert(mText.textInfo.meshInfo.Length == 1);
            //Debug.Assert(outputVertexs.Count / 4 * 6 == outputIndices.Count, outputVertexs.Count + " | " + outputIndices.Count);

            //解决Bug: Mesh.uv2 is out of bounds. The supplied array needs to be the same size as the Mesh.vertices array.
            for (int i = 0; i < mText.textInfo.materialCount; i++)
            {
                for (int j = outputMeshInfoList[i].vertices.Count; j < mText.textInfo.meshInfo[i].vertices.Length; j++)
                {
                    AddVertexInfo(i, Vector3.zero, s_DefaultColor, Vector2.zero, Vector2.zero, s_DefaultNormal, s_DefaultTangent);
                }
            }

            for (int i = 0; i < mText.textInfo.materialCount; i++)
            {
                mText.textInfo.meshInfo[i].mesh.Clear(false);

                mText.textInfo.meshInfo[i].mesh.SetVertices(outputMeshInfoList[i].vertices);
                mText.textInfo.meshInfo[i].mesh.SetUVs(0, outputMeshInfoList[i].uvs0);
                mText.textInfo.meshInfo[i].mesh.SetUVs(1, outputMeshInfoList[i].uvs2);
                mText.textInfo.meshInfo[i].mesh.SetColors(outputMeshInfoList[i].colors32);
                mText.textInfo.meshInfo[i].mesh.SetNormals(outputMeshInfoList[i].normals);
                mText.textInfo.meshInfo[i].mesh.SetTangents(outputMeshInfoList[i].tangents);
                mText.textInfo.meshInfo[i].mesh.SetTriangles(outputMeshInfoList[i].triangles, 0);

                mText.textInfo.meshInfo[i].mesh.RecalculateBounds();
            }
        }

        void ON_TEXT_CHANGED(UnityEngine.Object obj)
        {
            if (obj == mText)
            {
                if (!mWillFillInput.Equal(mText))
                {
                    InitTextMeshProMeshInfo(lastInput);
                    InitTextMeshProMeshInfo(Input);
                    InitTextMeshProMeshInfo(mWillFillInput);
                    InitMaxMeshSize();

                    TextBeatUtility.CopyTo(mWillFillInput, mText.textInfo);
                    InitUV2ScaleY(mWillFillInput);
                    BuildAni();
                }
            }
        }

        private void OnWillRenderObject()
        {
            BuildAni();
        }

        void BuildAni()
        {
           PlayAni();
        }

        void InitMaxMeshSize()
        {
            // 因为一直报错: Mesh.vertices is too small. The supplied vertex array has less vertices than are referenced by the triangles array.
            // 所以 初始化的时候就把尺寸调到最大
            int nMaxStringBuilerCapacity = GetCharacterMaxCount() * 2;
            for (int i = 0; i < mText.textInfo.materialCount; i++)
            {
                if (mText.textInfo.meshInfo[i].vertices.Length / 4 < nMaxStringBuilerCapacity)
                {
                    mText.textInfo.meshInfo[i].ResizeMeshInfo(nMaxStringBuilerCapacity);

                    mText.textInfo.meshInfo[i].mesh.uv = mText.textInfo.meshInfo[i].uvs0;
                    mText.textInfo.meshInfo[i].mesh.uv2 = mText.textInfo.meshInfo[i].uvs2;
                    mText.textInfo.meshInfo[i].mesh.colors32 = mText.textInfo.meshInfo[i].colors32;
                }
            }
        }

        void GetUV2(ref Vector2 uv2, ref float fScale)
        {
            float fNowScaleY = transform.lossyScale.y;
            float scaleDelta = fNowScaleY / fScale;
            if (scaleDelta == 0 || scaleDelta == float.PositiveInfinity)
            {
                return;
            }

            Vector2 oriUV2 = uv2;
            float modifyY = oriUV2.y * Mathf.Abs(scaleDelta);
            uv2 = new Vector2(oriUV2.x, modifyY);
            fScale = fNowScaleY;
        }

        bool orScaleYChanged()
        {
            return transform.lossyScale.y != m_previousLossyScaleY;
        }

        void InitUV2ScaleY(TextMeshProMeshInfo meshInfo)
        {
            for (int materialIndex = 0; materialIndex < mText.textInfo.materialCount; materialIndex++)
            {
                meshInfo.mListMeshInfo[materialIndex].uvs2ScaleY.Clear();
                for (int i = 0; i < meshInfo.mListMeshInfo[materialIndex].uvs2.Count; i++)
                {
                    meshInfo.mListMeshInfo[materialIndex].uvs2ScaleY.Add(transform.lossyScale.y);
                }
            }
        }

    }
}