using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public class CustomerUIImageForSliceMask : MonoBehaviour
{
    [SerializeField] private Image m_image_mask;
    [SerializeField] private RawImage m_rawImage_mask;
    [SerializeField] private Material m_CommonMat;

    private Material mMat = null;
    void Start()
    {
        var mImage = GetComponent<MaskableGraphic>();
        if (m_CommonMat != null)
        {
            mMat = m_CommonMat;
        }
        else
        {
            mMat = new Material(GetShader());
        }

        UpdateMask();
        mImage.material = mMat;
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
    }

}