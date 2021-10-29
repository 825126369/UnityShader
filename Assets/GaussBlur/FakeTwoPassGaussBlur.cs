using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeTwoPassGaussBlur : MonoBehaviour
{
    [Range(0, 6), Tooltip("[降采样次数]向下采样的次数。此值越大,则采样间隔越大,需要处理的像素点越少,运行速度越快。")]
    public int DownSampleNum = 2;
    public Material mMaterial;
    
    private void Start()
    {
        //mMaterial = transform.GetComponent<SpriteRenderer>().sharedMaterial;
    }

    // 挂载到照相机身上的脚本 才会执行这个方法，并且影响全局，并不可取！！！
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Debug.Log("OnRenderImage 1");

        if (mMaterial)
        {
            int renderWidth = src.width >> DownSampleNum;
            int renderHeight = src.height >> DownSampleNum;

            RenderTexture m_rt = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, src.format);
            RenderTexture m_Temprt = null;

            m_Temprt = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, src.format);
            Graphics.Blit(src, m_Temprt);
            RenderTexture.ReleaseTemporary(m_rt);
            m_rt = m_Temprt;

            // 横向
            m_Temprt = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, src.format);
            Graphics.Blit(m_rt, m_Temprt, mMaterial, 2);
            RenderTexture.ReleaseTemporary(m_rt);
            m_rt = m_Temprt;

            // 纵向
            m_Temprt = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, src.format);
            Graphics.Blit(m_rt, m_Temprt, mMaterial, 3);
            RenderTexture.ReleaseTemporary(m_rt);
            m_rt = m_Temprt;

            // 目的地
            Graphics.Blit(m_rt, dest);
            RenderTexture.ReleaseTemporary(m_rt);
        }
    }
}
