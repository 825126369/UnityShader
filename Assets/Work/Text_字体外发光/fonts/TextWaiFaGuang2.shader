Shader "Customer/UI/TextWaiFaGuang2"
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
         _DownsampleFactor("_DownsampleFactor", Float) = 2
         _ClipAlpha("_ClipAlpha", Float) = 0
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
       // Blend One One
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
            float4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            int _UIVertexColorAlwaysGammaSpace;

            float _LuminanceThreshold;
            float4 _GlowColor;
            float _GlowPower;
            float _BlurSizeX;
            float _BlurSizeY;
            float _BlurSpread;
            fixed4 _MainTex_TexelSize;
            float _DownsampleFactor;
            float _ClipAlpha;

            sampler2D _GrabTexture;
            sampler2D _BackgroundTexture;
            sampler2D GrabPass_Teture2;
            sampler2D GrabPass_Teture3;
            sampler2D GrabPass_Teture4;
            sampler2D GrabPass_Teture5;
            sampler2D GrabPass_Teture6;
            sampler2D GrabPass_Teture7;
            sampler2D GrabPass_Teture8;
            sampler2D GrabPass_Teture9;

            float4 _GrabTexture_TexelSize;
            float4 _BackgroundTexture_TexelSize;
            float4 GrabPass_Teture2_TexelSize;
            float4 GrabPass_Teture3_TexelSize;
            float4 GrabPass_Teture4_TexelSize;
            float4 GrabPass_Teture5_TexelSize;
            float4 GrabPass_Teture6_TexelSize;
            float4 GrabPass_Teture7_TexelSize;
            float4 GrabPass_Teture8_TexelSize;
            float4 GrabPass_Teture9_TexelSize;

            static const float weightArray[9] = {
		        0.05, 0.09, 0.12,
		        0.15, 0.18, 0.15,
		        0.12, 0.09, 0.05
            };

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
                OUT.grabPassPosition = ComputeGrabScreenPos(OUT.vertex);
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

        float4 GRABPIXEL(float4 grabPassPosition, int i, int j, int nIndex)
        {
            float4 grabPosUV = UNITY_PROJ_COORD(grabPassPosition); 
            grabPosUV.xy /= grabPosUV.w * _DownsampleFactor;

            float _BlurSizeXX =  _BlurSizeX + nIndex * _BlurSpread;
            float _BlurSizeYY =  _BlurSizeY + nIndex * _BlurSpread;
            return tex2D(_GrabTexture, float2(grabPosUV.x + _GrabTexture_TexelSize.x * i * _BlurSizeXX, grabPosUV.y + _GrabTexture_TexelSize.y * j * _BlurSizeYY));
        }

            // float4 GetGlowColor(float d, float scale)
            // {
	           //  float glow = d - (_GlowOffset) * 0.5 * scale;
	           //  float t = lerp(_GlowInner, _GlowOuter, step(0.0, glow)) * 0.5 * scale;
	           //  glow = saturate(abs(glow / (1.0 + t)));
	           //  glow = 1.0 - pow(glow, _GlowPower);
	           //  glow *= sqrt(min(1.0, t));
	           //  return float4(_GlowColor.rgb, saturate(_GlowColor.a * glow * 2));
            // }
            
            float4 frag(v2f IN) : SV_Target
            {
                float4 color = tex2Dproj(_BackgroundTexture, IN.grabPassPosition);
                //color.a *= color.r;
                color.rgb *= color.a;
               // color *= _GlowColor * _GlowPower;
                color = fragExtractBright(color);
                return color;
            }
        ENDCG

        CGINCLUDE
        v2f Vert_MoHu(appdata_t v)
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
       

        float4 MoHu_Hor_All(v2f IN, int nIndex)
        {
           // clip(-1);
          // float weightArray[3] = {0.4026, 0.2442, 0.0545}; // 大小为5的一维高斯核，实际只需记录3个权值
            float4 averageColor = (0, 0, 0, 0);
            for(int i = 0; i < 9; i++)
            {
                averageColor += GRABPIXEL(IN.grabPassPosition, i - 4, 0, nIndex) * weightArray[i];
            }
            
            //averageColor.a *= averageColor.r;
            averageColor.rgb *= averageColor.a;

                         // averageColor = fragExtractBright(averageColor);
                      //clip(averageColor.a - _ClipAlpha);

                                    clip(averageColor.a - _ClipAlpha);
            return averageColor;
        }

         float4 MoHu_Ver_All(v2f IN, int nIndex)
        {
               //  clip(-1);
            float4 averageColor = (0, 0, 0, 0);
            for(int i = 0; i < 9; i++)
            {
                averageColor += GRABPIXEL(IN.grabPassPosition, 0, i - 4, nIndex) * weightArray[i];
            }
            
            //averageColor.a *= averageColor.r;
            averageColor.rgb *= averageColor.a;
              //averageColor = fragExtractBright(averageColor);
            //clip(averageColor.a - _ClipAlpha);

            clip(averageColor.a - _ClipAlpha);
            return averageColor;
        }
        
        float4 Frag_Hor_MoHu1(v2f IN) : SV_Target
        {
            return MoHu_Hor_All(IN, 0);
        }

        float4 Frag_Ver_MoHu1(v2f IN) : SV_Target
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
                float4 color1 = tex2Dproj(_BackgroundTexture, IN.grabPassPosition);
                float4 color = color1;
                color.a *= color.r;

                clip(color.a - _ClipAlpha);
                color.rgb *= color.a;
                return color;
            }
        ENDCG

        GrabPass { "_BackgroundTexture"}
        Pass
        {
            Name "Pass1"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        GrabPass {}
        Pass
        {
            Name "Mohu1"
                    Blend One OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Hor_MoHu1
            ENDCG
        }

        GrabPass{}
        Pass
        {
                    Blend One OneMinusSrcAlpha
            Name "Mohu2"
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Ver_MoHu1
            ENDCG
        }

        GrabPass{}
        Pass
        {
                    Blend One OneMinusSrcAlpha
            Name "Mohu3"
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Hor_MoHu2
            ENDCG
        }

        GrabPass{}
        Pass
        {
                    Blend One OneMinusSrcAlpha
            Name "Mohu4"
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Ver_MoHu2
            ENDCG
        }

        GrabPass{}
        Pass
        {
            Blend One OneMinusSrcAlpha
            Name "Mohu5"
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Hor_MoHu3
            ENDCG
        }

        GrabPass{}
        Pass
        {
            Blend One OneMinusSrcAlpha
            Name "Mohu6"
            CGPROGRAM
            #pragma vertex Vert_MoHu
            #pragma fragment Frag_Ver_MoHu3
            ENDCG
        }
        
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
