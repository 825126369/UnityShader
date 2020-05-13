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

        private float fBeginAniTime = -100f;

        private TextMeshProMeshInfo lastInput = new TextMeshProMeshInfo();
        private TextMeshProMeshInfo Input = new TextMeshProMeshInfo();
        private List<InputInfo> InputList = new List<InputInfo>();
        private static List<TextMeshProMeshInfo.MeshInfo> outputMeshInfoList = new List<TextMeshProMeshInfo.MeshInfo>();

        private TMP_Text mText;
        private StringBuilder mStringBuilder;
        private StringBuilder lastStringBuilder;
        private String mString;
        private String lastString;
        private bool bLastBuild = false;

        private const int UInt64Length = 20;
        private const int oneSize = 4;

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

        void Start()
        {
            if (bUseNoGCStringBuilder)
            {
                InitNoGCStringBuilder();
            }
            else
            {
                lastString = mText.text;
            }
            
            mText.ForceMeshUpdate();
            TextBeatUtility.CopyTo(lastInput, mText.textInfo);
            bLastBuild = true;

            InitMaxMeshSize(); 
        }

        private int GetCharacterMaxCount()
        {
            return UInt64Length + prefix.Length;
        }
        
        private void InitNoGCStringBuilder()
        {
            int nMaxStringBuilerCapacity = GetCharacterMaxCount();
            if (mStringBuilder == null || mStringBuilder.Capacity < nMaxStringBuilerCapacity)
            {
                mStringBuilder = new StringBuilder(nMaxStringBuilerCapacity);
                mStringBuilder.GarbageFreeClear();
                mString = mStringBuilder.GetGarbageFreeString();

                lastStringBuilder = new StringBuilder(nMaxStringBuilerCapacity);
                lastStringBuilder.GarbageFreeClear();
                lastString = lastStringBuilder.GetGarbageFreeString();

                UpdateText(prefix, value);
            }
        }

        private void Update()
        {
            if (Time.time - fBeginUpdateTextTime > fUpdateTextMaxTime)
            {
                if (orCanChangeText())
                {
                    fBeginUpdateTextTime = Time.time;
                    if (bImmediatelyToTargetValue)
                    {
                        value = targetValue;
                        bImmediatelyToTargetValue = false;
                        UpdateText(prefix, value);
                    }
                    else if (value < targetValue)
                    {
                        value = value + (UInt64)UnityEngine.Random.Range(1, 100);
                        UpdateText(prefix, value);
                    }

                    //value = (UInt64)UnityEngine.Random.Range(1, UInt64.MaxValue);
                    //UpdateText(prefix, value);
                }
            }
        }

        private void LateUpdate()
        {
            BuildAni();
        }

        public void UpdateText(string prefixStr, UInt64 value)
        {
            if (bUseNoGCStringBuilder)
            {
                InitNoGCStringBuilder();
                mStringBuilder.GarbageFreeClear();
                mStringBuilder.Append(prefixStr);
                mStringBuilder.AppendUInt64(value);
                mStringBuilder.Align(mText.alignment);
                mText.text = mString;
                
                mText.havePropertiesChanged = true;
                mText.SetVerticesDirty();
                mText.SetLayoutDirty();
            }
            else
            {
                mText.text = prefix + value.ToString();
            }
        }

        public void UpdateText(UInt64 value)
        {
            if (bUseNoGCStringBuilder)
            {
                InitNoGCStringBuilder();
                mStringBuilder.GarbageFreeClear();
                mStringBuilder.AppendUInt64(value);
                mStringBuilder.Align(mText.alignment);
                mText.text = mString;

                mText.havePropertiesChanged = true;
                mText.SetVerticesDirty();
                mText.SetLayoutDirty();
            }
            else
            {
                mText.text = value.ToString();
            }
        }

        private bool orFinishAni()
        {
            return Time.time - fBeginAniTime > fAlphaTime;
        }

        private bool orCanChangeText()
        {
            return orFinishAni() && bLastBuild;
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
            for (int i = 0; i < outputMeshInfoList.Count; i++)
            {
                TextMeshProMeshInfo.mMeshInfoPool.Enqueue(outputMeshInfoList[i]);
            }

            outputMeshInfoList.Clear();

            for (int i = 0; i < mText.textInfo.materialCount; i++)
            {
                TextMeshProMeshInfo.MeshInfo mMeshInfo = null;
                if (TextMeshProMeshInfo.mMeshInfoPool.Count == 0)
                {
                    mMeshInfo = new TextMeshProMeshInfo.MeshInfo();
                }
                else
                {
                    mMeshInfo = TextMeshProMeshInfo.mMeshInfoPool.Dequeue();
                }

                mMeshInfo.vertices.Clear();
                mMeshInfo.colors32.Clear();
                mMeshInfo.uvs0.Clear();
                mMeshInfo.uvs2.Clear();
                mMeshInfo.normals.Clear();
                mMeshInfo.tangents.Clear();
                mMeshInfo.triangles.Clear();

                outputMeshInfoList.Add(mMeshInfo);
            }
        }

        private void PlayAni()
        {
            ClearOutputMeshInfoList();

            for (int k = 0; k < InputList.Count; k++)
            {
                int nLastVisibleIndex = 0;
                int nNowVisibleIndex = 0;

                float fBeginAniTime = InputList[k].fBeginAniTime;
                TextMeshProMeshInfo Input = InputList[k].Input;
                float fTimePercent = Mathf.Clamp01((Time.time - fBeginAniTime) / fAlphaTime);

                for (int i = 0; i < Input.mListCharacterInfo.Count; i++)
                {
                    int materialIndex = Input.mListCharacterInfo[i].materialReferenceIndex;
                    bool bChanged = false;
                    if (i < lastInput.mListCharacterInfo.Count)
                    {
                        bChanged = lastInput.mListCharacterInfo[i].character != Input.mListCharacterInfo[i].character;
                    }
                    else
                    {
                        bChanged = true;
                    }

                    if (bChanged)
                    {
                        if (i < lastInput.mListCharacterInfo.Count && lastInput.mListCharacterInfo[i].isVisible)
                        {
                            int LastMaterialIndex = lastInput.mListCharacterInfo[i].materialReferenceIndex;

                            int nBeginVertexIndex = outputMeshInfoList[LastMaterialIndex].vertices.Count;
                            AddIndices(LastMaterialIndex, nBeginVertexIndex);
                            for (int j = 0; j < oneSize; j++)
                            {
                                int nOirIndex = nLastVisibleIndex * oneSize + j;
                                Vector3 oriPos = lastInput.mListMeshInfo[LastMaterialIndex].vertices[nOirIndex];
                                Color32 oriColor32 = lastInput.mListMeshInfo[LastMaterialIndex].colors32[nOirIndex];
                                Vector2 uv0 = lastInput.mListMeshInfo[LastMaterialIndex].uvs0[nOirIndex];
                                Vector2 uv2 = lastInput.mListMeshInfo[LastMaterialIndex].uvs2[nOirIndex];
                                Vector3 normal = lastInput.mListMeshInfo[LastMaterialIndex].normals[nOirIndex];
                                Vector4 tangent = lastInput.mListMeshInfo[LastMaterialIndex].tangents[nOirIndex];

                                Vector3 targetPos = new Vector3(oriPos.x, oriPos.y + fTimePercent * fAniHeight, oriPos.z);
                                Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)((1 - fTimePercent) * 255));
                                AddVertexInfo(LastMaterialIndex, targetPos, targetColor32, uv0, uv2, normal, tangent);
                            }

                            lastInput.mListCharacterInfo[i].isPlayingAni = true;
                        }

                        if (Input.mListCharacterInfo[i].isVisible)
                        {
                            int nBeginVertexIndex = outputMeshInfoList[materialIndex].vertices.Count;
                            AddIndices(materialIndex, nBeginVertexIndex);
                            for (int j = 0; j < oneSize; j++)
                            {
                                int nOirIndex = nNowVisibleIndex * oneSize + j;
                                Vector3 oriPos = Input.mListMeshInfo[materialIndex].vertices[nOirIndex];
                                Color32 oriColor32 = Input.mListMeshInfo[materialIndex].colors32[nOirIndex];
                                Vector2 uv0 = Input.mListMeshInfo[materialIndex].uvs0[nOirIndex];
                                Vector2 uv2 = Input.mListMeshInfo[materialIndex].uvs2[nOirIndex];
                                Vector3 normal = Input.mListMeshInfo[materialIndex].normals[nOirIndex];
                                Vector4 tangent = Input.mListMeshInfo[materialIndex].tangents[nOirIndex];

                                Vector3 targetPos = new Vector3(oriPos.x, oriPos.y - (1 - fTimePercent) * fAniHeight, oriPos.z);
                                Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)(fTimePercent * 255));
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
                                int nOirIndex = nNowVisibleIndex * oneSize + j;
                                Vector3 oriPos = Input.mListMeshInfo[materialIndex].vertices[nOirIndex];
                                Color32 oriColor32 = Input.mListMeshInfo[materialIndex].colors32[nOirIndex];
                                Vector2 uv0 = Input.mListMeshInfo[materialIndex].uvs0[nOirIndex];
                                Vector2 uv2 = Input.mListMeshInfo[materialIndex].uvs2[nOirIndex];
                                Vector3 normal = Input.mListMeshInfo[materialIndex].normals[nOirIndex];
                                Vector4 tangent = Input.mListMeshInfo[materialIndex].tangents[nOirIndex];
                                AddVertexInfo(materialIndex, oriPos, oriColor32, uv0, uv2, normal, tangent);
                            };
                        }
                    }

                    if (Input.mListCharacterInfo[i].isVisible)
                    {
                        nNowVisibleIndex++;
                    }

                    if (i < lastInput.mListCharacterInfo.Count && lastInput.mListCharacterInfo[i].isVisible)
                    {
                        nLastVisibleIndex++;
                    }

                }
            }

            UpdateMesh();
        }
        
        public void UpdateMesh()
        {
            //Debug.Assert(mText.textInfo.meshInfo.Length == 1);
            //Debug.Assert(outputVertexs.Count / 4 * 6 == outputIndices.Count, outputVertexs.Count + " | " + outputIndices.Count);

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
                //if (orCanChangeText())
                {
                    if (mText.text != lastString)
                    {
                        bLastBuild = false;
                        fBeginAniTime = Time.time;
                        TextBeatUtility.CopyTo(Input, mText.textInfo);

                        InputInfo mInput = ObjectPool<InputInfo>.Pop();
                        mInput.fBeginAniTime = Time.time;
                        TextBeatUtility.CopyTo(mInput.Input, mText.textInfo);
                        InputList.Add(mInput);

                        PlayAni();
                    }
                }
            }
        }

        void BuildAni()
        {
            if (!orFinishAni())
            {
                PlayAni();
                bLastBuild = false;
            }
            else
            {

                if (!bLastBuild)
                {
                    if (bUseNoGCStringBuilder)
                    {
                        InitNoGCStringBuilder();
                        lastStringBuilder.GarbageFreeClear();
                        lastStringBuilder.Append(mText.text);
                    }
                    else
                    {
                        lastString = mText.text;
                    }
                    
                    TextBeatUtility.CopyTo(lastInput, mText.textInfo);
                    bLastBuild = true;

                    // 这里必须得重新ReSize 顶点信息，ReSize 完毕后，得重新赋值，否则会出现 某一帧 看不到的 Bug
                    //RefreshMeshSize();
                }
            }
        }

        //void RefreshMeshSize()
        //{
        //    for (int i = 0; i < mText.textInfo.materialCount; i++)
        //    {
        //        if (mText.textInfo.meshInfo[i].vertices.Length < mText.textInfo.meshInfo[i].mesh.vertices.Length)
        //        {
        //            int nReSize = mText.textInfo.meshInfo[i].mesh.vertices.Length / 4;
        //            mText.textInfo.meshInfo[i].ResizeMeshInfo(nReSize);

        //            //mText.textInfo.meshInfo[i].mesh.SetVertices(outputVertexs);
        //            //mText.textInfo.meshInfo[i].mesh.SetUVs(0, outputuv0s);
        //            //mText.textInfo.meshInfo[i].mesh.SetUVs(1, outputuv1s);
        //            //mText.textInfo.meshInfo[i].mesh.SetColors(outputColors);
        //            //mText.textInfo.meshInfo[i].mesh.SetNormals(outnormals);
        //            //mText.textInfo.meshInfo[i].mesh.SetTangents(outtangents);
        //            //mText.textInfo.meshInfo[i].mesh.SetTriangles(outputIndices, 0);
        //        }
        //    }
        //}

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
                }
            }
        }

    }
}