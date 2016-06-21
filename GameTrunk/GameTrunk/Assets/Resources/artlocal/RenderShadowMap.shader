Shader "Custom/Shadow/RenderShadowMap"
{
	
	SubShader
	{
		Tags
		{
			"Queue" = "Geometry"
		}

		Pass
		{

			ZWrite On
			ZTest Less
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma multi_compile __ SHADOW_DEPTH
			
			struct vb
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex	: SV_POSITION;
				float  depth	: TEXCOORD0;
			};
			
			v2f vert (vb v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#ifdef  SHADER_TARGET_GLSL
				o.depth = o.vertex.z / o.vertex.w * 0.5 + 0.5;
#else
				o.depth = o.vertex.z / o.vertex.w;
#endif
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
#ifdef SHADOW_DEPTH
				return EncodeFloatRGBA(i.depth);
#else
				return fixed4(1,1,1,1);
#endif
				
			}
			ENDCG
		}
	}
}
