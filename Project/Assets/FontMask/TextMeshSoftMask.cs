using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TextMeshSoftMask : MonoBehaviour
{
    public TextMesh mTextMesh;
    public Vector4[] _SoftClipList = new Vector4[1];
    public Vector4[] uvScaleOffsetList = new Vector4[1];
    int nTextCount = 0;
    public float fOffsetY = 0.0f;
    public float fOffsetX = 0.0f;
    public float fScale = 1.0f;

    private SpriteRenderer m_spriteRenderer;
    private MaterialPropertyBlock m_materialProperty;

    void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_materialProperty = new MaterialPropertyBlock();

        Debug.Log("fontSize: " + mTextMesh.font.fontSize);
    }

    int GetWordLength()
    {
        int nCount = 0;

        for (int i = 0; i < mTextMesh.text.Length; i++)
        {
            char c = mTextMesh.text[i];
            CharacterInfo mCharacterInfo;
            if (mTextMesh.font.GetCharacterInfo(c, out mCharacterInfo))
            {
                nCount++;
            }
        }

        return nCount;
    }

    // Update is called once per frame
    void Update()
    {
        nTextCount = 0;

        if (mTextMesh != null)
        {
            int nLength = GetWordLength();
            _SoftClipList = new Vector4[nLength];
            uvScaleOffsetList = new Vector4[nLength];

            float posX = mTextMesh.transform.position.x + fOffsetX;

            float fWidth = GetWidth();

            if (mTextMesh.alignment == TextAlignment.Left)
            {
                posX += 0f;
            }
            else if (mTextMesh.alignment == TextAlignment.Center)
            {
                posX += -fWidth / 2f;
            }
            else if (mTextMesh.alignment == TextAlignment.Right)
            {
                posX += -fWidth;
            }
            
            for (int i = 0; i < mTextMesh.text.Length; i++)
            {
                CharacterInfo info;
                if (mTextMesh.font.GetCharacterInfo(mTextMesh.text[i], out info))
                {
                    UpdateSingleWord(info, posX);
                    posX += info.advance;
                }
            }
        }

        m_materialProperty.SetTexture("_MainTex", m_spriteRenderer.sprite.texture);
        m_materialProperty.SetColor("_Color", m_spriteRenderer.color);

        m_materialProperty.SetFloat("nTextCount", nTextCount);

        if (mTextMesh != null)
        {
            m_materialProperty.SetTexture("_AlphaMask", mTextMesh.font.material.mainTexture);
            Vector4 uvScale = new Vector4(mTextMesh.font.material.mainTextureScale.x, mTextMesh.font.material.mainTextureScale.y, mTextMesh.font.material.mainTextureOffset.x, mTextMesh.font.material.mainTextureOffset.y);
            m_materialProperty.SetVector("_AlphaMask_ST", uvScale);

            m_materialProperty.SetVectorArray("_SoftClipList", _SoftClipList);
            m_materialProperty.SetVectorArray("_FontMask_ST", uvScaleOffsetList);
        }

        m_spriteRenderer.SetPropertyBlock(m_materialProperty);
    }

    Vector4 GetTextWordVector4(CharacterInfo info, float posX)
    {
        float posY = mTextMesh.transform.position.y + fOffsetY;

        Vector2 maskAreaMin = new Vector2(info.minX, info.minY) + new Vector2(posX, posY);
        Vector2 maskAreaMax = new Vector2(info.maxX, info.maxY) + new Vector2(posX, posY);
        Vector4 _SoftClip = new Vector4(maskAreaMin.x, maskAreaMin.y, maskAreaMax.x, maskAreaMax.y);
        _SoftClip *= mTextMesh.characterSize / mTextMesh.font.fontSize * 10 * fScale;

        return _SoftClip;
    }

    void UpdateSingleWord(CharacterInfo info, float fPosX)
    {
        float fUvWidth = info.uvTopRight.x - info.uvBottomLeft.x;
        float fUvHeight = info.uvTopRight.y - info.uvBottomLeft.y;
        
        Vector2 uvScale = new Vector2(fUvWidth, fUvHeight);
        Vector2 uvOffset = info.uvBottomLeft;
        Vector4 uvScaleOffset = new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);

        Vector4 _SoftClip = GetTextWordVector4(info, fPosX);

        uvScaleOffsetList[nTextCount] = uvScaleOffset;
        _SoftClipList[nTextCount] = _SoftClip;

        nTextCount++;
    }

    private float GetWidth()
    {
        float textWidth = 0.0f;
        for (int i = 0; i < mTextMesh.text.Length; i++)
        {
            char c = mTextMesh.text[i];
            CharacterInfo mCharacterInfo;
            if (mTextMesh.font.GetCharacterInfo(c, out mCharacterInfo))
            {
                textWidth += mCharacterInfo.advance;
            }
        }

        return textWidth;
    }

}
