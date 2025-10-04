Shader "Customer/GPUInstance/InstancedSprite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _ALPHAPREMULTIPLY_ON

             #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _Color;

        // #ifdef UNITY_INSTANCING_ENABLED
        //     UNITY_INSTANCING_BUFFER_START(InstanceProps)
        //         UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_InstanceColor)
        //         UNITY_DEFINE_INSTANCED_PROP(float4, unity_InstanceTransform)
        //     UNITY_INSTANCING_BUFFER_END(InstanceProps)
        // #endif

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);

                // 获取每个实例的颜色和位置（编码在矩阵中）
                 //fixed4 instColor = UNITY_ACCESS_INSTANCED_PROP(InstanceProps, unity_InstanceColor);
                // float4 instPosScale = UNITY_ACCESS_INSTANCED_PROP(InstanceProps, unity_InstanceTransform);

                // 提取位置和缩放（x,y = pos, z,w = scale）
                // float3 worldPos = float3(instPosScale.x, instPosScale.y, 0);
                // float2 scale = instPosScale.zw;

                // 应用缩放（可选）
                //v.vertex.xy *= scale;

                // 转换顶点
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = _Color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                clip(col.a - 0.1);
                return col;
            }
            ENDCG
        }
    }
}