using UnityEngine;
using UnityEngine.UI;

public static class StaticImageSliceMaskFunc
{
    static Image m_mask;
    static readonly Vector3[] m_worldCornors = new Vector3[4];
    const int nArrayLength = 12;
    static readonly Vector4[] uvScaleOffsetList = new Vector4[nArrayLength];
    static readonly Vector4[] _ClipRectList = new Vector4[nArrayLength];
    static readonly Vector4[] _TiledCountList = new Vector4[nArrayLength];
    static int nSliceCount = 0;
    static int nTiledSliceCount = 0;

    private enum SlicePosition : uint
    {
        Center = 0,

        TopLeft,
        TopCenter,
        TopRight,

        LeftCenter,
        RightCenter,

        BottomLeft,
        BottomCenter,
        BottomRight,

        LeftRight,
        TopBottom,
        LeftRightTopBottom,

        Size,
    }

    public static void UpdateMask(Image mImage,Material mMat)
    {
        m_mask = mImage;

        nSliceCount = 0;
        nTiledSliceCount = 0;
        for (int i = 0; i < _ClipRectList.Length; i++)
        {
            _ClipRectList[i] = Vector4.zero;
            uvScaleOffsetList[i] = Vector4.zero;
            _TiledCountList[i] = Vector4.zero;
        }

        if (m_mask == null || m_mask.sprite == null)
        {
            
        }
        else
        {
            if (m_mask.type == Image.Type.Sliced)
            {
                UpdateSliceSprite();
            }
            else if (m_mask.type == Image.Type.Tiled)
            {
                UpdateTiledSprite();
            }
            else
            {
                UpdateSimpleSprite();
            }
        }

        UpdateMat(mMat);
    }

    public static void UpdateMask(Image mImage, MaterialPropertyBlock mMat)
    {
        m_mask = mImage;

        nSliceCount = 0;
        nTiledSliceCount = 0;
        for (int i = 0; i < _ClipRectList.Length; i++)
        {
            _ClipRectList[i] = Vector4.zero;
            uvScaleOffsetList[i] = Vector4.zero;
            _TiledCountList[i] = Vector4.zero;
        }

        if (m_mask == null || m_mask.sprite == null)
        {

        }
        else
        {
            if (m_mask.type == Image.Type.Sliced)
            {
                UpdateSliceSprite();
            }
            else if (m_mask.type == Image.Type.Tiled)
            {
                UpdateTiledSprite();
            }
            else
            {
                UpdateSimpleSprite();
            }
        }

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

    static void UpdateSimpleSprite()
    {
        Vector2 tightOffset = new Vector2(m_mask.sprite.textureRectOffset.x / m_mask.sprite.rect.size.x, m_mask.sprite.textureRectOffset.y / m_mask.sprite.rect.size.y);
        Vector2 tightScale = new Vector2(m_mask.sprite.textureRect.size.x / m_mask.sprite.rect.size.x, m_mask.sprite.textureRect.size.y / m_mask.sprite.rect.size.y);

        Vector2 uvScale = new Vector2(m_mask.sprite.textureRect.size.x / m_mask.sprite.texture.width, m_mask.sprite.textureRect.size.y / m_mask.sprite.texture.height);
        Vector2 uvOffset = new Vector2(m_mask.sprite.textureRect.xMin / m_mask.sprite.texture.width, m_mask.sprite.textureRect.yMin / m_mask.sprite.texture.height);

        Vector2 maskSize = new Vector2(maskBounds.size.x, maskBounds.size.y);
        Vector2 maskPos = new Vector2(m_mask.transform.position.x, m_mask.transform.position.y);
        Vector2 offsetPosCoef = Vector2.one * 0.5f - maskPivot;
        maskPos = maskPos + new Vector2(maskSize.x * offsetPosCoef.x, maskSize.y * offsetPosCoef.y);

        Vector2 maskAreaMin = new Vector3(maskPos.x - maskSize.x / 2, maskPos.y - maskSize.y / 2);
        maskAreaMin += new Vector2(maskBounds.size.x * tightOffset.x, maskBounds.size.y * tightOffset.y);
        Vector2 maskAreaMax = maskAreaMin + new Vector2(maskBounds.size.x * tightScale.x, maskBounds.size.y * tightScale.y);

        Vector4 uvScaleOffset = new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);
        Vector4 _ClipRect = new Vector4(maskAreaMin.x, maskAreaMin.y, maskAreaMax.x, maskAreaMax.y);
        uvScaleOffsetList[0] = uvScaleOffset;
        _ClipRectList[0] = _ClipRect;
        nSliceCount = 1;
    }

