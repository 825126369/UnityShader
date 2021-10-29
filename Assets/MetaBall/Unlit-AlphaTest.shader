// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unlit alpha-cutout shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/Transparent Cutout" {
Properties {
	_AlphaMask ("Base (RGB) Trans (A)", 2D) = "white" {}
	_MainTex ("Sprite Texture", 2D) = "white" {}
	_NoiseTex("Noise Texture", 2D) = "white" {}
	_NoiseIntensity ("_NoiseIntensity", Float) = 2.0
	_MoveIntensity ("_MoveIntensity", Float) = 2.0
	_Color ("Main color", Color) = (1,1,1,1)

	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5

	_Stroke ("Stroke alpha", Range(0,1)) = 0.1
	_StrokeColor ("Stroke color", Color) = (1,1,1,1)
}
SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 100

	Lighting Off

	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord0 : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
				half2 texcoord2 : TEXCOORD2;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _AlphaMask;
			float4 _AlphaMask_ST;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			float _Cutoff;
			float _NoiseIntensity;
			float _MoveIntensity;

			half4 _Color;

			float _Stroke;
			half4 _StrokeColor;
			
			float2 SamplerFromNoise(float2 uv)
			{
				float2 newUV = uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				float2 noiseColor = tex2D(_NoiseTex, newUV);
				noiseColor = (noiseColor * 2 - 1) * 0.01;
				return noiseColor.xy;
			}

			v2f vert (appdata_t v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord0 = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.texcoord1 = TRANSFORM_TEX(v.texcoord, _AlphaMask);
				o.texcoord2 = TRANSFORM_TEX(v.texcoord, _NoiseTex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target {
				float4 col = tex2D(_AlphaMask, i.texcoord1);
				clip(col.a - _Cutoff);

				if (col.a < _Stroke) {
					col = _StrokeColor;
				} else {
					float2 timer =  float2(sin(_Time.x), sin(_Time.x));
					float2 noiseOffset = SamplerFromNoise(i.texcoord2 + float2(_Time.x, _Time.x));
					col = tex2D(_MainTex, i.texcoord0  + noiseOffset * _NoiseIntensity + float2(_Time.x, _Time.x) * _MoveIntensity);
					//col = tex2D(_MainTex, i.texcoord0);
				}

				return col;
			}
		ENDCG
	}
}

}
