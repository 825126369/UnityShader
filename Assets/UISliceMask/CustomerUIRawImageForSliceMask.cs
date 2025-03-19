using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
[DisallowMultipleComponent]
public class CustomerUIRawImageMask : BaseUIRawImageMasked
{
    [SerializeField] private Material m_CommonMat;
    private RawImage mImage;
    private Image;
    private Material mMat = null;
    protected override void Start()
    {
        base.Start();
        mImage = GetComponent<RawImage>();
        if (m_CommonMat != null)
        {
            mMat = m_CommonMat;
        }
        else
        {
            mMat = new Material(Shader.Find("Customer/CustomerUIImageSliceMasked"));
        }

        UpdateMask();
        UpdateSelf();
        mImage.material = mMat;
    }

    void LateUpdate()
    {
        UpdateMask();
        UpdateSelf();
    }

    void UpdateSelf()
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