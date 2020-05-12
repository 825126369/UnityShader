using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TextBeat
{
    public class GCTest : MonoBehaviour
    {
        public TextMeshPro mText;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            mText.text = UnityEngine.Random.Range(1, UInt64.MaxValue).ToString();
        }
    }
}
