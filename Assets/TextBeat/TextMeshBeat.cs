using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;

namespace TextBeat
{
    public class TextMeshBeat : MonoBehaviour
    {
        private TextMesh mText;
        private MeshRenderer mMeshRenderer;
            
        void Start()
        {
            mText = GetComponent<TextMesh>();
        }

       
        private void LateUpdate()
        {
            BuildAni();
        }

        void BuildAni()
        {
            List<CharacterInfo> mListInfo = new List<CharacterInfo>();
            for (int i = 0; i < mText.text.Length; i++)
            {
                char c = mText.text[i];
                CharacterInfo mCharacterInfo;
                if (mText.font.GetCharacterInfo(c, out mCharacterInfo))
                {
                    mListInfo.Add(mCharacterInfo);
                }
            }

            Debug.Log(mListInfo.Count);
        }

    }
}