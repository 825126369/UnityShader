// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Customer/ShaderEffect/WaterStrikeEffect"
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

        _MaskTex("Mask Texture", 2D) = "white" {}
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_NoiseIntensity ("_NoiseIntensity", Float) = 1.0
		_NoiseSpeedX ("_NoiseSpeedX", Float) = 1.0
		_NoiseSpeedY ("_NoiseSpeedY", Float) = 1.0
        
        _GradTex("Gradient", 2D) = "white" {}
        _WaveSpeed("Wave Speed", Range(-10, 10)) = 1
        _ReflectionStrength("反射 强度", Range(0, 10)) = 1
        _ReflectionColor("反射 颜色", Color) = (1,1,1,1)
        _RefractionStrength("折射 强度", Range(0, 10)) = 0.5
        _Aspect("Aspect", Float) = 1

		_WaveStrength("Wave Strength",Float) = 0.01
        _WaveFactor("Wave Factor",Float) = 50
        _TimeScale("Time Scale",Float) = 10
		_WaveDuration("Wave Duration",Float) = 3
		_WaveUVDistance("WaveUV Distance",Float) = 0.3

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
            fixed4 _Color;
            
            sampler2D _NoiseTex;
			fixed4 _NoiseTex_ST;
			sampler2D _MaskTex;
			fixed4 _MaskTex_ST;
			fixed _NoiseIntensity;
			fixed _NoiseSpeedX;
			fixed _NoiseSpeedY;

            sampler2D _GradTex;
            uniform float4 _WaveCenters[100];
			uniform int _WaveCenters_Num = 0;
            float _WaveSpeed;
            float _ReflectionStrength;
            float4 _ReflectionColor;
            float _RefractionStrength;
            float _Aspect;

            float _WaveDuration;
			float _WaveStrength;
            float _WaveFactor;
            float _TimeScale;
			float _WaveUVDistance;

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

            float wave(float2 position, float2 origin, float time)
            {
                float dis = length(position - origin);
                float t = time - dis / _WaveSpeed;
                
                //float t = 0;
                float offset = (tex2D(_GradTex, float2(t, 0)).a - 0.5) * 2;
                return offset;
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
                float2 dx = float2(0.01, 0);
                float2 dy = float2(0, 0.01);
                fixed2 waveOffset = fixed2(0, 0);
				for(int i = 0; i < _WaveCenters_Num - 1; i++)
				{
                    float2 p = IN.texcoord * fixed2(_Aspect, 1);
                    fixed fStartTime = _WaveCenters[i].z;
                    fixed fPlayTime = _Time.y - fStartTime;
                    fixed2 oriPos = _WaveCenters[i].xy * fixed2(_Aspect, 1);
                    float dis = length(p - oriPos);
                    fixed2 uvDir = normalize(p - oriPos);

                    float w = wave(p, oriPos, fPlayTime);
                    float2 dw = float2(wave(p + dx, oriPos, fPlayTime) - w, wave(p + dy, oriPos, fPlayTime) - w);
                    float2 waveOffset1 = dw * fixed2(1, 1 /_Aspect) * 0.2 * _RefractionStrength;
                    waveOffset1 *= (1 - saturate(fPlayTime / 2.0)) * (1 - saturate(dis / 1.0));
                    waveOffset += waveOffset1;
                    
                    //fixed2 lastPos = _WaveCenters[i + 1].xy * fixed2(_Aspect, 1);

                    // fixed2 direction = normalize(oriPos - lastPos); // 划水的时候的此刻方向， 法线方向
                    // fixed k1 = -1 / ((oriPos.y - lastPos.y) / (oriPos.x - lastPos.x));
                    // fixed x1 = oriPos.x + 0.01;
                    // fixed y1 = k1 * (x1 - oriPos.x) + oriPos.y;
                    // fixed2 tangent = normalize(fixed2(x1, y1) - oriPos);

                    // float dis = length(p - oriPos);
                    // fixed2 pDir = normalize(p - oriPos);
                    // if (dis < 0.2)
                    // {
                    //     waveOffset += (1 - dot(pDir, direction));
                    // }

                    // if (dis <= _WaveUVDistance)
                    // {
                    //     fixed _WaveStrength1 = _WaveStrength * (_WaveDuration - (_Time.y - fStartTime)) / _WaveDuration;
                    //     fixed Strength = _WaveStrength1 * (_WaveUVDistance - dis) / _WaveUVDistance;
                    //     fixed Offset = _WaveFactor * dis;
                    //     waveOffset += Strength * uvDir * sin(_Time.y * _TimeScale + Offset);
                    // }
                }

                fixed2 timer =  fixed2(_Time.x, _Time.x);
				fixed2 noiseOffset = SamplerFromNoise(IN.texcoord1 + timer * fixed2(_NoiseSpeedX, _NoiseSpeedY));
				fixed4 maskColor = tex2D(_MaskTex, IN.texcoord);
                fixed4 color = tex2D(_MainTex, IN.texcoord + waveOffset + noiseOffset * _NoiseIntensity * maskColor.a);
                // float fr = pow(length(dwAverage) * 3 * _ReflectionStrength, 3);
                // color = lerp(color, _ReflectionColor, fr);
                
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
