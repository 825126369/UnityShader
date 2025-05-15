Shader "MyShader/Bloom" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {} // 主纹理
		_Bloom ("Bloom (RGB)", 2D) = "black" {} // Bloom处理需要的纹理(即高斯模糊处理后的纹理)
		_LuminanceThreshold ("Luminance Threshold", Float) = 0.5 // 亮度阈值
		_BlurSize ("Blur Size", Float) = 1.0 // 模糊尺寸(纹理坐标的偏移量)
	}

	SubShader {
		CGINCLUDE
		
		#include "UnityCG.cginc"
		
		sampler2D _MainTex; // 主纹理
		half4 _MainTex_TexelSize; // _MainTex的像素尺寸大小, float4(1/width, 1/height, width, height)
		sampler2D _Bloom; // Bloom处理需要的纹理(即高斯模糊处理后的纹理)
		float _LuminanceThreshold; // 亮度阈值
		float _BlurSize; // 模糊尺寸(纹理坐标的偏移量)

		fixed luminance(fixed4 color) { // 计算亮度
			return  0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b; 
		}
		
		// 采样纹理的亮度减去亮度阈值, 小于0的值将取0
		fixed4 fragExtractBright(v2f_img i) : SV_Target { // v2f_img为内置结构图, 里面只包含pos和uv
			fixed4 c = tex2D(_MainTex, i.uv);
			fixed val = saturate(luminance(c) - _LuminanceThreshold);
			return c * val;
		}
		
		struct v2fBloom { // v2fBloom之所以不用v2f_img替代, 因为v2fBloom.uv是四维的, 而v2f_img.uv是二维的
			float4 pos : SV_POSITION; // 裁剪空间顶点坐标
			half4 uv : TEXCOORD0; // 纹理uv坐标
		};
		
		v2fBloom vertBloom(appdata_img v) {
			v2fBloom o;
			o.pos = UnityObjectToClipPos (v.vertex); // 模型空间顶点坐标变换到裁剪空间, 等价于: mul(UNITY_MATRIX_MVP, v.vertex)
			o.uv.xy = v.texcoord;
			o.uv.zw = v.texcoord;
			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0.0)
				o.uv.w = 1.0 - o.uv.w; // 平台差异化处理
			#endif
			return o;
		}
		
		fixed4 fragBloom(v2fBloom i) : SV_Target {
			return tex2D(_MainTex, i.uv.xy) + tex2D(_Bloom, i.uv.zw);
			//return tex2D(_Bloom, i.uv.zw);
		}

		ENDCG

		ZTest Always Cull Off ZWrite Off
		
		Pass {  
			CGPROGRAM
			#pragma vertex vert_img // 使用内置的vert_img顶点着色器
			#pragma fragment fragExtractBright
			ENDCG  
		}
		
		UsePass "MyShader/GaussianBlur/GAUSSIAN_BLUR_VERTICAL" // 垂直高斯模糊处理
		
		UsePass "MyShader/GaussianBlur/GAUSSIAN_BLUR_HORIZONTAL" // 水平高斯模糊处理
		
		Pass {  
			CGPROGRAM  
			#pragma vertex vertBloom
			#pragma fragment fragBloom  
			ENDCG  
		}
	}

	FallBack Off
}
