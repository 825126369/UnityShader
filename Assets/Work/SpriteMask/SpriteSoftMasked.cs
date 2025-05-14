using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class SpriteSoftMasked : MonoBehaviour {
	public SpriteRenderer m_mask; 
	public BlendOption m_blendOption;
	public int m_stencilRef = 0;
	public CompareFunction m_stencilComp = CompareFunction.Always;
	public StencilOp m_stencilOp = StencilOp.Keep;

	private SpriteRenderer m_spriteRenderer;
	private Material m_material;
	int m_alphaMaskPropertyId;
	int m_stencilRefPropertyId;
	int m_stencilCompPropertyId;
	int m_stencilOpPropertyId;
	int m_srcBendPropertyId;
	int m_dstBendPropertyId;

	private static Material m_defaultNormalMaterial;
	private static Material m_defaultAddictiveMaterial;
	private static Material m_defaultLightenMaterial;
	private MaterialPropertyBlock m_materialProperty;

	static Material defaultNormalMaterial
	{
		get
		{
			if (m_defaultNormalMaterial == null) {
				m_defaultNormalMaterial = new Material (Shader.Find ("Customer/SpriteSoftMasked"));
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
				m_defaultAddictiveMaterial = new Material (Shader.Find ("Customer/SpriteSoftMasked"));
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
				m_defaultLightenMaterial = new Material (Shader.Find ("Customer/SpriteSoftMasked"));
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
	void Start () {
		m_spriteRenderer = GetComponent<SpriteRenderer> ();
		m_alphaMaskPropertyId = Shader.PropertyToID("_AlphaMask");
		m_stencilRefPropertyId = Shader.PropertyToID("_Stencil");
		m_stencilCompPropertyId = Shader.PropertyToID("_StencilComp");
		m_stencilOpPropertyId = Shader.PropertyToID("_StencilOp");
		m_srcBendPropertyId = Shader.PropertyToID("_SrcBlend");
		m_dstBendPropertyId = Shader.PropertyToID("_DstBlend");

		m_material = GetDefaultMaterial(m_blendOption);
		m_spriteRenderer.sharedMaterial = m_material;
		m_spriteRenderer.SetPropertyBlock (m_materialProperty);
	}

	void LateUpdate()
	{
		if (m_spriteRenderer == null || m_mask == null)
			return;
		if(m_materialProperty == null)
			m_materialProperty = new MaterialPropertyBlock ();
		m_materialProperty.SetTexture ("_MainTex", m_spriteRenderer.sprite.texture);

		Vector2 tightOffset = new Vector2(m_mask.sprite.textureRectOffset.x / m_mask.sprite.rect.size.x, m_mask.sprite.textureRectOffset.y / m_mask.sprite.rect.size.y);
		Vector2 tightScale = new Vector2(m_mask.sprite.textureRect.size.x / m_mask.sprite.rect.size.x, m_mask.sprite.textureRect.size.y / m_mask.sprite.rect.size.y);
		
		Vector2 uvScale = new Vector2(m_mask.sprite.textureRect.size.x / m_mask.sprite.texture.width, m_mask.sprite.textureRect.size.y / m_mask.sprite.texture.height);
		Vector2 uvOffset = new Vector2(m_mask.sprite.textureRect.xMin / m_mask.sprite.texture.width, m_mask.sprite.textureRect.yMin / m_mask.sprite.texture.height);

		Vector2 maskSize = new Vector2(m_mask.bounds.size.x, m_mask.bounds.size.y);
		Vector2 maskPos =  new Vector2(m_mask.transform.position.x, m_mask.transform.position.y);
		Vector2 maskAreaMin = new Vector3 (maskPos.x - maskSize.x / 2, maskPos.y - maskSize.y / 2);
		maskAreaMin += new Vector2(m_mask.bounds.size.x * tightOffset.x, m_mask.bounds.size.y * tightOffset.y);
		Vector2 maskAreaMax = maskAreaMin + new Vector2(m_mask.bounds.size.x * tightScale.x, m_mask.bounds.size.y * tightScale.y);

        m_materialProperty.SetVector("_ClipRect", new Vector4(maskAreaMin.x, maskAreaMin.y, maskAreaMax.x, maskAreaMax.y));
		m_materialProperty.SetVector ("_AlphaMask_ST", new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y));
		m_materialProperty.SetTexture ("_AlphaMask", m_mask.sprite.texture);
		m_spriteRenderer.SetPropertyBlock (m_materialProperty);
	}

}
