// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Own/Chapter12-MotionBlur"
｛
    Properties
    ｛
        //对应了输入的纹理
        _MainTex("Base (RGB)",2D) = "white"｛｝
        //混合图像时使用的混合系数
        _BlurAmount("Blur Amount",Float) = 1.0
    ｝
    SubShader
    ｛
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        fixed _BlurAmount;
        struct v2f｛
            float4 pos : SV_POSITION;
            half2 uv : TEXCOORD0;
        ｝;
        v2f vert(appdata_img v)｛
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            return o;
        ｝
        fixed4 fragRGB(v2f i):SV_Target｛
            return fixed4(tex2D(_MainTex,i.uv).rgb,_BlurAmount);
        ｝
        half fragA(v2f i):SV_Target｛
            return tex2D(_MainTex,i.uv);
        ｝
        ENDCG
        ZTest Always Cull Off ZWrite Off
        Pass｛
            Blend SrcAlpha OneMinusSrcAlpha
            //ColorMask可以让我们制定渲染结果的输出通道，而不是通常情况下的RGBA这4个通道全部写入。可选参数是 RGBA 的任意组合以及 
            //0，这将意味着不会写入到任何通道，可以用来单独做一次Z测试，而不将结果写入颜色通道
            ColorMask RGB
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragRGB
            ENDCG
        ｝
        Pass｛
            Blend One Zero
            ColorMask A
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragA
            ENDCG
        ｝
    ｝
    FallBack Off
｝