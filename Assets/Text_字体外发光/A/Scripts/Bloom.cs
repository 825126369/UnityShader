using UnityEngine;

[ExecuteInEditMode] // 编辑态可以查看脚本运行效果
[RequireComponent(typeof(Camera))] // 需要相机组件
public class Bloom : MonoBehaviour {
	private Material material = null; // 材质
	[Range(0, 4)]
	public int iterations = 3; // 高斯模糊迭代次数
	[Range(0.2f, 3.0f)]
	public float blurSpread = 0.6f; // 每次迭代纹理坐标偏移的速度
	[Range(1, 8)]
	public int downSample = 2; // 降采样比率
	[Range(0.0f, 4.0f)]
	public float luminanceThreshold = 0.6f; // 亮度阈值

    private void Start() {
		material = new Material(Shader.Find("MyShader/Bloom"));
		material.hideFlags = HideFlags.DontSave;
	}

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
		if (material != null) {
			material.SetFloat("_LuminanceThreshold", luminanceThreshold); // 设置亮度阈值
			int rtW = src.width/downSample; // 降采样的纹理宽度
			int rtH = src.height/downSample; // 降采样的纹理高度
			RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
			buffer0.filterMode = FilterMode.Bilinear; // 滤波模式设置为双线性
			Graphics.Blit(src, buffer0, material, 0);
			for (int i = 0; i < iterations; i++) {
				material.SetFloat("_BlurSize", 1.0f + i * blurSpread);
				RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
				Graphics.Blit(buffer0, buffer1, material, 1); // 渲染垂直的Pass(高斯模糊)
				RenderTexture.ReleaseTemporary(buffer0);
				buffer0 = buffer1;
				buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
				Graphics.Blit(buffer0, buffer1, material, 2); // 渲染垂直的Pass(高斯模糊)
				RenderTexture.ReleaseTemporary(buffer0);
				buffer0 = buffer1;
			}
			material.SetTexture("_Bloom", buffer0); // 将高斯模糊处理后的纹理设置给_Bloom
			Graphics.Blit(src, dest, material, 3);
			RenderTexture.ReleaseTemporary(buffer0);
		} else {
			Graphics.Blit(src, dest);
		}
	}
}
