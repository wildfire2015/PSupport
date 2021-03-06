﻿Shader "UI/ETC1SplitAlpha"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest off
				
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
			sampler2D _AlphaTex;
			
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.a = tex2D(_AlphaTex, i.uv).a;		
				return col;
			}
			ENDCG
		}
	}
}
