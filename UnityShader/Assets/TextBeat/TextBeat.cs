using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TextBeat : BaseMeshEffect 
{
    public class UIVertexInfo
    {
        public string text;
        public List<UIVertex> input = new List<UIVertex>();
    }
    
    public int value = 1000;
    public int targetValue = 1000000000;
	public float fAlphaTime = 0.5f;
    public float fUpdateTextMaxTime = 0.5f;
    private float fBeginUpdateTextTime;
    
    private float fBeginAniTime = -100f;
	public float Height = 100;

    List<UIVertex> LastInput = new List<UIVertex>();
    List<UIVertex> input = new List<UIVertex>();
    List<UIVertex> output = new List<UIVertex>();
    List<int> outputIndices = new List<int>();
    private bool bInitLastInput = false;
    private bool bLastBuild = true;

    private int oneSize = 6;

    Queue<UIVertexInfo> mUIVertexInfoList = new Queue<UIVertexInfo>();

    private Text mText;
    private StringBuilder mStringBuilder;
    private StringBuilder lastStringBuilder;
    private String mString;
    private String lastString;

    protected override void Start()
    {
        mText = GetComponent<Text>();

        mStringBuilder = new StringBuilder(64);
        mStringBuilder.GarbageFreeClear();
        mString = mStringBuilder.GetGarbageFreeString();
        mText.text = mString;

        lastStringBuilder = new StringBuilder(64);
        lastStringBuilder.GarbageFreeClear();
        lastString = lastStringBuilder.GetGarbageFreeString();
    }

    private void Update()
    {
        if (Time.time - fBeginUpdateTextTime > fUpdateTextMaxTime && orFinishAni1())
        {
            fBeginUpdateTextTime = Time.time;
            value = value + UnityEngine.Random.Range(1, 10000);

            mStringBuilder.GarbageFreeClear();
            mStringBuilder.ConcatFormat<int>("{0}", value);
            mText.text = mString;

            mText.SetLayoutDirty();
            mText.SetVerticesDirty();
            //mText.FontTextureChanged();
        }

        if(!orFinishAni1())
        {
            mText.SetVerticesDirty();
            mText.SetMaterialDirty();
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
                lastStringBuilder.GarbageFreeClear();
                lastStringBuilder.ConcatFormat<string>("{0}", mText.text);

                LastInput.Clear();
                vh.GetUIVertexStream(LastInput);
                bInitLastInput = true;

                Debug.Log("LastInput.Count: " + LastInput.Count);
            }

            bLastBuild = false;

            if (mText.text != lastString)
            {
                fBeginAniTime = Time.time;
                input.Clear();
                vh.GetUIVertexStream(input);
                bInitLastInput = false;

                Debug.Log("input.Count: " + input.Count);
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
