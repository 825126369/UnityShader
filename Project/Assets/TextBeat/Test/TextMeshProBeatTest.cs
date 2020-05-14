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
        
    public int[] nMultuile = new int[4]
    {
        10000, 30000, 2000000, 50000000,
    };

    public float[] AddCoef = new float[4]
    {
            0.0123f, 0.02789f, 0.0323432f, 0.042470f,
    };
     
    public TextMeshProBeat[] mTextMeshProBeatList = new TextMeshProBeat[4];

    private float fLastTime = 0.0f;
    public float fSwitchInternalTime = 5.0f;

    void Start()
    {
        for (int i = 0; i < mTextMeshProBeatList.Length; i++)
        {
            mTextMeshProBeatList[i].fUpdateTextMaxTime = fUpdateTextMaxTime;
            mTextMeshProBeatList[i].fAlphaTime = fAlphaTime;
            mTextMeshProBeatList[i].fAniHeight = fAniHeight;
            mTextMeshProBeatList[i].value = (ulong)UnityEngine.Random.Range(1, 10000);
            mTextMeshProBeatList[i].targetValue = mTextMeshProBeatList[i].value;
        }
    }

    private void Update()
    {
        for (int i = 0; i < mTextMeshProBeatList.Length; i++)
        {
            mTextMeshProBeatList[i].targetValue += (ulong)(nMultuile[i] * AddCoef[i]);
        }

        if (Time.time - fLastTime > fSwitchInternalTime)
        {
            fLastTime = Time.time;

            for (int i = 0; i < mTextMeshProBeatList.Length; i++)
            {
                mTextMeshProBeatList[i].bImmediatelyToTargetValue = true;
            }
        }
    }
}
