Shader "Customer/UI/TextWaiFaGuang1"
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

        _AtmoColor("Atmosphere Color", Color) = (0.5, 0.5, 1.0, 1)
        _Size("Size", Float) = 0.1
        _Falloff("Falloff", Float) = 5
        _FalloffPlanet("Falloff Planet", Float) = 5
        _Transparency("Transparency", Float) = 15
        _TransparencyPlanet("Transparency Planet", Float) = 1
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
            Name "Default"
            Tags {"LightMode" = "Always"}
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
                float3 normal   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4  mask : TEXCOORD2;
                    
                float3 worldvertpos : TEXCOORD3;
                float3 normal : TEXCOORD4;
                    
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            int _UIVertexColorAlwaysGammaSpace;

            float4 _AtmoColor;
            float _FalloffPlanet;
            float _TransparencyPlanet;
            // float3 _WorldSpaceCameraPos;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;
                    
                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));


                if (_UIVertexColorAlwaysGammaSpace)
                {
                    if(!IsGammaSpace())
                    {
                        v.color.rgb = UIGammaToLinear(v.color.rgb);
                    }
                }

                OUT.worldvertpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                OUT.normal = v.normal;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                //Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
                //The incoming alpha could have numerical instability, which makes it very sensible to
                //HDR color transparency blend, when it blends with the world's texture.
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;

                half4 color = IN.color * (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.rgb *= color.a;
               // return color;
                
                IN.normal = normalize(IN.normal);
                float3 viewdir = normalize(UnityWorldSpaceViewDir(IN.worldvertpos));
                float4 atmo = _AtmoColor;
                    
                float tt = saturate(dot(viewdir, IN.normal));
                float tt2 = (1.0 - tt);
                atmo.a *= pow(tt2, _FalloffPlanet);
                
                atmo.a *= _TransparencyPlanet;
                color.rgb = lerp(color.rgb, atmo.rgb, atmo.a);
                return color * dot(normalize(UnityWorldSpaceLightDir(IN.worldvertpos)), IN.normal);
            }
        ENDCG
        }

        // Pass
        // {
        //     Name "Default2"
        //     Tags {"LightMode" = "Always"}
        // CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment frag
        //     #pragma target 2.0

        //     #include "UnityCG.cginc"
        //     #include "UnityUI.cginc"

        //     #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
        //     #pragma multi_compile_local _ UNITY_UI_ALPHACLIP


        //     struct appdata_t
        //     {
        //         float4 vertex   : POSITION;
        //         float4 color    : COLOR;
        //         float2 texcoord : TEXCOORD0;
        //         float3 normal   : NORMAL;
        //         UNITY_VERTEX_INPUT_INSTANCE_ID
        //     };

        //     struct v2f
        //     {
        //         float4 vertex   : SV_POSITION;
        //         fixed4 color    : COLOR;
        //         float2 texcoord  : TEXCOORD0;
        //         float4 worldPosition : TEXCOORD1;
        //         float4  mask : TEXCOORD2;
                    
        //         float3 worldvertpos : TEXCOORD3;
        //         float3 normal : TEXCOORD4;
                    
        //         UNITY_VERTEX_OUTPUT_STEREO
        //     };

        //     sampler2D _MainTex;
        //     fixed4 _Color;
        //     fixed4 _TextureSampleAdd;
        //     float4 _ClipRect;
        //     float4 _MainTex_ST;
        //     float _UIMaskSoftnessX;
        //     float _UIMaskSoftnessY;
        //     int _UIVertexColorAlwaysGammaSpace;

        //     uniform float4 _AtmoColor;
        //     uniform float _Size;
        //     uniform float _Falloff;
        //     uniform float _Transparency;
                
        //     v2f vert(appdata_t v)
        //     {
        //         v2f OUT;
        //         UNITY_SETUP_INSTANCE_ID(v);
        //         UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
        //         float4 vPosition = UnityObjectToClipPos(v.vertex);
        //         OUT.worldPosition = v.vertex;
        //         OUT.vertex = vPosition;
                    
        //         float2 pixelSize = vPosition.w;
        //         pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

        //         float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
        //         float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
        //         OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
        //         OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));


        //         if (_UIVertexColorAlwaysGammaSpace)
        //         {
        //             if(!IsGammaSpace())
        //             {
        //                 v.color.rgb = UIGammaToLinear(v.color.rgb);
        //             }
        //         }

        //         OUT.worldvertpos = mul(unity_ObjectToWorld, v.vertex).xyz;
        //         OUT.normal = v.normal;
        //         OUT.color = v.color * _Color;
        //         return OUT;
        //     }
                
        //     float4 frag(v2f IN) : Color
        //     {
        //         IN.normal = normalize(IN.normal);
        //         float3 viewdir = normalize(UnityWorldSpaceViewDir(IN.worldvertpos));
        //         float4 color = _AtmoColor;
                    
        //         float tt = saturate(dot(viewdir, IN.normal));
        //         color.a = pow(tt, _Falloff);
        //         color.a *= _Transparency * _Color * dot(normalize(UnityWorldSpaceLightDir(IN.worldvertpos)), IN.normal);
        //         return color;

        //         return float4(1, 1, 1, 0);
        //     }
        // ENDCG
        // }
    }
}
