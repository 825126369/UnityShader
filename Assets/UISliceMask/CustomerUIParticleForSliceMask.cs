using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[ExecuteAlways]
[DisallowMultipleComponent]
public class CustomerUIParticleForSliceMask : BaseUISoftSliceMasked
{
    private ParticleSystemRenderer m_ParticleRenderer;
    public Material m_OriginalMmaterial;

    // Use this for initialization
    protected override void Start()
    {
        m_ParticleRenderer = GetComponent<ParticleSystemRenderer>();
        m_ParticleRenderer.sharedMaterial = m_OriginalMmaterial;

        m_materialProperty = new MaterialPropertyBlock();
        m_ParticleRenderer.GetPropertyBlock(m_materialProperty);
        m_ParticleRenderer.SetPropertyBlock(m_materialProperty);

        CheckMaterialParma();
    }

    private void CheckMaterialParma()
    {
        Debug.Assert(m_OriginalMmaterial.HasProperty("nSliceCount"), string.Format("{0}: 脚本: CustomerParticleForSliceMask 请求的材质Shader 属性: {1} 不存在", gameObject.name, "nSliceCount"));
        Debug.Assert(m_OriginalMmaterial.HasProperty("nTiledSliceCount"), string.Format("{0}: 脚本: CustomerParticleForSliceMask 请求的材质Shader 属性: {1} 不存在", gameObject.name, "nTiledSliceCount"));
    }

    void LateUpdate()
    {
        UpdateMask();
        UpdateSelf();
    }

    void UpdateSelf()
    {
        m_ParticleRenderer.SetPropertyBlock(m_materialProperty);
    }

}