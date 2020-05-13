using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextBeat;

public class TextMeshProBeatTest : MonoBehaviour
{
    public float fUpdateTextMaxTime = 0.5f;
    private float fBeginUpdateTextTime;
    public float fAlphaTime = 0.5f;
    public float fAniHeight = 100;

    public TextMeshProBeat[] mTextMeshProBeat;
    
    void Start()
    {
        mTextMeshProBeat.fAlphaTime = fAlphaTime;
        mTextMeshProBeat.fAniHeight = fAniHeight;
    }

    private void ResetValue()
    {

    }

    private void Update()
    {
        if (Time.time - fBeginUpdateTextTime > fUpdateTextMaxTime && orCanChangeText())
        {
            fBeginUpdateTextTime = Time.time;
            value = value + 1;
            //value = (UInt64)UnityEngine.Random.Range(1, UInt64.MaxValue);
            UpdateText(prefix, value);
        }

        BuildAni();
    }
}
