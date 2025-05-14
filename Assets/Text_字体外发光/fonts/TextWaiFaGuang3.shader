Shader "Customer/UI/TextWaiFaGuang3"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowPower ("Glow Power", Float) = 2.0

         _LuminanceThreshold ("_LuminanceThreshold", Float) = 0.5
         _BlurSizeX("_BlurSizeX", Float) = 2
         _BlurSizeY("_BlurSizeY", Float) = 2
         _BlurSpread("_BlurSpread", Float) = 2
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
        
        CGINCLUDE
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
                float3 normal   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4  mask : TEXCOORD2;

                float4 grabPassPosition : TEXCOORD3;
                float2 uv[9]: TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            int _UIVertexColorAlwaysGammaSpace;

            float _LuminanceThreshold;
            float4 _GlowColor;
            float _GlowPower;

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
                return OUT;
            }

             float luminance(float4 color) 
             { // 计算亮度
			    return  0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b; 
		     }
		
		    // 采样纹理的亮度减去亮度阈值, 小于0的值将取0
		    float4 fragExtractBright(float4 c) 
            {
               float tt = luminance(c) - _LuminanceThreshold;
			    float val = saturate(tt);
			    return c * val;
		    }

            float4 frag(v2f IN) : SV_Target
            {
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

                color = fragExtractBright(color);
                color *=_GlowColor * _GlowPower;
                color.rgb *= color.a;
                return color;
            }
        ENDCG

        CGINCLUDE
        float _BlurSizeX;
        float _BlurSizeY;
        float _BlurSpread;
        fixed4 _MainTex_TexelSize;
        sampler2D _GrabTexture;
        float4 _GrabTexture_TexelSize;

        static const float weightArray[9] = {
		        0.05, 0.09, 0.12,
		        0.15, 0.18, 0.15,
		        0.12, 0.09, 0.05
		};


        v2f Vert_Hor_MoHu(appdata_t v)
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
			half2 uv = OUT.texcoord;
			OUT.uv[0] = uv;
			OUT.uv[1] = uv + float2(_MainTex_TexelSize.y * 1.0, 0) * _BlurSize;
			OUT.uv[2] = uv - float2(_MainTex_TexelSize.y * 1.0, 0) * _BlurSize;
			OUT.uv[3] = uv + float2(_MainTex_TexelSize.y * 2.0, 0) * _BlurSize;
			OUT.uv[4] = uv - float2(_MainTex_TexelSize.y * 2.0, 0) * _BlurSize;
            return OUT;
        }

        v2f Vert_Ver_MoHu(appdata_t v)
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

            float _BlurSize =  _BlurSizeY + nIndex * _BlurSpread;
			half2 uv = OUT.texcoord;
			o.uv[0] = uv;
			o.uv[1] = uv + float2(0, _MainTex_TexelSize.x * 1.0) * _BlurSize;
			o.uv[2] = uv - float2(0, _MainTex_TexelSize.x * 1.0) * _BlurSize;
			o.uv[3] = uv + float2(0, _MainTex_TexelSize.x * 2.0) * _BlurSize;
			o.uv[4] = uv - float2(0, _MainTex_TexelSize.x * 2.0) * _BlurSize;
            return OUT;
        }

        float4 Frag_MoHu(v2f IN, int nIndex)
        {
            float weight[3] = {0.4026, 0.2442, 0.0545}; // 大小为5的一维高斯核，实际只需记录3个权值
			fixed3 sum = tex2D(_MainTex, i.uv[0]).rgb * weight[0];
			for (int j = 1; j < 3; j++) {
				sum += tex2D(_MainTex, i.uv[j * 2 - 1]).rgb * weight[j]; // 中心右侧或下侧的纹理*权值
				sum += tex2D(_MainTex, i.uv[j * 2]).rgb * weight[j]; // 中心左侧或上侧的纹理*权值
			}
			return fixed4(sum, 1.0);
        }
        
        float4 Ver_Hor_MoHu_1(appdata_t v) : SV_Target
        {
            return Vert_Hor_MoHu(IN, 0);
        }

        float4 Ver_Hor_MoHu_2(v2f IN) : SV_Target
        {
            return MoHu_Ver_All(IN, 0);
        }

        float4 Ver_Hor_MoHu_3(v2f IN) : SV_Target
        {
            return MoHu_Ver_All(IN, 0);
        }

        float4 Frag_Hor_MoHu2(v2f IN) : SV_Target
        {
            return MoHu_Hor_All(IN, 1);
        }

        float4 Frag_Ver_MoHu2(v2f IN) : SV_Target
        {
            return MoHu_Ver_All(IN, 1);
        }

        float4 Frag_Hor_MoHu3(v2f IN) : SV_Target
        {
            return MoHu_Hor_All(IN, 2);
        }

        float4 Frag_Ver_MoHu3(v2f IN) : SV_Target
        {
            return MoHu_Ver_All(IN, 2);
        }
        ENDCG

        CGINCLUDE
            v2f Vert_Bloom(appdata_t v)
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
                OUT.grabPassPosition = ComputeGrabScreenPos(OUT.vertex);
                return OUT;
            }
            
            float4 Frag_Bloom(v2f IN) : SV_Target
            {
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

                color = color + GRABPIXEL(IN.grabPassPosition, 0, 0, 0);
                color.rgb *= color.a;
                return color;
            }
        ENDCG

        //GrabPass {}
        // Pass
        // {
        //     Name "Pass1"
        //     CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment frag
        //     ENDCG
        // }

        GrabPass {}
        Pass
        {
            Name "Mohu1"

            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Hor_MoHu1
            ENDCG
        }

        GrabPass {}
        Pass
        {
            Name "Mohu2"
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Ver_MoHu1
            ENDCG
        }

        GrabPass {}
        Pass
        {
            Name "Mohu3"
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Hor_MoHu2
            ENDCG
        }

        GrabPass {}
        Pass
        {
            Name "Mohu4"
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Ver_MoHu2
            ENDCG
        }

        GrabPass {}
        Pass
        {
            Name "Mohu5"
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Hor_MoHu3
            ENDCG
        }

        GrabPass {}
        Pass
        {
            Name "Mohu6"
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Ver_MoHu3
            ENDCG
        }

        GrabPass {}
        Pass
        {
            Name "Pass Bloom"
            CGPROGRAM
            #pragma vertex Vert_Bloom
            #pragma fragment Frag_Bloom
            ENDCG
        }
    }
}
