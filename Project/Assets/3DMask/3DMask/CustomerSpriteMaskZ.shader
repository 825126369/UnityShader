// Upgrade NOTE: upgraded instancing buffer 'PerDrawSprite' to new syntax.

Shader "Customer/Theme/CrazyDollar/CustomerSpriteMaskZ" {
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

		_WorldPosMaskPosUp("_WorldPosMaskPosUp", Vector) = (0.0, 10000.0, 0.0, 1.0)
        _WorldPosMaskPosDown("_WorldPosMaskPosDown", Vector) = (0.0, -10000.0, 0.0, 1.0)
        
        _Stencil ("Stencil ID", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
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
        Blend One OneMinusSrcAlpha

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            #ifdef UNITY_INSTANCING_ENABLED

                UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
                    // SpriteRenderer.Color while Non-Batched/Instanced.
                    fixed4 unity_SpriteRendererColorArray[UNITY_INSTANCED_ARRAY_SIZE];
                    // this could be smaller but that's how bit each entry is regardless of type
                    float4 unity_SpriteFlipArray[UNITY_INSTANCED_ARRAY_SIZE];
                UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

                #define _RendererColor unity_SpriteRendererColorArray[unity_InstanceID]
                #define _Flip unity_SpriteFlipArray[unity_InstanceID]

            #endif // instancing

            CBUFFER_START(UnityPerDrawSprite)
            #ifndef UNITY_INSTANCING_ENABLED
                fixed4 _RendererColor;
                float4 _Flip;
            #endif
                float _EnableExternalAlpha;
            CBUFFER_END

            // Material Color.
            fixed4 _Color;
            sampler2D _MainTex;
            sampler2D _AlphaTex;

            float4 _WorldPosMaskPosUp;
            float4 _WorldPosMaskPosDown;

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
				float4 screenPosMaskUp : TEXCOORD1;
                float4 screenPosMaskDown : TEXCOORD2;
                float4 vertexScreenPos : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f SpriteVert(appdata_t IN)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID (IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

            #ifdef UNITY_INSTANCING_ENABLED
                IN.vertex.xy *= _Flip.xy;
            #endif
                
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                OUT.vertexScreenPos = ComputeScreenPos(OUT.vertex);
				OUT.screenPosMaskUp = ComputeScreenPos(mul(UNITY_MATRIX_VP, _WorldPosMaskPosUp));
                OUT.screenPosMaskDown = ComputeScreenPos(mul(UNITY_MATRIX_VP, _WorldPosMaskPosDown));
                
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

            fixed4 SpriteFrag(v2f IN) : SV_Target
            {
				float2 vertexScreenPos = IN.vertexScreenPos.xy / IN.vertexScreenPos.w;
                float2 screenPosMaskDown = IN.screenPosMaskDown.xy / IN.screenPosMaskDown.w;
                float2 screenPosMaskUp = IN.screenPosMaskUp.xy / IN.screenPosMaskUp.w;
                
                if (vertexScreenPos.y < screenPosMaskDown.y || vertexScreenPos.y > screenPosMaskUp.y)
                {
                    clip(-1);
                }
				
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                c.rgb *= c.a;
                return c;
            }

        ENDCG
        }
    }
}