    static bool HasSliceBorder()
    {
        return m_mask.sprite.border != Vector4.zero;
    }

    static void UpdateSliceSprite()
    {
        if (HasSliceBorder())
        {
            if (m_mask.sprite.textureRectOffset != Vector2.zero)
            {
                Debug.Assert(false, "Please Set Sprite MeshType is FullRect !!!");
            }

            for (SlicePosition i = 0; i < SlicePosition.Size; i++)
            {
                UpdateSliceQuard(i);
            }
        }
        else
        {
            UpdateSimpleSprite();
        }

    }

    static void UpdateTiledSprite()
    {
        if (m_mask.sprite.textureRectOffset != Vector2.zero)
        {
            Debug.Assert(false, "Please Set Sprite MeshType is FullRect !!!");
        }

        if (HasSliceBorder())
        {
            for (SlicePosition i = 0; i < SlicePosition.Size; i++)
            {
                UpdateSliceQuard(i);
            }
        }
        else
        {
            UpdateSimpleTiled();
        }
    }

    static void UpdateSliceQuard(SlicePosition align)
    {
        float leftSlice = m_mask.sprite.border.x;
        float bottomSlice = m_mask.sprite.border.y;
        float rightSlice = m_mask.sprite.border.z;
        float topSlice = m_mask.sprite.border.w;

        Vector2 uvScale1 = Vector2.zero;
        Vector2 uvOffset1 = Vector2.zero;

        float fMaskSpriteWidth = m_mask.sprite.textureRect.width;
        float fMaskSpriteHeigh = m_mask.sprite.textureRect.height;
        float fMaskSpriteXMin = m_mask.sprite.textureRect.xMin;
        float fMaskSpriteYMin = m_mask.sprite.textureRect.yMin;

        bool bLeftRightCombine = rightSlice + leftSlice >= m_mask.rectTransform.sizeDelta.x;
        bool bTopBottomCombine = topSlice + bottomSlice >= m_mask.rectTransform.sizeDelta.y;
        bool bAllCombine = bLeftRightCombine && bTopBottomCombine;
        bool bAnyCombine = bLeftRightCombine || bTopBottomCombine;

        //bLeftRightCombine = false;
        //bTopBottomCombine = false;
        //bAllCombine = false;
        //bAnyCombine = false;

        if (align == SlicePosition.TopLeft)
        {
            if (leftSlice <= 0 || topSlice <= 0.0f || bAnyCombine)
            {
                return;
            }

            uvScale1 = new Vector2(leftSlice / fMaskSpriteWidth, topSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(0, (fMaskSpriteHeigh - topSlice) / fMaskSpriteHeigh);
        }
        else if (align == SlicePosition.TopCenter)
        {
            if (topSlice <= 0.0f || bTopBottomCombine)
            {
                return;
            }

            uvScale1 = new Vector2((fMaskSpriteWidth - leftSlice - rightSlice) / fMaskSpriteWidth, topSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(leftSlice / fMaskSpriteWidth, (fMaskSpriteHeigh - topSlice) / fMaskSpriteHeigh);
        }
        else if (align == SlicePosition.TopRight)
        {
            if (rightSlice <= 0 || topSlice <= 0.0f || bAnyCombine)
            {
                return;
            }

            uvScale1 = new Vector2(rightSlice / fMaskSpriteWidth, topSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2((fMaskSpriteWidth - rightSlice) / fMaskSpriteWidth, (fMaskSpriteHeigh - topSlice) / fMaskSpriteHeigh);
        }
        else if (align == SlicePosition.LeftCenter)
        {
            if (leftSlice <= 0 || bLeftRightCombine)
            {
                return;
            }

            uvScale1 = new Vector2(leftSlice / fMaskSpriteWidth, (fMaskSpriteHeigh - topSlice - bottomSlice) / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(0, bottomSlice / fMaskSpriteHeigh);
        }
        else if (align == SlicePosition.Center)
        {
            if (bAllCombine)
            {
                return;
            }

            uvScale1 = new Vector2((fMaskSpriteWidth - leftSlice - rightSlice) / fMaskSpriteWidth, (fMaskSpriteHeigh - topSlice - bottomSlice) / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(leftSlice / fMaskSpriteWidth, bottomSlice / fMaskSpriteHeigh);
        }
        else if (align == SlicePosition.RightCenter)
        {
            if (rightSlice <= 0 || bLeftRightCombine)
            {
                return;
            }

            uvScale1 = new Vector2(rightSlice / fMaskSpriteWidth, (fMaskSpriteHeigh - topSlice - bottomSlice) / fMaskSpriteHeigh);
            uvOffset1 = new Vector2((fMaskSpriteWidth - rightSlice) / fMaskSpriteWidth, bottomSlice / fMaskSpriteHeigh);
        }
        else if (align == SlicePosition.BottomLeft)
        {
            if (leftSlice <= 0 || bottomSlice <= 0.0f || bAnyCombine)
            {
                return;
            }

            uvScale1 = new Vector2(leftSlice / fMaskSpriteWidth, bottomSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(0, 0);
        }
        else if (align == SlicePosition.BottomCenter)
        {
            if (bottomSlice <= 0.0f || bTopBottomCombine)
            {
                return;
            }

            uvScale1 = new Vector2((fMaskSpriteWidth - leftSlice - rightSlice) / fMaskSpriteWidth, bottomSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(leftSlice / fMaskSpriteWidth, 0);
        }
        else if (align == SlicePosition.BottomRight)
        {
            if (rightSlice <= 0 || bottomSlice <= 0.0f || bAnyCombine)
            {
                return;
            }

            uvScale1 = new Vector2(rightSlice / fMaskSpriteWidth, bottomSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2((fMaskSpriteWidth - rightSlice) / fMaskSpriteWidth, 0);
        }
        else if (align == SlicePosition.LeftRight)
        {
            if (!bLeftRightCombine)
            {
                return;
            }

            uvScale1 = new Vector2(1.0f, (fMaskSpriteHeigh - topSlice - bottomSlice) / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(0, bottomSlice / fMaskSpriteHeigh);
        }
        else if (align == SlicePosition.TopBottom)
        {
            if (!bTopBottomCombine)
            {
                return;
            }

            uvScale1 = new Vector2((fMaskSpriteWidth - leftSlice - rightSlice) / fMaskSpriteWidth, 1.0f);
            uvOffset1 = new Vector2(leftSlice / fMaskSpriteWidth, 0.0f);
        }
        else if (align == SlicePosition.LeftRightTopBottom)
        {
            if (!bAllCombine)
            {
                return;
            }

            uvScale1 = new Vector2(1.0f, 1.0f);
            uvOffset1 = new Vector2(0.0f, 0.0f);
        }
        else
        {
            return;
        }

        Vector2 uvScale0 = new Vector2(fMaskSpriteWidth / m_mask.sprite.texture.width, fMaskSpriteHeigh / m_mask.sprite.texture.height);
        Vector2 uvOffset0 = new Vector2(fMaskSpriteXMin / m_mask.sprite.texture.width, fMaskSpriteYMin / m_mask.sprite.texture.height);

        Vector2 uvOffset = uvOffset0 + new Vector2(uvOffset1.x * uvScale0.x, uvOffset1.y * uvScale0.y);
        Vector2 uvScale = new Vector2(uvScale0.x * uvScale1.x, uvScale0.y * uvScale1.y);

        float fBoundSizeX = maskBounds.size.x;
        float fBoundSizeY = maskBounds.size.y;
        
        float topSliceSizeX = topSlice * m_mask.transform.lossyScale.y;
        float bottomSliceSizeX = bottomSlice * m_mask.transform.lossyScale.y;
        float rightSliceSizeX = rightSlice * m_mask.transform.lossyScale.x;
        float leftSliceSizeX = leftSlice * m_mask.transform.lossyScale.x;

        Vector2 maskPos = new Vector2(m_mask.transform.position.x, m_mask.transform.position.y);
        Vector2 offsetPosCoef = Vector2.one * 0.5f - maskPivot;
        maskPos = maskPos + new Vector2(fBoundSizeX * offsetPosCoef.x, fBoundSizeY * offsetPosCoef.y);

        Vector2 maskAreaMin = new Vector3(maskPos.x - fBoundSizeX / 2, maskPos.y - fBoundSizeY / 2);
        Vector2 maskAreaMax = new Vector3(maskPos.x + fBoundSizeX / 2, maskPos.y + fBoundSizeY / 2);

        if (align == SlicePosition.Center)
        {
            maskAreaMin = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMin.y + bottomSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMax.y - topSliceSizeX);
        }
        else if (align == SlicePosition.RightCenter)
        {
            maskAreaMin = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMin.y + bottomSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMax.x, maskAreaMax.y - topSliceSizeX);
        }
        else if (align == SlicePosition.LeftCenter)
        {
            maskAreaMin = new Vector2(maskAreaMin.x, maskAreaMin.y + bottomSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMax.y - topSliceSizeX);
        }
        else if (align == SlicePosition.TopLeft)
        {
            maskAreaMin = new Vector2(maskAreaMin.x, maskAreaMax.y - topSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMax.y);
        }
        else if (align == SlicePosition.TopCenter)
        {
            maskAreaMin = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMax.y - topSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMax.y);
        }
        else if (align == SlicePosition.TopRight)
        {
            maskAreaMin = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMax.y - topSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMax.x, maskAreaMax.y);
        }
        else if (align == SlicePosition.BottomLeft)
        {
            maskAreaMin = new Vector2(maskAreaMin.x, maskAreaMin.y);
            maskAreaMax = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMin.y + bottomSliceSizeX);
        }
        else if (align == SlicePosition.BottomCenter)
        {
            maskAreaMin = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMin.y);
            maskAreaMax = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMin.y + bottomSliceSizeX);
        }
        else if (align == SlicePosition.BottomRight)
        {
            maskAreaMin = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMin.y);
            maskAreaMax = new Vector2(maskAreaMax.x, maskAreaMin.y + bottomSliceSizeX);
        }
        else if (align == SlicePosition.LeftRight)
        {
            maskAreaMin = new Vector2(maskAreaMin.x, maskAreaMin.y + bottomSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMax.x, maskAreaMax.y - bottomSliceSizeX);
        }
        else if (align == SlicePosition.TopBottom)
        {
            maskAreaMin = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMin.y);
            maskAreaMax = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMax.y);
        }
        else if (align == SlicePosition.LeftRightTopBottom)
        {
            maskAreaMin = new Vector2(maskAreaMin.x, maskAreaMin.y);
            maskAreaMax = new Vector2(maskAreaMax.x, maskAreaMax.y);
        }

