// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Customer/Theme/CrazyDollar/CustomerUnlitTextureShadow" {
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

     _ShadowInvLen("_ShadowInvLen", Float) = 1.0
    _ShadowFalloff ("_ShadowFalloff", Float) = 1.35
    _ShadowFadeParams ("_ShadowFadeParams", Vector) = (0.0, 1.5, 0.7, 0.0)
    _ShadowPlane ("_ShadowPlane", Vector) = (0.0, 1.0, 0.0, 0.1)
    _ShadowProjDir ("_ShadowProjDir", Vector) = (0, 0, 1, 0)
    _WorldPos ("_WorldPos", Vector) = (0, 0, 1, 0)
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

    Pass
    {
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        
        Stencil
        {
            Ref 0		
            Comp Equal			
            WriteMask 255		
            ReadMask 255
            //Pass IncrSat
            Pass Invert
            Fail Keep
            ZFail Keep
        }
        
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        float4 _WorldPosMaskPosUp;
        float4 _WorldPosMaskPosDown;

        float4 _ShadowPlane;
        float4 _ShadowProjDir;
        float4 _WorldPos;
        float _ShadowInvLen;
        float4 _ShadowFadeParams;
        float _ShadowFalloff;
        
        struct appdata
        {
            float4 vertex : POSITION;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;

            float4 screenPosMaskUp : TEXCOORD1;
            float4 screenPosMaskDown : TEXCOORD2;
            float4 vertexScreenPos : TEXCOORD3;

            float3 xlv_TEXCOORD0 : TEXCOORD4;
            float3 xlv_TEXCOORD1 : TEXCOORD5;
        };

        v2f vert(appdata v)
        {
            v2f o;
            float3 worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;

            float3 lightdir = normalize(_ShadowProjDir);
            float3 objForwardDir = normalize(_ShadowPlane.xyz);
            float objForwardDirDistance = _ShadowPlane.w;
            
            float3 LigthobjForwardDirAngle = dot(objForwardDir, lightdir.xyz);  
            float objForwardDirDistance1 = dot(objForwardDir, worldpos - _WorldPos.xyz);
            float objForwardDirDistance2 = objForwardDirDistance - objForwardDirDistance1;
            float worldPosToShadowPlaneDistance = objForwardDirDistance2 / LigthobjForwardDirAngle;
            
            worldpos = worldpos + worldPosToShadowPlaneDistance * lightdir.xyz;
            o.vertex = mul(unity_MatrixVP, float4(worldpos, 1.0));
            o.xlv_TEXCOORD0 = _WorldPos.xyz;
            o.xlv_TEXCOORD1 = worldpos;

            o.vertexScreenPos = ComputeScreenPos(o.vertex);
            o.screenPosMaskUp = ComputeScreenPos(mul(UNITY_MATRIX_VP, _WorldPosMaskPosUp));
            o.screenPosMaskDown = ComputeScreenPos(mul(UNITY_MATRIX_VP, _WorldPosMaskPosDown));

            return o;
        }
        
        float4 frag(v2f i) : SV_Target
        {
            float2 vertexScreenPos = i.vertexScreenPos.xy / i.vertexScreenPos.w;
            float2 screenPosMaskDown = i.screenPosMaskDown.xy / i.screenPosMaskDown.w;
            float2 screenPosMaskUp = i.screenPosMaskUp.xy / i.screenPosMaskUp.w;

            if (vertexScreenPos.y < screenPosMaskDown.y || vertexScreenPos.y > screenPosMaskUp.y)
            {
                clip(-1);
            }

            float4 color = float4(0, 0, 0, 1);

            // // 下面两种阴影衰减公式都可以使用(当然也可以自己写衰减公式)
            // // 1. 王者荣耀的衰减公式
            float3 posToPlane_2 = (i.xlv_TEXCOORD0 - i.xlv_TEXCOORD1);
            color.a = (pow((1.0 - clamp(((sqrt(dot(posToPlane_2, posToPlane_2)) * _ShadowInvLen) - _ShadowFadeParams.x), 0.0, 1.0)), _ShadowFadeParams.y) * _ShadowFadeParams.z);

            // 2. https://zhuanlan.zhihu.com/p/31504088 这篇文章介绍的另外的阴影衰减公式
            //color.a = 1.0 - saturate(distance(i.xlv_TEXCOORD0, i.xlv_TEXCOORD1) * _ShadowFalloff);

            //color.a  = 1;
            //color.xyz *= color.a;
            return color;
        }

        ENDCG
    }
}

}
