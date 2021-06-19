using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public enum BlendOption
{
	Normal = 0,
	Addictive = 1,
	Lighten = 3
	//	Darken = 2,
	//	Lighten = 3,
	//	Multiply = 4
}

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class SpriteSoftSliceMasked : BaseSoftSliceMasked
{
	public BlendOption m_blendOption;
	private SpriteRenderer m_spriteRenderer;
    private Material m_material;
	private static Material m_defaultNormalMaterial;
	private static Material m_defaultAddictiveMaterial;
	private static Material m_defaultLightenMaterial;

    static Material defaultNormalMaterial
	{
		get
		{
			if (m_defaultNormalMaterial == null) {
				m_defaultNormalMaterial = new Material (Shader.Find ("Customer/SpriteSoftSliceMasked"));
				m_defaultNormalMaterial.name = "default normal materail";
				m_defaultNormalMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				m_defaultNormalMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			}
			return m_defaultNormalMaterial;
		}
	}

	static Material defaultAddictivelMaterial
	{
		get
		{
			if (m_defaultAddictiveMaterial == null) {
				m_defaultAddictiveMaterial = new Material (Shader.Find ("Customer/SpriteSoftSliceMasked"));
				m_defaultAddictiveMaterial.name = "default addictive materail";
				m_defaultAddictiveMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				m_defaultAddictiveMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
			}
			return m_defaultAddictiveMaterial;
		}
	}

	static Material defaultLightenMaterial
	{
		get
		{
			if (m_defaultLightenMaterial == null) {
				m_defaultLightenMaterial = new Material (Shader.Find ("Customer/SpriteSoftSliceMasked"));
				m_defaultLightenMaterial.name = "default Lighten materail";
				m_defaultLightenMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
				m_defaultLightenMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
			}
			return m_defaultLightenMaterial;
		}
	}

	Material GetDefaultMaterial(BlendOption blendOption)
	{
		if (blendOption == BlendOption.Addictive) {
			return defaultAddictivelMaterial;
		} else if (blendOption == BlendOption.Lighten) {
			return defaultLightenMaterial;
		} else {
			return defaultNormalMaterial;
		}
	}

	// Use this for initialization
	protected override void Start ()
    {
		m_spriteRenderer = GetComponent<SpriteRenderer> ();
		m_material = GetDefaultMaterial(m_blendOption);
		m_spriteRenderer.sharedMaterial = m_material;

        m_materialProperty = new MaterialPropertyBlock();
        m_spriteRenderer.GetPropertyBlock(m_materialProperty);
		m_spriteRenderer.SetPropertyBlock (m_materialProperty);

    }

	void LateUpdate()
	{
        UpdateMask();
        UpdateSelf();
	}

    void UpdateSelf()
    {
        if (m_spriteRenderer && m_spriteRenderer.sprite)
        {
            m_materialProperty.SetTexture("_MainTex", m_spriteRenderer.sprite.texture);
        }

        m_spriteRenderer.SetPropertyBlock(m_materialProperty);
    }
    
}
