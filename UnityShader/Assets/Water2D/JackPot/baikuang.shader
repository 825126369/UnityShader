// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Customer/ShaderEffect/UI/Default"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOption("Blend Option", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend mode", Float) = 1
		
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_NoiseIntensity ("_NoiseIntensity", Float) = 1.0
		_NoiseSpeedX ("_NoiseSpeedX", Float) = 1.0
		_NoiseSpeedY ("_NoiseSpeedY", Float) = 1.0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend [_SrcBlend] [_DstBlend]
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float2 texcoord  : TEXCOORD0;
				float2 texcoord1  : TEXCOORD2;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

			sampler2D _NoiseTex;
			fixed4 _NoiseTex_ST;
			fixed _NoiseIntensity;
			fixed _NoiseSpeedX;
			fixed _NoiseSpeedY;

			float _Aspect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;

				OUT.texcoord1 = TRANSFORM_TEX(v.texcoord, _NoiseTex);
                return OUT;
            }

			half2 SamplerFromNoise(float2 uv)
			{
				half2 newUV = uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				half2 noiseColor = tex2D(_NoiseTex, newUV);
				noiseColor = (noiseColor * 2 - 1) * 0.01;
				return noiseColor.xy;
			}

            fixed4 frag(v2f IN) : SV_Target
            {
				half2 timer =  half2(_Time.x, _Time.x);
				half2 noiseOffset = SamplerFromNoise(IN.texcoord1 + timer * fixed2(_NoiseSpeedX, _NoiseSpeedY));
				half4 color = tex2D(_MainTex, IN.texcoord + noiseOffset * _NoiseIntensity);
				//half4 color = tex2D(_MainTex, IN.texcoord);

                color = (color + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
