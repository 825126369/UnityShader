// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Customer/CustomerParticleForSliceMask" {
Properties {
    _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
    _MainTex ("Particle Texture", 2D) = "white" {}
    _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
        
	_AlphaMask ("AlphaMask Texture", 2D) = "white" {}
    nSliceCount ("nSliceCount", Float) = 0
    nTiledSliceCount ("nTiledSliceCount", Float) = 0

}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Blend SrcAlpha One
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off

    SubShader {
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _TintColor;
            
            sampler2D _AlphaMask;
			float4 _AlphaMask_ST[9];
			float4 _ClipRect[9];
            int nSliceCount;

            int nTiledSliceCount;
            float2 _TiledCount[9];

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                #ifdef SOFTPARTICLES_ON
                float4 projPos : TEXCOORD1;
                #endif
                
                fixed3 worldPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                #ifdef SOFTPARTICLES_ON
                o.projPos = ComputeScreenPos (o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);
                #endif
                
               	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;
                
                o.color = v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;


            // for 循环中 有时候不能用 break 语句，否则会导致 Unity 编译shader 卡死
            fixed GetMakAlpha(fixed3 worldPos)
            {
                fixed fAlpha = 0.0f;

				for (int i = 0; i < nSliceCount; i++)
				{
					float4 inMask = float4( 
						step(_ClipRect[i].xy, worldPos.xy), 
						step(worldPos.xy, _ClipRect[i].zw) 
					);
					if (all(inMask))
					{
						fixed2 maskUV0 = (worldPos.xy - _ClipRect[i].xy) / (_ClipRect[i].zw - _ClipRect[i].xy);
						fixed2 maskUV1 = maskUV0 * _AlphaMask_ST[i].xy + _AlphaMask_ST[i].zw;
						fAlpha = tex2D(_AlphaMask, maskUV1).a;
					}
				}
				
				for (int i = 0; i < nTiledSliceCount; i++)
				{
					float4 inMask = float4( 
						step(_ClipRect[i].xy, worldPos.xy), 
						step(worldPos.xy, _ClipRect[i].zw) 
					);
					if (all(inMask))
					{
						fixed2 maskUV0 = (worldPos.xy - _ClipRect[i].xy) / (_ClipRect[i].zw - _ClipRect[i].xy);
						maskUV0 *= _TiledCount[i];

						maskUV0 = fixed2(frac(maskUV0.x), frac(maskUV0.y));
						fixed2 maskUV1 = maskUV0 * _AlphaMask_ST[i].xy + _AlphaMask_ST[i].zw;
						fAlpha = tex2D(_AlphaMask, maskUV1).a;
					}
				}
                
                return fAlpha;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                #ifdef SOFTPARTICLES_ON
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float partZ = i.projPos.z;
                float fade = saturate (_InvFade * (sceneZ-partZ));
                i.color.a *= fade;
                #endif

                fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
                fixed fMaskAlpha = GetMakAlpha(i.worldPos);
                col.a *= fMaskAlpha;

                col.a = saturate(col.a); // alpha should not have double-brightness applied to it, but we can't fix that legacy behaior without breaking everyone's effects, so instead clamp the output to get sensible HDR behavior (case 967476)

                UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode

                return col;
            }
            ENDCG
        }
    }
}
}
