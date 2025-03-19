Shader "Customer/CustomerUIImageSliceMasked"
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
        
	    _MyAlphaMask ("AlphaMask Texture", 2D) = "white" {}
        nSliceCount ("nSliceCount", Float) = 0
        nTiledSliceCount ("nTiledSliceCount", Float) = 0
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
            Name "CustomerUIImageSliceMasked"
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
                float4 worldPosition : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            float4 _MainTex_ST;
            
			sampler2D _MyAlphaMask;
			float4 _SliceAlphaMask_ST[12];
			float4 _SliceClipRect[12];
            int nSliceCount;
            
            int nTiledSliceCount;
            float2 _TiledCount[12];
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;
                
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                
			    float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                OUT.worldPos = worldPos.xyz;
                
                OUT.color = v.color * _Color;
                return OUT;
            }

            // for 循环中 有时候不能用 break 语句，否则会导致 Unity 编译shader 卡死
            float GetMakAlpha(float3 worldPos)
            {
                float fAlpha = 0.0f;
				
				for (int i = 0; i < nSliceCount; i++)
				{
					float4 inMask = float4( 
						step(_SliceClipRect[i].xy, worldPos.xy), 
						step(worldPos.xy, _SliceClipRect[i].zw) 
					);
					if (all(inMask))
					{
						fixed2 maskUV0 = (worldPos.xy - _SliceClipRect[i].xy) / (_SliceClipRect[i].zw - _SliceClipRect[i].xy);
						fixed2 maskUV1 = maskUV0 * _SliceAlphaMask_ST[i].xy + _SliceAlphaMask_ST[i].zw;
						fAlpha = tex2D(_MyAlphaMask, maskUV1).a;
					}
				}
				
				for (int i = 0; i < nTiledSliceCount; i++)
				{
					float4 inMask = float4( 
						step(_SliceClipRect[i].xy, worldPos.xy), 
						step(worldPos.xy, _SliceClipRect[i].zw) 
					);
					if (all(inMask))
					{
						fixed2 maskUV0 = (worldPos.xy - _SliceClipRect[i].xy) / (_SliceClipRect[i].zw - _SliceClipRect[i].xy);
						maskUV0 *= _TiledCount[i];
						
						maskUV0 = fixed2(frac(maskUV0.x), frac(maskUV0.y));
						fixed2 maskUV1 = maskUV0 * _SliceAlphaMask_ST[i].xy + _SliceAlphaMask_ST[i].zw;
						fAlpha = tex2D(_MyAlphaMask, maskUV1).a;
					}
				}
                
                return fAlpha;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;
                
                fixed4 color = IN.color * (tex2D(_MainTex, IN.texcoord));
                
                float fMaskAlpha = GetMakAlpha(IN.worldPos);
                color.a *= fMaskAlpha;
                color.rgb *= color.a;

                
                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
                return color;
            }
        ENDCG
        }
    }
}
