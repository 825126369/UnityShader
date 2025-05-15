// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Customer/WaterNoiseOffset"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

		_MaskTex("Mask Texture", 2D) = "white" {}
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_NoiseIntensity ("_NoiseIntensity", Float) = 1.0
		_NoiseSpeedX ("_NoiseSpeedX", Float) = 1.0
		_NoiseSpeedY ("_NoiseSpeedY", Float) = 1.0

		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOption("Blend Option", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend mode", Float) = 1
		
		_WaveStrength("Wave Strength",Float) = 0.01
        _WaveFactor("Wave Factor",Float) = 50
        _TimeScale("Time Scale",Float) = 10
		_WaveDuration("Wave Duration",Float) = 3
		_WaveUVDistance("WaveUV Distance",Float) = 0.3

		_Aspect("Aspect", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend [_SrcBlend] [_DstBlend]

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

			#include "UnityCG.cginc"
			#ifdef UNITY_INSTANCING_ENABLED

				UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
					// SpriteRenderer.Color while Non-Batched/Instanced.
					UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
					// this could be smaller but that's how bit each entry is regardless of type
					UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
				UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

				#define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
				#define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)

			#endif // instancing

			CBUFFER_START(UnityPerDrawSprite)
			#ifndef UNITY_INSTANCING_ENABLED
				fixed4 _RendererColor;
				fixed2 _Flip;
			#endif
				float _EnableExternalAlpha;
			CBUFFER_END

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			// Material Color.
			fixed4 _Color;
			sampler2D _NoiseTex;
			fixed4 _NoiseTex_ST;
			sampler2D _MaskTex;
			fixed4 _MaskTex_ST;
			fixed _NoiseIntensity;
			fixed _NoiseSpeedX;
			fixed _NoiseSpeedY;

            uniform float4 _WaveCenters[100];
			uniform int _WaveCenters_Num = 0;
			
			float _WaveDuration;
			float _WaveStrength;
            float _WaveFactor;
            float _TimeScale;
			float _WaveUVDistance;

			float _Aspect;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
			{
				return float4(pos.xy * flip, pos.z, 1.0);
			}

			v2f SpriteVert(appdata_t IN)
			{
				v2f OUT;

				UNITY_SETUP_INSTANCE_ID (IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
				OUT.vertex = UnityObjectToClipPos(OUT.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color * _RendererColor;

				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				OUT.texcoord1 = TRANSFORM_TEX(IN.texcoord, _NoiseTex);

				return OUT;
			}

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

			#if ETC1_EXTERNAL_ALPHA
				fixed4 alpha = tex2D (_AlphaTex, uv);
				color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
			#endif

				return color;
			}

			fixed2 SamplerFromNoise(float2 uv)
			{
				fixed2 newUV = uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				fixed2 noiseColor = tex2D(_NoiseTex, newUV);
				noiseColor = (noiseColor * 2 - 1) * 0.01;
				return noiseColor.xy;
			}

			fixed4 SpriteFrag(v2f IN) : SV_Target
			{
				fixed2 waveOffset = fixed2(0, 0);
				for(int i = 0; i< _WaveCenters_Num; i++)
				{
					fixed2 UvPos = IN.texcoord * fixed2(_Aspect, 1);
					fixed2 centerPos = _WaveCenters[i].xy * fixed2(_Aspect, 1);
					fixed2 uvDir = normalize(UvPos - centerPos);
					fixed dis = distance(UvPos, centerPos);
					
					if (dis <= _WaveUVDistance)
					{
						fixed fStartTime = _WaveCenters[i].z;
						fixed _WaveStrength1 = _WaveStrength * (_WaveDuration - (_Time.y - fStartTime)) / _WaveDuration;
						fixed Strength = _WaveStrength1 * (_WaveUVDistance - dis) / _WaveUVDistance;
						fixed Offset = _WaveFactor * dis;
						waveOffset += Strength * uvDir * sin(_Time.y * _TimeScale + Offset);
					}
				}

				fixed2 timer =  fixed2(_Time.x, _Time.x);
				fixed2 noiseOffset = SamplerFromNoise(IN.texcoord1 + timer * fixed2(_NoiseSpeedX, _NoiseSpeedY));

				fixed4 maskColor = tex2D(_MaskTex, IN.texcoord);
				fixed4 color = tex2D(_MainTex, IN.texcoord + waveOffset + noiseOffset * _NoiseIntensity * maskColor.a);
				
			#if ETC1_EXTERNAL_ALPHA
				fixed4 alpha = tex2D (_AlphaTex, IN.texcoord);
				color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
			#endif

				color *= IN.color;
				color.rgb *= color.a;

				return color;
			}
			ENDCG
        }
    }
}
