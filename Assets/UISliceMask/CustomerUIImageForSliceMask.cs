using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(MaskableGraphic))]
[DisallowMultipleComponent]
public class CustomerUIImageForSliceMask : MonoBehaviour
{
    [SerializeField] private Image m_image_mask;
    [SerializeField] private RawImage m_rawImage_mask;
    [SerializeField] private Material m_CommonMat;

    private Material mMat = null;
    private Image mImage;
    private RawImage mRawImage;
    void Start()
    {
        mImage = GetComponent<Image>();
        if(mImage == null)
        {
            mRawImage = GetComponent<RawImage>();
        }
        
        if (m_CommonMat != null)
        {
            mMat = m_CommonMat;
        }
        else
        {
            mMat = new Material(GetShader());
        }

        UpdateMask();

        if (mImage != null)
        {
            mImage.material = mMat;
        }
        else
        {
            mRawImage.material = mMat;
        }
    }

    void LateUpdate()
    {
        UpdateMask();
    }

    Shader GetShader()
    {
        if (m_image_mask != null)
        {
            return Shader.Find("Customer/UI/UIImageSliceMasked");
        }
        else
        {
            return Shader.Find("Customer/UI/UIRawImageMasked");
        }
    }

    void UpdateMask()
    {
        if (m_image_mask != null)
        {
            StaticImageSliceMaskFunc.UpdateMask(m_image_mask, mMat);
        }
        else
        {
            StaticRawImageMaskFunc.UpdateMask(m_rawImage_mask, mMat);
        }

        if (mImage != null)
        {
            mMat.SetTexture("_MainTex", mImage.sprite.texture);
        }
        else
        {
            mMat.SetTexture("_MainTex", mRawImage.texture);
        }
    }

}