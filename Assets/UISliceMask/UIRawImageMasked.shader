Shader "Customer/UI/UIRawImageMasked"
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
            Name "CustomerRawImageMasked"
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
                half2 maskTexcoord : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            float4 _MainTex_ST;
            
			sampler2D _Z_AlphaMask;
			float4 _Z_ClipRect;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                //OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;            
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.color = v.color * _Color;

			    float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                OUT.maskTexcoord = (worldPos.xy - _Z_ClipRect.xy) / _Z_ClipRect.zw;

                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;
                
                fixed4 color = IN.color * (tex2D(_MainTex, IN.texcoord));
                // if ((IN.mask_uv.x < 0 || IN.mask_uv.x > 1 || IN.mask_uv.y < 0 || IN.mask_uv.y > 1))
                // {
                //     color.a = 0;
                // }
                
                float mask_alpha = tex2D(_Z_AlphaMask, IN.maskTexcoord).a;
                color.a *= mask_alpha;
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
