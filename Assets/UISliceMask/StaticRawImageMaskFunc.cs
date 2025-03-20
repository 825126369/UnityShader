using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public static class StaticRawImageMaskFunc
{
    static RawImage m_mask;
    static readonly Vector3[] m_worldCornors = new Vector3[4];
    static Vector4 _ClipRect;
    static Vector4 uvScaleOffset;

    public static void UpdateMask(RawImage mRawImage, Material mMat)
    {
        m_mask = mRawImage;
        UpdateSimpleSprite();
        UpdateMat(mMat);
    }

    public static void UpdateMask(RawImage mRawImage, MaterialPropertyBlock mMat)
    {
        m_mask = mRawImage;
        UpdateSimpleSprite();
        UpdateMat(mMat);
    }

    private static Bounds maskBounds
    {
        get
        {
            m_mask.rectTransform.GetWorldCorners(m_worldCornors);
            Bounds bounds = new Bounds();
            bounds.min = m_worldCornors[0];
            bounds.max = m_worldCornors[2];
            return bounds;
        }
    }

    private static Vector2 maskPivot
    {
        get
        {
            return m_mask.rectTransform.pivot;
        }
    }

    private static void UpdateSimpleSprite()
    {
        Vector2 maskSize = new Vector2(maskBounds.size.x, maskBounds.size.y);
        Vector2 maskPos = new Vector2(m_mask.transform.position.x, m_mask.transform.position.y);
        Vector2 offsetPosCoef = Vector2.one * 0.5f - maskPivot;
        maskPos = maskPos + new Vector2(maskSize.x * offsetPosCoef.x, maskSize.y * offsetPosCoef.y);

        Vector2 maskAreaMin = new Vector3(maskPos.x - maskSize.x / 2, maskPos.y - maskSize.y / 2);
        _ClipRect = new Vector4(maskAreaMin.x, maskAreaMin.y, maskSize.x, maskSize.y);
        uvScaleOffset = new Vector4(1, 1, 0, 0);
    }

    static void UpdateMat(Material mMat)
    {
        mMat.SetVector("_ClipRect", _ClipRect);
        mMat.SetVector("_AlphaMask_ST", uvScaleOffset);
        if (m_mask && m_mask.texture)
        {
            mMat.SetTexture("_AlphaMask", m_mask.texture);
        }
        else
        {
            mMat.SetTexture("_AlphaMask", Texture2D.whiteTexture);
        }
    }

    static void UpdateMat(MaterialPropertyBlock mMat)
    {
        mMat.SetVector("_ClipRect", _ClipRect);
        mMat.SetVector("_AlphaMask_ST", uvScaleOffset);
        if (m_mask && m_mask.texture)
        {
            mMat.SetTexture("_AlphaMask", m_mask.texture);
        }
        else
        {
            mMat.SetTexture("_AlphaMask", Texture2D.whiteTexture);
        }
    }

}