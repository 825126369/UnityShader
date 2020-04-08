// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Customer/Gaussianblur2"
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

		_BlurSize("Blure Size", Float) = 1.0
        
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

        GrabPass {                        
            //Tags { "LightMode" = "Always" }
        }

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
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
			float _BlurSize;

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;

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
                
                OUT.grabPassPosition = ComputeGrabScreenPos(OUT.vertex);
                return OUT;
            }

            half4 GRABPIXEL(float4 grabPassPosition, int i, int j)
            {
                float4 grabPosUV = UNITY_PROJ_COORD(grabPassPosition); 
                return tex2Dproj(_GrabTexture, (half4(grabPosUV.x + _GrabTexture_TexelSize.x * i * _BlurSize, grabPosUV.y + _GrabTexture_TexelSize.y * j * _BlurSize, grabPosUV.z, grabPosUV.w)));
            }

            fixed4 SpriteFrag(v2f IN) : SV_Target
            {
                if (true)
                {
                    const float weightArray[3][3] = {
                        0.0947416, 0.118318, 0.0947416,
                        0.118318, 0.147761, 0.118318,
                        0.0947416, 0.118318, 0.0947416
                    };

                    half4 averageColor = fixed4(0, 0, 0, 0);

                    for(int i = 0; i <= 2; i++)
                    {
                        for(int j = 0; j <= 2; j++)
                        {
                            half4 color = GRABPIXEL(IN.grabPassPosition, i - 1, j - 1);
                            averageColor += color * weightArray[i][j];
                        }
                    }

                    averageColor.rgb *= averageColor.a;
                    // fixed4 tint = tex2D(_MainTex, IN.texcoord) * IN.color;
                    // tint.rgb *= tint.a;

                    // fixed4 color2 = averageColor * tint;
                    return averageColor;
                }
                else
                {
                    fixed4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
                    color.rgb *= color.a;

                    return color;
                }
            }
            ENDCG
        }
    }
}
