Shader "Custom/CandleFlameWobble"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Speed ("Speed", Float) = 0.2
		_Strength ("Strength", Float) = 0.1
	}
	SubShader
	{
		Tags {
			"RenderType"="Transparent"
			"Queue"="Transparent"
			"PreviewType"="Plane"
		}

		Blend SrcAlpha One  // Additive
		Cull Off
		Lighting Off
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Speed;
			float _Strength;

			float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                // need full precision, otherwise half overflows when p > 1
                float x = float(34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float Unity_GradientNoise_float(float2 UV, float Scale)
            {
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }

			float4 WobbleVertex(float4 vertex, float2 uv)
			{
				float time = _Time.y * _Speed;
				float3 worldPosition = mul(float4(vertex.xyz, 1), unity_ObjectToWorld);
				// Offset with time to get movement and with world pos to get different results for different objects.
				float2 noiseUv = uv + float2(time, worldPosition.x);

				// Noise [-1, 1]
				float noiseValue = Unity_GradientNoise_float(noiseUv, 10) * 2 - 1;
				noiseValue *= _Strength;
				noiseValue *= uv.y;

				// Displace x-axis
				return vertex + float4(noiseValue, 0, 0, 0);
			}

			v2f vert (appdata v)
			{
				float4 vertexPosition = WobbleVertex(v.vertex, v.uv);
				v2f o;
				o.vertex = UnityObjectToClipPos(vertexPosition);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}