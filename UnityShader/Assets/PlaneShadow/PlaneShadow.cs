using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ThemeReal3DCustomerShadow : MonoBehaviour
{
    public float fDistance = 25;
    public float _ShadowFalloff = 0.01f;
    public float _ShadowInvLen = 1.0f;
    public Vector4 _ShadowFadeParams = new Vector4(500f, 0.0f, 0.2f, 0.0f);
    
    Dictionary<Renderer, MaterialPropertyBlock> mMatList = new Dictionary<Renderer, MaterialPropertyBlock>();

    #region 内置函数

    private void Start()
    {
        Transform goOriShadow = transform.Find("ShadowCenter");
        if (goOriShadow)
        {
            goOriShadow.gameObject.SetActive(false);
        }

        Renderer[] renderlist = GetComponentsInChildren<Renderer>();

        foreach (var render in renderlist)
        {
            if (render == null)
                continue;

            Material mOriMat = render.sharedMaterial;
            if(mOriMat.shader.name != "Customer/Theme/CrazyDollar/CustomerUnlitTexture")
            {
                continue;
            }

            Material mMat = new Material(Shader.Find("Customer/Theme/CrazyDollar/CustomerUnlitTextureShadow"));
            render.sharedMaterial = mMat;

            MaterialPropertyBlock mBlock = new MaterialPropertyBlock();
            CopyPropertiesFromMaterial(mOriMat, mBlock);
            mMatList[render] = mBlock;
        }
    }

    private void CopyPropertiesFromMaterial(Material oriMat, MaterialPropertyBlock targetMat)
    {
        Texture _MainTex = oriMat.GetTexture("_MainTex");
        float _CullMode = oriMat.GetFloat("_CullMode");
        Vector4 _WorldPosMaskPosUp = oriMat.GetVector("_WorldPosMaskPosUp");
        Vector4 _WorldPosMaskPosDown = oriMat.GetVector("_WorldPosMaskPosDown");

        targetMat.SetTexture("_MainTex", _MainTex);
        targetMat.SetFloat("_CullMode", _CullMode);
        targetMat.SetVector("_WorldPosMaskPosUp", _WorldPosMaskPosUp);
        targetMat.SetVector("_WorldPosMaskPosDown", _WorldPosMaskPosDown);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        SetShadowShader();
    }

    #endregion

    #region 函数

    void SetShadowShader()
    {
        GameObject goCuvePos = transform.Find("CurvePos").gameObject;
        Vector4 worldpos = goCuvePos.transform.position;
        
        Vector4 _ShadowPlane = new Vector4(goCuvePos.transform.forward.x, goCuvePos.transform.forward.y, goCuvePos.transform.forward.z, fDistance);

        float fRadius = 345;
        float lightDirY = -(goCuvePos.transform.position.y) / fRadius;
        if (Mathf.Abs(lightDirY) < 0.01f)
        {
            lightDirY = -0.2f;
        }
            
        Vector4 projdir = new Vector4(0, lightDirY, 1, 0);

        foreach (var keyValue in mMatList)
        {
            Renderer render = keyValue.Key;
            MaterialPropertyBlock mBlock = keyValue.Value;
            mBlock.SetVector("_WorldPos", worldpos);
            mBlock.SetVector("_ShadowProjDir", projdir);
            mBlock.SetVector("_ShadowPlane", _ShadowPlane);
            mBlock.SetVector("_ShadowFadeParams", _ShadowFadeParams);
            mBlock.SetFloat("_ShadowFalloff", _ShadowFalloff);
            mBlock.SetFloat("_ShadowInvLen", _ShadowInvLen);
            render.SetPropertyBlock(mBlock);
        }
    }

    #endregion
}
