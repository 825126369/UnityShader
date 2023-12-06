using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Unity.Mathematics;
using UnityEngine.U2D;
using Spine.Unity.AttachmentTools;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public class CustomerUIImageForZhengFanMian : MonoBehaviour
{
    private Material m_materialProperty;
    void LateUpdate()
    {
        Image mImage = transform.GetComponent<Image>();
        SpriteAtlas atl_game1 = ResCenter.Instance.mBundleGameAllRes.GetAtlas(AtlasNames.Lobby_Game_Cards_Cardback);
        string p_name_back = "cardback_" + DataCenter.Instance.sysConfig.themeBackId;
        Sprite mBackSprite = atl_game1.GetSprite(p_name_back);

        Vector2 tightOffset = new Vector2(mBackSprite.textureRectOffset.x / mBackSprite.rect.size.x, mBackSprite.textureRectOffset.y / mBackSprite.rect.size.y);
        Vector2 tightScale = new Vector2(mBackSprite.textureRect.size.x / mBackSprite.rect.size.x, mBackSprite.textureRect.size.y / mBackSprite.rect.size.y);

        Vector2 uvScale = new Vector2(mBackSprite.textureRect.size.x / mBackSprite.texture.width, mBackSprite.textureRect.size.y / mBackSprite.texture.height);
        Vector2 uvOffset = new Vector2(mBackSprite.textureRect.xMin / mBackSprite.texture.width, mBackSprite.textureRect.yMin / mBackSprite.texture.height);

        Vector3[] m_worldCornors = new Vector3[4];
        mImage.rectTransform.GetWorldCorners(m_worldCornors);;
        Bounds maskBounds = new Bounds();
        maskBounds.min = m_worldCornors[0];
        maskBounds.max = m_worldCornors[2];
        Vector2 maskPivot = mImage.rectTransform.pivot;
        
        Vector2 maskSize = new Vector2(maskBounds.size.x, maskBounds.size.y);
        Vector2 maskPos = new Vector2(mImage.transform.position.x, mImage.transform.position.y);
        Vector2 offsetPosCoef = Vector2.one * 0.5f - maskPivot;
        maskPos = maskPos + new Vector2(maskSize.x * offsetPosCoef.x, maskSize.y * offsetPosCoef.y);
        
        Vector2 maskAreaMin = new Vector3(maskPos.x - maskSize.x / 2, maskPos.y - maskSize.y / 2);
        maskAreaMin += new Vector2(maskBounds.size.x * tightOffset.x, maskBounds.size.y * tightOffset.y);
        Vector2 maskAreaMax = maskAreaMin + new Vector2(maskBounds.size.x * tightScale.x, maskBounds.size.y * tightScale.y);

        Vector4 uvScaleOffset = new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);
        Vector4 _ClipRect = new Vector4(maskAreaMin.x, maskAreaMin.y, maskAreaMax.x, maskAreaMax.y);

        if(m_materialProperty == null)
        {
            m_materialProperty = new Material(mImage.material.shader);
			m_materialProperty.CopyPropertiesFromMaterial(mImage.material);
        }
		m_materialProperty.SetTexture ("_MainTex", mImage.sprite.texture);
        m_materialProperty.SetVector("_ClipRect2", new Vector4(maskAreaMin.x, maskAreaMin.y, maskAreaMax.x, maskAreaMax.y));
		m_materialProperty.SetVector ("_BackTex_ST", new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y));
		m_materialProperty.SetTexture ("_BackTex", mBackSprite.texture);
        mImage.material = m_materialProperty;
    }

}