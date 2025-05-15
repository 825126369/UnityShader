using UnityEngine;
using System;

namespace TextBeat
{
    public class CustomerTextMeshBeatTest : MonoBehaviour
    {
        public float fUpdateTextMaxTime;
        public int nTestType;
        private CustomerTextMeshBeat mTextBeat;
        private float fBeginUpdateTextTime;
        private UInt64 value;

        void Start()
        {
            mTextBeat = GetComponent<CustomerTextMeshBeat>();
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
            else if (nTestType == 6)
            {
                Test6();
            }
            else if (nTestType == 7)
            {
                Test7();
            }
            else if (nTestType == 8)
            {
                Test8();
            }
        }

        UInt64 testValue8 = 0;
        private void Test8()
        {
            if (Time.time - fBeginUpdateTextTime >= fUpdateTextMaxTime)
            {
                fBeginUpdateTextTime = Time.time;
                string text = string.Empty;

                int strLength = UnityEngine.Random.Range(0, 10);
                for (int i = 0; i < strLength; i++)
                {
                    int nType = UnityEngine.Random.Range(0, 4);
                    if (nType == 0)
                    {
                        //text += " ";
                    }
                    if (nType == 1)
                    {
                        text += "<sprite=" + UnityEngine.Random.Range(0, 30) + ">";
                    }
                    else
                    {
                        text += UnityEngine.Random.Range(0, 10);
                    }
                    text += i;
                }

                mTextBeat.UpdateText(text);

                if (testValue7 > 20)
                {
                    testValue7 = 0;
                }
            }
        }

        UInt64 testValue7 = 0;
        private void Test7()
        {
            if (Time.time - fBeginUpdateTextTime >= fUpdateTextMaxTime)
            {
                fBeginUpdateTextTime = Time.time;
                string text = "ABCDEFG<sprite=" + testValue7++ + ">";
                mTextBeat.UpdateText(text);

                if (testValue7 > 20)
                {
                    testValue7 = 0;
                }
            }
        }

        UInt64 testValue2 = 1000000000;
        private void Test6()
        {
            if (Time.time - fBeginUpdateTextTime >= fUpdateTextMaxTime)
            {
                fBeginUpdateTextTime = Time.time;
                testValue2 += 1;
                mTextBeat.UpdateText(testValue2);
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
            if (Time.time - fBeginUpdateTextTime >= fUpdateTextMaxTime)
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
            if (Time.time - fBeginUpdateTextTime >= fUpdateTextMaxTime)
            {
                fBeginUpdateTextTime = Time.time;
                value = (ulong)UnityEngine.Random.Range(1, UInt64.MaxValue);
                mTextBeat.UpdateText(value);
            }
        }

        private void Test2()
        {
            if (Time.time - fBeginUpdateTextTime >= fUpdateTextMaxTime)
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