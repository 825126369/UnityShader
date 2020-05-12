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
        public bool bUseNoGCStringBuilder = false;
        public char prefix = '$';
        public UInt64 value = 10000000000;
        public UInt64 targetValue = 1000000000000000;
        public float fUpdateTextMaxTime = 0.5f;

        public float fAlphaTime = 0.5f;
        public float fAniHeight = 100;

        private float fBeginUpdateTextTime;
        private float fBeginAniTime = -100f;

        private bool hasTextChanged;

        private TextMeshProMeshInfo lastInput = new TextMeshProMeshInfo();
        private TextMeshProMeshInfo Input = new TextMeshProMeshInfo();
        private static List<Vector3> outputVertexs = new List<Vector3>();
        private static List<Color32> outputColors = new List<Color32>();
        private static List<Vector2> outputuv0s = new List<Vector2>();
        private static List<Vector2> outputuv1s = new List<Vector2>();
        private static List<Vector3> outnormals = new List<Vector3>();
        private static List<Vector4> outtangents = new List<Vector4>();
        private static List<int> outputIndices = new List<int>();

        private TMP_Text mText;
        private StringBuilder mStringBuilder;
        private StringBuilder lastStringBuilder;
        private String mString;
        private String lastString;
        private bool bLastBuild = false;

        private const int UInt64Length = 20;
        private int nMaxStringBuilerCapacity;
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
            nMaxStringBuilerCapacity = UInt64Length + 1;
            if (bUseNoGCStringBuilder)
            {
                InitNoGCStringBuilder();
            }
            else
            {
                lastString = mText.text;
            }

            StartCoroutine(AnimateVertexColors());
        }
        
        private void InitNoGCStringBuilder()
        {
            if (mStringBuilder == null)
            {
                mStringBuilder = new StringBuilder(nMaxStringBuilerCapacity);
                mStringBuilder.GarbageFreeClear();
                mString = mStringBuilder.GetGarbageFreeString();

                lastStringBuilder = new StringBuilder(nMaxStringBuilerCapacity);
                lastStringBuilder.GarbageFreeClear();
                lastString = lastStringBuilder.GetGarbageFreeString();

                UpdateText(value);
            }
        }

        private void Update()
        {
            if (Time.time - fBeginUpdateTextTime > fUpdateTextMaxTime && orFinishAni1())
            {
                fBeginUpdateTextTime = Time.time;
                value = value + 1;
                //value = value + (UInt64)UnityEngine.Random.Range(1, 1000000000);
                UpdateText(value);
            }
        }

        private void UpdateText(UInt64 value)
        {
            if (bUseNoGCStringBuilder)
            {
                InitNoGCStringBuilder();
                mStringBuilder.GarbageFreeClear();
                mStringBuilder.Append(prefix);
                mStringBuilder.AppendUInt64(value);
                //mStringBuilder.Align(mText.alignment);
                mText.text = mString;

                //mText.cachedTextGenerator.Invalidate();
                mText.SetVerticesDirty();
                mText.SetLayoutDirty();
            }
            else
            {
                mText.text = prefix + value.ToString();
            }
        }

        private bool orFinishAni()
        {
            return Time.time - fBeginAniTime > fAlphaTime;
        }

        private bool orFinishAni1()
        {
            return orFinishAni() && bLastBuild;
        }

        private void AddVertexInfo(Vector3 pos, Color32 color, Vector2 uv0, Vector2 uv1, Vector3 normal, Vector4 tangent)
        {
            outputVertexs.Add(pos);
            outputColors.Add(color);
            outputuv0s.Add(uv0);
            outputuv1s.Add(uv1);
            outnormals.Add(normal);
            outtangents.Add(tangent);
        }

        private void AddIndices(int nBeginIndex)
        {
            outputIndices.Add(nBeginIndex);
            outputIndices.Add(nBeginIndex + 1);
            outputIndices.Add(nBeginIndex + 2);

            outputIndices.Add(nBeginIndex + 2);
            outputIndices.Add(nBeginIndex + 3);
            outputIndices.Add(nBeginIndex + 0);
        }

        private void PlayAni()
        {
            outputVertexs.Clear();
            outputColors.Clear();
            outputuv0s.Clear();
            outputuv1s.Clear();
            outnormals.Clear();
            outtangents.Clear();
            outputIndices.Clear();

            float fTimePercent = Mathf.Clamp01((Time.time - fBeginAniTime) / fAlphaTime);

            int nMeshCount = mText.textInfo.meshInfo.Length;

            for (int i = 0; i < Input.characterCount; i++)
            {
                int materialIndex = Input.m_TMP_CharacterInfos[i].materialReferenceIndex;
                bool bChanged = false;
                if (i < lastInput.characterCount)
                {
                    bChanged = lastInput.m_TMP_CharacterInfos[i].character != Input.m_TMP_CharacterInfos[i].character;
                }
                else
                {
                    bChanged = true;
                }
                
                if (bChanged)
                {
                    if (i < lastInput.characterCount)
                    {
                        int LastMaterialIndex = lastInput.m_TMP_CharacterInfos[i].materialReferenceIndex;
                        AddIndices(outputVertexs.Count);
                        for (int j = 0; j < oneSize; j++)
                        {
                            int nOirIndex = i * oneSize + j;
                            Vector3 oriPos = lastInput.m_TMP_MeshInfos[LastMaterialIndex].vertices[nOirIndex];
                            Color32 oriColor32 = lastInput.m_TMP_MeshInfos[LastMaterialIndex].colors32[nOirIndex];
                            Vector2 uv0 = lastInput.m_TMP_MeshInfos[LastMaterialIndex].uvs0[nOirIndex];
                            Vector2 uv2 = lastInput.m_TMP_MeshInfos[LastMaterialIndex].uvs2[nOirIndex];
                            Vector3 normal = lastInput.m_TMP_MeshInfos[LastMaterialIndex].normals[nOirIndex];
                            Vector4 tangent = lastInput.m_TMP_MeshInfos[LastMaterialIndex].tangents[nOirIndex];

                            Vector3 targetPos = new Vector3(oriPos.x, oriPos.y + fTimePercent * fAniHeight, oriPos.z);
                            Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)((1 - fTimePercent) * 255));
                            AddVertexInfo(targetPos, targetColor32, uv0, uv2, normal, tangent);
                        }
                    }
                    
                    AddIndices(outputVertexs.Count);
                    for (int j = 0; j < oneSize; j++)
                    {
                        int nOirIndex = i * oneSize + j;
                        Vector3 oriPos = Input.m_TMP_MeshInfos[materialIndex].vertices[nOirIndex];
                        Color32 oriColor32 = Input.m_TMP_MeshInfos[materialIndex].colors32[nOirIndex];
                        Vector2 uv0 = Input.m_TMP_MeshInfos[materialIndex].uvs0[nOirIndex];
                        Vector2 uv2 = Input.m_TMP_MeshInfos[materialIndex].uvs2[nOirIndex];
                        Vector3 normal = Input.m_TMP_MeshInfos[materialIndex].normals[nOirIndex];
                        Vector4 tangent = Input.m_TMP_MeshInfos[materialIndex].tangents[nOirIndex];

                        Vector3 targetPos = new Vector3(oriPos.x, oriPos.y - (1 - fTimePercent) * fAniHeight, oriPos.z);
                        Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)(fTimePercent * 255));
                        AddVertexInfo(targetPos, targetColor32, uv0, uv2, normal, tangent);
                    };
                }
                else
                {

                    AddIndices(outputVertexs.Count);
                    for (int j = 0; j < oneSize; j++)
                    {
                        int nOirIndex = i * oneSize + j;
                        Vector3 oriPos = Input.m_TMP_MeshInfos[materialIndex].vertices[nOirIndex];
                        Color32 oriColor32 = Input.m_TMP_MeshInfos[materialIndex].colors32[nOirIndex];
                        Vector2 uv0 = Input.m_TMP_MeshInfos[materialIndex].uvs0[nOirIndex];
                        Vector2 uv2 = Input.m_TMP_MeshInfos[materialIndex].uvs2[nOirIndex];
                        Vector3 normal = Input.m_TMP_MeshInfos[materialIndex].normals[nOirIndex];
                        Vector4 tangent = Input.m_TMP_MeshInfos[materialIndex].tangents[nOirIndex];
                        AddVertexInfo(oriPos, oriColor32, uv0, uv2, normal, tangent);
                    };
                }

            }

            UpdateMesh();
        }


        public void UpdateMesh()
        {
            Debug.Assert(mText.textInfo.meshInfo.Length == 1);
            Debug.Assert(outputVertexs.Count / 4 * 6 == outputIndices.Count, outputVertexs.Count + " | " + outputIndices.Count);

            for (int i = 0; i < mText.textInfo.meshInfo.Length; i++)
            {
                mText.textInfo.meshInfo[i].mesh.Clear();
                mText.textInfo.meshInfo[i].mesh.vertices = outputVertexs.ToArray();
                mText.textInfo.meshInfo[i].mesh.uv = outputuv0s.ToArray();
                mText.textInfo.meshInfo[i].mesh.uv2 = outputuv1s.ToArray();
                mText.textInfo.meshInfo[i].mesh.colors32 = outputColors.ToArray();
                mText.textInfo.meshInfo[i].mesh.normals = outnormals.ToArray();
                mText.textInfo.meshInfo[i].mesh.tangents = outtangents.ToArray();

                mText.textInfo.meshInfo[i].mesh.triangles = outputIndices.ToArray();
                mText.textInfo.meshInfo[i].mesh.RecalculateBounds();
            }
        }

        void ON_TEXT_CHANGED(UnityEngine.Object obj)
        {
            if (obj == mText)
            {
                hasTextChanged = true;

                if (hasTextChanged)
                {
                    hasTextChanged = false;

                    if (orFinishAni1())
                    {
                        if (mText.text != lastString)
                        {
                            bLastBuild = false;
                            fBeginAniTime = Time.time;
                            TextBeatUtility.CopyTo(Input, mText.textInfo);
                            Debug.Log("String: " + mText.text);
                            PlayAni();
                        }
                    }
                }
            }
        }

        IEnumerator AnimateVertexColors()
        {
            mText.ForceMeshUpdate();

            lastString = mText.text;
            TextBeatUtility.CopyTo(lastInput, mText.textInfo);
            bLastBuild = true;

            int loopCount = 0;
            
            while (true)
            {
                if (hasTextChanged)
                {
                    hasTextChanged = false;

                    if (orFinishAni1())
                    {
                        if (mText.text != lastString)
                        {
                            bLastBuild = false;
                            fBeginAniTime = Time.time;
                            TextBeatUtility.CopyTo(Input, mText.textInfo);
                            Debug.Log("String: " + mText.text);
                        }
                    }
                }

                if (!orFinishAni())
                {
                    PlayAni();
                    bLastBuild = false;
                }
                else
                {

                    if (!bLastBuild)
                    {
                        lastString = mText.text;
                        TextBeatUtility.CopyTo(lastInput, mText.textInfo);
                        bLastBuild = true;
                        Debug.Log("lastString: " + mText.text);

                        for (int i = 0; i < mText.textInfo.meshInfo.Length; i++)
                        {
                            mText.textInfo.meshInfo[i].vertices = null;
                        }
                    }
                }

                loopCount += 1;
                yield return 0;
            }
        }
    }
}