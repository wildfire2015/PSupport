Shader "Custom/Shadow/RenderShadow"
{
	Properties
	{
		ShadowMap("ShadowMap", 2D) = "white" {}
		ShadowColor("ShadowColor", Color) = (1,1,1,1)
		ShadowStrength("ShadowStrength",Range(0,1)) = 1.0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Geometry+1000"
		}

		Pass
		{
			ZWrite on
			ZTest on
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma	vertex vert
			#pragma fragment frag
			#pragma multi_compile __ SHADOW_DEPTH
			#include "UnityCG.cginc"

			sampler2D ShadowMap;
			fixed4 ShadowColor;
			fixed ShadowStrength;
			float4x4 myShadowCameraVP;
			
			struct vb
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex		: POSITION;
				float3 uvShadow		: TEXCOORD0;
			};

			v2f vert(vb v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				float4 posW = mul(myShadowCameraVP,mul(_Object2World, v.vertex));
				o.uvShadow.xyz = (posW.xyz / posW.w) * 0.5 + 0.5;

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed2 uv = (sign(0.5 - abs(i.uvShadow.xy - 0.5)) + 1) * 0.5;
#ifdef SHADOW_DEPTH
				fixed shadowz = DecodeFloatRGBA(tex2D(ShadowMap, i.uvShadow.xy)) ;
				if ((shadowz + 0.001) < i.uvShadow.z)
				{
					return fixed4(ShadowColor.xyz, ShadowStrength * uv.x * uv.y);
				}
				else
				{
					return fixed4(0, 0, 0, 0);
				}		
#else
				
				float color = tex2D(ShadowMap, i.uvShadow.xy).r;
				fixed4 shadowcolor = fixed4(color * ShadowColor.xyz, color * ShadowStrength * uv.x * uv.y);
				return shadowcolor;
#endif
				
				
			}
			ENDCG
		}
	}
}
