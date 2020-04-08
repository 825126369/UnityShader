// Upgrade NOTE: upgraded instancing buffer 'PerDrawSprite' to new syntax.

Shader "Customer/SpriteSoftSliceMasked" {
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

        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

		 _AlphaMask ("AlphaMask Texture", 2D) = "white" {}
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
        BlendOp [_BlendOption]
        Blend [_SrcBlend] [_DstBlend]

		Pass
		{
		CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
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
			sampler2D _AlphaMask;

			float4 _AlphaMask_ST[9];
			float4 _ClipRect[9];
            int nSliceCount;

            int nTiledSliceCount;
            float2 _TiledCount[9];
			
			struct appdata_t
			{
			    float4 vertex   : POSITION;
			    float4 color    : COLOR;
			    fixed2 texcoord : TEXCOORD0;
			    UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
			    float4 vertex   : SV_POSITION;
			    fixed4 color    : COLOR;
			    fixed2 texcoord : TEXCOORD0;
				fixed3 worldPos : TEXCOORD1;
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
                OUT.worldPos = worldPos.xyz;


			    OUT.color = IN.color * _Color * _RendererColor;
				
			    #ifdef PIXELSNAP_ON
			    OUT.vertex = UnityPixelSnap (OUT.vertex);
			    #endif

			    return OUT;
			}


			fixed4 SampleSpriteTexture (fixed2 uv)
			{
			    fixed4 color = tex2D (_MainTex, uv);

			#if ETC1_EXTERNAL_ALPHA
			    fixed4 alpha = tex2D (_AlphaTex, uv);
			    color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
			#endif

			    return color;
			}

            // for 循环中 有时候不能用 break 语句，否则会导致 Unity 编译shader 卡死
            fixed GetMakAlpha(fixed3 worldPos)
            {
                fixed fAlpha = 0.0f;
                if (nSliceCount > 0)
                {
                    for (int i = 0; i < nSliceCount; i++)
                    {
                        float4 inMask = float4( 
                            step(_ClipRect[i].xy, worldPos.xy), 
                            step(worldPos.xy, _ClipRect[i].zw) 
                        );
                        if (all(inMask))
                        {
                            fixed2 maskUV0 = (worldPos.xy - _ClipRect[i].xy) / (_ClipRect[i].zw - _ClipRect[i].xy);
                            fixed2 maskUV1 = maskUV0 * _AlphaMask_ST[i].xy + _AlphaMask_ST[i].zw;
                            fAlpha = tex2D(_AlphaMask, maskUV1).a;
                        }
                    }
                }
                else if (nTiledSliceCount > 0)
                {
                    for (int i = 0; i < nTiledSliceCount; i++)
                    {
                        float4 inMask = float4( 
                            step(_ClipRect[i].xy, worldPos.xy), 
                            step(worldPos.xy, _ClipRect[i].zw) 
                        );
                        if (all(inMask))
                        {
                            fixed2 maskUV0 = (worldPos.xy - _ClipRect[i].xy) / (_ClipRect[i].zw - _ClipRect[i].xy);
                            maskUV0 *= _TiledCount[i];

                            maskUV0 = fixed2(frac(maskUV0.x), frac(maskUV0.y));
                            fixed2 maskUV1 = maskUV0 * _AlphaMask_ST[i].xy + _AlphaMask_ST[i].zw;
                            fAlpha = tex2D(_AlphaMask, maskUV1).a;
                        }
                    }
                }
                return fAlpha;
            }

			fixed4 SpriteFrag(v2f IN) : SV_Target
			{
			    fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                
                fixed fMaskAlpha = GetMakAlpha(IN.worldPos);
                c.a *= fMaskAlpha;
			    c.rgb *= c.a;
                
			    return c;
			}

		ENDCG
		}
	}
}