        if (m_mask.type == Image.Type.Sliced)
        {
            Vector4 uvScaleOffset = new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);
            Vector4 _ClipRect = new Vector4(maskAreaMin.x, maskAreaMin.y, maskAreaMax.x, maskAreaMax.y);

            uvScaleOffsetList[nSliceCount] = uvScaleOffset;
            _ClipRectList[nSliceCount] = _ClipRect;
            nSliceCount = nSliceCount + 1;
        }
        else if (m_mask.type == Image.Type.Tiled)
        {
            float fTiledWith = 0;
            float fTiledHeight = 0;

            if (align == SlicePosition.Center)
            {
                fTiledWith = (m_mask.sprite.rect.width - leftSlice - rightSlice) * m_mask.transform.lossyScale.x;
                fTiledHeight = (m_mask.sprite.rect.height - topSlice - bottomSlice) * m_mask.transform.lossyScale.y;
            }
            else if (align == SlicePosition.RightCenter)
            {
                fTiledWith = rightSlice * m_mask.transform.lossyScale.x;
                fTiledHeight = (m_mask.sprite.rect.height - topSlice - bottomSlice) * m_mask.transform.lossyScale.y;
            }
            else if (align == SlicePosition.LeftCenter)
            {
                fTiledWith = leftSlice * m_mask.transform.lossyScale.x;
                fTiledHeight = (m_mask.sprite.rect.height - topSlice - bottomSlice) * m_mask.transform.lossyScale.y;
            }
            else if (align == SlicePosition.TopCenter)
            {
                fTiledWith = (m_mask.sprite.rect.width - leftSlice - rightSlice) * m_mask.transform.lossyScale.x;
                fTiledHeight = topSlice * m_mask.transform.lossyScale.y;
            }
            else if (align == SlicePosition.BottomCenter)
            {
                fTiledWith = (m_mask.sprite.rect.width - leftSlice - rightSlice) * m_mask.transform.lossyScale.x;
                fTiledHeight = bottomSlice * m_mask.transform.lossyScale.y;
            }
            else
            {
                fTiledWith = maskAreaMax.x - maskAreaMin.x;
                fTiledHeight = maskAreaMax.y - maskAreaMin.y;
            }

            UpdateTiledQuard(maskAreaMin, maskAreaMax, uvScale, uvOffset, fTiledWith, fTiledHeight);
        }

    }

    private static void UpdateSimpleTiled()
    {
        Vector2 uvScale = new Vector2(m_mask.sprite.textureRect.size.x / m_mask.sprite.texture.width, m_mask.sprite.textureRect.size.y / m_mask.sprite.texture.height);
        Vector2 uvOffset = new Vector2(m_mask.sprite.textureRect.xMin / m_mask.sprite.texture.width, m_mask.sprite.textureRect.yMin / m_mask.sprite.texture.height);

        Vector2 maskSize = new Vector2(maskBounds.size.x, maskBounds.size.y);
        Vector2 maskPos = new Vector2(m_mask.transform.position.x, m_mask.transform.position.y);
        Vector2 offsetPosCoef = Vector2.one * 0.5f - maskPivot;
        maskPos = maskPos + new Vector2(maskSize.x * offsetPosCoef.x, maskSize.y * offsetPosCoef.y);

        Vector2 maskAreaMin = new Vector3(maskPos.x - maskSize.x / 2, maskPos.y - maskSize.y / 2);
        Vector2 maskAreaMax = new Vector3(maskPos.x + maskSize.x / 2, maskPos.y + maskSize.y / 2);

        float fTiledWith = m_mask.sprite.rect.width * m_mask.transform.lossyScale.x;
        float fTiledHeight = m_mask.sprite.rect.height * m_mask.transform.lossyScale.y;

        UpdateTiledQuard(maskAreaMin, maskAreaMax, uvScale, uvOffset, fTiledWith, fTiledHeight);
    }

    private static void UpdateTiledQuard(Vector2 maskAreaMin, Vector2 maskAreaMax, Vector2 uvScale, Vector2 uvOffset, float fTiledWith, float fTiledHeight)
    {
        float fTiledWidthCount = (maskAreaMax.x - maskAreaMin.x) / fTiledWith;
        float fTiledHeightCount = (maskAreaMax.y - maskAreaMin.y) / fTiledHeight;

        Vector4 uvScaleOffset = new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);
        Vector4 _ClipRect = new Vector4(maskAreaMin.x, maskAreaMin.y, maskAreaMax.x, maskAreaMax.y);
        Vector4 _TiledCount = new Vector4(fTiledWidthCount, fTiledHeightCount, 0, 0);

        int nIndex = nTiledSliceCount;
        uvScaleOffsetList[nIndex] = uvScaleOffset;
        _ClipRectList[nIndex] = _ClipRect;
        _TiledCountList[nIndex] = _TiledCount;
        nTiledSliceCount = nTiledSliceCount + 1;
    }

    static void UpdateMat(Material mMat)
    {
        mMat.SetFloat("nSliceCount", nSliceCount);
        mMat.SetFloat("nTiledSliceCount", nTiledSliceCount);
        mMat.SetVectorArray("_TiledCount", _TiledCountList);
        mMat.SetVectorArray("_SliceClipRect", _ClipRectList);
        mMat.SetVectorArray("_SliceAlphaMask_ST", uvScaleOffsetList);

        if (m_mask != null && m_mask.sprite != null && m_mask.sprite.texture)
        {
            mMat.SetTexture("_AlphaMask", m_mask.sprite.texture);
        }
        else
        {
            mMat.SetTexture("_AlphaMask", Texture2D.whiteTexture);
        }
    }

    static void UpdateMat(MaterialPropertyBlock mMat)
    {
        mMat.SetFloat("nSliceCount", nSliceCount);
        mMat.SetFloat("nTiledSliceCount", nTiledSliceCount);
        mMat.SetVectorArray("_TiledCount", _TiledCountList);
        mMat.SetVectorArray("_SliceClipRect", _ClipRectList);
        mMat.SetVectorArray("_SliceAlphaMask_ST", uvScaleOffsetList);

        if (m_mask != null && m_mask.sprite != null && m_mask.sprite.texture)
        {
            mMat.SetTexture("_AlphaMask", m_mask.sprite.texture);
        }
        else
        {
            mMat.SetTexture("_AlphaMask", Texture2D.whiteTexture);
        }
    }

}