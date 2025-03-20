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
            bounds.SetMinMax(m_worldCornors[0], m_worldCornors[2]);
            return bounds;
        }
    }

    private static void UpdateSimpleSprite()
    {
        Bounds mBounds = maskBounds;
        _ClipRect = new Vector4(mBounds.min.x, mBounds.min.y, mBounds.size.x, mBounds.size.y);
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