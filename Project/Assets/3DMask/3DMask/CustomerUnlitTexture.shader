// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Customer/Theme/CrazyDollar/CustomerUnlitTexture" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    [Enum(UnityEngine.Rendering.CullMode)]_CullMode ("_CullMode", Float) = 0

    _WorldPosMaskPosUp("_WorldPosMaskPosUp", Vector) = (0.0, 10000.0, 0.0, 1.0)
    _WorldPosMaskPosDown("_WorldPosMaskPosDown", Vector) = (0.0, -10000.0, 0.0, 1.0)

    _Stencil ("Stencil ID", Float) = 0
    [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Float) = 8
    [Enum(UnityEngine.Rendering.StencilOp)]_StencilOp ("Stencil Operation", Float) = 0
    _StencilWriteMask ("Stencil Write Mask", Float) = 255
    _StencilReadMask ("Stencil Read Mask", Float) = 255
}

SubShader {
    Tags { "RenderType"="Opaque"}
    LOD 100

	Cull [_CullMode]

    Stencil
    {
        Ref [_Stencil]
        Comp [_StencilComp]
        Pass [_StencilOp] 
        ReadMask [_StencilReadMask]
        WriteMask [_StencilWriteMask]
    }
    
    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
				float4 screenPosMaskUp : TEXCOORD1;
                float4 screenPosMaskDown : TEXCOORD2;
                float4 vertexScreenPos : TEXCOORD3;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _WorldPosMaskPosUp;
            float4 _WorldPosMaskPosDown;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                o.vertexScreenPos = ComputeScreenPos(o.vertex);
				o.screenPosMaskUp = ComputeScreenPos(mul(UNITY_MATRIX_VP, _WorldPosMaskPosUp));
                o.screenPosMaskDown = ComputeScreenPos(mul(UNITY_MATRIX_VP, _WorldPosMaskPosDown));

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                float2 vertexScreenPos = i.vertexScreenPos.xy / i.vertexScreenPos.w;
                float2 screenPosMaskDown = i.screenPosMaskDown.xy / i.screenPosMaskDown.w;
                float2 screenPosMaskUp = i.screenPosMaskUp.xy / i.screenPosMaskUp.w;

                if (vertexScreenPos.y < screenPosMaskDown.y || vertexScreenPos.y > screenPosMaskUp.y)
                {
                    clip(-1);
                }
                
                fixed4 col = tex2D(_MainTex, i.texcoord);
                UNITY_APPLY_FOG(i.fogCoord, col);
                UNITY_OPAQUE_ALPHA(col.a);
                return col;
            }
        ENDCG
    }
}

}
