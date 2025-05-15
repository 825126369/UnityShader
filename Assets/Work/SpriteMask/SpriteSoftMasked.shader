// Upgrade NOTE: upgraded instancing buffer 'PerDrawSprite' to new syntax.

Shader "Customer/SpriteSoftMasked" {
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _AlphaMask ("AlphaMask Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0

		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0

		[PerRendererData] _ClipRect ("Mask Rect", Vector) = (-32767, -32767, 32767, 32767)
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

		Stencil {
            Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
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
			float4 _AlphaMask_ST;
			sampler2D _AlphaMask;
			float4 _ClipRect;

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
				half2 maskTexcoord : TEXCOORD1;
			    half2 maskClipUV : TEXCOORD2;
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

				float3 worldPos = mul(unity_ObjectToWorld, IN.vertex);
				half2 maskUV = (worldPos.xy - _ClipRect.xy) / (_ClipRect.zw - _ClipRect.xy);
				
				OUT.maskTexcoord = TRANSFORM_TEX(maskUV, _AlphaMask);
				OUT.maskClipUV = maskUV;

			    OUT.color = IN.color * _Color * _RendererColor;

			    #ifdef PIXELSNAP_ON
			    OUT.vertex = UnityPixelSnap (OUT.vertex);
			    #endif

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
			    fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
			    float4 inMask = float4( 
					step(float2(0.0f, 0.0f), IN.maskClipUV), 
					step(IN.maskClipUV, float2(1.0f, 1.0f)) );
				if (all(inMask) == false )
				{
					c.a = 0;
				}

				c.a *= tex2D(_AlphaMask, IN.maskTexcoord).a;
			    c.rgb *= c.a;
			    return c;
			}

		ENDCG
		}
	}
}
