using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ParticleSystem))]
[ExecuteAlways]
[DisallowMultipleComponent]
public class CustomerUIParticleForSliceMask:MonoBehaviour
{
    [SerializeField] private Image m_image_mask;
    [SerializeField] private RawImage m_rawImage_mask;
    private ParticleSystemRenderer m_ParticleRenderer;
    public Material m_OriginalMmaterial;
    private MaterialPropertyBlock m_materialProperty;

    void Start()
    {
        m_ParticleRenderer = GetComponent<ParticleSystemRenderer>();
        m_ParticleRenderer.sharedMaterial = m_OriginalMmaterial;

        m_materialProperty = new MaterialPropertyBlock();
        m_ParticleRenderer.GetPropertyBlock(m_materialProperty);
        m_ParticleRenderer.SetPropertyBlock(m_materialProperty);

        CheckMaterialParma();
        UpdateMask();
    }

    private void CheckMaterialParma()
    {
        Debug.Assert(m_OriginalMmaterial.HasProperty("nSliceCount"), string.Format("{0}: 脚本: CustomerParticleForSliceMask 请求的材质Shader 属性: {1} 不存在", gameObject.name, "nSliceCount"));
        Debug.Assert(m_OriginalMmaterial.HasProperty("nTiledSliceCount"), string.Format("{0}: 脚本: CustomerParticleForSliceMask 请求的材质Shader 属性: {1} 不存在", gameObject.name, "nTiledSliceCount"));
    }

    void LateUpdate()
    {
        UpdateMask();
    }

    void UpdateMask()
    {
        if (m_image_mask != null)
        {
            StaticImageSliceMaskFunc.UpdateMask(m_image_mask, m_materialProperty);
        }
        else
        {
            StaticRawImageMaskFunc.UpdateMask(m_rawImage_mask, m_materialProperty);
        }

        m_ParticleRenderer.SetPropertyBlock(m_materialProperty);
    }

}