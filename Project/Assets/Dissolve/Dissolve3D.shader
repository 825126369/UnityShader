Shader "Custom/Dissolve3D"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_NoiseTex("Noise",2D) = "white"{}
		_edgeColor1("_edgeColor1",color) =(1.,1.,1.,1.)         //燃烧颜色，可以用贴图代替
		_edgeColor2("_edgeColor2",color) = (1.,1.,1.,1.)
		_DisVal("Threshold", Range(0., 1.01)) = 0.0
	}
	
	SubShader
	{
		Pass
		{
			Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "LightMode" = "ForwardBase" }
			
			Blend One OneMinusSrcAlpha
			Cull Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
			};
			
			v2f vert(appdata_base v)
			{
				v2f o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.pos = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.));
				o.worldNormal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
				o.uv = v.texcoord;
				return o;
			}
			
	sampler2D _MainTex;
	sampler2D _NoiseTex;
	
	fixed4 _LightColor0;
	fixed4 _edgeColor1;
	fixed4 _edgeColor2;
	fixed _Diffuse;
	fixed _DisVal;
	
	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 _Diffuse = tex2D(_MainTex, i.uv);
		fixed cnoise = tex2D(_NoiseTex, i.uv);
		
		float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
 
		float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
 
		float3 worldNormal = normalize(i.worldNormal);
		
		fixed4 NdotL = max(0., dot(worldNormal, lightDir) * _LightColor0);
		fixed4 dif = (NdotL+0.3) * _Diffuse ;
	
		fixed4 light = dif;
	
		float cc = pow(cnoise.r,2);
		if ( cc< _DisVal)
		{
			if (_DisVal - cc < 0.1f)
			{
				return NdotL*(_edgeColor1 + (0.3+1.5*(_edgeColor2 - _edgeColor1)*(_DisVal - cc) / 0.03f));
			}
			discard;
		}
		return light;
	}
 
		ENDCG
	}
	}
}