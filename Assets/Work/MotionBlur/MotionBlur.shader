//作者：许珂
//抓取屏幕上的像素点，对其进行高斯模糊
Shader "Customer/MotionBlur"
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
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend mode", Float) = 10

        _MotionBlurSizeX("_MotionBlurSizeX", Float) = 0.0
        _MotionBlurSizeY("_MotionBlurSizeY", Float) = 0.0
		_GuassBlurSizeX("_GuassBlurSizeX", Float) = 0.0
        _GuassBlurSizeY("_GuassBlurSizeY", Float) = 0.0
        _SoftNearDistance("_SoftNearDistance", Float) = 0.0
         _SoftStrength("_SoftStrength", Float) = 0.0
        _fScaleY("_fScaleY", Float) = 1.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            //"IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend [_SrcBlend] [_DstBlend]

        CGINCLUDE
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #pragma fragmentoption ARB_precision_hint_fastest
                
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

            fixed _GuassBlurSizeX;
            fixed _GuassBlurSizeY;
            fixed _MotionBlurSizeX;
            fixed _MotionBlurSizeY;

            fixed _SoftNearDistance;
            fixed _SoftStrength;

            fixed _fScaleY;

            // 通过 GrabPass 自动赋值的变量
            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;

            static const float weightArray_Motion[9] = {
                    0.05, 0.09, 0.12,
                    0.15, 0.18, 0.15,
                    0.12, 0.09, 0.05
            };

            static const float weightArray_Guass[9] = {
                    0.05, 0.09, 0.12,
                    0.15, 0.18, 0.15,
                    0.12, 0.09, 0.05
            };

            // static const float weightArray_Motion[3] = {
		    //     0.2, 0.6, 0.2,
		    // };
            
            // static const float weightArray_Guass[9] = {
            //         0.05, 0.09, 0.12,
            //         0.15, 0.18, 0.15,
            //         0.12, 0.09, 0.05
            // };

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
                float4 grabPassPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
			
            inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
            {
                return float4(pos.xy * flip, pos.z, 1.0);
            }

            v2f SpriteVert_Normal(appdata_t IN)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID (IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                IN.vertex.y *= _fScaleY;
                OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                
                return OUT;
            }

            float GetSoftAlpha(float fOriAlpha, float uvY)
            {   
                float fAlpha = fOriAlpha;
                if (uvY > _SoftNearDistance + 0.5)
                {
                    fAlpha *= clamp((1.0 - uvY) / (1 - _SoftNearDistance - 0.5) *  _SoftStrength, 0.0, 1.0);
                }else if (0.5 - _SoftNearDistance > uvY)
                {
                    fAlpha *= clamp(uvY / (0.5 - _SoftNearDistance) *  _SoftStrength, 0.0, 1.0);
                }
                return fAlpha;
            }

            fixed4 SpriteFrag_Normal(v2f IN) : SV_Target
            {
               fixed4 averageColor = tex2D(_MainTex, IN.texcoord);

                averageColor.a = GetSoftAlpha(averageColor.a, IN.texcoord.y);
                averageColor.rgb *= averageColor.a;
                return averageColor;
            }

            half4 GRABPIXEL_Motion(float4 grabPassPosition, int i, int j)
            {
                float4 grabPosUV = UNITY_PROJ_COORD(grabPassPosition); 
                grabPosUV.xy /= grabPosUV.w;
                return tex2D(_GrabTexture, half2(grabPosUV.x + _GrabTexture_TexelSize.x * i * _MotionBlurSizeX, grabPosUV.y + _GrabTexture_TexelSize.y * j * _MotionBlurSizeY));
            }

            half4 GRABPIXEL_Gauss(float4 grabPassPosition, int i, int j)
            {
                float4 grabPosUV = UNITY_PROJ_COORD(grabPassPosition); 
                grabPosUV.xy /= grabPosUV.w;
                return tex2D(_GrabTexture, half2(grabPosUV.x + _GrabTexture_TexelSize.x * i * _GuassBlurSizeX, grabPosUV.y + _GrabTexture_TexelSize.y * j * _GuassBlurSizeY));
            }

            v2f SpriteVert_MotionBlur(appdata_t IN)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID (IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                IN.vertex.y *= _fScaleY;

                OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                OUT.grabPassPosition = ComputeGrabScreenPos(OUT.vertex);
                return OUT;
            }

            fixed4 SpriteFrag_MotionBlur(v2f IN) : SV_Target
            {
               fixed4 averageColor = (0, 0, 0, 0);

                for(int j = 0; j < 9; j++)
                {
                    averageColor += GRABPIXEL_Motion(IN.grabPassPosition, 0, j- 4) * weightArray_Motion[j];  
                }

                //averageColor.a = GetSoftAlpha(averageColor.a, IN.texcoord.y);
                averageColor.rgb *= averageColor.a;
                return averageColor;
            }

            v2f SpriteVert_GaussBlur(appdata_t IN)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID (IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                IN.vertex.y *= _fScaleY;
                OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                OUT.grabPassPosition = ComputeGrabScreenPos(OUT.vertex);
                return OUT;
            }

            fixed4 SpriteFrag_GaussBlur_Hor(v2f IN) : SV_Target
            {
                fixed4 averageColor = (0, 0, 0, 0);

                for(int j = 0; j < 9; j++)
                {
                    averageColor += GRABPIXEL_Gauss(IN.grabPassPosition, j - 4, 0) * weightArray_Guass[j]; 
                }

                //averageColor.a = GetSoftAlpha(averageColor.a, IN.texcoord.y);
                //averageColor.rgb *= averageColor.a;
                return averageColor;
            }
            
            fixed4 SpriteFrag_GaussBlur_Ver(v2f IN) : SV_Target
            {
                fixed4 averageColor = (0, 0, 0, 0);

                for(int j = 0; j < 9; j++)
                {
                    averageColor += GRABPIXEL_Gauss(IN.grabPassPosition, 0, j - 4) * weightArray_Guass[j]; 
                }

                //averageColor.a = GetSoftAlpha(averageColor.a, IN.texcoord.y);
                averageColor.rgb *= averageColor.a;
                return averageColor;
            }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert_Normal
            #pragma fragment SpriteFrag_Normal
            ENDCG
        }

        GrabPass {                        
            
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert_MotionBlur
            #pragma fragment SpriteFrag_MotionBlur
            ENDCG
        }

        GrabPass {                        
            
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert_GaussBlur
            #pragma fragment SpriteFrag_GaussBlur_Hor
            ENDCG
        }

        GrabPass {                        
            
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert_GaussBlur
            #pragma fragment SpriteFrag_GaussBlur_Ver
            ENDCG
        }

       
    }
}
