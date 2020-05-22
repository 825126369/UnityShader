using UnityEngine;
using System;

namespace TextBeat
{
    public class TextMeshProBeatTest : MonoBehaviour
    {
        public float fUpdateTextMaxTime;
        public int nTestType;
        private TextMeshProBeat mTextBeat;
        private float fBeginUpdateTextTime;
        private UInt64 value;

        void Start()
        {
            mTextBeat = GetComponent<TextMeshProBeat>();
        }

        private void Update()
        {
            if (nTestType == 1)
            {
                Test1();
            }
            else if (nTestType == 2)
            {
                Test2();
            }
            else if (nTestType == 3)
            {
                Test3();
            }
            else if (nTestType == 4)
            {
                Test4();
            }
            else if (nTestType == 5)
            {
                Test5();
            }
        }

        UInt64 testValue1 = 1000000000;
        private void Test5()
        {
            if (Time.time - fBeginUpdateTextTime > fUpdateTextMaxTime)
            {
                fBeginUpdateTextTime = Time.time;
                testValue1 += testValue1 * 10;
                mTextBeat.UpdateText(testValue1);
            }
        }

        private void Test4()
        {
            if (Time.time - fBeginUpdateTextTime > fUpdateTextMaxTime)
            {
                fBeginUpdateTextTime = Time.time;
                string AAA = "";
                int nStrLength = UnityEngine.Random.Range(0, 100);
                for (int i = 0; i < nStrLength; i++)
                {
                    AAA = AAA + UnityEngine.Random.Range(0, 10);
                }

                mTextBeat.UpdateText(AAA);
            }
        }

        private void Test3()
        {
            if (Time.time - fBeginUpdateTextTime > fUpdateTextMaxTime)
            {
                fBeginUpdateTextTime = Time.time;
                value = (ulong)UnityEngine.Random.Range(1, UInt64.MaxValue);
                mTextBeat.UpdateText(value);
            }
        }

        private void Test2()
        {
            if (Time.time - fBeginUpdateTextTime > fUpdateTextMaxTime)
            {
                fBeginUpdateTextTime = Time.time;
                value = (ulong)UnityEngine.Random.Range(1, 10000);
                if (UnityEngine.Random.Range(1, 3) == 1)
                {
                    value *= 10 ^ (ulong)UnityEngine.Random.Range(1, 10);
                }
                else
                {
                    value /= 10 ^ (ulong)UnityEngine.Random.Range(1, 10);
                }
                mTextBeat.UpdateText(value);
            }
        }

        float testValue = 10005f;
        private void Test1()
        {
            testValue -= Time.deltaTime;

            UInt64 t = (UInt64)Mathf.FloorToInt(testValue);
            mTextBeat.UpdateText(t);
        }
    }
}