using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

[RequireComponent(typeof(ParticleSystem))]
public class CustomerParticleForSliceMask : BaseSoftSliceMasked
{
    private Texture mTexture2D;
    private ParticleSystemRenderer m_ParticleRenderer;
	public Material m_OriginalMmaterial;

    // Use this for initialization
    protected override void Start ()
    {
		m_ParticleRenderer = GetComponent<ParticleSystemRenderer> ();
		m_ParticleRenderer.sharedMaterial = m_OriginalMmaterial;

        m_materialProperty = new MaterialPropertyBlock();
        m_ParticleRenderer.GetPropertyBlock(m_materialProperty);
		m_ParticleRenderer.SetPropertyBlock (m_materialProperty);

        CheckMaterialParma();
    }

    private void CheckMaterialParma()
    {
        Debug.Assert(m_OriginalMmaterial.HasProperty("nSliceCount"), string.Format("脚本: CustomerParticleForSliceMask 请求的材质Shader 属性: {0} 不存在", "nSliceCount"));
        Debug.Assert(m_OriginalMmaterial.HasProperty("nTiledSliceCount"), string.Format("脚本: CustomerParticleForSliceMask 请求的材质Shader 属性: {0} 不存在", "nTiledSliceCount"));
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
