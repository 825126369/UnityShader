Shader "Customer/UI/TextOutlineGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 0.01
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowWidth ("Glow Width", Float) = 0.02
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineWidth;
            float4 _GlowColor;
            float _GlowWidth;
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 outline = tex2D(_MainTex, i.uv + float2(_OutlineWidth, _OutlineWidth)) * _OutlineColor; // Outline effect on the edges of the text.
                fixed4 glow = tex2D(_MainTex, i.uv + float2(_GlowWidth, _GlowWidth)) * _GlowColor; // Glow effect on the edges of the text.
                col += outline + glow; // Combine the outline and glow effects with the original text color.
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}