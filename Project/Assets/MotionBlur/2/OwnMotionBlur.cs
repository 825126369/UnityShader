using UnityEngine;
public class OwnMotionBlur : MonoBehaviour 
｛
    public Shader motionBlurShader;
    private Material motionBlurMaterial = null;
    public Material material
    ｛
        get
        ｛
            motionBlurMaterial = CheckShaderAndCreateMaterial(motionBlurShader, motionBlurMaterial);
            return motionBlurMaterial;
        ｝
    ｝
    //定义运动模糊在混合图像时使用的模糊参数，blurAmount的值越大，运动拖尾的效果就越明显，为了防止拖尾效果
    //完全替代当前帧的渲染结果，我们把它的值截取在0.0~0.9范围内。
    [Range(0.0f, 0.9f)]
    public float blurAmount = 0.5f;
    //保存之前图像叠加的结果
    private RenderTexture accumulationTexture;
    private void OnDisable()
    ｛
        DestroyImmediate(accumulationTexture);
    ｝
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    ｛
        if(material != null)
        ｛
            if(accumulationTexture == null || accumulationTexture.width != source.width || accumulationTexture.height != source.height)
            ｛
                DestroyImmediate(accumulationTexture);
                accumulationTexture = new RenderTexture(source.width, source.height, 0);
                //这个变量不会显示在Hierarchy中，也不会保存在场景中
                accumulationTexture.hideFlags = HideFlags.HideAndDontSave;
                Graphics.Blit(source, accumulationTexture);
            ｝
            //表明我们需要进行一个渲染纹理的恢复操作。恢复操作发生在渲染到纹理而该纹理又没有被提前清空或销毁的清空的情况下
            accumulationTexture.MarkRestoreExpected();
            material.SetFloat("_BlurAmount", 1.0f - blurAmount);
            Graphics.Blit(source, accumulationTexture, material);
            Graphics.Blit(accumulationTexture, destination);
        ｝
        else
        ｛
            Graphics.Blit(source, destination);
        ｝
    ｝
｝