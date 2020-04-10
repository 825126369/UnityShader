// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Customer/WaterStrike"
{
    Properties
    {
         _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOption("Blend Option", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend mode", Float) = 1

        // 用于确保 在哪个区域 噪声图 起作用
        _MaskTex("Mask Texture", 2D) = "white" {}

		_NoiseTex("Noise Texture", 2D) = "white" {}
		_NoiseIntensity ("_NoiseIntensity", Float) = 2.0
		_NoiseSpeedX ("_NoiseSpeedX", Float) = 2.0
		_NoiseSpeedY ("_NoiseSpeedY", Float) = 2.0
        
        _GradTex("Gradient", 2D) = "white" {}
        _WaveSpeed("Wave Speed", Range(-10, 10)) = 1
        _RefractionStrength("折射 强度", Range(0, 10)) = 0.5
        _Aspect(" W / H Aspect", Float) = 1
		_WaveDuration("Wave Duration",Float) = 2.0

        _ReflectionStrength("反射 强度", Range(0, 10)) = 1
        _ReflectionColor("反射 颜色", Color) = (1,1,1,1)
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
                    UNITY_DEFINE_INSTANCED_PROP(float4, unity_SpriteRendererColorArray)
                    // this could be smaller but that's how bit each entry is regardless of type
                    UNITY_DEFINE_INSTANCED_PROP(float2, unity_SpriteFlipArray)
                UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

                #define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
                #define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)

            #endif // instancing

            CBUFFER_START(UnityPerDrawSprite)
            #ifndef UNITY_INSTANCING_ENABLED
                float4 _RendererColor;
                float2 _Flip;
            #endif
                float _EnableExternalAlpha;
            CBUFFER_END

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float4 _Color;
            
            sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			float _NoiseIntensity;
			float _NoiseSpeedX;
			float _NoiseSpeedY;

            sampler2D _GradTex;
            float4 _WaveCenters[100];
			int _WaveCenters_Num;
            float _WaveSpeed;
            float _RefractionStrength;
            float _Aspect;

            float _ReflectionStrength;
            float4 _ReflectionColor;

            float _WaveDuration;

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
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            inline float4 UnityFlipSprite(in float3 pos, in float2 flip)
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
                
                return OUT;
            }

            float2 SamplerFromNoise(v2f IN)
			{
                float2 timer = float2(_Time.x, _Time.x);
                float2 newUV = IN.texcoord + timer * float2(_NoiseSpeedX, _NoiseSpeedY);
				float4 noiseColor = tex2D(_NoiseTex, newUV);
				noiseColor = (noiseColor * 2 - 1);
                noiseColor *= _NoiseIntensity * 0.01;
				return noiseColor.xy;
			}

            float2 wave(float2 uv, float2 origin, float time)
            {
                float dis = length(uv - origin);
                float t = time - dis / _WaveSpeed;
                float2 offset = (tex2D(_GradTex, float2(t, 0)).xx - 0.5) * 2;
                return offset;
            }

            float2 UvWaveAll(v2f IN)
            {
                float2 dxUv = float2(0.01, 0.01);
                float2 waveOffset = float2(0, 0);
                for(int i = 0; i < _WaveCenters_Num; i++)
				{
                    float2 p = IN.texcoord;
                    float fPlayTime = _Time.y - _WaveCenters[i].z;
                    float2 oriPos = _WaveCenters[i].xy;
                    float dis = length(p - oriPos);
                    float2 uvDir = normalize(p - oriPos);

                    float2 dw = wave(p + dxUv, oriPos, fPlayTime) - wave(p, oriPos, fPlayTime);
                    float2 waveOffset1 = dw * float2(_Aspect, 1) * _RefractionStrength;
                    waveOffset1 *= (1 - saturate(fPlayTime / _WaveDuration)) * (1 - saturate(dis / 1.0));
                    waveOffset += waveOffset1;
                }

                return waveOffset;
            }

            float4 SpriteFrag(v2f IN) : SV_Target
            {
                float2 waveOffset = UvWaveAll(IN);
				float2 noiseOffset = SamplerFromNoise(IN);
				float maskWaterAlpha = tex2D(_MaskTex, IN.texcoord).a;

                float4 color = tex2D(_MainTex, IN.texcoord + (waveOffset + noiseOffset) * maskWaterAlpha);

                #if ETC1_EXTERNAL_ALPHA
                    float4 alpha = tex2D (_AlphaTex, IN.texcoord);
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
