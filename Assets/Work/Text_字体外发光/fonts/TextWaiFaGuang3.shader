// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Customer/UI/TextWaiFaGuang3"
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

        _FaceColor			("Face Color", Color) = (1,1,1,1)

        _GlowColor			("_GlowColor", Color) = (0, 1, 0, 0.5)
        _GlowSize			("_GlowSize", Range(-1, 1)) = 0
        _GlowPower			("_GlowPower", Range(0, 1)) = 0.75

	    _GlowOffset			("_GlowOffset", Range(-1,1)) = 0
	    _GlowInner			("_GlowInner", Range(0,1)) = 0.05
	    _GlowOuter			("_GlowOuter", Range(0,1)) = 0.05
        _GradientScale    ("_GradientScale", Range(0, 100)) = 0.75

        _BlurSize("_BlurSize", Range(0, 0.5)) = 0
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
        Blend One OneMinusSrcAlpha
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
                float2 texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO

                float4 param: TEXCOORD3;		// alphaClip, scale, bias, weight
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            int _UIVertexColorAlwaysGammaSpace;

            float4 _FaceColor;
            float _GlowSize;
            float _BlurSize;
            uniform fixed4 		_GlowColor;					// RGBA : Color + Intesity
            uniform float 		_GlowOffset;				// v[-1, 1]
            uniform float 		_GlowOuter;					// v[ 0, 1]
            uniform float 		_GlowInner;					// v[ 0, 1]
            uniform float 		_GlowPower;					// v[ 1, 1/(1+4*4)]
            uniform float 		_GradientScale;				// v[ 0, 100]

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));


                if (_UIVertexColorAlwaysGammaSpace)
                {
                    if(!IsGammaSpace())
                    {
                        v.color.rgb = UIGammaToLinear(v.color.rgb);
                    }
                }

                OUT.color = v.color * _Color;

               // --------------------------------------------------------------------------------
			    float scale = rsqrt(dot(pixelSize, pixelSize));
                scale *= abs(v.texcoord1.y) * _GradientScale;
                scale = 1;
       //          float weight = lerp(_WeightNormal, _WeightBold, bold) / 4.0;
			    // weight = (weight + _FaceDilate) * _ScaleRatioA * 0.5;

                float weight = 0;
			    float bias = 1.0;

                float alphaClip = 1.0;
			    alphaClip = min(alphaClip, 1.0 - _GlowOffset - _GlowOuter);
			    alphaClip = alphaClip / 2.0 - (0.5 / scale) - weight;

                OUT.param =	float4(alphaClip, scale, bias, weight);
                return OUT;
            }

            float4 GetGlowColor(float d, float scale)
            {
                float _ScaleRatioB = 1;

	            float glow = d - (_GlowOffset*_ScaleRatioB) * 0.5 * scale;
	            float t = lerp(_GlowInner, (_GlowOuter * _ScaleRatioB), step(0.0, glow)) * 0.5 * scale;
	            glow = saturate(abs(glow/(1.0 + t)));
	            glow = 1.0-pow(glow, _GlowPower);
	            glow *= sqrt(min(1.0, t)); // Fade off glow thinner than 1 screen pixel
	            return float4(_GlowColor.rgb, saturate(_GlowColor.a * glow * 2));
            }

            fixed4 GetColor(half d, fixed4 faceColor)
            {
	            half faceAlpha = 1 - saturate(d / (1.0));
	            faceColor.rgb *= faceColor.a;
	            faceColor *= faceAlpha;
	            return faceColor;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                //Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
                //The incoming alpha could have numerical instability, which makes it very sensible to
                //HDR color transparency blend, when it blends with the world's texture.
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;

                half4 color = IN.color * (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
                color.rgb *= color.a;

                float glow = smoothstep(0.5 - _GlowSize, 0.5, color.a);
                glow = pow(glow, _GlowPower);
                return lerp(color, _GlowColor, glow);
                
                // float glow = smoothstep(0.5 - _GlowSize, 0.5, dot(IN.texcoord - 0.5, IN.texcoord - 0.5)) * IN.color;
                // return color + _GlowColor * glow;

                // float2 offset = float2(_BlurSize, 0);
                // fixed4 col = tex2D(_MainTex, IN.texcoord) * 0.25;
                // col += tex2D(_MainTex, IN.texcoord + offset) * 0.25;
                // col += tex2D(_MainTex, IN.texcoord - offset) * 0.25;
                // col += tex2D(_MainTex, IN.texcoord + float2(0, _BlurSize)) * 0.25;
                // col += tex2D(_MainTex, IN.texcoord - float2(0, _BlurSize)) * 0.25;
                // return col;
            }
        ENDCG
        }
    }
}
