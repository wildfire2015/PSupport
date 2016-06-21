Shader "Custom/StandardWithShadow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		// [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		// _MetallicGlossMap("Metallic", 2D) = "white" {}

		_SpecColor("Specular", Color) = (0.2,0.2,0.2)
		_SpecGlossMap("Specular", 2D) = "white" {}

		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}

		// _EmissionColor("Color", Color) = (0,0,0)
		// _EmissionMap("Emission", 2D) = "black" {}

		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			Name "FORWARD"
			Tags{"LightMode" = "ForwardBase"}

			Blend off /*Blend [_SrcBlend] [_DstBlend]*/
			zwrite on

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			// #pragma target 3.0
			#pragma multi_compile __ SHADOW_DEPTH

			#include "UnityCG.cginc"
			#include "UnityStandardCore.cginc"

			struct Input
			{
				float4 vertex	: POSITION;
				half3 normal	: NORMAL;
				half2 uv0		: TEXCOORD0;
				half4 tangent	: TANGENT;
			};

			struct VertexOutputForwardBaseTmp
			{
				float4 pos							: SV_POSITION;
				float4 tex							: TEXCOORD0;
				half3 eyeVec 						: TEXCOORD1;
				half4 tangentToWorldAndParallax[3]	: TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
				half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UV
				half4 _ShadowCoord					: TEXCOORD6;

				// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
				#if UNITY_SPECCUBE_BOX_PROJECTION
					float3 posWorld					: TEXCOORD8;
				#endif

				#if UNITY_OPTIMIZE_TEXCUBELOD
					#if UNITY_SPECCUBE_BOX_PROJECTION
						half3 reflUVW				: TEXCOORD9;
					#else
						half3 reflUVW				: TEXCOORD8;
					#endif
				#endif
			};

			inline half3 WorldNormal(float4 i_tex, half4 tangentToWorld[3])
			{
				half3 tangent = tangentToWorld[0].xyz;
				half3 binormal = tangentToWorld[1].xyz;
				half3 normal = tangentToWorld[2].xyz;
				half3 normalTangent = UnpackScaleNormal(tex2D (_BumpMap, i_tex.xy), _BumpScale);
				/*优化为NoNormalize*/
				half3 normalWorld = normalize(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z);
				return normalWorld;
			}
		#ifdef SHADOW_DEPTH
			float4x4 myShadowCameraVP;
			sampler2D ShadowMap;
			float Bias;
		#endif
			VertexOutputForwardBaseTmp vert (Input v)
			{
				VertexOutputForwardBaseTmp o;
				UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBaseTmp,o); 
				float4 posWorld = mul(_Object2World,v.vertex); 
				half4 tangent = v.tangent; 

				o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
				o.tex.xy = TRANSFORM_TEX(v.uv0, _MainTex);
				o.eyeVec = normalize(posWorld.xyz - _WorldSpaceCameraPos);
				float3 normalWorld = UnityObjectToWorldNormal(v.normal);
				float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

				fixed tangentSign = v.tangent.w  * unity_WorldTransformParams.w;
				half3 worldBinormal = cross(normalWorld,tangentWorld) * tangentSign;

				o.tangentToWorldAndParallax[0].xyz = tangentWorld.xyz;
				o.tangentToWorldAndParallax[1].xyz = worldBinormal.xyz;
				o.tangentToWorldAndParallax[2].xyz = normalWorld.xyz;
				// TRANSFER_SHADOW(o);
			#ifdef SHADOW_DEPTH
				o._ShadowCoord = mul(myShadowCameraVP,posWorld);
				o._ShadowCoord.xyz = o._ShadowCoord.xyz/o._ShadowCoord.w * 0.5 + 0.5;
			#endif
				// 环境光SH
				// o.ambientOrLightmapUV.rgb = max(half3(0, 0, 0), ShadeSH9(half4(normalWorld, 1.0)));
				// o.ambientOrLightmapUV.rgb += SHEvalLinearL2 (half4(normalWorld, 1.0));
			#ifdef UNITY_SHOULD_SAMPLE_SH
				// o.ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, o.ambientOrLightmapUV.rgb);
				o.ambientOrLightmapUV.rgb = ShadeSH9 (float4(normalWorld,1.0));
			#endif
			return o;
			}
			

			fixed4 frag (VertexOutputForwardBaseTmp i) : COLOR
			{
				FragmentCommonData o;
				UNITY_INITIALIZE_OUTPUT(FragmentCommonData,o);

				// albedo
				half4 diffuseC = tex2D(_MainTex,i.tex.xy);
				half3 albedo = diffuseC.rgb * _Color.rgb;
				// half3 albedo = Albedo(i.tex);

			// #if META
			// 	// mata gloss
			// 	half2 metallicGloss = MetallicGloss(i.tex.xy);
			// 	half metallic = metallicGloss.x;
			// 	o.oneMinusRoughness = metallicGloss.y;
				
			// 	// specColor	
			// 	o.specColor = lerp(unity_ColorSpaceDielectricSpec.rgb,albedo,metallic);
			// 			// We'll need oneMinusReflectivity, so
			// 			//   1-reflectivity = 1-lerp(dielectricSpec, 1, metallic) = lerp(1-dielectricSpec, 0, metallic)
			// 			// store (1-dielectricSpec) in unity_ColorSpaceDielectricSpec.a, then
			// 			//	 1-reflectivity = lerp(alpha, 0, metallic) = alpha + metallic*(0 - alpha) = 
			// 			//                  = alpha - metallic * alpha
			// 	o.oneMinusReflectivity = unity_ColorSpaceDielectricSpec.a - metallic*unity_ColorSpaceDielectricSpec.a;
				
			// 	// diffColor
			// 	o.diffColor = albedo * o.oneMinusReflectivity;
			// #elif SPEC
				half4 specGloss = tex2D(_SpecGlossMap, i.tex.xy);
				o.specColor = specGloss.rgb;
				o.oneMinusRoughness = specGloss.a;
				o.oneMinusReflectivity = 1 - o.specColor.r;
				o.diffColor = albedo * o.oneMinusReflectivity;

			// #endif

				o.normalWorld = WorldNormal(i.tex,i.tangentToWorldAndParallax);
				o.eyeVec = i.eyeVec;

				// 主光源
				UnityLight mainLight = MainLight(o.normalWorld);

				// 阴影 AO

				// half atten = SHADOW_ATTENUATION(i);
				half atten = 0;
			#ifdef SHADOW_DEPTH
				fixed shadowz = DecodeFloatRGBA(tex2D(ShadowMap, i._ShadowCoord.xy)) ;
				if ((shadowz + Bias) < i._ShadowCoord.z)
				{
					atten = 0.5;
				}
				else
				{
					atten = 1;
				}	
			#else
				atten = 1;
			#endif

				half occlusion = tex2D(_OcclusionMap, i.tex).g;
				

				Unity_GlossyEnvironmentData g;
				g.roughness = 1 - o.oneMinusRoughness;
				g.reflUVW = reflect(o.eyeVec,o.normalWorld);

				// gi-base
				UnityGI gi_base;
				ResetUnityGI(gi_base);
					// sample sh diffuse
				gi_base.light = mainLight;
				gi_base.light.color *= atten;
			// #if UNITY_SAMPLE_FULL_SH_PER_PIXEL
			// 	gi_base.indirect.diffuse = ShadeSHPerPixel(o.normalWorld,i.ambientOrLightmapUV.rgb);
			// #else
				gi_base.indirect.diffuse = i.ambientOrLightmapUV.rgb;
			// #endif
				gi_base.indirect.diffuse *= occlusion;
					// specular
				// half3 env0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0),unity_SpecCube0_HDR,g);
				half mip = g.roughness * UNITY_SPECCUBE_LOD_STEPS;
				half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, g.reflUVW, mip);
				half3 env0 = unity_SpecCube0_HDR.x * rgbm.rgb;
				gi_base.indirect.specular = env0 * occlusion;

				// half4 c = BRDF1_Unity_PBS(o.diffColor,o.specColor,o.oneMinusReflectivity,o.oneMinusRoughness,o.normalWorld,-o.eyeVec,gi_base.light,gi_base.indirect);
				/*暂时使用brdf2优化*/
				half4 c = BRDF2_Unity_PBS(o.diffColor,o.specColor,o.oneMinusReflectivity,o.oneMinusRoughness,o.normalWorld,-o.eyeVec,gi_base.light,gi_base.indirect);
				c.a = diffuseC.a;
				return c;
			}
			ENDCG
		}
	}
	fallback "Diffuse"
}
