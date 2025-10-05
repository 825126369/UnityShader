Shader "Customer/GPUInstance/GISpriteRendererExample2"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        _Flip ("Flip", Vector) = (1,1,1,1)
         _AlphaTex ("External Alpha", 2D) = "white" {}
         _EnableExternalAlpha ("Enable External Alpha", Float) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
           // #include "UnitySprites.cginc"

            // Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

           #include "UnityCG.cginc"
           #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
           #include "UnityIndirect.cginc"
           #include "UnityInstancing.cginc"

           #ifdef UNITY_INSTANCING_ENABLED
                UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
                    UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
                    UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
                    UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_InstanceColor)
                    UNITY_DEFINE_INSTANCED_PROP(float4, unity_InstanceTransform)
                UNITY_INSTANCING_BUFFER_END(PerDrawSprite)
                #define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
                #define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)
           #endif

           CBUFFER_START(UnityPerDrawSprite)
              #ifndef UNITY_INSTANCING_ENABLED
                 fixed4 _RendererColor;
                 fixed2 _Flip;
              #endif
                float _EnableExternalAlpha;
           CBUFFER_END

           fixed4 _Color;

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
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
            {
                return float4(pos.xy * flip, pos.z, 1.0);
            }

            uniform float4x4 _ObjectToWorld;
            StructuredBuffer<float4x4> _Transforms;

            v2f SpriteVert(appdata_t IN, uint svInstanceID : SV_InstanceID)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                InitIndirectDrawArgs(0);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // uint cmdID = GetCommandID(0);
                // uint instanceID = GetIndirectInstanceID(svInstanceID);

                // float4 wpos = mul(_ObjectToWorld, IN.vertex + float4(instanceID, cmdID, 0, 0));
                // OUT.vertex = mul(UNITY_MATRIX_VP, wpos);
                // OUT.color = float4(cmdID & 1 ? 0.0f : 1.0f, cmdID & 1 ? 1.0f : 0.0f, instanceID / float(GetIndirectInstanceCount()), 0.0f);

                OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);

                float4x4 obj2world = _Transforms[svInstanceID];
                OUT.vertex = mul(obj2world, OUT.vertex);
                OUT.vertex = mul(UNITY_MATRIX_VP, OUT.vertex);

                OUT.texcoord = IN.texcoord;
                OUT.color =  _Color;

                #ifdef PIXELSNAP_ON
                    OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;

            fixed4 SampleSpriteTexture (float2 uv)
            {
                fixed4 color = tex2D (_MainTex, uv);

            #if ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D (_AlphaTex, uv);
                color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
            #endif

                return color;
            }       

            fixed4 SpriteFrag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                c.rgb *= c.a;
                return c;
            }
        ENDCG
        }
    }
}
