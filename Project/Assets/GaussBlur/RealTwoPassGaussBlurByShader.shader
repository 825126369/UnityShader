//作者：许珂
//对 SpriteRenderer 组件模糊
Shader "Customer/Sprite-RealTwoPassGaussBlurByShader"
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
        
		_BlurSizeX("X Blure Size", Float) = 1.0
        _BlurSizeY("Y Blure Size", Float) = 1.0
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

        CGINCLUDE
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
        float _BlurSizeX;
        float _BlurSizeY;
        
		static const float weightArray[9] = {
		        0.05, 0.09, 0.12,
		        0.15, 0.18, 0.15,
		        0.12, 0.09, 0.05
		};

        fixed4 _MainTex_TexelSize;

        // 通过 GrabPass 自动赋值的变量
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
            float2 uv[9]: TEXCOORD2;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        
        inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
        {
            return float4(pos.xy * flip, pos.z, 1.0);
        }
        
        v2f SpriteVert_Horizontal(appdata_t IN)
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

            float2 uv = IN.texcoord;
            int k = 0;
			for(int i = -4; i <= 4; i++)
			{
			 	OUT.uv[k] = uv + float2(_MainTex_TexelSize.x * i, 0) * _BlurSizeX;
			 	k++;
			}

            OUT.grabPassPosition = ComputeGrabScreenPos(OUT.vertex);
            return OUT;
        }

        v2f SpriteVert_Vertical(appdata_t IN)
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

            float2 uv = IN.texcoord;
            int k = 0;
			for(int i = -4; i <= 4; i++)
			{
				OUT.uv[k] = uv + float2(0,  _MainTex_TexelSize.y * i) * _BlurSizeY;
			 	k++;
			}

            OUT.grabPassPosition = ComputeGrabScreenPos(OUT.vertex);
            return OUT;
        }
        
        half4 GRABPIXEL(float4 grabPassPosition, int i, int j)
        {
            float4 grabPosUV = UNITY_PROJ_COORD(grabPassPosition); 
            grabPosUV.xy /= grabPosUV.w;
            return tex2D(_GrabTexture, half2(grabPosUV.x + _GrabTexture_TexelSize.x * i * _BlurSizeX, grabPosUV.y + _GrabTexture_TexelSize.y * j * _BlurSizeY));
        }

        fixed4 SpriteFrag(v2f IN) : SV_Target
        {
            fixed4 averageColor = (0, 0, 0, 0);
            for(int i = 0; i < 9; i++)
            {
			    averageColor += tex2D(_MainTex, IN.uv[i]) * weightArray[i];
            }
            
            averageColor.rgb *= averageColor.a;
            return averageColor;
        }
        
        fixed4 SpriteFrag_HorizontalGRAB(v2f IN) : SV_Target
        {
            fixed4 averageColor = (0, 0, 0, 0);
            for(int i = 0; i < 9; i++)
            {
                averageColor += GRABPIXEL(IN.grabPassPosition, i - 4, 0) * weightArray[i];
            }
            
            averageColor.rgb *= averageColor.a;
            return averageColor;
        }

        fixed4 SpriteFrag_VerticalGRAB(v2f IN) : SV_Target
        {
            fixed4 averageColor = (0, 0, 0, 0);
            for(int i = 0; i < 9; i++)
            {
                averageColor += GRABPIXEL(IN.grabPassPosition, 0, i - 4) * weightArray[i];
            }
            
            averageColor.rgb *= averageColor.a;
            return averageColor;
        }
        
        ENDCG
        
        Pass
        {
            CGPROGRAM
             #pragma vertex SpriteVert_Horizontal
            #pragma fragment SpriteFrag
            ENDCG
        }

        GrabPass {                        
            
        }

        Pass
        {
            CGPROGRAM
             #pragma vertex SpriteVert_Vertical
            #pragma fragment SpriteFrag_VerticalGRAB
            ENDCG
        }
    }
}
