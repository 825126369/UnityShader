using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class SpriteSoftSliceMasked : MonoBehaviour {
    public enum BlendOption
    {
        Normal = 0,
        Addictive = 1,
        Lighten = 3
    }

	public SpriteRenderer m_mask; 
	public BlendOption m_blendOption;
	private SpriteRenderer m_spriteRenderer;
	private Material m_material;
	private static Material m_defaultNormalMaterial;
	private static Material m_defaultAddictiveMaterial;
	private static Material m_defaultLightenMaterial;
	private MaterialPropertyBlock m_materialProperty;
    
    private const int nArrayLength = 9;
    private Vector4[] uvScaleOffsetList = new Vector4[nArrayLength];
    private Vector4[] _ClipRectList = new Vector4[nArrayLength];
    private Vector4[] _TiledCountList = new Vector4[nArrayLength];
    int nSliceCount = 0;
    int nTiledSliceCount = 0;

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
	void Start () {
		m_spriteRenderer = GetComponent<SpriteRenderer> ();
		m_material = GetDefaultMaterial(m_blendOption);
		m_spriteRenderer.sharedMaterial = m_material;

        m_materialProperty = new MaterialPropertyBlock();
        m_spriteRenderer.GetPropertyBlock(m_materialProperty);
		m_spriteRenderer.SetPropertyBlock (m_materialProperty);

    }

	void LateUpdate()
	{
		Update1();
	}

	void Update1()
	{
        if (m_materialProperty == null)
            m_materialProperty = new MaterialPropertyBlock();

        if (_ClipRectList == null || _ClipRectList.Length < nArrayLength)
        {
            _ClipRectList = new Vector4[nArrayLength];
            uvScaleOffsetList = new Vector4[nArrayLength];
            _TiledCountList = new Vector4[nArrayLength];
        }

        nSliceCount = 0;
        nTiledSliceCount = 0;
        for (int i = 0; i < _ClipRectList.Length; i++)
        {
            _ClipRectList[i] = Vector4.zero;
            uvScaleOffsetList[i] = Vector4.zero;
            _TiledCountList[i] = Vector4.zero;
        }

        if (m_spriteRenderer == null || m_spriteRenderer.sprite == null || m_mask == null || m_mask.sprite == null)
        {
            m_materialProperty.SetFloat("nSliceCount", nSliceCount);
            m_materialProperty.SetFloat("nTiledSliceCount", nTiledSliceCount);
            m_materialProperty.SetVectorArray("_ClipRect", _ClipRectList);
            m_materialProperty.SetVectorArray("_AlphaMask_ST", uvScaleOffsetList);
            m_materialProperty.SetVectorArray("_TiledCount", _TiledCountList);

            if (m_mask && m_mask.sprite)
            {
                m_materialProperty.SetTexture("_AlphaMask", m_mask.sprite.texture);
            }else
            {
                m_materialProperty.SetTexture("_AlphaMask", Texture2D.blackTexture);
            }

            if (m_spriteRenderer && m_spriteRenderer.sprite)
            {
                m_materialProperty.SetTexture("_MainTex", m_spriteRenderer.sprite.texture);
            }else
            {
                m_materialProperty.SetTexture("_MainTex", Texture2D.blackTexture);
            }

            m_spriteRenderer.SetPropertyBlock(m_materialProperty);
        }
        else
        {
            if (m_mask.drawMode == SpriteDrawMode.Sliced)
            {
                UpdateSliceSprite();
            }
            else if (m_mask.drawMode == SpriteDrawMode.Tiled)
            {
                UpdateTiledSprite();
            }
            else
            {
                UpdateSimpleSprite();
            }

            m_materialProperty.SetFloat("nSliceCount", nSliceCount);
            m_materialProperty.SetFloat("nTiledSliceCount", nTiledSliceCount);
            m_materialProperty.SetVectorArray("_ClipRect", _ClipRectList);
            m_materialProperty.SetVectorArray("_AlphaMask_ST", uvScaleOffsetList);
            m_materialProperty.SetVectorArray("_TiledCount", _TiledCountList);
            
            m_materialProperty.SetTexture("_AlphaMask", m_mask.sprite.texture);
            m_materialProperty.SetTexture("_MainTex", m_spriteRenderer.sprite.texture);
            m_spriteRenderer.SetPropertyBlock(m_materialProperty);
        }
	}

    void UpdateSimpleSprite()
    {
        Vector2 tightOffset = new Vector2(m_mask.sprite.textureRectOffset.x / m_mask.sprite.rect.size.x, m_mask.sprite.textureRectOffset.y / m_mask.sprite.rect.size.y);
        Vector2 tightScale = new Vector2(m_mask.sprite.textureRect.size.x / m_mask.sprite.rect.size.x, m_mask.sprite.textureRect.size.y / m_mask.sprite.rect.size.y);

        Vector2 uvScale = new Vector2(m_mask.sprite.textureRect.size.x / m_mask.sprite.texture.width, m_mask.sprite.textureRect.size.y / m_mask.sprite.texture.height);
        Vector2 uvOffset = new Vector2(m_mask.sprite.textureRect.xMin / m_mask.sprite.texture.width, m_mask.sprite.textureRect.yMin / m_mask.sprite.texture.height);

        Vector2 maskSize = new Vector2(m_mask.bounds.size.x, m_mask.bounds.size.y);
        Vector2 maskPos = new Vector2(m_mask.transform.position.x, m_mask.transform.position.y);
        Vector2 maskAreaMin = new Vector3(maskPos.x - maskSize.x / 2, maskPos.y - maskSize.y / 2);
        maskAreaMin += new Vector2(m_mask.bounds.size.x * tightOffset.x, m_mask.bounds.size.y * tightOffset.y);
        Vector2 maskAreaMax = maskAreaMin + new Vector2(m_mask.bounds.size.x * tightScale.x, m_mask.bounds.size.y * tightScale.y);
        
        Vector4 uvScaleOffset = new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);
        Vector4 _ClipRect = new Vector4(maskAreaMin.x, maskAreaMin.y, maskAreaMax.x, maskAreaMax.y);
        uvScaleOffsetList[0] = uvScaleOffset;
        _ClipRectList[0] = _ClipRect;
        nSliceCount = 1;
    }

    bool HasSliceBorder()
    {
        return m_mask.sprite.border != Vector4.zero;
    }

    void UpdateSliceSprite()
    {
        if (HasSliceBorder())
        {
            if (m_mask.sprite.textureRectOffset != Vector2.zero)
            {
                Debug.Assert(false, "Please Set Sprite MeshType is FullRect !!!");
            }

            for (int i = 0; i < (int)SpriteAlignment.Custom; i++)
            {
                UpdateSliceQuard((SpriteAlignment)i);
            }
        }
        else
        {
            UpdateSimpleSprite();
        }

    }

    void UpdateTiledSprite()
    {
        if (m_mask.sprite.textureRectOffset != Vector2.zero)
        {
            Debug.Assert(false, "Please Set Sprite MeshType is FullRect !!!");
        }

        if (HasSliceBorder())
        {
            for (SpriteAlignment i = 0; i < SpriteAlignment.Custom; i++)
            {
                UpdateSliceQuard(i);
            }
        }
        else
        {
            UpdateSimpleTiled();
        }
    }

    void UpdateSliceQuard(SpriteAlignment align)
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
            
        if (align == SpriteAlignment.TopLeft)
        {
            if (leftSlice <= 0 || topSlice <= 0.0f)
            {
                return;
            }

            uvScale1 = new Vector2(leftSlice / fMaskSpriteWidth, topSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(0, (fMaskSpriteHeigh - topSlice) / fMaskSpriteHeigh);
        }
        else if (align == SpriteAlignment.TopCenter)
        {
            if (topSlice <= 0.0f)
            {
                return;
            }

            uvScale1 = new Vector2((fMaskSpriteWidth- leftSlice - rightSlice) / fMaskSpriteWidth, topSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(leftSlice / fMaskSpriteWidth, (fMaskSpriteHeigh - topSlice) / fMaskSpriteHeigh);
        }
        else if (align == SpriteAlignment.TopRight)
        {
            if (rightSlice <= 0 || topSlice <= 0.0f)
            {
                return;
            }

            uvScale1 = new Vector2(rightSlice / fMaskSpriteWidth, topSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2((fMaskSpriteWidth- rightSlice) / fMaskSpriteWidth, (fMaskSpriteHeigh - topSlice) / fMaskSpriteHeigh);
        }
        else if (align == SpriteAlignment.LeftCenter)
        {
            if (leftSlice <= 0)
            {
                return;
            }

            uvScale1 = new Vector2(leftSlice / fMaskSpriteWidth, (fMaskSpriteHeigh - topSlice - bottomSlice) / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(0, bottomSlice / fMaskSpriteHeigh);
        }
        else if (align == SpriteAlignment.Center)
        {
            uvScale1 = new Vector2((fMaskSpriteWidth- leftSlice - rightSlice) / fMaskSpriteWidth, (fMaskSpriteHeigh - topSlice - bottomSlice) / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(leftSlice / fMaskSpriteWidth, bottomSlice / fMaskSpriteHeigh);
        }
        else if (align == SpriteAlignment.RightCenter)
        {
            if (rightSlice <= 0)
            {
                return;
            }

            uvScale1 = new Vector2(rightSlice / fMaskSpriteWidth, (fMaskSpriteHeigh - topSlice - bottomSlice) / fMaskSpriteHeigh);
            uvOffset1 = new Vector2((fMaskSpriteWidth- rightSlice) / fMaskSpriteWidth, bottomSlice / fMaskSpriteHeigh);
        }
        else if (align == SpriteAlignment.BottomLeft)
        {
            if (leftSlice <= 0 || bottomSlice <= 0.0f)
            {
                return;
            }

            uvScale1 = new Vector2(leftSlice / fMaskSpriteWidth, bottomSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(0, 0);
        }
        else if (align == SpriteAlignment.BottomCenter)
        {
            if (bottomSlice <= 0.0f)
            {
                return;
            }

            uvScale1 = new Vector2((fMaskSpriteWidth- leftSlice - rightSlice) / fMaskSpriteWidth, bottomSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2(leftSlice / fMaskSpriteWidth, 0);
        }
        else if (align == SpriteAlignment.BottomRight)
        {
            if (rightSlice <= 0 || bottomSlice <= 0.0f)
            {
                return;
            }

            uvScale1 = new Vector2(rightSlice / fMaskSpriteWidth, bottomSlice / fMaskSpriteHeigh);
            uvOffset1 = new Vector2((fMaskSpriteWidth- rightSlice) / fMaskSpriteWidth, 0);
        }
        else
        {
            return;
        }

        Vector2 uvScale0 = new Vector2(fMaskSpriteWidth / m_mask.sprite.texture.width, fMaskSpriteHeigh / m_mask.sprite.texture.height);
		Vector2 uvOffset0 = new Vector2(fMaskSpriteXMin / m_mask.sprite.texture.width, fMaskSpriteYMin / m_mask.sprite.texture.height);

		Vector2 uvOffset =  uvOffset0 + new Vector2(uvOffset1.x * uvScale0.x, uvOffset1.y * uvScale0.y);
		Vector2 uvScale =  new Vector2 (uvScale0.x * uvScale1.x, uvScale0.y * uvScale1.y);

        float fBoundSizeX = m_mask.size.x * m_mask.transform.lossyScale.x;
        float fBoundSizeY = m_mask.size.y * m_mask.transform.lossyScale.y;

        float topSliceSizeX = topSlice * m_mask.transform.lossyScale.y;
        float bottomSliceSizeX = bottomSlice * m_mask.transform.lossyScale.y;
        float rightSliceSizeX = rightSlice * m_mask.transform.lossyScale.x;
        float leftSliceSizeX = leftSlice * m_mask.transform.lossyScale.x;

        Vector2 maskPos = new Vector2(m_mask.transform.position.x, m_mask.transform.position.y);
        Vector2 maskAreaMin = new Vector3(maskPos.x - fBoundSizeX / 2, maskPos.y - fBoundSizeY / 2);
        Vector2 maskAreaMax = new Vector3(maskPos.x + fBoundSizeX / 2, maskPos.y + fBoundSizeY / 2);

        if (align == SpriteAlignment.Center)
        {
            maskAreaMin = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMin.y + bottomSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMax.y - topSliceSizeX);
        }
        else if (align == SpriteAlignment.RightCenter)
        {
            maskAreaMin = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMin.y + bottomSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMax.x, maskAreaMax.y - topSliceSizeX);
        }
        else if (align == SpriteAlignment.LeftCenter)
        {
            maskAreaMin = new Vector2(maskAreaMin.x, maskAreaMin.y + bottomSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMax.y - topSliceSizeX);
        }
        else if (align == SpriteAlignment.TopLeft)
        {
            maskAreaMin = new Vector2(maskAreaMin.x, maskAreaMax.y - topSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMax.y);
        }
        else if (align == SpriteAlignment.TopCenter)
        {
            maskAreaMin = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMax.y - topSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMax.y);
        }
        else if (align == SpriteAlignment.TopRight)
        {
            maskAreaMin = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMax.y - topSliceSizeX);
            maskAreaMax = new Vector2(maskAreaMax.x, maskAreaMax.y);
        }
        else if (align == SpriteAlignment.BottomLeft)
        {
            maskAreaMin = new Vector2(maskAreaMin.x, maskAreaMin.y);
            maskAreaMax = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMin.y + bottomSliceSizeX);
        }
        else if (align == SpriteAlignment.BottomCenter)
        {
            maskAreaMin = new Vector2(maskAreaMin.x + leftSliceSizeX, maskAreaMin.y);
            maskAreaMax = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMin.y + bottomSliceSizeX);
        }
        else if (align == SpriteAlignment.BottomRight)
        {
            maskAreaMin = new Vector2(maskAreaMax.x - rightSliceSizeX, maskAreaMin.y);
            maskAreaMax = new Vector2(maskAreaMax.x, maskAreaMin.y + bottomSliceSizeX);
        }

        if (m_mask.drawMode == SpriteDrawMode.Sliced)
        {
            Vector4 uvScaleOffset = new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);
            Vector4 _ClipRect = new Vector4(maskAreaMin.x, maskAreaMin.y, maskAreaMax.x, maskAreaMax.y);
            
            uvScaleOffsetList[nSliceCount] = uvScaleOffset;
            _ClipRectList[nSliceCount] = _ClipRect;
            nSliceCount = nSliceCount + 1;
        }
        else if (m_mask.drawMode == SpriteDrawMode.Tiled)
        {
            float fTiledWith = 0;
            float fTiledHeight = 0;

            if (align == SpriteAlignment.Center)
            {
                fTiledWith = (m_mask.sprite.rect.width - leftSlice - rightSlice) * m_mask.transform.lossyScale.x;
                fTiledHeight = (m_mask.sprite.rect.height - topSlice - bottomSlice) * m_mask.transform.lossyScale.y;
            }
            else if (align == SpriteAlignment.RightCenter)
            {
                fTiledWith = rightSlice * m_mask.transform.lossyScale.x;
                fTiledHeight = (m_mask.sprite.rect.height - topSlice - bottomSlice) * m_mask.transform.lossyScale.y;
            }
            else if (align == SpriteAlignment.LeftCenter)
            {
                fTiledWith = leftSlice * m_mask.transform.lossyScale.x;
                fTiledHeight = (m_mask.sprite.rect.height - topSlice - bottomSlice) * m_mask.transform.lossyScale.y;
            }
            else if (align == SpriteAlignment.TopCenter)
            {
                fTiledWith = (m_mask.sprite.rect.width - leftSlice - rightSlice) * m_mask.transform.lossyScale.x;
                fTiledHeight = topSlice * m_mask.transform.lossyScale.y;
            }
            else if (align == SpriteAlignment.BottomCenter)
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

    private void UpdateSimpleTiled()
    {
        Vector2 uvScale = new Vector2(m_mask.sprite.textureRect.size.x / m_mask.sprite.texture.width, m_mask.sprite.textureRect.size.y / m_mask.sprite.texture.height);
        Vector2 uvOffset = new Vector2(m_mask.sprite.textureRect.xMin / m_mask.sprite.texture.width, m_mask.sprite.textureRect.yMin / m_mask.sprite.texture.height);

        Vector2 maskSize = new Vector2(m_mask.bounds.size.x, m_mask.bounds.size.y);
        Vector2 maskPos = new Vector2(m_mask.transform.position.x, m_mask.transform.position.y);
        Vector2 maskAreaMin = new Vector3(maskPos.x - maskSize.x / 2, maskPos.y - maskSize.y / 2);
        Vector2 maskAreaMax = new Vector3(maskPos.x + maskSize.x / 2, maskPos.y + maskSize.y / 2);

        float fTiledWith = m_mask.sprite.rect.width * m_mask.transform.lossyScale.x;
        float fTiledHeight = m_mask.sprite.rect.height * m_mask.transform.lossyScale.y;

        UpdateTiledQuard(maskAreaMin, maskAreaMax, uvScale, uvOffset, fTiledWith, fTiledHeight);
    }

    private void UpdateTiledQuard(Vector2 maskAreaMin, Vector2 maskAreaMax, Vector2 uvScale, Vector2 uvOffset, float fTiledWith, float fTiledHeight)
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

    

}
