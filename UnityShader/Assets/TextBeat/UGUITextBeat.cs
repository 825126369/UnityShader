using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UGUITextBeat : BaseMeshEffect 
{
    public bool bUseNoGCStringBuilder = true;
    public char prefix = '$';
    public UInt64 value = 10000000000;
    public UInt64 targetValue = 1000000000000000;
    public float fUpdateTextMaxTime = 0.5f;

    public float fAlphaTime = 0.5f;
    public float Height = 100;

    private float fBeginUpdateTextTime;
    private float fBeginAniTime = -100f;
    
    private List<UIVertex> LastInput = new List<UIVertex>();
    private List<UIVertex> input = new List<UIVertex>();
    private static List<UIVertex> output = new List<UIVertex>();
    private static List<int> outputIndices = new List<int>();

    private bool bInitLastInput = false;
    private bool bLastBuild = true;

    private Text mText;
    private StringBuilder mStringBuilder;
    private StringBuilder lastStringBuilder;
    private String mString;
    private String lastString;

    private const int oneSize = 6;
    private const int UInt64Length = 20;
    private int nMaxStringBuilerCapacity;

    protected override void Start()
    {
        nMaxStringBuilerCapacity = UInt64Length + 1;
        mText = GetComponent<Text>();

        if (bUseNoGCStringBuilder)
        {
            InitNoGCStringBuilder();
        }
        else
        {
            lastString = mText.text;
        }
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
        }

        if(!orFinishAni1())
        {
            mText.SetVerticesDirty();
        }
    }

    private void UpdateText(UInt64 value)
    {
        if(bUseNoGCStringBuilder)
        {
            InitNoGCStringBuilder();
            mStringBuilder.GarbageFreeClear();
            mStringBuilder.Append(prefix);
            mStringBuilder.AppendUInt64(value);
            mStringBuilder.Align(mText.alignment);
            mText.text = mString;

            mText.cachedTextGenerator.Invalidate();
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

    public void AddUIVertexQuad()
    {
        int startIndex = output.Count;

        for (int i = 0; i < oneSize; i++)
        {
            outputIndices.Add(startIndex + i);
        }
    }

    private void PlayAni(VertexHelper vh)
    {
        int nStringIndex = 0;
        float fTimePercent = Mathf.Clamp01((Time.time - fBeginAniTime) / fAlphaTime);
        for (int i = 0; i < input.Count; i += oneSize)
        {
            if (orChanged(nStringIndex))
            {
                if (i + oneSize - 1 < LastInput.Count)
                {
                    AddUIVertexQuad();
                    for (int j = 0; j < oneSize; j++)
                    {
                        UIVertex vt = LastInput[i + j];
                        vt.position += new Vector3(0, fTimePercent * Height, 0);
                        vt.color *= new Color(1, 1, 1, 1 - fTimePercent);
                        output.Add(vt);
                    }
                }

                AddUIVertexQuad();
                for (int j = 0; j < oneSize; j++)
                {
                    UIVertex vt = input[i + j];
                    vt.position += new Vector3(0, -Height + fTimePercent * Height, 0);
                    vt.color *= new Color(1, 1, 1, fTimePercent);
                    output.Add(vt);
                }
            }
            else
            {
                AddUIVertexQuad();
                for (int j = 0; j < oneSize; j++)
                {
                    UIVertex vt = input[i + j];
                    output.Add(vt);
                }
            }
                
            nStringIndex++;
        }
    }

    public bool orFinishAni()
    {
        return Time.time - fBeginAniTime > fAlphaTime;
    }

    public bool orFinishAni1()
    {
        return orFinishAni() && bLastBuild == false;
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (orFinishAni())
        {
            if (!bInitLastInput)
            {
                if (bUseNoGCStringBuilder)
                {
                    InitNoGCStringBuilder();
                    lastStringBuilder.GarbageFreeClear();
                    lastStringBuilder.Append(mText.text);
                }else
                {
                    lastString = mText.text;
                }

                LastInput.Clear();
                vh.GetUIVertexStream(LastInput);
                bInitLastInput = true;

                //Debug.Log("LastInput.Count: " + LastInput.Count);
            }

            bLastBuild = false;

            if (mText.text != lastString)
            {
                fBeginAniTime = Time.time;
                input.Clear();
                vh.GetUIVertexStream(input);
                bInitLastInput = false;

                //Debug.Log("input.Count: " + input.Count);
            }
        }

        if (!orFinishAni())
        {
            vh.Clear();
            output.Clear();
            outputIndices.Clear();
            PlayAni(vh);
            vh.AddUIVertexStream(output, outputIndices);
            bLastBuild = true;
        }
    }
}
