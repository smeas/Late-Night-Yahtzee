// Author: Jonatan Johansson

Shader "Custom/TriplanarMapping" {
	Properties {
		_XTexture ("X Texture", 2D) = "white" {}
		_YTexture ("Y Texture", 2D) = "white" {}
		_ZTexture ("Z Texture", 2D) = "white" {}

		[Toggle(_USE_LOCAL_SPACE)] _UseLocalSpace ("Use Local Space", Float) = 0
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
		_BlendSharpness ("Blend Sharpness", Range(0, 100)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma multi_compile __ _USE_LOCAL_SPACE

		#pragma target 3.0

		#include <UnityCG.cginc>

		struct Input {
			float3 position;
			float3 normal;
		};

		sampler2D _XTexture; float4 _XTexture_ST;
		sampler2D _YTexture; float4 _YTexture_ST;
		sampler2D _ZTexture; float4 _ZTexture_ST;

		half _Glossiness;
		half _Metallic;
		float _BlendSharpness;

		void vert(inout appdata_full v, out Input o) {
		#if _USE_LOCAL_SPACE
			o.position = v.vertex;
			o.normal = v.normal;
		#else
			o.position = mul(unity_ObjectToWorld, v.vertex);
			o.normal = mul(unity_ObjectToWorld, v.normal);
		#endif
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float2 uvs[3] = {
				IN.position.zy,
				IN.position.xz,
				IN.position.xy
			};

			float3 samples[3] = {
				tex2D(_XTexture, TRANSFORM_TEX(uvs[0], _XTexture)).xyz,
				tex2D(_YTexture, TRANSFORM_TEX(uvs[1], _YTexture)).xyz,
				tex2D(_ZTexture, TRANSFORM_TEX(uvs[2], _ZTexture)).xyz
			};

			float3 blendWeights = pow(abs(IN.normal), _BlendSharpness);
			blendWeights /= blendWeights.x + blendWeights.y + blendWeights.z;

			o.Albedo = samples[0] * blendWeights.x + samples[1] * blendWeights.y + samples[2] * blendWeights.z;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
