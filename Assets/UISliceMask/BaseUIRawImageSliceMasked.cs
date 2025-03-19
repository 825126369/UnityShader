using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public abstract class BaseUIRawImageSoftSliceMasked : MonoBehaviour
{
    public RawImage m_mask;
    protected MaterialPropertyBlock m_materialProperty;
    readonly Vector3[] m_worldCornors = new Vector3[4];
    protected virtual void Start()
    {

    }

    protected void UpdateMask()
    {
        UpdateSimpleSprite();
    }

    private Bounds maskBounds
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

    private Vector2 maskPivot
    {
        get
        {
            return m_mask.rectTransform.pivot;
        }
    }

    private void UpdateSimpleSprite()
    {
        m_mask.rectTransform.GetWorldCorners(m_worldCornors);
        Vector4 bounds = new Vector4(m_worldCornors[0].x, m_worldCornors[0].y, m_worldCornors[2].x, m_worldCornors[2].y);

        Vector2 maskSize = new Vector2(maskBounds.size.x, maskBounds.size.y);
        Vector2 maskPos = new Vector2(m_mask.transform.position.x, m_mask.transform.position.y);
        Vector2 offsetPosCoef = Vector2.one * 0.5f - maskPivot;
        maskPos = maskPos + new Vector2(maskSize.x * offsetPosCoef.x, maskSize.y * offsetPosCoef.y);

        Vector2 maskAreaMin = new Vector3(maskPos.x - maskSize.x / 2, maskPos.y - maskSize.y / 2);
        Vector2 maskAreaMax = maskBounds.size;
        Vector4 _ClipRect = new Vector4(maskAreaMin.x, maskAreaMin.y, maskAreaMax.x, maskAreaMax.y);
        Vector4 uvScaleOffset = new Vector4(1, 1, 0, 0);

        m_materialProperty.SetVector("_ClipRect", _ClipRect);
        m_materialProperty.SetVector("_AlphaMask_ST", uvScaleOffset);
        if (m_mask && m_mask.texture)
        {
            m_materialProperty.SetTexture("_AlphaMask", m_mask.texture);
        }
        else
        {
            m_materialProperty.SetTexture("_AlphaMask", Texture2D.whiteTexture);
        }
    }

}