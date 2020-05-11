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

        public TMP_CharacterInfo[] cacheCharacterInfo;
        private TMP_MeshInfo[] cachedMeshInfo = null;
        private TMP_TextInfo mInputTextInfo = null;
        private static List<Vector3> outputVertexs = new List<Vector3>();
        private static List<Color32> outputColors = new List<Color32>();
        private static List<Vector2> outputuv0s = new List<Vector2>();
        private static List<Vector2> outputuv1s = new List<Vector2>();
        private static List<int> outputIndices = new List<int>();

        private TMP_Text mText;
        private StringBuilder mStringBuilder;
        private StringBuilder lastStringBuilder;
        private String mString;
        private String lastString;
        private bool bLastBuild = true;
        private bool bInitLastInput = false;

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
            StartCoroutine(AnimateVertexColors());

            nMaxStringBuilerCapacity = UInt64Length + 1;
            if (bUseNoGCStringBuilder)
            {
                InitNoGCStringBuilder();
            }
            else
            {
                lastString = mText.text;
            }
        }

        void ON_TEXT_CHANGED(UnityEngine.Object obj)
        {
            if (obj == mText)
                hasTextChanged = true;
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
                //value = value + (UInt64)UnityEngine.Random.Range(1, 1000000000);
                value = value + 1;

                UpdateText(value);

                Debug.Log("Update");
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

        private bool orChanged(int nIndex)
        {
            if (nIndex < lastString.Length)
            {
                return lastString[nIndex] != mText.text[nIndex];
            }
            else
            {
                return true;
            }
        }

        private bool orFinishAni()
        {
            return Time.time - fBeginAniTime > fAlphaTime;
        }

        private bool orFinishAni1()
        {
            return orFinishAni() && bLastBuild == false;
        }

        private void AddVertexInfo(Vector3 pos, Color32 color, Vector2 uv0, Vector2 uv1)
        {
            outputVertexs.Add(pos);
            outputColors.Add(color);
            outputuv0s.Add(uv0);
            outputuv1s.Add(uv1);
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
            outputIndices.Clear();
            outputuv0s.Clear();
            outputuv1s.Clear();

            float fTimePercent = Mathf.Clamp01((Time.time - fBeginAniTime) / fAlphaTime);
            TMP_TextInfo textInfo = mText.textInfo;
            int characterCount = mText.textInfo.characterCount;
            int lastCharacterCount = lastString.Length;
            
            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                bool bChanged = false;
                if (i < lastCharacterCount)
                {
                    bChanged = lastString[i] != charInfo.character;
                }
                else
                {
                    bChanged = true;
                }
                
                if (bChanged)
                {
                    if (i < lastCharacterCount)
                    {
                        int LastMaterialIndex = cacheCharacterInfo[i].materialReferenceIndex;
                        AddIndices(outputVertexs.Count);
                        for (int j = 0; j < oneSize; j++)
                        {
                            int nOirIndex = i * oneSize + j;
                            Debug.Assert(nOirIndex < cachedMeshInfo[LastMaterialIndex].vertices.Length, nOirIndex + " | " + cachedMeshInfo[LastMaterialIndex].vertices.Length);
                            Vector3 oriPos = cachedMeshInfo[LastMaterialIndex].vertices[nOirIndex];
                            Color32 oriColor32 = cachedMeshInfo[LastMaterialIndex].colors32[nOirIndex];
                            Vector2 uv0 = cachedMeshInfo[LastMaterialIndex].uvs0[nOirIndex];
                            Vector2 uv2 = cachedMeshInfo[LastMaterialIndex].uvs2[nOirIndex];

                            Vector3 targetPos = new Vector3(oriPos.x, fTimePercent * fAniHeight, oriPos.z);
                            Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)((1 - fTimePercent) * 255));
                            AddVertexInfo(targetPos, targetColor32, uv0, uv2);
                        }
                    }

                    AddIndices(outputVertexs.Count);
                    for (int j = 0; j < oneSize; j++)
                    {
                        int nOirIndex = i * oneSize + j;
                        Vector3 oriPos = textInfo.meshInfo[materialIndex].vertices[nOirIndex];
                        Color32 oriColor32 = textInfo.meshInfo[materialIndex].colors32[nOirIndex];
                        Vector2 uv0 = textInfo.meshInfo[materialIndex].uvs0[nOirIndex];
                        Vector2 uv2 = textInfo.meshInfo[materialIndex].uvs2[nOirIndex];

                        Vector3 targetPos = new Vector3(oriPos.x, -(1 - fTimePercent) * fAniHeight, oriPos.z);
                        Color32 targetColor32 = new Color32(oriColor32.r, oriColor32.g, oriColor32.b, (byte)(fTimePercent * 255));
                        AddVertexInfo(targetPos, targetColor32, uv0, uv2);
                    };
                }
                else
                {

                    AddIndices(outputVertexs.Count);
                    for (int j = 0; j < oneSize; j++)
                    {
                        int nOirIndex = i * oneSize + j;
                        Vector3 oriPos = textInfo.meshInfo[materialIndex].vertices[nOirIndex];
                        Color32 oriColor32 = textInfo.meshInfo[materialIndex].colors32[nOirIndex];
                        Vector2 uv0 = textInfo.meshInfo[materialIndex].uvs0[nOirIndex];
                        Vector2 uv2 = textInfo.meshInfo[materialIndex].uvs2[nOirIndex];
                        AddVertexInfo(oriPos, oriColor32, uv0, uv2);
                    };
                }

            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].vertices = outputVertexs.ToArray();
                textInfo.meshInfo[i].uvs0 = outputuv0s.ToArray();
                textInfo.meshInfo[i].uvs2 = outputuv1s.ToArray();
                textInfo.meshInfo[i].colors32 = outputColors.ToArray();
                textInfo.meshInfo[i].triangles = outputIndices.ToArray();
                
                Mesh mesh = textInfo.meshInfo[i].mesh;
                mesh.Clear();
                //mesh.MarkDynamic();

                mesh.vertices = textInfo.meshInfo[i].vertices;
                mesh.uv = textInfo.meshInfo[i].uvs0;
                mesh.uv2 = textInfo.meshInfo[i].uvs2;
                mesh.colors32 = textInfo.meshInfo[i].colors32;
                mesh.triangles = textInfo.meshInfo[i].triangles;

                mesh.RecalculateBounds();
            }

            hasTextChanged = true;
        }

        IEnumerator AnimateVertexColors()
        {
            mText.ForceMeshUpdate();

            int loopCount = 0;
            hasTextChanged = true;

            TMP_TextInfo textInfo = mText.textInfo;
            cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
            cacheCharacterInfo = textInfo.characterInfo.Clone() as TMP_CharacterInfo[];

            while (true)
            {
                if (hasTextChanged)
                {
                    if (orFinishAni())
                    {
                        if (!bInitLastInput)
                        {
                            lastString = mText.text;
                            cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
                            cacheCharacterInfo = textInfo.characterInfo.Clone() as TMP_CharacterInfo[];
                            bInitLastInput = true;
                        }

                        bLastBuild = false;

                        if (mText.text != lastString)
                        {
                            fBeginAniTime = Time.time;
                            bInitLastInput = false;

                            mInputTextInfo = mText.textInfo;
                        }
                    }
                    
                    hasTextChanged = false;
                }

                if (!orFinishAni())
                {
                    PlayAni();
                    bLastBuild = true;
                }

                loopCount += 1;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}