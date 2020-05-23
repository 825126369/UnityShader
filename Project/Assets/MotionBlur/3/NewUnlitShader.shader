Shader "Customer/"{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white"{}
        _Diff("Diff", Range(0, 1)) = 0.5

        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOption("Blend Option", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend mode", Float) = 10
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

        Pass
        { 
            Tags{ "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Diff;

            struct a2v{
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f{
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD1;
            };

            v2f vert(a2v v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target{
                fixed4 srcColor = tex2D(_MainTex, i.uv).rgba;
                float dx = i.uv.x - 0.5;  
                float dy = i.uv.y - 0.5;  
                float dstSq = pow(dx, 2.0) + pow(dy, 2.0); 
                float v = (dstSq / _Diff); 
                float r = clamp(srcColor.r + v, 0.0, 1.0);  
                float g = clamp(srcColor.g + v, 0.0, 1.0);  
                float b = clamp(srcColor.b + v, 0.0, 1.0);
                float a = clamp(srcColor.a - v, 0.0, 1.0);

                srcColor = fixed4(r,g, b, a);
                srcColor *= a;
                return srcColor;
            }
            ENDCG
        }
    } 
    FallBack "Specular"
}